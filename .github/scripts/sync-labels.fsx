#if INTERACTIVE
// Автоматически работает при запуске из dotnet fsi
#endif

open System
open System.IO
open System.Runtime.InteropServices
open System.Text
open System.Diagnostics
open System.Text.Json
open System.Text.Json.Serialization

#if !WINDOWS
let isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
#else
let isWindows = true
#endif

let scriptDir = __SOURCE_DIRECTORY__
let labelsJsonPath = Path.Combine(scriptDir, "../config/labels.json")

let allowedLabels =
    set
        [
            "duplicate"
            "invalid"
            "wontfix"
            "question"
            "help wanted"
            "good first issue"
        ]

let args = fsi.CommandLineArgs |> Array.skip 1
let hasApply = Array.contains "--apply" args
let hasDryRun = Array.contains "--dry-run" args
let isQuiet = Array.contains "--quiet" args
let isVerbose = Array.contains "--verbose" args

let verboseLog msg = if isVerbose then printfn "[VERBOSE] %s" msg

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

let log msg = if not isQuiet then printfn "%s" msg
let logError msg = eprintfn "%s" msg

let runCommand (cmd: string) : string * int =
    let startInfo = ProcessStartInfo()
    startInfo.UseShellExecute <- false
    startInfo.RedirectStandardOutput <- true
    startInfo.RedirectStandardError <- true
    startInfo.CreateNoWindow <- true
    startInfo.FileName <- if isWindows then "pwsh" else "bash"
    startInfo.Arguments <-
        if isWindows then
            sprintf "-NoProfile -Command \"%s\"" cmd
        else
            sprintf "-c \"%s\"" cmd

    use proc = new Process()
    proc.StartInfo <- startInfo
    proc.Start() |> ignore

    let output = proc.StandardOutput.ReadToEnd().Trim()
    let error = proc.StandardError.ReadToEnd().Trim()

    proc.WaitForExit()

    if not (String.IsNullOrEmpty error) then
        logError ("STDERR: " + error)

    output, proc.ExitCode

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
    if not isDryRun then
        log "🔐 Проверка авторизации в gh..."
        let (out, code) = runCommand "gh auth status"
        if code <> 0 || out.Contains("not logged in") then
            logError "❌ Не авторизован в GitHub CLI"
            exit 1
        else
            log "✅ Авторизация подтверждена"
    else
        log "🧪 Режим --dry-run: пропуск проверки авторизации"

let wasLabelUsed (name: string) =
    if String.IsNullOrEmpty name then false else
    let safeName = name.Replace("'", "\\'")
    let cmd = sprintf "gh search issues --label '%s' --json number --limit 1" safeName
    let (output, code) = runCommand cmd
    code = 0 && not (String.IsNullOrWhiteSpace output) && output <> "[]"

type Label =
    {
        Name: string
        Color: string
        Description: string
    }

