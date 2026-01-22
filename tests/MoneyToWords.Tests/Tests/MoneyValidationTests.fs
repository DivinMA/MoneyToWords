namespace MoneyToWords.Tests

open Swensen.Unquote
open Xunit
open MoneyToWords.Domain

module MoneyValidationTests =

    /// Проверка: корректные значения → Ok
    [<Fact>]
    let ``TryCreate: 0.00 (ноль) → Ok`` () =
        let result = Money.TryCreate(0L, 0)
        test <@ Result.isOk result @>

    [<Fact>]
    let ``TryCreate: 1.00 → Ok`` () =
        let result = Money.TryCreate(1L, 0)
        test <@ Result.isOk result @>

    [<Fact>]
    let ``TryCreate: 99.99 → Ok`` () =
        let result = Money.TryCreate(99L, 99)
        test <@ Result.isOk result @>

    [<Fact>]
    let ``TryCreate: 100.00 → Ok`` () =
        let result = Money.TryCreate(100L, 0)
        test <@ Result.isOk result @>

    [<Fact>]
    let ``TryCreate: 999.99 → Ok`` () =
        let result = Money.TryCreate(999L, 99)
        test <@ Result.isOk result @>

    [<Fact>]
    let ``TryCreate: 1_000.00 → Ok`` () =
        let result = Money.TryCreate(1_000L, 0)
        test <@ Result.isOk result @>

    [<Fact>]
    let ``TryCreate: 1_000.99 → Ok`` () =
        let result = Money.TryCreate(1_000L, 99)
        test <@ Result.isOk result @>

    [<Fact>]
    let ``TryCreate: 999_999.99 → Ok`` () =
        let result = Money.TryCreate(999_999L, 99)
        test <@ Result.isOk result @>

    [<Fact>]
    let ``TryCreate: 1_000_000.00 → Ok`` () =
        let result = Money.TryCreate(1_000_000L, 0)
        test <@ Result.isOk result @>

    [<Fact>]
    let ``TryCreate: 1_000_000.99 → Ok`` () =
        let result = Money.TryCreate(1_000_000L, 99)
        test <@ Result.isOk result @>

    [<Fact>]
    let ``TryCreate: 999_999_999.99 → Ok`` () =
        let result = Money.TryCreate(999_999_999L, 99)
        test <@ Result.isOk result @>

    [<Fact>]
    let ``TryCreate: 1_000_000_000.00 → Ok`` () =
        let result = Money.TryCreate(1_000_000_000L, 0)
        test <@ Result.isOk result @>

    [<Fact>]
    let ``TryCreate: 1_000_000_000.99 → Ok`` () =
        let result = Money.TryCreate(1_000_000_000L, 99)
        test <@ Result.isOk result @>

    [<Fact>]
    let ``TryCreate: 999_999_999_999.99 → Ok`` () =
        let result = Money.TryCreate(999_999_999_999L, 99)
        test <@ Result.isOk result @>

    [<Fact>]
    let ``TryCreate: 1_000_000_000_000.00 → Ok`` () =
        let result = Money.TryCreate(1_000_000_000_000L, 0)
        test <@ Result.isOk result @>

    [<Fact>]
    let ``TryCreate: 999_999_999_999_999.99 → Ok (максимальное поддерживаемое значение)`` () =
        let result = Money.TryCreate(999_999_999_999_999L, 99)
        test <@ Result.isOk result @>

    /// Проверка: некорректные значения → Error

    [<Fact>]
    let ``TryCreate: -1.00 → Error (отрицательные рубли)`` () =
        let result = Money.TryCreate(-1L, 0)
        test <@ result = Error "Rubles cannot be negative." @>

    [<Fact>]
    let ``TryCreate: -100.00 → Error (отрицательные рубли)`` () =
        let result = Money.TryCreate(-100L, 0)
        test <@ result = Error "Rubles cannot be negative." @>

    [<Fact>]
    let ``TryCreate: 0.-1 → Error (копейки < 0)`` () =
        let result = Money.TryCreate(0L, -1)
        test <@ result = Error "Kopecks must be between 0 and 99." @>

    [<Fact>]
    let ``TryCreate: 10.-1 → Error (копейки < 0)`` () =
        let result = Money.TryCreate(10L, -1)
        test <@ result = Error "Kopecks must be between 0 and 99." @>

    [<Fact>]
    let ``TryCreate: 0.100 → Error (копейки > 99)`` () =
        let result = Money.TryCreate(0L, 100)
        test <@ result = Error "Kopecks must be between 0 and 99." @>

    [<Fact>]
    let ``TryCreate: 0.101 → Error (копейки > 99)`` () =
        let result = Money.TryCreate(0L, 101)
        test <@ result = Error "Kopecks must be between 0 and 99." @>

    [<Fact>]
    let ``TryCreate: 10.100 → Error (копейки > 99)`` () =
        let result = Money.TryCreate(10L, 100)
        test <@ result = Error "Kopecks must be between 0 and 99." @>

    [<Fact>]
    let ``TryCreate: рубли слишком велики (1e18) → Error`` () =
        let result = Money.TryCreate(1_000_000_000_000_000_000L, 0)
        test <@ result = Error "Rubles too large (max 999 quadrillion)." @>

    [<Fact>]
    let ``TryCreate: рубли = Int64.MaxValue → Error`` () =
        let result = Money.TryCreate(System.Int64.MaxValue, 0)
        test <@ result = Error "Rubles too large (max 999 quadrillion)." @>

    /// Проверка: граничные значения копеек

    [<Fact>]
    let ``TryCreate: 0.0 → Ok (минимальное значение копеек)`` () =
        let result = Money.TryCreate(0L, 0)
        test <@ Result.isOk result @>

    [<Fact>]
    let ``TryCreate: 0.99 → Ok (максимальное значение копеек)`` () =
        let result = Money.TryCreate(0L, 99)
        test <@ Result.isOk result @>

    /// Проверка: комбинированные граничные случаи

    [<Fact>]
    let ``TryCreate: 0.0 → Ok (минимальный возможный Money)`` () =
        let result = Money.TryCreate(0L, 0)
        test <@ Result.isOk result @>

    [<Fact>]
    let ``TryCreate: 999_999_999_999_999.99 → Ok (максимальный возможный Money)`` () =
        let result = Money.TryCreate(999_999_999_999_999L, 99)
        test <@ Result.isOk result @>