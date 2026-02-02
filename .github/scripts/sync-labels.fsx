#r "nuget: Newtonsoft.Json"

open System
open System.IO
open System.Runtime.InteropServices
open System.Text
open System.Threading
open System.Diagnostics
open Newtonsoft.Json
open Newtonsoft.Json.Linq

// ==============================================================================
// 0. Платформа
// ==============================================================================

#if !WINDOWS
let isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
#else
let isWindows = true
#endif

// ==============================================================================
// 1. Пути
// ==============================================================================

let scriptDir = __SOURCE_DIRECTORY__
let labelsJsonPath = Path.Combine(scriptDir, "../config/labels.json")

// ==============================================================================
// 2. Разрешённые метки (не удаляем)
// ==============================================================================

let allowedLabels =
    set [
        "duplicate"
        "invalid"
        "wontfix"
        "question"
        "help wanted"
        "good first issue"
    ]

// ==============================================================================
// 3. Аргументы и режим
// ==============================================================================

let args = fsi.CommandLineArgs |> Array.skip 1

let hasApply = Array.contains "--apply" args
let hasDryRun = Array.contains "--dry-run" args
let isQuiet = Array.contains "--quiet" args

let isDryRun =
    if hasDryRun then true
    elif hasApply then false
    else Environment.GetEnvironmentVariable("CI") <> "true"

if not isQuiet then
    printfn "⚙️  Режим выполнения:"
    if isDryRun then
        printfn "   🧪 --dry-run: изменения НЕ будут применены"
    else
        printfn "   🚀 --apply: изменения БУДУТ применены"
    
    if hasApply then printfn "   📌 Флаг: --apply"
    if hasDryRun then printfn "   📌 Флаг: --dry-run"
    printfn "   🌐 Среда: CI=%b" (Environment.GetEnvironmentVariable("CI") = "true")

// ==============================================================================
// 4. Логирование
// ==============================================================================

let log msg = if not isQuiet then printfn "%s" msg
let logError msg = eprintfn "%s" msg

// ==============================================================================
// 5. Выполнение команд
// ==============================================================================

let runCommand (cmd: string) : string * int =
    let startInfo = ProcessStartInfo()
    startInfo.UseShellExecute <- false
    startInfo.RedirectStandardOutput <- true
    startInfo.RedirectStandardError <- true
    startInfo.CreateNoWindow <- true

    startInfo.FileName <-
        if isWindows then "pwsh" else "bash"

    startInfo.Arguments <-
        if isWindows then sprintf "-NoProfile -Command \"%s\"" cmd
        else sprintf "-c \"%s\"" cmd

    use proc = new Process()
    proc.StartInfo <- startInfo

    let output = StringBuilder()
    let error = StringBuilder()

    proc.OutputDataReceived.Add(fun args ->
        if args.Data <> null then output.AppendLine(args.Data) |> ignore)

    proc.ErrorDataReceived.Add(fun args ->
        if args.Data <> null then error.AppendLine(args.Data) |> ignore)

    proc.Start() |> ignore
    proc.BeginOutputReadLine()
    proc.BeginErrorReadLine()

    let finished = proc.WaitForExit(30000)

    if not finished then
        try proc.Kill() with _ -> ()
        logError "❌ Команда прервана по таймауту"
        "", 1
    else
        let out = output.ToString().Trim()
        let err = error.ToString().Trim()
        if not (String.IsNullOrEmpty err) then logError ("STDERR: " + err)
        out, proc.ExitCode

// ==============================================================================
// 6. Проверки окружения
// ==============================================================================

let ensurePowerShell () =
    if isWindows then
        log "🔧 Проверка pwsh..."
        let (out, code) = runCommand "pwsh --version"
        if code <> 0 then
            logError "❌ Требуется PowerShell Core"
            exit 1
        elif not isQuiet then
            log $"✅ pwsh найден: {out}"

let ensureLoggedIn () =
    log "🔐 Проверка авторизации в gh..."
    let (out, code) = runCommand "gh auth status"
    if code <> 0 then
        logError "❌ Не авторизован в GitHub CLI"
        logError "Выполните: gh auth login"
        exit 1
    else
        log "✅ Авторизация подтверждена"

let wasLabelUsed (name: string) =
    let safeName = name.Replace("'", "\\'")
    let cmd = sprintf "gh search issues --label '%s' --json number --limit 1" safeName
    let (_, code) = runCommand cmd
    code = 0

