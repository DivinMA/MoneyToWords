/// <summary>
/// Полная синхронизация лейблов GitHub с labels.json.
/// Добавляет отсутствующие, удаляет лишние (кроме разрешённых).
/// Поддерживает --dry-run и --quiet.
/// Кроссплатформенный: работает на Windows, Linux, macOS, в CI.
/// Совместимо с .NET 10 и F# 10.
/// </summary>

#r "nuget: Newtonsoft.Json"

open System
open System.IO
open System.Runtime.InteropServices
open System.Text
open Newtonsoft.Json
open Newtonsoft.Json.Linq

// ==============================================================================
// 0. Определение платформы
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
// 2. Разрешённые стандартные лейблы
// ==============================================================================

let allowedLabels = set [
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

let isDryRun = Array.contains "--dry-run" args
let isQuiet = Array.contains "--quiet" args

if isDryRun && not isQuiet then
    printfn "🧪 Режим: DRY RUN — изменения не будут применены"

// ==============================================================================
// 4. Вспомогательные функции
// ==============================================================================

let log message =
    if not isQuiet then
        printfn "%s" message

let logError message =
    eprintfn "%s" message

/// Экранирует строку для безопасного использования в PowerShell
let escapePowerShell (s: string) =
    s.Replace("\\", "\\\\")
     .Replace("\"", "\\\"")
     .Replace("$", "`$")
     .Replace("`", "``")
     .Replace("(", "`(")
     .Replace(")", "`)") 
     .Replace("^", "^^")
     .Replace("&", "`&")
     .Replace("|", "`|")
     .Replace("<", "`<")
     .Replace(">", "`>")
     .Replace("@", "`@")
     .Replace("'", "`'")

let runCommand (cmd: string) : string * int =
    let psi =
        if isWindows then
            let cmdBytes = Text.Encoding.Unicode.GetBytes(cmd)
            let encoded = Convert.ToBase64String(cmdBytes)
            System.Diagnostics.ProcessStartInfo("pwsh", $"-NoProfile -EncodedCommand {encoded}")
        else
            let escaped = cmd.Replace("\\", "\\\\").Replace("\"", "\\\"")
            System.Diagnostics.ProcessStartInfo("bash", $"-c \"exec {escaped}\"")

    psi.UseShellExecute <- false
    psi.RedirectStandardOutput <- true
    psi.RedirectStandardError <- true
    psi.CreateNoWindow <- true

    use proc = System.Diagnostics.Process.Start(psi)
    proc.WaitForExit()
    let output = proc.StandardOutput.ReadToEnd()
    let error = proc.StandardError.ReadToEnd()
    if not (String.IsNullOrEmpty error) then
        eprintfn "[STDERR] %s" error
    output.Trim(), proc.ExitCode

let ensurePowerShell () =
    if isWindows then
        log "🔧 Проверка наличия PowerShell Core (pwsh)..."
        let (output, exitCode) = runCommand "pwsh --version"
        if exitCode <> 0 then
            logError "❌ Требуется PowerShell Core (pwsh)"
            logError "Установите: https://aka.ms/powershell"
            exit 1
        elif not isQuiet then
            log $"✅ pwsh найден: {output}"

let ensureLoggedIn () =
    log "🔐 Проверка авторизации в GitHub CLI..."
    let (output, exitCode) = runCommand "gh auth status"
    if exitCode <> 0 then
        logError "❌ Не авторизован в GitHub CLI"
        logError "Запустите: gh auth login"
        exit 1
    elif not isQuiet then
        log "✅ Авторизация подтверждена"

// ==============================================================================
// 5. Получение текущих лейблов
// ==============================================================================

let getCurrentLabels () : (string * string * string) list =
    log "🔍 Получаем текущие лейблы из GitHub..."
    let (output, exitCode) = runCommand "gh label list --json name,color,description --jq '.[] | {name: .name, color: .color, description: .description}'"
    
    if exitCode <> 0 then
        logError $"❌ Ошибка получения лейблов: {output}"
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
// 6. Чтение ожидаемых лейблов
// ==============================================================================

let getExpectedLabelsMap () : Map<string, JObject> =
    if not (File.Exists labelsJsonPath) then
        logError $"❌ Файл не найден: {labelsJsonPath}"
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
    ensurePowerShell()
    ensureLoggedIn()

    let currentLabels = getCurrentLabels()
    let currentNames = currentLabels |> List.map (fun (n, _, _) -> n) |> Set.ofList
    let expectedMap = getExpectedLabelsMap()
    let expectedNames = Set.ofSeq expectedMap.Keys

    let missing = Set.difference expectedNames currentNames
    let extra = 
        Set.difference currentNames expectedNames
        |> Set.filter (fun name -> not (Set.contains name allowedLabels))

    log "\n🔁 Начинаем синхронизацию..."

    // Удаление лишних
    if not (Set.isEmpty extra) then
        log $"🗑️  Найдено %d{extra.Count} лишних лейблов:"
        for name in extra do
            log $"   - %s{name}"

        log "\n🔄 Удаляем..."
        let mutable deleted = 0
        for name in extra do
            let cmd = $"gh label delete \"{name}\" --force"
            if isDryRun then
                log $"   🧪 [DRY RUN] Удалить: %s{name}"
            else
                let (_, exitCode) = runCommand cmd
                if exitCode = 0 then
                    log $"   ✅ Удалён: %s{name}"
                    deleted <- deleted + 1
                else
                    logError $"   ❌ Ошибка при удалении '%s{name}'"
            System.Threading.Thread.Sleep(200) // Анти-rate limit
        log $"   ✅ Удалено: %d{deleted}"
    else
        log "✅ Нет лишних лейблов"

    // Добавление недостающих
    if not (Set.isEmpty missing) then
        log $"🆕 Найдено %d{missing.Count} новых лейблов:"
        for name in missing do
            log $"   - %s{name}"

        log "\n➕ Добавляем..."
        let mutable added = 0
        for name in missing do
            match expectedMap.TryFind name with
            | Some labelData ->
                let color = labelData.Value<string>("color")
                let desc = Option.ofObj (labelData.Value<string>("description")) |> Option.defaultValue ""
                let safeDesc = desc.Replace("\"", "\\\"")  // Для командной строки
                let cmd = $"gh label create \"{name}\" --color \"{color}\" --description \"{safeDesc}\""

                if isDryRun then
                    log $"   🧪 [DRY RUN] Создать: %s{name}"
                else
                    let (output, exitCode) = runCommand cmd
                    if exitCode = 0 || output.Contains("already exists") then
                        log $"   ✅ Добавлен: %s{name}"
                        added <- added + 1
                    else
                        logError $"   ❌ Ошибка при создании '%s{name}': %s{output}"
            | None ->
                logError $"❌ Лейбл '%s{name}' объявлен, но не найден в labels.json"
            System.Threading.Thread.Sleep(200) // Анти-rate limit
        log $"   ✅ Добавлено: %d{added}"
    else
        log "✅ Все лейблы уже существуют"

    // Итог
    log "\n🎉 Синхронизация завершена успешно"
    0

// ==============================================================================
// 8. Запуск
// ==============================================================================

try
    let exitCode = syncLabels()
    exit exitCode
with ex ->
    logError $"❌ Критическая ошибка: {ex.Message}"
    exit 1
