namespace MoneyToWords.Tests

open Swensen.Unquote
open Xunit
open MoneyToWords.Infrastructure
open MoneyToWords.Domain

module RussianRulesTests =

    /// Получаем глобальные правила (единственный экземпляр)
    let rules = RussianRules.Value

    /// === Тесты для UnitsMasc (мужской род: рубль, тысяча, миллион...) ===

    [<Fact>]
    let ``UnitsMasc: 1 → один`` () =
        test <@ rules.UnitsMasc.[1] = "один" @>

    [<Fact>]
    let ``UnitsMasc: 2 → два`` () =
        test <@ rules.UnitsMasc.[2] = "два" @>

    [<Fact>]
    let ``UnitsMasc: 3 → три`` () =
        test <@ rules.UnitsMasc.[3] = "три" @>

    [<Fact>]
    let ``UnitsMasc: 4 → четыре`` () =
        test <@ rules.UnitsMasc.[4] = "четыре" @>

    [<Fact>]
    let ``UnitsMasc: 5 → пять`` () =
        test <@ rules.UnitsMasc.[5] = "пять" @>

    [<Fact>]
    let ``UnitsMasc: 6 → шесть`` () =
        test <@ rules.UnitsMasc.[6] = "шесть" @>

    [<Fact>]
    let ``UnitsMasc: 7 → семь`` () =
        test <@ rules.UnitsMasc.[7] = "семь" @>

    [<Fact>]
    let ``UnitsMasc: 8 → восемь`` () =
        test <@ rules.UnitsMasc.[8] = "восемь" @>

    [<Fact>]
    let ``UnitsMasc: 9 → девять`` () =
        test <@ rules.UnitsMasc.[9] = "девять" @>

    /// === Тесты для UnitsFem (женский род: копейка, тысяча...) ===

    [<Fact>]
    let ``UnitsFem: 1 → одна`` () =
        test <@ rules.UnitsFem.[1] = "одна" @>

    [<Fact>]
    let ``UnitsFem: 2 → две`` () =
        test <@ rules.UnitsFem.[2] = "две" @>

    /// === Тесты для Teens (11–19) ===

    [<Fact>]
    let ``Teens: 1 → одиннадцать`` () =
        test <@ rules.Teens.[1] = "одиннадцать" @>

    [<Fact>]
    let ``Teens: 2 → двенадцать`` () =
        test <@ rules.Teens.[2] = "двенадцать" @>

    [<Fact>]
    let ``Teens: 3 → тринадцать`` () =
        test <@ rules.Teens.[3] = "тринадцать" @>

    [<Fact>]
    let ``Teens: 4 → четырнадцать`` () =
        test <@ rules.Teens.[4] = "четырнадцать" @>

    [<Fact>]
    let ``Teens: 5 → пятнадцать`` () =
        test <@ rules.Teens.[5] = "пятнадцать" @>

    [<Fact>]
    let ``Teens: 6 → шестнадцать`` () =
        test <@ rules.Teens.[6] = "шестнадцать" @>

    [<Fact>]
    let ``Teens: 7 → семнадцать`` () =
        test <@ rules.Teens.[7] = "семнадцать" @>

    [<Fact>]
    let ``Teens: 8 → восемнадцать`` () =
        test <@ rules.Teens.[8] = "восемнадцать" @>

    [<Fact>]
    let ``Teens: 9 → девятнадцать`` () =
        test <@ rules.Teens.[9] = "девятнадцать" @>

    /// === Тесты для Tens (десятки) ===

    [<Fact>]
    let ``Tens: 2 → двадцать`` () =
        test <@ rules.Tens.[2] = "двадцать" @>

    [<Fact>]
    let ``Tens: 3 → тридцать`` () =
        test <@ rules.Tens.[3] = "тридцать" @>

    [<Fact>]
    let ``Tens: 4 → сорок`` () =
        test <@ rules.Tens.[4] = "сорок" @>

    [<Fact>]
    let ``Tens: 5 → пятьдесят`` () =
        test <@ rules.Tens.[5] = "пятьдесят" @>

    [<Fact>]
    let ``Tens: 6 → шестьдесят`` () =
        test <@ rules.Tens.[6] = "шестьдесят" @>

    [<Fact>]
    let ``Tens: 7 → семьдесят`` () =
        test <@ rules.Tens.[7] = "семьдесят" @>

    [<Fact>]
    let ``Tens: 8 → восемьдесят`` () =
        test <@ rules.Tens.[8] = "восемьдесят" @>

    [<Fact>]
    let ``Tens: 9 → девяносто`` () =
        test <@ rules.Tens.[9] = "девяносто" @>

    /// === Тесты для Hundreds (сотни) ===

    [<Fact>]
    let ``Hundreds: 1 → сто`` () =
        test <@ rules.Hundreds.[1] = "сто" @>

    [<Fact>]
    let ``Hundreds: 2 → двести`` () =
        test <@ rules.Hundreds.[2] = "двести" @>

    [<Fact>]
    let ``Hundreds: 3 → триста`` () =
        test <@ rules.Hundreds.[3] = "триста" @>

    [<Fact>]
    let ``Hundreds: 4 → четыреста`` () =
        test <@ rules.Hundreds.[4] = "четыреста" @>

    [<Fact>]
    let ``Hundreds: 5 → пятьсот`` () =
        test <@ rules.Hundreds.[5] = "пятьсот" @>

    [<Fact>]
    let ``Hundreds: 6 → шестьсот`` () =
        test <@ rules.Hundreds.[6] = "шестьсот" @>

    [<Fact>]
    let ``Hundreds: 7 → семьсот`` () =
        test <@ rules.Hundreds.[7] = "семьсот" @>

    [<Fact>]
    let ``Hundreds: 8 → восемьсот`` () =
        test <@ rules.Hundreds.[8] = "восемьсот" @>

    [<Fact>]
    let ``Hundreds: 9 → девятьсот`` () =
        test <@ rules.Hundreds.[9] = "девятьсот" @>

    /// === Тесты для Ranks (разряды: тысяч, миллионов, миллиардов) ===

    [<Fact>]
    let ``Ranks[0]: тысяча, тысячи, тысяч`` () =
        let (Form(one, two, many)) = rules.Ranks.[0]
        test <@ one = "тысяча" @>
        test <@ two = "тысячи" @>
        test <@ many = "тысяч" @>

    [<Fact>]
    let ``Ranks[1]: миллион, миллиона, миллионов`` () =
        let (Form(one, two, many)) = rules.Ranks.[1]
        test <@ one = "миллион" @>
        test <@ two = "миллиона" @>
        test <@ many = "миллионов" @>

    [<Fact>]
    let ``Ranks[2]: миллиард, миллиарда, миллиардов`` () =
        let (Form(one, two, many)) = rules.Ranks.[2]
        test <@ one = "миллиард" @>
        test <@ two = "миллиарда" @>
        test <@ many = "миллиардов" @>

    [<Fact>]
    let ``Ranks[3]: триллион, триллиона, триллионов`` () =
        let (Form(one, two, many)) = rules.Ranks.[3]
        test <@ one = "триллион" @>
        test <@ two = "триллиона" @>
        test <@ many = "триллионов" @>

    /// === Тесты для Currency (рубль/рубля/рублей) ===

    [<Fact>]
    let ``Currency: рубль, рубля, рублей`` () =
        let (Form(one, two, many)) = rules.Currency
        test <@ one = "рубль" @>
        test <@ two = "рубля" @>
        test <@ many = "рублей" @>

    /// === Тесты для Subunit (копейка/копейки/копеек) ===

    [<Fact>]
    let ``Subunit: копейка, копейки, копеек`` () =
        let (Form(one, two, many)) = rules.Subunit
        test <@ one = "копейка" @>
        test <@ two = "копейки" @>
        test <@ many = "копеек" @>

    /// === Тесты для Conjunction (соединительная частица) ===

    [<Fact>]
    let ``Conjunction: и (с пробелами)`` () =
        test <@ rules.Conjunction = " и " @>

    /// === Проверка, что все Ranks до 3 (включительно) определены ===

    [<Fact>]
    let ``Ranks length ≥ 4`` () =
        test <@ Array.length rules.Ranks >= 4 @>

    /// === Примеры составных проверок для убедительности ===

    [<Fact>]
    let ``Rules consistency: единицы + десятки + сотни (пример: 123)`` () =
        let h = rules.Hundreds.[1]  // "сто"
        let t = rules.Tens.[2]      // "двадцать"
        let u = rules.UnitsMasc.[3] // "три"
        test <@ h = "сто" && t = "двадцать" && u = "три" @>

    [<Fact>]
    let ``Rules consistency: teens (пример: 15)`` () =
        let teen = rules.Teens.[5]  // "пятнадцать"
        test <@ teen = "пятнадцать" @>