// Простой JSON-конвертер для F# record'ов
type LabelJsonConverter() =
    inherit JsonConverter<Label>()

    override _.Read(reader, typeToConvert, options) =
        let mutable label = { Name = ""; Color = ""; Description = "" }
        let mutable propName = ""

        let mutable currentReader = reader
        let mutable nameSet = false
        let mutable colorSet = false

        // Создаём копию токенов в памяти, чтобы избежать работы с byref
        let json = 
            let mutable depth = 0
            let sb = StringBuilder()
            while currentReader.Read() do
                match currentReader.TokenType with
                | JsonTokenType.StartObject when depth = 0 -> depth <- depth + 1
                | JsonTokenType.StartObject -> 
                    depth <- depth + 1
                    sb.Append("{") |> ignore
                | JsonTokenType.EndObject when depth = 1 ->
                    sb.Append("}") |> ignore
                    depth <- depth - 1
                    // Выходим, чтобы не читать дальше
                | JsonTokenType.EndObject ->
                    sb.Append("}") |> ignore
                    depth <- depth - 1
                | JsonTokenType.PropertyName ->
                    propName <- currentReader.GetString()
                    sb.Append($"\"{propName}\":") |> ignore
                | JsonTokenType.String ->
                    let value = currentReader.GetString()
                    sb.Append($"\"{value}\"") |> ignore
                    // Присваиваем поля
                    match propName.ToLowerInvariant() with
                    | "name" -> label <- { label with Name = value }; nameSet <- true
                    | "color" -> label <- { label with Color = value }; colorSet <- true
                    | "description" -> label <- { label with Description = value }
                    | _ -> ()
                | JsonTokenType.Number ->
                    let value = currentReader.GetDecimal()
                    sb.Append(value.ToString()) |> ignore
                | JsonTokenType.True -> sb.Append("true") |> ignore
                | JsonTokenType.False -> sb.Append("false") |> ignore
                | JsonTokenType.Null -> sb.Append("null") |> ignore
                | _ -> ()
            sb.ToString()

        if not nameSet || not colorSet then
            raise (JsonException("Label must have 'name' and 'color'"))

        label

    override _.Write(writer, value, options) =
        writer.WriteStartObject()
        writer.WriteString("name", value.Name)
        writer.WriteString("color", value.Color)
        if not (String.IsNullOrEmpty value.Description) then
            writer.WriteString("description", value.Description)
        writer.WriteEndObject()

let jsonOptions =
    let opts = JsonSerializerOptions()
    opts.Converters.Add(LabelJsonConverter())
    opts.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
    opts.WriteIndented <- false
    opts

let getExpectedLabels () : Label list =
    if not (File.Exists labelsJsonPath) then
        logError $"❌ Файл конфигурации не найден: {labelsJsonPath}"
        exit 1

    try
        let json = File.ReadAllText labelsJsonPath
        let doc = JsonDocument.Parse(json)
        let root = doc.RootElement

        let labels =
            seq {
                if root.ValueKind = JsonValueKind.Array then
                    yield! root.EnumerateArray()
                else
                    yield root
            }

        let result =
            labels
            |> Seq.choose (fun el ->
                try
                    let name = el.GetProperty("name").GetString()
                    let color = el.GetProperty("color").GetString()

                    let mutable descriptionElement = JsonElement()
                    let description =
                        if el.TryGetProperty("description", &descriptionElement) then
                            descriptionElement.GetString()
                        else
                            ""

                    if String.IsNullOrEmpty name || String.IsNullOrEmpty color then
                        None
                    else
                        Some { Name = name; Color = color; Description = description }
                with _ -> None)
            |> List.ofSeq

        doc.Dispose()
        result
    with ex ->
        logError $"❌ Ошибка чтения labels.json: {ex.Message}"
        exit 1

let getCurrentLabels () : Label list =
    log "🔍 Получаем метки из GitHub..."
    let cmd = "gh api --paginate 'repos/DivinMA/MoneyToWords/labels'"
    let (output, code) = runCommand cmd

    if isVerbose then
        verboseLog $"Ответ от GitHub API:\n{output}"

    if code <> 0 || String.IsNullOrWhiteSpace output then
        logError $"❌ API вернул ошибку: код={code}"
        []
    else
        try
            let doc = JsonDocument.Parse(output)
            let mutable labels = []

            for element in doc.RootElement.EnumerateArray() do
                try
                    let name = element.GetProperty("name").GetString()
                    let color = element.GetProperty("color").GetString()

                    let mutable descriptionElement = JsonElement()
                    let description =
                        if element.TryGetProperty("description", &descriptionElement) then
                            descriptionElement.GetString()
                        else
                            ""

                    if not (String.IsNullOrEmpty name) && not (String.IsNullOrEmpty color) then
                        labels <- { Name = name; Color = color; Description = description } :: labels
                with
                | _ -> logError "⚠️ Пропущена некорректная метка из API"

            doc.Dispose()
            List.rev labels  // восстанавливаем порядок
        with ex ->
            logError $"❌ Ошибка парсинга JSON: {ex.Message}"
            logError $"📄 Ответ от API:\n{output}"
            []

