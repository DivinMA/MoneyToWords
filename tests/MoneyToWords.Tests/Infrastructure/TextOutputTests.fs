namespace MoneyToWords.Tests.Infrastructure

open Xunit
open FsUnit.Xunit
open MoneyToWords.Infrastructure

/// <summary>
/// Tests for TextOutput.joinWords and TextOutput.withConjunction behaviors.
/// </summary>
module TextOutputTests =

    [<Fact>]
    let ``joinWords returns "ноль" for empty list`` () =
        TextOutput.joinWords [] |> should equal "ноль"

    [<Fact>]
    let ``joinWords joins words with single spaces`` () =
        TextOutput.joinWords ["сто"; "двадцать"; "три"] |> should equal "сто двадцать три"

    [<Fact>]
    let ``withConjunction includes kopecks when not zero`` () =
        TextOutput.withConjunction " и " "сто рублей" "двадцать три копейки"
        |> should equal "сто рублей и двадцать три копейки"

    [<Fact>]
    let ``withConjunction omits second part when it starts with ноль`` () =
        TextOutput.withConjunction " и " "сто рублей" "ноль копеек"
        |> should equal "сто рублей"

    [<Fact>]
    let ``withConjunctionEx includes zero when includeZero true`` () =
        TextOutput.withConjunctionEx " и " "сто рублей" "ноль копеек" true
        |> should equal "сто рублей и ноль копеек"
