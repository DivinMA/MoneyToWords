/// <summary>
/// Генерирует CONTRIBUTING.md из labels.json.
/// Совместимо с .NET 10 и F# 8.
/// </summary>

#r "nuget: Newtonsoft.Json"

open System
open System.IO
open Newtonsoft.Json
open Newtonsoft.Json.Linq

// ==============================================================================
// 1. Пути
// ==============================================================================

/// <summary>
/// Директория, где находится скрипт.
/// </summary>
let scriptDir = __SOURCE_DIRECTORY__

/// <summary>
/// Путь к конфигурации лейблов.
/// </summary>
let labelsJsonPath = Path.Combine(scriptDir, "../config/labels.json")

/// <summary>
/// Путь к целевому файлу документации.
/// </summary>
let contributingMdPath = Path.Combine(scriptDir, "../../CONTRIBUTING.md")

// ==============================================================================
// 2. Вспомогательные функции
// ==============================================================================

/// <summary>
/// Извлекает строковое значение из JSON-объекта по ключу.
/// </summary>
let getValue (key: string) (jobj: JObject) : string =
    match jobj.TryGetValue(key) with
    | true, (:? JValue as v) when v.Type = JTokenType.String -> v.Value :?> string
    | _ -> failwith ("Key '" + key + "' not found or not a string")

/// <summary>
/// Извлекает булево значение; если отсутствует — false.
/// </summary>
let getBoolValue (key: string) (jobj: JObject) : bool =
    match jobj.TryGetValue(key) with
    | true, (:? JValue as v) when v.Type = JTokenType.Boolean -> v.Value :?> bool
    | _ -> false

/// <summary>
/// Извлекает опциональное строковое значение.
/// </summary>
let getOptionalValue (key: string) (jobj: JObject) : string option =
    match jobj.TryGetValue(key) with
    | true, (:? JValue as v) when v.Type = JTokenType.String -> Some(v.Value :?> string)
    | _ -> None

// ==============================================================================
// 3. Чтение и парсинг JSON
// ==============================================================================

if not (File.Exists labelsJsonPath) then
    eprintfn "❌ Ошибка: файл не найден — %s" labelsJsonPath
    exit 1

let fileInfo = FileInfo(labelsJsonPath)

if fileInfo.Length > 10240L then
    eprintfn "❌ Ошибка: файл слишком большой (%d байт)" fileInfo.Length
    exit 1

let rawJson = File.ReadAllText(labelsJsonPath)

let jsonArray =
    try
        JsonConvert.DeserializeObject<JArray>(rawJson)
    with ex ->
        eprintfn "❌ Ошибка парсинга JSON: %s" ex.Message
        exit 1

let labels = jsonArray |> Seq.cast<JObject> |> List.ofSeq

// Проверка дубликатов имён
let names = labels |> List.map (getValue "name")

if List.length (List.distinct names) <> List.length names then
    let duplicates =
        names
        |> Seq.countBy id
        |> Seq.filter (fun (_, c) -> c > 1)
        |> Seq.map fst
        |> String.concat ", "

    eprintfn "❌ Ошибка: дубликаты имён лейблов: %s" duplicates
    exit 1

// Группировка по 'group'
let groupedLabels = labels |> List.groupBy (getValue "group") |> Map.ofList

// ==============================================================================
// 4. Генерация таблицы поведенческих лейблов
// ==============================================================================

let generateBehavioralTable (labels: JObject list) =
    let header =
        """### 🔹 Поведенческие (без префикса)
Участвуют в **автоматическом версионировании** и **changelog**.

| Лейбл | Цвет | Описание | Участвует в релизе? |
|------|------|--------|-------------------|"""

    let row (l: JObject) =
        let name = getValue "name" l
        let color = getValue "color" l
        let desc = getValue "description" l
        let inRelease = if getBoolValue "inRelease" l then "✅ Да" else "❌ Нет"

        let versionEffect =
            match getOptionalValue "versionEffect" l with
            | Some ve -> " (" + ve + ")"
            | None -> ""

        "| `"
        + name
        + "` | <code style=\"background:#"
        + color
        + "; color:white\">  </code> | "
        + desc
        + " | "
        + inRelease
        + versionEffect
        + " |"

    let rows =
        labels |> List.sortBy (getValue "name") |> List.map row |> String.concat "\n"

    [ header; rows; "" ]

