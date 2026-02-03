// tests/MoneyToWords.Tests/Properties/PropertyTests.fs

namespace MoneyToWords.Tests.Properties

open FsCheck
open FsCheck.FSharp
open FsCheck.Xunit
open MoneyToWords.Domain
open MoneyToWords.Presentation

module PropertyTests =

    let toWordsResult rubles kopecks =
        match Money.TryCreate(rubles, kopecks) with
        | Ok money -> Ok (MoneyToWords.toWords money)
        | Error _ -> Error ()

    [<Property>]
    let ``Output is never null, empty, or whitespace`` (PositiveInt rubles) (NonNegativeInt kopecks) =
        let k = kopecks % 100
        match toWordsResult (int64 rubles) k with
        | Ok result -> not (System.String.IsNullOrWhiteSpace result)
        | Error _ -> true

    [<Property>]
    let ``Output has no leading or trailing whitespace`` (PositiveInt rubles) (NonNegativeInt kopecks) =
        let k = kopecks % 100
        match toWordsResult (int64 rubles) k with
        | Ok result -> result = result.Trim()
        | Error _ -> true

    [<Property>]
    let ``Output contains no double spaces`` (PositiveInt rubles) (NonNegativeInt kopecks) =
        let k = kopecks % 100
        match toWordsResult (int64 rubles) k with
        | Ok result -> not (result.Contains("  "))
        | Error _ -> true

    [<Property>]
    let ``Output contains рубль/рубля/рублей`` (PositiveInt rubles) (NonNegativeInt kopecks) =
        let k = kopecks % 100
        match toWordsResult (int64 rubles) k with
        | Ok result ->
            result.Contains("рубль") ||
            result.Contains("рубля") ||
            result.Contains("рублей")
        | Error _ -> true

    [<Property>]
    let ``Output contains копейка/копейки/копеек when kopecks > 0`` (PositiveInt rubles) (PositiveInt kopecks) =
        let k = min kopecks 99
        match toWordsResult (int64 rubles) k with
        | Ok result ->
            result.Contains("копейка") ||
            result.Contains("копейки") ||
            result.Contains("копеек")
        | Error _ -> true

    [<Property>]
    let ``Output contains ' и ' when kopecks > 0`` (PositiveInt rubles) (PositiveInt kopecks) =
        let k = min kopecks 99
        match toWordsResult (int64 rubles) k with
        | Ok result -> result.Contains(" и ")
        | Error _ -> true

    [<Property>]
    let ``1 (not 11-19) returns: рубль`` (PositiveInt baseValue) =
        let rubles = int64 baseValue * 10L + 1L
        let isTeen = (rubles % 100L) / 10L = 1L
        not isTeen ==> lazy
        match toWordsResult rubles 0 with
        | Ok result -> result.Contains("рубль")
        | Error _ -> true

    [<Property>]
    let ``2-4 (not 12-14) returns: рубля`` (PositiveInt baseValue) =
        let rubles = int64 baseValue
        let lastTwo = rubles % 100L
        let lastDigit = lastTwo % 10L
        let isTeen = lastTwo >= 12L && lastTwo <= 14L
        let isTwoThreeFour = lastDigit >= 2L && lastDigit <= 4L
        (isTwoThreeFour && not isTeen) ==> lazy
        match toWordsResult rubles 0 with
        | Ok result -> result.Contains("рубля")
        | Error _ -> true

    [<Property>]
    let ``0,5-9,10-14 returns: рублей`` (PositiveInt baseValue) =
        let rubles = int64 baseValue
        let lastDigit = rubles % 10L
        let lastTwo = rubles % 100L
        let condition = lastDigit = 0L || lastDigit >= 5L || (lastTwo >= 10L && lastTwo <= 14L)
        condition ==> lazy
        match toWordsResult rubles 0 with
        | Ok result -> result.Contains("рублей")
        | Error _ -> true

    /// === Проверка склонения копеек ===

    [<Property>]
    let ``kopecks = 1 returns: копейка`` (PositiveInt rubles) =
        let k = 1
        match toWordsResult (int64 rubles) k with
        | Ok result -> result.Contains("копейка")
        | Error _ -> true

    [<Property>]
    let ``kopecks = 2 returns: копейки (not 12)`` (PositiveInt rubles) =
        let k = 2
        let isTeen = k = 12  // не актуально для k=2
        isTeen |> not ==> lazy
        match toWordsResult (int64 rubles) k with
        | Ok result -> result.Contains("копейки")
        | Error _ -> true

    [<Property>]
    let ``kopecks = 5 returns: копеек`` (PositiveInt rubles) =
        let k = 5
        match toWordsResult (int64 rubles) k with
        | Ok result -> result.Contains("копеек")
        | Error _ -> true

    [<Property>]
    let ``kopecks = 21 returns: копейка`` (PositiveInt rubles) =
        let k = 21
        let isTeen = (k % 100) / 10 = 1
        isTeen |> not ==> lazy
        match toWordsResult (int64 rubles) k with
        | Ok result -> result.Contains("копейка")
        | Error _ -> true

    /// === Проверка тысяч, миллионов, миллиардов ===

    [<Property>]
    let ``Thousands: 1000 returns: одна тысяча`` () =
        match toWordsResult 1_000L 0 with
        | Ok result -> result.StartsWith("одна тысяча")
        | Error _ -> true

    [<Property>]
    let ``Thousands: 2000 returns: две тысячи`` () =
        match toWordsResult 2_000L 0 with
        | Ok result -> result.StartsWith("две тысячи")
        | Error _ -> true

    [<Property>]
    let ``Millions: 1_000_000 returns: один миллион`` () =
        match toWordsResult 1_000_000L 0 with
        | Ok result -> result.StartsWith("один миллион")
        | Error _ -> true

    [<Property>]
    let ``Millions: 2_000_000 returns: два миллиона`` () =
        match toWordsResult 2_000_000L 0 with
        | Ok result -> result.StartsWith("два миллиона")
        | Error _ -> true

    [<Property>]
    let ``Billions: 1_000_000_000 returns: один миллиард`` () =
        match toWordsResult 1_000_000_000L 0 with
        | Ok result -> result.StartsWith("один миллиард")
        | Error _ -> true

    [<Property>]
    let ``Trillions: 1_000_000_000_000 returns: один триллион`` () =
        match toWordsResult 1_000_000_000_000L 0 with
        | Ok result -> result.StartsWith("один триллион")
        | Error _ -> true

    /// === Инвариант: Max значение не падает ===

    [<Property>]
    let ``Max supported value produces valid output`` =
        match toWordsResult 999_999_999_999_999L 99 with
        | Ok result ->
            not (System.String.IsNullOrWhiteSpace result) &&
            result.Contains("рублей") &&
            result.Contains("копеек")
        | Error _ -> false

    /// === Инвариант: минимальное значение (0.00) → корректная строка ===

    [<Property>]
    let ``Min value 0.00 returns:  ноль рублей`` =
        match toWordsResult 0L 0 with
        | Ok result -> result = "ноль рублей"
        | Error _ -> false

    /// === Инвариант: любые валидные входы → не пусто и не null ===

    [<Property>]
    let ``Any valid input produces non-empty result`` (rubles: byte) (kopecks: byte) =
        let r = int64 rubles
        let k = int kopecks % 100  // Ограничиваем 99
        match toWordsResult r k with
        | Ok result -> not (System.String.IsNullOrEmpty result)
        | Error _ -> true  // Ошибка валидации — не ошибка теста

    /// === Проверка, что результат не зависит от порядка (идемпотентность) ===

    [<Property>]
    let ``toWords is deterministic`` (PositiveInt rubles) (NonNegativeInt kopecks) =
        let r = int64 rubles
        let k = int kopecks % 100
        match toWordsResult r k with
        | Ok result1 ->
            let result2 = 
                match Money.TryCreate(r, k) with
                | Ok money -> MoneyToWords.toWords money
                | Error _ -> ""
            result1 = result2 && not (System.String.IsNullOrWhiteSpace result1)
        | Error _ -> true