namespace MoneyToWords.Tests

open Swensen.Unquote
open Xunit
open MoneyToWords.Application
open MoneyToWords.Domain

module DeclensionTests =

    /// Формы для рублей
    let currency = Form("рубль", "рубля", "рублей")

    /// Формы для копеек
    let subunit = Form("копейка", "копейки", "копеек")

    /// === Тесты для рублей ===

    /// --- Числа 1–20 ---

    [<Fact>]
    let ``Rubles: 1 → рубль`` () =
        test <@ Declension.form currency 1L = "рубль" @>

    [<Fact>]
    let ``Rubles: 2 → рубля`` () =
        test <@ Declension.form currency 2L = "рубля" @>

    [<Fact>]
    let ``Rubles: 3 → рубля`` () =
        test <@ Declension.form currency 3L = "рубля" @>

    [<Fact>]
    let ``Rubles: 4 → рубля`` () =
        test <@ Declension.form currency 4L = "рубля" @>

    [<Fact>]
    let ``Rubles: 5 → рублей`` () =
        test <@ Declension.form currency 5L = "рублей" @>

    [<Fact>]
    let ``Rubles: 6 → рублей`` () =
        test <@ Declension.form currency 6L = "рублей" @>

    [<Fact>]
    let ``Rubles: 7 → рублей`` () =
        test <@ Declension.form currency 7L = "рублей" @>

    [<Fact>]
    let ``Rubles: 8 → рублей`` () =
        test <@ Declension.form currency 8L = "рублей" @>

    [<Fact>]
    let ``Rubles: 9 → рублей`` () =
        test <@ Declension.form currency 9L = "рублей" @>

    [<Fact>]
    let ``Rubles: 10 → рублей`` () =
        test <@ Declension.form currency 10L = "рублей" @>

    [<Fact>]
    let ``Rubles: 11 → рублей (исключение: 11–14 всегда «рублей»)`` () =
        test <@ Declension.form currency 11L = "рублей" @>

    [<Fact>]
    let ``Rubles: 12 → рублей`` () =
        test <@ Declension.form currency 12L = "рублей" @>

    [<Fact>]
    let ``Rubles: 13 → рублей`` () =
        test <@ Declension.form currency 13L = "рублей" @>

    [<Fact>]
    let ``Rubles: 14 → рублей`` () =
        test <@ Declension.form currency 14L = "рублей" @>

    [<Fact>]
    let ``Rubles: 15 → рублей`` () =
        test <@ Declension.form currency 15L = "рублей" @>

    [<Fact>]
    let ``Rubles: 16 → рублей`` () =
        test <@ Declension.form currency 16L = "рублей" @>

    [<Fact>]
    let ``Rubles: 17 → рублей`` () =
        test <@ Declension.form currency 17L = "рублей" @>

    [<Fact>]
    let ``Rubles: 18 → рублей`` () =
        test <@ Declension.form currency 18L = "рублей" @>

    [<Fact>]
    let ``Rubles: 19 → рублей`` () =
        test <@ Declension.form currency 19L = "рублей" @>

    [<Fact>]
    let ``Rubles: 20 → рублей`` () =
        test <@ Declension.form currency 20L = "рублей" @>

    /// --- Десятки: 21–29 ---

    [<Fact>]
    let ``Rubles: 21 → рубль (окончание 1, не 11–14)`` () =
        test <@ Declension.form currency 21L = "рубль" @>

    [<Fact>]
    let ``Rubles: 22 → рубля`` () =
        test <@ Declension.form currency 22L = "рубля" @>

    [<Fact>]
    let ``Rubles: 23 → рубля`` () =
        test <@ Declension.form currency 23L = "рубля" @>

    [<Fact>]
    let ``Rubles: 24 → рубля`` () =
        test <@ Declension.form currency 24L = "рубля" @>

    [<Fact>]
    let ``Rubles: 25 → рублей`` () =
        test <@ Declension.form currency 25L = "рублей" @>

    [<Fact>]
    let ``Rubles: 26 → рублей`` () =
        test <@ Declension.form currency 26L = "рублей" @>

    [<Fact>]
    let ``Rubles: 29 → рублей`` () =
        test <@ Declension.form currency 29L = "рублей" @>

    /// --- 30, 40, 50, ..., 90 ---

    [<Fact>]
    let ``Rubles: 30 → рублей`` () =
        test <@ Declension.form currency 30L = "рублей" @>

    [<Fact>]
    let ``Rubles: 40 → рублей`` () =
        test <@ Declension.form currency 40L = "рублей" @>

    [<Fact>]
    let ``Rubles: 50 → рублей`` () =
        test <@ Declension.form currency 50L = "рублей" @>

    [<Fact>]
    let ``Rubles: 60 → рублей`` () =
        test <@ Declension.form currency 60L = "рублей" @>

    [<Fact>]
    let ``Rubles: 70 → рублей`` () =
        test <@ Declension.form currency 70L = "рублей" @>

    [<Fact>]
    let ``Rubles: 80 → рублей`` () =
        test <@ Declension.form currency 80L = "рублей" @>

    [<Fact>]
    let ``Rubles: 90 → рублей`` () =
        test <@ Declension.form currency 90L = "рублей" @>

    /// --- Сотни ---

    [<Fact>]
    let ``Rubles: 100 → рублей`` () =
        test <@ Declension.form currency 100L = "рублей" @>

    [<Fact>]
    let ``Rubles: 101 → рубль`` () =
        test <@ Declension.form currency 101L = "рубль" @>

    [<Fact>]
    let ``Rubles: 102 → рубля`` () =
        test <@ Declension.form currency 102L = "рубля" @>

    [<Fact>]
    let ``Rubles: 104 → рубля`` () =
        test <@ Declension.form currency 104L = "рубля" @>

    [<Fact>]
    let ``Rubles: 105 → рублей`` () =
        test <@ Declension.form currency 105L = "рублей" @>

    [<Fact>]
    let ``Rubles: 111 → рублей (исключение: 111 оканчивается на 11)`` () =
        test <@ Declension.form currency 111L = "рублей" @>

    [<Fact>]
    let ``Rubles: 112 → рублей`` () =
        test <@ Declension.form currency 112L = "рублей" @>

    [<Fact>]
    let ``Rubles: 121 → рубль`` () =
        test <@ Declension.form currency 121L = "рубль" @>

    [<Fact>]
    let ``Rubles: 122 → рубля`` () =
        test <@ Declension.form currency 122L = "рубля" @>

    [<Fact>]
    let ``Rubles: 125 → рублей`` () =
        test <@ Declension.form currency 125L = "рублей" @>

    [<Fact>]
    let ``Rubles: 200 → рублей`` () =
        test <@ Declension.form currency 200L = "рублей" @>

    [<Fact>]
    let ``Rubles: 201 → рубль`` () =
        test <@ Declension.form currency 201L = "рубль" @>

    [<Fact>]
    let ``Rubles: 999 → рублей`` () =
        test <@ Declension.form currency 999L = "рублей" @>

    /// === Тесты для копеек ===

    /// --- Kopecks: 1–20 ---

    [<Fact>]
    let ``Kopecks: 1 → копейка`` () =
        test <@ Declension.form subunit 1L = "копейка" @>

    [<Fact>]
    let ``Kopecks: 2 → копейки`` () =
        test <@ Declension.form subunit 2L = "копейки" @>

    [<Fact>]
    let ``Kopecks: 3 → копейки`` () =
        test <@ Declension.form subunit 3L = "копейки" @>

    [<Fact>]
    let ``Kopecks: 4 → копейки`` () =
        test <@ Declension.form subunit 4L = "копейки" @>

    [<Fact>]
    let ``Kopecks: 5 → копеек`` () =
        test <@ Declension.form subunit 5L = "копеек" @>

    [<Fact>]
    let ``Kopecks: 6 → копеек`` () =
        test <@ Declension.form subunit 6L = "копеек" @>

    [<Fact>]
    let ``Kopecks: 10 → копеек`` () =
        test <@ Declension.form subunit 10L = "копеек" @>

    [<Fact>]
    let ``Kopecks: 11 → копеек (исключение)`` () =
        test <@ Declension.form subunit 11L = "копеек" @>

    [<Fact>]
    let ``Kopecks: 12 → копеек`` () =
        test <@ Declension.form subunit 12L = "копеек" @>

    [<Fact>]
    let ``Kopecks: 14 → копеек`` () =
        test <@ Declension.form subunit 14L = "копеек" @>

    [<Fact>]
    let ``Kopecks: 15 → копеек`` () =
        test <@ Declension.form subunit 15L = "копеек" @>

    [<Fact>]
    let ``Kopecks: 20 → копеек`` () =
        test <@ Declension.form subunit 20L = "копеек" @>

    /// --- Kopecks: 21–29 ---

    [<Fact>]
    let ``Kopecks: 21 → копейка`` () =
        test <@ Declension.form subunit 21L = "копейка" @>

    [<Fact>]
    let ``Kopecks: 22 → копейки`` () =
        test <@ Declension.form subunit 22L = "копейки" @>

    [<Fact>]
    let ``Kopecks: 25 → копеек`` () =
        test <@ Declension.form subunit 25L = "копеек" @>

    [<Fact>]
    let ``Kopecks: 29 → копеек`` () =
        test <@ Declension.form subunit 29L = "копеек" @>

    /// --- Kopecks: 30–99 ---

    [<Fact>]
    let ``Kopecks: 30 → копеек`` () =
        test <@ Declension.form subunit 30L = "копеек" @>

    [<Fact>]
    let ``Kopecks: 40 → копеек`` () =
        test <@ Declension.form subunit 40L = "копеек" @>

    [<Fact>]
    let ``Kopecks: 50 → копеек`` () =
        test <@ Declension.form subunit 50L = "копеек" @>

    [<Fact>]
    let ``Kopecks: 90 → копеек`` () =
        test <@ Declension.form subunit 90L = "копеек" @>

    [<Fact>]
    let ``Kopecks: 91 → копейка`` () =
        test <@ Declension.form subunit 91L = "копейка" @>

    [<Fact>]
    let ``Kopecks: 92 → копейки`` () =
        test <@ Declension.form subunit 92L = "копейки" @>

    [<Fact>]
    let ``Kopecks: 95 → копеек`` () =
        test <@ Declension.form subunit 95L = "копеек" @>

    [<Fact>]
    let ``Kopecks: 99 → копеек`` () =
        test <@ Declension.form subunit 99L = "копеек" @>

    /// --- Kopecks: 100+ (для полноты) ---

    [<Fact>]
    let ``Kopecks: 100 → копеек`` () =
        test <@ Declension.form subunit 100L = "копеек" @>

    [<Fact>]
    let ``Kopecks: 101 → копейка`` () =
        test <@ Declension.form subunit 101L = "копейка" @>

    [<Fact>]
    let ``Kopecks: 121 → копейка`` () =
        test <@ Declension.form subunit 121L = "копейка" @>

    [<Fact>]
    let ``Kopecks: 122 → копейки`` () =
        test <@ Declension.form subunit 122L = "копейки" @>

    [<Fact>]
    let ``Kopecks: 125 → копеек`` () =
        test <@ Declension.form subunit 125L = "копеек" @>