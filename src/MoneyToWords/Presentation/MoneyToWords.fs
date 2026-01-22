// Presentation/MoneyToWords.fs

namespace MoneyToWords.Presentation

open MoneyToWords.Domain
open MoneyToWords.Application
open MoneyToWords.Infrastructure

/// <summary>
/// Основной API библиотеки.
/// </summary>
[<RequireQualifiedAccess>]
module MoneyToWords =

    /// <summary>
    /// Преобразует денежную сумму в строку прописью на русском языке.
    /// Всегда включает " и ноль копеек".
    /// </summary>
    /// <param name="money">Денежная сумма</param>
    /// <returns>Строка: "один рубль и одна копейка"</returns>
    /// <exception cref="System.ArgumentException">Если сумма недопустима</exception>
    /// <example>
    /// <code>
    /// let money = Money.TryCreate(123L, 45).Value
    /// let text = MoneyToWords.toWords money
    /// // → "сто двадцать три рубля и сорок пять копеек"
    /// </code>
    /// </example>
    let toWords (money: Money) : string =
        let rules = RussianRules.Value

        // === Рубли ===
        let rubleWords = NumberToWords.toWordList rules (money.Rubles.Value)
        let rubleForm = Declension.form rules.Currency (money.Rubles.Value)
        let rubleText = TextOutput.joinWords (rubleWords @ [rubleForm])

        // === Копейки — обрабатываем отдельно с женским родом ===
        let kopecks = int money.Kopecks.Value
        let kopeckForm = Declension.form rules.Subunit (int64 kopecks)

        let kopeckText =
            if kopecks = 0 then "ноль копеек"
            else
                let teen = kopecks % 100
                let t = teen / 10
                let u = teen % 10
                let words =
                    [
                        if teen >= 10 && teen <= 19 then
                            yield rules.Teens.[teen - 10]
                        else
                            if t > 0 then yield rules.Tens.[t]
                            if u > 0 then yield rules.UnitsFem.[u]  // ✅ UnitsFem — "одна", "две"
                    ]
                let joined = TextOutput.joinWords words
                sprintf "%s %s" joined kopeckForm  // → "одна копейка", "две копейки"

        // === Сборка результата ===
        TextOutput.withConjunction rules.Conjunction rubleText kopeckText
