open System
open System.Globalization

/// Модуль для демонстрации MoneyToWordsFSharpLib — финальная версия UI
module MoneyToWordsDemo =

    open MoneyToWords.Domain
    open MoneyToWords.Presentation

    // ──────────────────────────────────────────────
    // Настройка консоли
    // ──────────────────────────────────────────────

    /// Устанавливает размер окна консоли, если возможно
    let ensureConsoleHeight minHeight maxHeight =
        let needed = max minHeight 25
        let currentWidth = Console.WindowWidth
        let maxWidth = Console.LargestWindowWidth
        let availableHeight = Console.LargestWindowHeight
        let targetHeight = min needed (min availableHeight maxHeight)
        let targetWidth = min 100 (max currentWidth 80)

        if targetHeight > Console.WindowHeight || targetWidth > Console.WindowWidth then
            try
                Console.SetWindowSize(targetWidth, targetHeight)
            with _ -> ()  // Игнорируем ошибки (CI, SSH, etc)

    // ──────────────────────────────────────────────
    // Утилиты
    // ──────────────────────────────────────────────

    /// Преобразует сумму в текст, безопасно
    let toWordsSafe rubles kopecks =
        match Money.TryCreate(rubles, kopecks) with
        | Ok money -> MoneyToWords.toWords money
        | Error err -> sprintf "❌ %s" (MoneyErrors.toRussian err)

    /// Парсит строку в (rubles, kopecks), поддерживая . и ,
    let tryParseMoney (input: string) : Result<int64 * int, string> =
        let inputClean = input.Trim().Replace(',', '.')
        match Decimal.TryParse(inputClean, NumberStyles.Float, CultureInfo.InvariantCulture) with
        | true, value when value >= 0.0m ->
            if value > 999_999_999_999_999_999.99m then
                Error "Сумма слишком велика. Максимум — 999 квадриллионов 999 триллионов 999 миллиардов 999 миллионов 999 тысяч 999 рублей 99 копеек."
            else
                let r = int64 value
                let k = int ((value - decimal r) * 100.0m + 0.5m)
                if k > 99 then Error "Копейки не могут быть больше 99"
                else Ok (r, k)
        | true, _ -> Error "Сумма не может быть отрицательной"
        | false, _ -> Error "Некорректный формат числа. Используйте цифры и точку/запятую как разделитель (например: 123.45 или 123,45)"

    /// Форматирует число с выравниванием по правому краю в поле 25 символов
    let formatNumberAligned (r: int64) (k: int) =
        let rFormatted = r.ToString("N0").Replace(',', ' ')
        let numStr = sprintf "%s,%s" rFormatted (k.ToString("D2"))
        numStr.PadLeft(25)  // ← Ключ: правое выравнивание

    /// Печатает разделитель
    let printSection (title: string) =
        Console.ForegroundColor <- ConsoleColor.Yellow
        printfn "\n✨ %s" title
        Console.ForegroundColor <- ConsoleColor.DarkGray
        let line = "─".PadRight(max (Console.WindowWidth - 1) 50, '─')
        printfn "%s" line
        Console.ResetColor()

    /// Печатает иконку и текст
    let printIcon icon text =
        Console.ForegroundColor <- ConsoleColor.Cyan
        printfn "%s %s" icon text
        Console.ResetColor()

    // ──────────────────────────────────────────────
    // UI: Логотип
    // ──────────────────────────────────────────────

    /// Показывает логотип
    let showLogo () =
        Console.ResetColor()
        Console.ForegroundColor <- ConsoleColor.DarkYellow
        printfn @"
    __  ___                  ______   _      __            __  
   /  |/  /__  ___  ___ __ _/_  __/__| | /| / /__  _______/ /__
  / /|_/ / _ \/ _ \/ -_) // // / / _ \ |/ |/ / _ \/ __/ _  (_-<
 /_/  /_/\___/_//_/\__/\_, //_/  \___/__/|__/\___/_/  \_,_/___/
                      /___/.NET10 F# Money → Words version 1.1.0