// ==============================================================================
// 7. Модель
// ==============================================================================

type Label = {
    Name: string
    Color: string
    Description: string
}

// ==============================================================================
// 8. Чтение ожидаемых меток
// ==============================================================================

let getExpectedLabels () : Label list =
    if not (File.Exists labelsJsonPath) then
        logError $"❌ Файл не найден: {labelsJsonPath}"
        exit 1

    try
        let json = File.ReadAllText labelsJsonPath
        let jarray = JsonConvert.DeserializeObject<JArray>(json)

        jarray
        |> Seq.cast<JObject>
        |> Seq.map (fun o ->
            {
                Name = o.Value<string>("name")
                Color = o.Value<string>("color")
                Description = defaultArg (Option.ofObj(o.Value<string>("description"))) ""
            })
        |> Seq.toList
    with ex ->
        logError $"❌ Ошибка парсинга labels.json: {ex.Message}"
        exit 1

// ==============================================================================
// 9. Получение текущих меток
// ==============================================================================

let getCurrentLabels () : Label list =
    log "🔍 Получаем метки из GitHub..."
    let cmd = "gh api --paginate repos/DivinMA/MoneyToWords/labels"
    let (output, code) = runCommand cmd

    if code <> 0 || String.IsNullOrWhiteSpace(output) then
        logError "❌ Не удалось получить метки"
        []
    else
        try
            let jarray = JsonConvert.DeserializeObject<JArray>(output.Trim())
            jarray
            |> Seq.cast<JObject>
            |> Seq.map (fun o ->
                {
                    Name = o.Value<string>("name")
                    Color = o.Value<string>("color")
                    Description = defaultArg (Option.ofObj(o.Value<string>("description"))) ""
                })
            |> Seq.toList
        with ex ->
            logError $"❌ Ошибка парсинга ответа: {ex.Message}"
            []

// ==============================================================================
// 10. План синхронизации
// ==============================================================================

type SyncPlan = {
    Missing: Set<string>
    ToDelete: Set<string>
    ToDeprecate: Set<string>
    AlreadyDeprecated: Set<string>
    Outdated: (string * Label * Label) list
    CurrentMap: Map<string, Label>
    ExpectedMap: Map<string, Label>
}

// ==============================================================================
// 11. Анализ различий
// ==============================================================================

let analyzeLabels (current: Label list) (expected: Label list) =
    let currentMap = Map [ for l in current -> l.Name, l ]
    let expectedMap = Map [ for l in expected -> l.Name, l ]

    // ✅ Явное преобразование ICollection -> seq -> Set
    let currentNames = currentMap |> Map.toSeq |> Seq.map fst |> Set.ofSeq
    let expectedNames = expectedMap |> Map.toSeq |> Seq.map fst |> Set.ofSeq

    let missing = Set.difference expectedNames currentNames

    let extra =
        Set.difference currentNames expectedNames
        |> Set.filter (fun n -> not (Set.contains n allowedLabels))

    let isProperlyDeprecated (label: Label) =
        label.Description.StartsWith("Deprecated: use corresponding type:* label instead")
        && label.Color = "6A737D"

    let alreadyDeprecated =
        extra
        |> Set.filter (fun name ->
            match currentMap.TryFind name with
            | Some label -> isProperlyDeprecated label
            | None -> false)

    let notDeprecated = Set.difference extra alreadyDeprecated
    let usedNotDeprecated = Set.filter wasLabelUsed notDeprecated
    let toDelete = Set.difference notDeprecated usedNotDeprecated
    let toDeprecate = usedNotDeprecated

    let outdated =
        expectedMap
        |> Map.toList
        |> List.choose (fun (name, expectedLabel) ->
            match currentMap.TryFind name with
            | Some currentLabel when
                currentLabel.Color <> expectedLabel.Color ||
                currentLabel.Description <> expectedLabel.Description ->
                Some (name, currentLabel, expectedLabel)
            | _ -> None)

    {
        Missing = missing
        ToDelete = toDelete
        ToDeprecate = toDeprecate
        AlreadyDeprecated = alreadyDeprecated
        Outdated = outdated
        CurrentMap = currentMap
        ExpectedMap = expectedMap
    }

// ==============================================================================
// 12. Вывод статуса
// ==============================================================================

