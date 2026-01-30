/// <summary>
/// Полная синхронизация лейблов GitHub с labels.json.
/// Добавляет отсутствующие, удаляет лишние (кроме разрешённых).
/// Игнорирует ошибки дублирования.
/// Поддерживает --dry-run и --quiet.
/// Совместимо с .NET 10 и F# 10.
/// </summary>

#r "nuget: Newtonsoft.Json"

open System
open System.IO
open Newtonsoft.Json
open Newtonsoft.Json.Linq

// ==============================================================================
// 1. Пути
// ==============================================================================

let scriptDir = __SOURCE_DIRECTORY__
let labelsJsonPath = Path.Combine(scriptDir, "../config/labels.json")

// ==============================================================================
// 2. Разрешённые стандартные лейблы
// ==============================================================================

let allowedLabels = Set [
    "bug"
    "chore"
    "documentation"
    "duplicate"
    "enhancement"
    "good first issue"
    "help wanted"
    "invalid"
    "major"
    "minor"
    "patch"
    "question"
    "wontfix"
]

// ==============================================================================
// 3. Аргументы командной строки
// ==============================================================================

let args = fsi.CommandLineArgs |> Array.skip 1

let isDryRun = args.Contains("--dry-run")
let isQuiet = args.Contains("--quiet")

if isDryRun && not isQuiet then
    printfn "🧪 Режим: DRY RUN — изменения не будут применены"

// ==============================================================================
// 4. Вспомогательные функции
// ==============================================================================

let log message =
    if not isQuiet then
        printfn "%s" message

let runCommand (cmd: string) : string * int =
    let psi = System.Diagnostics.ProcessStartInfo("bash", "-c \"" + cmd.Replace("\"", "\\\"") + "\"")
    psi.UseShellExecute <- false
    psi.RedirectStandardOutput <- true
    psi.RedirectStandardError <- true
    psi.CreateNoWindow <- true

    use proc = System.Diagnostics.Process.Start(psi)
    proc.WaitForExit()
    let output = proc.StandardOutput.ReadToEnd() + proc.StandardError.ReadToEnd()
    output.Trim(), proc.ExitCode

let ensureLoggedIn () =
    let (output, exitCode) = runCommand "gh auth status --quiet"
    if exitCode <> 0 then
        eprintfn "❌ Не авторизован в GitHub CLI"
        eprintfn "Запустите: gh auth login"
        exit 1

// ==============================================================================
// 5. Получение текущих лейблов
// ==============================================================================

let getCurrentLabels () : (string * string * string) list =
    log "🔍 Получаем текущие лейблы из GitHub..."
    let (output, exitCode) = runCommand "gh label list --json name,color,description --jq '.[] | {name: .name, color: .color, description: .description}'"
    
    if exitCode <> 0 then
        eprintfn "❌ Ошибка получения лейблов: %s" output
        exit 1

    if String.IsNullOrEmpty output then
        []
    else
        output.Split('\n', StringSplitOptions.RemoveEmptyEntries)
        |> Array.map (fun line ->
            try
                let jobj = JsonConvert.DeserializeObject<JObject>(line)
                jobj.Value<string>("name"),
                jobj.Value<string>("color"),
                Option.ofObj (jobj.Value<string>("description")) |> Option.defaultValue ""
            with _ -> "", "", ""
        )
        |> Array.filter (fun (n, _, _) -> not (String.IsNullOrEmpty n))
        |> List.ofArray

// ==============================================================================
// 6. Чтение ожидаемых лейблов (один раз!)
// ==============================================================================

let getExpectedLabelsMap () : Map<string, JObject> =
    if not (File.Exists labelsJsonPath) then
        eprintfn "❌ Файл не найден: %s" labelsJsonPath
        exit 1

    let json = File.ReadAllText(labelsJsonPath)
    let jarray = JsonConvert.DeserializeObject<JArray>(json)

    jarray
    |> Seq.cast<JObject>
    |> Seq.map (fun o -> o.Value<string>("name"), o)
    |> Map.ofSeq

// ==============================================================================
// 7. Синхронизация
// ==============================================================================

let syncLabels () =
    ensureLoggedIn()

    let currentLabels = getCurrentLabels()
    let currentNames = currentLabels |> List.map (fun (n, _, _) -> n) |> Set.ofList
    let expectedMap = getExpectedLabelsMap()
    let expectedNames = Set.ofSeq expectedMap.Keys

    let missing = Set.difference expectedNames currentNames
    let extra = 
        Set.difference currentNames expectedNames
        |> Set.filter (fun name -> not (allowedLabels.Contains name))

    // Удаление лишних
    if not (Set.isEmpty extra) then
        log $"🗑️  Найдено %d{extra.Count} лишних лейблов для удаления:"
        for name in extra do
            log $"   - %s{name}"

        log "\n🔄 Удаляем..."
        let mutable deleted = 0
        for name in extra do
            let cmd = $"gh label delete \"%s{name}\" --confirm"
            if isDryRun then
                log $"   🧪 [DRY RUN] Would delete: %s{name}"
            else
                let (_, exitCode) = runCommand cmd
                if exitCode = 0 then
                    log $"   ✅ %s{name}"
                    deleted <- deleted + 1
                else
                    eprintfn "   ❌ Не удалось удалить '%s'" name
        log $"   Удалено: %d{deleted}"
    else
        log "✅ Нет лишних лейблов для удаления"

    // Добавление недостающих
    if not (Set.isEmpty missing) then
        log $"🆕 Найдено %d{missing.Count} новых лейблов для добавления:"
        for name in missing do
            log $"   - %s{name}"

        log "\n➕ Добавляем..."
        let mutable added = 0
        for name in missing do
            match expectedMap.TryFind name with
            | Some labelData ->
                let color = labelData.Value<string>("color")
                let desc = Option.ofObj (labelData.Value<string>("description")) |> Option.defaultValue ""
                let safeDesc = desc.Replace("\"", "\\\"")
                let cmd = $"gh label create \"%s{name}\" --color \"%s{color}\" --description \"%s{safeDesc}\""

                if isDryRun then
                    log $"   🧪 [DRY RUN] Would create: %s{name}"
                else
                    let (output, exitCode) = runCommand cmd
                    if exitCode = 0 || output.Contains("already exists") then
                        log $"   ✅ %s{name} (создан или уже существует)"
                        added <- added + 1
                    else
                        eprintfn "   ❌ Ошибка при создании '%s': %s" name output
            | None ->
                eprintfn "❌ Лейбл '%s' объявлен как отсутствующий, но не найден в labels.json" name
        log $"   Добавлено: %d{added}"
    else
        log "✅ Все ожидаемые лейблы уже существуют"

    // Итог
    log "\n🎉 Синхронизация лейблов завершена"
    0

// ==============================================================================
// 8. Запуск
// ==============================================================================

try
    let exitCode = syncLabels()
    exit exitCode
with ex ->
    eprintfn "❌ Критическая ошибка: %s" ex.Message
    exit 1