type SyncPlan =
    {
        Missing: Set<string>
        ToDelete: Set<string>
        ToDeprecate: Set<string>
        AlreadyDeprecated: Set<string>
        Outdated: (string * Label * Label) list
        CurrentMap: Map<string, Label>
        ExpectedMap: Map<string, Label>
    }

let analyzeLabels (current: Label list) (expected: Label list) =
    let currentMap = Map [ for l in current -> l.Name, l ]
    let expectedMap = Map [ for l in expected -> l.Name, l ]
    let currentNames = currentMap |> Map.keys |> Set.ofSeq
    let expectedNames = expectedMap |> Map.keys |> Set.ofSeq

    let missing = Set.difference expectedNames currentNames
    let extra = Set.difference currentNames expectedNames |> Set.filter (fun n -> not (Set.contains n allowedLabels))

    let isProperlyDeprecated (label: Label) =
        label.Description.StartsWith("Deprecated: use corresponding type:* label instead") && label.Color = "6A737D"

    let alreadyDeprecated = extra |> Set.filter (fun name -> currentMap |> Map.tryFind name |> Option.map isProperlyDeprecated |> Option.defaultValue false)
    let notDeprecated = Set.difference extra alreadyDeprecated
    let usedNotDeprecated = Set.filter wasLabelUsed notDeprecated
    let toDelete = Set.difference notDeprecated usedNotDeprecated
    let toDeprecate = usedNotDeprecated

    let outdated =
        expectedMap
        |> Map.toList
        |> List.choose (fun (name, expectedLabel) ->
            match currentMap |> Map.tryFind name with
            | Some currentLabel when currentLabel.Color <> expectedLabel.Color || currentLabel.Description <> expectedLabel.Description ->
                Some(name, currentLabel, expectedLabel)
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

let printStatus (plan: SyncPlan) =
    log ""; log "📊 Состояние меток:"
    let active = Set.intersect (plan.CurrentMap |> Map.keys |> Set.ofSeq) (plan.ExpectedMap |> Map.keys |> Set.ofSeq)
    log $"  ✅ Актуальные: {active.Count}"

    if not (Set.isEmpty plan.Missing) then
        log $"""  🔽 Отсутствуют: {String.concat ", " (Set.toList plan.Missing)}"""
    if not (Set.isEmpty plan.ToDelete) then
        log $"""  🗑️  Будут удалены: {String.concat ", " (Set.toList plan.ToDelete)}"""
    if not (Set.isEmpty plan.ToDeprecate) then
        log $"""  🟡 Будут помечены как deprecated: {String.concat ", " (Set.toList plan.ToDeprecate)}"""
    if not (Set.isEmpty plan.AlreadyDeprecated) then
        log $"""  ✅ Уже deprecated: {String.concat ", " (Set.toList plan.AlreadyDeprecated)}"""
    if not (List.isEmpty plan.Outdated) then
        log $"""  🔄 Требуют обновления: {plan.Outdated |> List.map (fun (n, _, _) -> n) |> String.concat ", "}"""

    if plan.Missing.IsEmpty && plan.ToDelete.IsEmpty && plan.ToDeprecate.IsEmpty && plan.Outdated.IsEmpty then
        log "🎉 Все метки в порядке"; true
    else
        false

let applyChanges (plan: SyncPlan) =
    log ""; log "🔁 Применяем изменения..."
    let currentNames = getCurrentLabels() |> List.map (fun l -> l.Name) |> Set.ofList
    log $"📌 Уже существует {currentNames.Count} меток"

    let mutable errors = 0

    for name in plan.Missing do
        if currentNames.Contains name then
            log $"🟡 Пропуск: метка '{name}' уже существует"
        else
            let lbl = plan.ExpectedMap.[name]
            let cmd = sprintf "gh label create \"%s\" --force --color %s --description \"%s\"" name lbl.Color lbl.Description
            log $"🔄 Создаём: {name}"
            let (_, code) = runCommand cmd
            if code = 0 then log $"✅ Создан: {name}" else logError $"❌ Ошибка: {name}"; errors <- errors + 1

    for name in plan.ToDeprecate do
        let cmd = sprintf "gh label edit \"%s\" --color 6A737D --description \"Deprecated: use corresponding type:* label instead\"" name
        log $"🔄 Помечаем как deprecated: {name}"
        let (_, code) = runCommand cmd
        if code = 0 then log $"✅ Deprecated: {name}" else logError $"⚠️ Не удалось: {name}"; errors <- errors + 1

    for name in plan.ToDelete do
        let cmd = sprintf "gh label delete \"%s\" --yes" name
        log $"🗑️ Удаляем: {name}"
        let (_, code) = runCommand cmd
        if code = 0 then log $"✅ Удалён: {name}" else logError $"❌ Ошибка: {name}"; errors <- errors + 1

    for (name, _, expected) in plan.Outdated do
        let cmd = sprintf "gh label edit \"%s\" --color %s --description \"%s\"" name expected.Color expected.Description
        log $"🔄 Обновляем: {name}"
        let (_, code) = runCommand cmd
        if code = 0 then log $"✅ Обновлён: {name}" else logError $"❌ Ошибка: {name}"; errors <- errors + 1

    log ""
    if errors = 0 then log "🎉 Синхронизация успешна"; 0 else logError $"❌ Ошибок: {errors}"; 1

let syncLabels () =
    ensurePowerShell ()
    ensureLoggedIn ()

    let current = getCurrentLabels ()
    let expected = getExpectedLabels ()
    let plan = analyzeLabels current expected

    if printStatus plan then
        log "💡 Нет изменений — завершаем."
        exit 0

    if isDryRun then
        log ""
        log "💡 Это DRY RUN. Чтобы применить, используйте --apply"
        log "🚀 В CI изменения применяются автоматически."
        exit 0

    let errors = applyChanges plan

    let postPrComment () =
        let prNumberStr = Environment.GetEnvironmentVariable("PR_NUMBER")
        if String.IsNullOrEmpty prNumberStr then
            log "⏭️ PR_NUMBER не задан — пропуск комментария"
        else
            let mutable prNumber = 0
            if Int32.TryParse(prNumberStr, &prNumber) && prNumber > 0 then
                let sb = Text.StringBuilder()
                sb.AppendLine("### 🔄 Синхронизация меток завершена") |> ignore
                if not plan.Missing.IsEmpty then sb.AppendLine($"- ✅ Создано: {Set.count plan.Missing}") |> ignore
                if not plan.ToDeprecate.IsEmpty then sb.AppendLine($"- 🟡 Помечено как deprecated: {Set.count plan.ToDeprecate}") |> ignore
                if not plan.ToDelete.IsEmpty then sb.AppendLine($"- 🗑️ Удалено: {Set.count plan.ToDelete}") |> ignore
                if not plan.Outdated.IsEmpty then sb.AppendLine($"- 🔄 Обновлено: {List.length plan.Outdated}") |> ignore
                sb.AppendLine(if errors = 0 then "\n🎉 Все изменения применены успешно." else $"\n⚠️ Ошибок: {errors}") |> ignore
                let body = sb.ToString().Replace("'", "\\'")
                let cmd = sprintf "gh issue comment %d --body '%s'" prNumber body
                log "💬 Публикуем комментарий в PR..."
                let (_, code) = runCommand cmd
                if code = 0 then log "✅ Комментарий добавлен" else logError "❌ Не удалось добавить комментарий"

    let totalChanges = plan.Missing.Count + plan.ToDeprecate.Count + plan.ToDelete.Count + plan.Outdated.Length
    if totalChanges > 0 then postPrComment ()
    exit errors

try
    syncLabels ()
with ex ->
    logError $"❌ Критическая ошибка: {ex.Message}"
    exit 1