let behavioralLabels =
    match groupedLabels.TryFind "behavioral" with
    | Some ls -> ls
    | None -> []

let behavioralSection =
    if List.isEmpty behavioralLabels then
        [ "### 🔹 Поведенческие (без префикса)"; ""; "⚠️ Нет данных" ]
    else
        generateBehavioralTable behavioralLabels

// ==============================================================================
// 5. Генерация структурных таблиц
// ==============================================================================

let generateGroupTable groupName titleDesc (labels: JObject list) =
    let title = "#### `" + groupName + ":...` — " + titleDesc
    let tableHeader = "| Лейбл | Описание |\n|------|--------|"

    let row (l: JObject) =
        let name = getValue "name" l
        let desc = getValue "description" l
        "| `" + name + "` | " + desc + " |"

    let rows =
        labels |> List.sortBy (getValue "name") |> List.map row |> String.concat "\n"

    [ title; ""; tableHeader; rows; "" ]

let structuredSections =
    [
        for (groupName, titleDesc) in
            [
                "type", "тип задачи"
                "area", "область кода"
                "priority", "приоритет"
                "effort", "сложность"
            ] do
            match groupedLabels.TryFind groupName with
            | Some ls -> yield! generateGroupTable groupName titleDesc ls
            | None -> ()
    ]

// ==============================================================================
// 6. Сборка полного Markdown
// ==============================================================================

let header =
    """# How to Contribute

Спасибо, что хочешь помочь! Мы ценим каждый вклад.

Проект использует **полностью автоматизированную систему управления**, чтобы упростить участие.

---

## 🏷️ Система лейблов

Мы используем два типа лейблов:
"""

let footer =
    """

---

## 📝 Шаблоны задач и PR

При создании:
- **Issue** — выберите тип: баг, фича, вопрос
- **PR** — заполнится шаблон автоматически

---

## 🌲 Ветки

Используйте:
- `feat/...` — новые возможности
- `fix/...` — исправления
- `docs/...` — документация
- `chore/...` — техническое обслуживание
- `refactor/...` — рефакторинг

Пример: `feat/add-currency-support`

---

## 📦 Коммиты

Используйте [Conventional Commits](https://www.conventionalcommits.org/): `<type>: <описание> [тело]`

Поддерживаемые типы:
- `feat`: новая функция
- `fix`: исправление
- `docs`: документация
- `chore`: обслуживание
- `refactor`: рефакторинг
- `test`: тесты
- `ci`: CI/CD

Пример: `feat: add support for EUR currency`

---

## 🔍 Как узнать версию библиотеки?

```bash
dotnet list package | grep MoneyToWordsFSharpLib
```

🔄 Автоматизация
Лейблы ставятся автоматически по ветке и файлам.
Не нужно вручную ставить feature, area:core и т.д.
CI проверяет, что есть patch/minor/major.

Если возникнут вопросы — см. задачу [#11: Настройка автоматизации](https://github.com/DivinMA/MoneyToWords/issues/11)

---
"""

// 🔧 Добавляем время генерации

let generationNote =
    sprintf "> ⚙️ ⚙️ Сгенерировано автоматически • система документации"

let markdownContent =
    header
    + (String.concat "\n" (behavioralSection @ [ "---"; "" ] @ structuredSections))
    + footer
    + generationNote

// ============================================================================== // 7. Запись в файл // ==============================================================================

try
    File.WriteAllText(contributingMdPath, markdownContent)
    printfn "✅ Успешно обновлён CONTRIBUTING.md"
with ex ->
    eprintfn "❌ Ошибка при записи файла: %s" ex.Message
    exit 1
