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