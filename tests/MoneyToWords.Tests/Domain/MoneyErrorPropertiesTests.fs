namespace MoneyToWords.Tests.Domain

open Swensen.Unquote
open Xunit
open MoneyToWords.Domain

module MoneyErrorPropertiesTests =

    [<Fact>]
    let ``Code: stable values`` () =
        test <@ MoneyError.NegativeRubles.Code = "MONEY_001" @>
        test <@ (MoneyError.InvalidKopecks 100).Code = "MONEY_002" @>
        test <@ (MoneyError.RublesTooLarge 123L).Code = "MONEY_003" @>
        test <@ (MoneyError.CompositionError "x").Code = "MONEY_004" @>

    [<Fact>]
    let ``Description: includes actual values and handles empty reason`` () =
        test <@ (MoneyError.InvalidKopecks 100).Description = "Kopecks must be in range 0 to 99 (actual: 100)" @>
        test <@ (MoneyError.RublesTooLarge 999999999999L).Description = "Rubles value is too large (actual: 999999999999)" @>
        test <@ (MoneyError.CompositionError "").Description = "Money composition error" @>
        test <@ (MoneyError.CompositionError "Invalid rubles and kopecks").Description = "Money composition error: Invalid rubles and kopecks" @>