"
        Console.ResetColor()

    // ──────────────────────────────────────────────
    // UI: Меню с выделением (ровное, стабильное)
    // ──────────────────────────────────────────────

    /// Дополняет строку пробелами справа
    let padRight (s: string) (totalWidth: int) =
        if s.Length >= totalWidth then s
        else s + new string(' ', totalWidth - s.Length)

    /// Показывает меню с выделением (ровная ширина)
    let showMenu (selectedIndex: int) =
        Console.ResetColor()
        Console.Clear()

        let itemsText = 
            [| 
                "1 — Показать примеры"
                "2 — Ввести сумму вручную"
                "q — Выйти"
            |]

        let maxWidth = 
            itemsText 
            |> Array.map (fun s -> s.Length) 
            |> Array.max
            |> max 40

        showLogo()
        printSection "ГЛАВНОЕ МЕНЮ"

        for i = 0 to itemsText.Length - 1 do
            Console.Write "   "  // Отступ

            let padded = padRight itemsText.[i] maxWidth

            if i = selectedIndex then
                Console.ForegroundColor <- ConsoleColor.White
                Console.BackgroundColor <- ConsoleColor.Blue
            else
                Console.ForegroundColor <- ConsoleColor.Gray
                Console.BackgroundColor <- ConsoleColor.Black

            Console.Write padded
            Console.ResetColor()
            Console.WriteLine()

        Console.WriteLine()
        printIcon "💡" "↑↓ — выбор | Enter — подтвердить | 1,2,q — быстрый ввод"

    /// Интерактивное меню с навигацией
    let rec interactiveMenu () =
        let itemCount = 3
        let mutable selectedIndex = 0

        let rec render () =
            Console.ResetColor()
            Console.Clear()
            showMenu selectedIndex

        render()

        let rec loop () =
            let key = Console.ReadKey(true).Key

            match key with
            | ConsoleKey.UpArrow ->
                selectedIndex <- (selectedIndex - 1 + itemCount) % itemCount
                render()
                loop()
            | ConsoleKey.DownArrow ->
                selectedIndex <- (selectedIndex + 1) % itemCount
                render()
                loop()
            | ConsoleKey.Enter -> selectedIndex
            | ConsoleKey.Q | ConsoleKey.Escape -> 2
            | ConsoleKey.D1 -> 0
            | ConsoleKey.D2 -> 1
            | ConsoleKey.D3 -> 2
            | _ -> loop()

        loop()

    // ──────────────────────────────────────────────
    // UI: Примеры
    // ──────────────────────────────────────────────

    /// Показывает примеры с автоматической высотой окна
    let showExamples () =
        ensureConsoleHeight 25 35  // ← Автоподбор высоты

        Console.ResetColor()
        Console.Clear()
        showLogo()
        printSection "ПРЕДНАСТРОЕННЫЕ ПРИМЕРЫ"

        let show (r, k) =
            let result = toWordsSafe r k
            let color = if result.StartsWith "❌" then ConsoleColor.Red else ConsoleColor.Green
            let numAligned = formatNumberAligned r k
            Console.Write $"  💬 {numAligned} → "
            Console.ForegroundColor <- color
            printfn $"{result}"
            Console.ResetColor()

        // Примеры
        show (0L, 0)
        show (1L, 0)
        show (2L, 0)
        show (5L, 0)
        show (21L, 0)
        show (1L, 1)
        show (1L, 2)
        show (1L, 5)
        show (1L, 0)
        show (11L, 0)
        show (20L, 0)
        show (99L, 0)
        show (100L, 0)
        show (999L, 0)
        show (1_000L, 0)
        show (2_000L, 0)
        show (21_000L, 0)
        show (999_000L, 0)
        show (1_000_000L, 0)
        show (1_000_000_000L, 0)
        show (1_000_000_000_000L, 0)
        show (1_000_000_000_000_000L, 0)
        show (1_234_567L, 89)
        show (999_999_999_999_999L, 99)

        printfn ""
        printIcon "⬅️" "Нажмите любую клавишу, чтобы вернуться в меню..."
        Console.ReadKey(true) |> ignore

    // ──────────────────────────────────────────────
    // UI: Интерактивный ввод
    // ──────────────────────────────────────────────

    /// Интерактивный ввод с адаптивной высотой
    let runInteractive () =
        ensureConsoleHeight 20 30

        Console.ResetColor()
        Console.Clear()
        showLogo()
        printSection "ИНТЕРАКТИВНЫЙ РЕЖИМ"
        printIcon "⌨️" "Введите сумму (например: 123.45 или 123,45) или 'q' для выхода"

        let rec loop () =
            Console.Write "💵 Введите сумму > "
            match Console.ReadLine() with
            | null ->
                printIcon "⚠️" "Ввод не может быть пустым"
                loop()
            | input when input.Trim().ToLower() = "q" ->
                printIcon "👋" "Возврат в меню..."
            | input ->
                match tryParseMoney input with
                | Ok (r, k) ->
                    let text = toWordsSafe r k
                    let numAligned = formatNumberAligned r k
                    Console.Write $"📝 {numAligned} → "
                    Console.ForegroundColor <-
                        if text.StartsWith "❌" then ConsoleColor.Red else ConsoleColor.Green
                    printfn $"{text}"
                    Console.ResetColor()
                    loop()
                | Error msg ->
                    Console.ForegroundColor <- ConsoleColor.Red
                    printfn $"❌ {msg}"
                    Console.ResetColor()
                    loop()
        loop()

        printfn ""
        printIcon "⬅️" "Нажмите любую клавишу, чтобы вернуться в меню..."
        Console.ReadKey(true) |> ignore

    // ──────────────────────────────────────────────
    // Запуск
    // ──────────────────────────────────────────────

    /// Запуск
    let run () =
        let mutable continueLoop = true
        while continueLoop do
            let choice = interactiveMenu()

            match choice with
            | 0 -> showExamples()
            | 1 -> runInteractive()
            | 2 ->
                Console.ResetColor()
                Console.Clear()
                printIcon "👋" "До новых встреч!"
                continueLoop <- false
            | _ -> ()

[<EntryPoint>]
let main argv =
    try
        MoneyToWordsDemo.run()
        0
    with
    | ex ->
        eprintfn "🚨 Критическая ошибка: %s" ex.Message
        1
