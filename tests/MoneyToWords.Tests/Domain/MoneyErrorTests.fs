// tests/MoneyToWords.Tests/Domain/MoneyErrorTests.fs

namespace MoneyToWords.Tests.Domain

open Swensen.Unquote
open Xunit
open MoneyToWords.Domain
open MoneyToWords.Presentation

module MoneyErrorTests =

    [<Fact>]
    let ``TryCreate with negative rubles returns NegativeRubles`` () =
        let result = Money.TryCreate(-1L, 0)
        test <@ result = Error MoneyError.NegativeRubles @>

    [<Fact>]
    let ``TryCreate with rubles too large returns RublesTooLarge`` () =
        let value = 1_000_000_000_000_000_000L
        let result = Money.TryCreate(value, 0)
        test <@ result = Error (MoneyError.RublesTooLarge value) @>

    [<Fact>]
    let ``TryCreate with rubles = Int64.MaxValue returns RublesTooLarge`` () =
        let value = System.Int64.MaxValue
        let result = Money.TryCreate(value, 0)
        test <@ result = Error (MoneyError.RublesTooLarge value) @>

    [<Fact>]
    let ``TryCreate with kopecks < 0 returns InvalidKopecks`` () =
        let result = Money.TryCreate(10L, -1)
        test <@ result = Error (MoneyError.InvalidKopecks -1) @>

    [<Fact>]
    let ``TryCreate with kopecks = 100 returns InvalidKopecks`` () =
        let result = Money.TryCreate(10L, 100)
        test <@ result = Error (MoneyError.InvalidKopecks 100) @>

    [<Fact>]
    let ``TryCreate with kopecks = 101 returns InvalidKopecks`` () =
        let result = Money.TryCreate(10L, 101)
        test <@ result = Error (MoneyError.InvalidKopecks 101) @>

    [<Fact>]
    let ``toRussian formats NegativeRubles correctly`` () =
        let msg = MoneyErrors.toRussian MoneyError.NegativeRubles
        test <@ msg = "Сумма не может быть отрицательной" @>

    [<Fact>]
    let ``toRussian formats RublesTooLarge correctly`` () =
        let value = 1234567890L
        let msg = MoneyErrors.toRussian (MoneyError.RublesTooLarge value)
        test <@ msg.Contains("1234567890") @>
        test <@ msg.Contains("999 квадриллионов") @>

    [<Fact>]
    let ``toRussian formats InvalidKopecks correctly`` () =
        let msg = MoneyErrors.toRussian (MoneyError.InvalidKopecks 100)
        test <@ msg.Contains("100") @>
        test <@ msg.Contains("99") @>
        test <@ msg.Contains("указано") || msg.Contains("Указано") @>

    [<Fact>]
    let ``toRussian formats CompositionError correctly`` () =
        let reason = "Invalid rubles and kopecks"
        let error = MoneyError.CompositionError reason
        let msg = MoneyErrors.toRussian error
        let expected = "Ошибка составления денежной суммы: Invalid rubles and kopecks"
    
        test <@ msg = expected @>