let printStatus (plan: SyncPlan) =
    log ""
    log "📊 Состояние меток:"

    let currentKeys = plan.CurrentMap |> Map.toSeq |> Seq.map fst |> Set.ofSeq
    let expectedKeys = plan.ExpectedMap |> Map.toSeq |> Seq.map fst |> Set.ofSeq
    let active = Set.intersect currentKeys expectedKeys

    log $"  ✅ Актуальные: {active.Count}"

    if not (Set.isEmpty plan.Missing) then
        let names = String.concat ", " (Set.toList plan.Missing)
        log $"  🔽 Отсутствуют: {names}"

    if not (Set.isEmpty plan.ToDelete) then
        let names = String.concat ", " (Set.toList plan.ToDelete)
        log $"  🗑️  Будут удалены: {names}"

    if not (Set.isEmpty plan.ToDeprecate) then
        let names = String.concat ", " (Set.toList plan.ToDeprecate)
        log $"  🟡 Будут помечены как deprecated: {names}"

    if not (Set.isEmpty plan.AlreadyDeprecated) then
        let names = String.concat ", " (Set.toList plan.AlreadyDeprecated)
        log $"  ✅ Уже deprecated: {names}"

    if not (List.isEmpty plan.Outdated) then
        let names = plan.Outdated |> List.map (fun (n, _, _) -> n) |> String.concat ", "
        log $"  🔄 Требуют обновления: {names}"

    if plan.Missing.IsEmpty && plan.ToDelete.IsEmpty && plan.ToDeprecate.IsEmpty && plan.Outdated.IsEmpty then
        log ""
        log "🎉 Все метки в порядке"
        true
    else
        false

// ==============================================================================
// 13. Применение изменений
// ==============================================================================

let applyChanges (plan: SyncPlan) =
    log ""
    log "🔁 Применяем изменения..."

    let mutable errors = 0

    for name in plan.Missing do
        let lbl = plan.ExpectedMap.[name]
        let cmd = sprintf "gh label create '%s' --force --color %s --description '%s'" name lbl.Color lbl.Description
        log $"🔄 Создаём: {name}"
        let (output, code) = runCommand cmd
        if code = 0 then
            log $"✅ Создан: {name}"
        elif output.Contains("already exists") then
            log $"🟡 Уже существует: {name} (пропущено)"
        else
            logError $"❌ Ошибка: {name}"
            logError $"   Вывод: {output}"
            errors <- errors + 1
        Thread.Sleep(500)


    for name in plan.ToDeprecate do
        let cmd = sprintf "gh label edit '%s' --color 6A737D --description 'Deprecated: use corresponding type:* label instead'" name
        log $"🔄 Помечаем как deprecated: {name}"
        let (_, code) = runCommand cmd
        if code = 0 then log $"✅ Deprecated: {name}" else logError $"⚠️  Не удалось: {name}"; errors <- errors + 1
        Thread.Sleep(500)

    for name in plan.ToDelete do
        let cmd = sprintf "gh label delete '%s' --yes" name
        log $"🗑️  Удаляем: {name}"
        let (_, code) = runCommand cmd
        if code = 0 then log $"✅ Удалён: {name}" else logError $"❌ Ошибка: {name}"; errors <- errors + 1
        Thread.Sleep(500)

    for (name, _, expected) in plan.Outdated do
        let cmd = sprintf "gh label edit '%s' --color %s --description '%s'" name expected.Color expected.Description
        log $"🔄 Обновляем: {name}"
        let (_, code) = runCommand cmd
        if code = 0 then log $"✅ Обновлён: {name}" else logError $"❌ Ошибка: {name}"; errors <- errors + 1
        Thread.Sleep(500)

    log ""
    if errors = 0 then
        log "🎉 Синхронизация успешна"
        0
    else
        logError $"❌ Ошибок: {errors}"
        1

// ==============================================================================
// 14. Основной поток
// ==============================================================================

let syncLabels () =
    ensurePowerShell()
    ensureLoggedIn()

    let current = getCurrentLabels()
    let expected = getExpectedLabels()

    let plan = analyzeLabels current expected

    if printStatus plan then
        log "💡 Нет изменений — завершаем."
        exit 0

    if isDryRun then
        log ""
        log "💡 Это DRY RUN. Чтобы применить, используйте --apply"
        log "🚀 В CI изменения применяются автоматически."
        exit 0

    applyChanges plan

// ==============================================================================
// 15. Запуск
// ==============================================================================

try
    let result = syncLabels()
    exit result
with ex ->
    logError $"❌ Критическая ошибка: {ex.Message}"
    exit 1
