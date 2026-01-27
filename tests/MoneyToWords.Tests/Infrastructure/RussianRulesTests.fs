namespace MoneyToWords.Tests.Infrastructure

open Xunit
open Swensen.Unquote
open FsUnit.Xunit
open MoneyToWords.Infrastructure
open MoneyToWords.Domain
open MoneyToWords.Application

/// <summary>
/// Tests for Russian linguistic rules.
/// Verifies correctness of number forms, declension, and word generation.
/// </summary>
module RussianRulesTests =

    /// === Unit Tests: Masculine Units (один, два, три...) ===

    [<Fact>]
    let ``unitMasc returns "один" for 1`` () =
        RussianRules.unitMasc 1 |> should equal (Some "один")

    [<Fact>]
    let ``unitMasc returns "два" for 2`` () =
        RussianRules.unitMasc 2 |> should equal (Some "два")

    [<Fact>]
    let ``unitMasc returns "три" for 3`` () =
        RussianRules.unitMasc 3 |> should equal (Some "три")

    [<Fact>]
    let ``unitMasc returns "четыре" for 4`` () =
        RussianRules.unitMasc 4 |> should equal (Some "четыре")

    [<Fact>]
    let ``unitMasc returns "пять" for 5`` () =
        RussianRules.unitMasc 5 |> should equal (Some "пять")

    [<Fact>]
    let ``unitMasc returns "шесть" for 6`` () =
        RussianRules.unitMasc 6 |> should equal (Some "шесть")

    [<Fact>]
    let ``unitMasc returns "семь" for 7`` () =
        RussianRules.unitMasc 7 |> should equal (Some "семь")

    [<Fact>]
    let ``unitMasc returns "восемь" for 8`` () =
        RussianRules.unitMasc 8 |> should equal (Some "восемь")

    [<Fact>]
    let ``unitMasc returns "девять" for 9`` () =
        RussianRules.unitMasc 9 |> should equal (Some "девять")

    [<Fact>]
    let ``unitMasc returns None for 0`` () =
        RussianRules.unitMasc 0 |> should equal None

    [<Fact>]
    let ``unitMasc returns None for 10`` () =
        RussianRules.unitMasc 10 |> should equal None


    /// === Unit Tests: Feminine Units (одна, две...) ===

    [<Fact>]
    let ``unitFem returns "одна" for 1`` () =
        RussianRules.unitFem 1 |> should equal (Some "одна")

    [<Fact>]
    let ``unitFem returns "две" for 2`` () =
        RussianRules.unitFem 2 |> should equal (Some "две")

    [<Fact>]
    let ``unitFem returns "три" for 3`` () =
        RussianRules.unitFem 3 |> should equal (Some "три")

    [<Fact>]
    let ``unitFem returns "девять" for 9`` () =
        RussianRules.unitFem 9 |> should equal (Some "девять")


    /// === Unit Tests: Teens (11–19) ===

    [<Fact>]
    let ``teen returns "одиннадцать" for 11`` () =
        RussianRules.teen 11 |> should equal (Some "одиннадцать")

    [<Fact>]
    let ``teen returns "двенадцать" for 12`` () =
        RussianRules.teen 12 |> should equal (Some "двенадцать")

    [<Fact>]
    let ``teen returns "девятнадцать" for 19`` () =
        RussianRules.teen 19 |> should equal (Some "девятнадцать")

    [<Fact>]
    let ``teen returns "десять" for 10`` () =
        RussianRules.teen 10 |> should equal (Some "десять")

    [<Fact>]
    let ``teen returns None for 9`` () =
        RussianRules.teen 9 |> should equal None

    [<Fact>]
    let ``teen returns None for 20`` () =
        RussianRules.teen 20 |> should equal None


    /// === Unit Tests: Tens (20, 30...) ===

    [<Fact>]
    let ``ten returns "двадцать" for 2`` () =
        RussianRules.ten 2 |> should equal (Some "двадцать")

    [<Fact>]
    let ``ten returns "тридцать" for 3`` () =
        RussianRules.ten 3 |> should equal (Some "тридцать")

    [<Fact>]
    let ``ten returns "девяносто" for 9`` () =
        RussianRules.ten 9 |> should equal (Some "девяносто")


    /// === Unit Tests: Hundreds (100, 200...) ===

    [<Fact>]
    let ``hundred returns "сто" for 1`` () =
        RussianRules.hundred 1 |> should equal (Some "сто")

    [<Fact>]
    let ``hundred returns "двести" for 2`` () =
        RussianRules.hundred 2 |> should equal (Some "двести")

    [<Fact>]
    let ``hundred returns "девятьсот" for 9`` () =
        RussianRules.hundred 9 |> should equal (Some "девятьсот")


    /// === Unit Tests: Ranks (thousands, millions...) ===

    [<Fact>]
    let ``ranks returns "тысяча" for rank 1 and value 1`` () =
        RussianRules.ranks 1 1L |> should equal "тысяча"

    [<Fact>]
    let ``ranks returns "тысячи" for rank 1 and value 2`` () =
        RussianRules.ranks 1 2L |> should equal "тысячи"

    [<Fact>]
    let ``ranks returns "тысяч" for rank 1 and value 5`` () =
        RussianRules.ranks 1 5L |> should equal "тысяч"

    [<Fact>]
    let ``ranks returns "миллион" for rank 2 and value 1`` () =
        RussianRules.ranks 2 1L |> should equal "миллион"

    [<Fact>]
    let ``ranks returns "миллиона" for rank 2 and value 2`` () =
        RussianRules.ranks 2 2L |> should equal "миллиона"

    [<Fact>]
    let ``ranks returns "миллионов" for rank 2 and value 5`` () =
        RussianRules.ranks 2 5L |> should equal "миллионов"

    [<Fact>]
    let ``ranks returns "миллиард" for rank 3 and value 1`` () =
        RussianRules.ranks 3 1L |> should equal "миллиард"


    /// === Unit Tests: Currency and Subunit Forms ===

    [<Fact>]
    let ``currency form is "рубль" for 1`` () =
        Declension.form RussianRules.currency 1L |> should equal "рубль"

    [<Fact>]
    let ``currency form is "рубля" for 2`` () =
        Declension.form RussianRules.currency 2L |> should equal "рубля"

    [<Fact>]
    let ``currency form is "рублей" for 5`` () =
        Declension.form RussianRules.currency 5L |> should equal "рублей"

    [<Fact>]
    let ``subunit form is "копейка" for 1`` () =
        Declension.form RussianRules.subunit 1L |> should equal "копейка"

    [<Fact>]
    let ``subunit form is "копейки" for 2`` () =
        Declension.form RussianRules.subunit 2L |> should equal "копейки"

    [<Fact>]
    let ``subunit form is "копеек" for 0`` () =
        Declension.form RussianRules.subunit 0L |> should equal "копеек"


    /// === Unit Test: Conjunction ===

    [<Fact>]
    let ``conjunction is " и "`` () =
        RussianRules.conjunction |> should equal " и "


    /// === Unit Test: zeroSubunit ===

    [<Fact>]
    let ``zeroSubunit is "ноль копеек"`` () =
        RussianRules.zeroSubunit |> should equal "ноль копеек"


    /// === Integration-Style Tests ===

    [<Fact>]
    let ``consistency: hundreds + tens + units (123)`` () =
        let hundreds = RussianRules.hundred 1
        let tens = RussianRules.ten 2
        let units = RussianRules.unitMasc 3

        hundreds |> should equal (Some "сто")
        tens |> should equal (Some "двадцать")
        units |> should equal (Some "три")

    [<Fact>]
    let ``consistency: teen form for 15`` () =
        let teen = RussianRules.teen 15
        teen |> should equal (Some "пятнадцать")
