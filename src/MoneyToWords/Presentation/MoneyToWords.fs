namespace MoneyToWords.Presentation

open MoneyToWords.Domain
open MoneyToWords.Application
open MoneyToWords.Infrastructure

/// <summary>
/// Primary public API for converting monetary amounts into Russian words.
/// Formats a <see cref="Money" /> value into a natural language string like "сто двадцать три рубля и сорок пять копеек".
/// </summary>
/// <remarks>
/// This module is part of the <b>Presentation</b> layer and uses:
/// <list type="bullet">
///   <item><see cref="RussianRules" /> for linguistic rules</item>
///   <item><see cref="NumberToWords.toWordList" /> for number-to-words conversion</item>
///   <item><see cref="Declension.form" /> for correct noun declension</item>
///   <item><see cref="TextOutput.joinWords" /> and <see cref="TextOutput.withConjunction" /> for efficient string building</item>
/// </list>
/// 
/// By default the output omits zero subunits (e.g., "ноль копеек"). To force inclusion of zero parts, call
/// <c>TextOutput.withConjunctionEx</c> with <c>includeZero = true</c> or compose a custom formatter.
/// 
/// This function is pure, total (for valid input), and designed for reuse in UI, CLI, or API layers.
/// </remarks>
[<RequireQualifiedAccess>]
module MoneyToWords =

    /// <summary>
    /// Converts a <see cref="Money" /> amount into its Russian textual representation.
    /// </summary>
    /// <param name="money">The validated monetary amount to format.</param>
    /// <returns>
    /// A string representing the amount in words, e.g.:
    /// <list type="bullet">
    ///   <item>"один рубль и одна копейка"</item>
    ///   <item>"двести рублей" (по умолчанию опускается "и ноль копеек")</item>
    ///   <item>"ноль рублей и пятнадцать копеек"</item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// let money = Money.TryCreate(123L, 45).Value
    /// let text = MoneyToWords.toWords money
    /// // → "сто двадцать три рубля и сорок пять копеек"
    /// 
    /// let zeroKopecks = Money.TryCreate(1L, 0).Value
    /// let text2 = MoneyToWords.toWords zeroKopecks
    /// // → "один рубль" (ноль копеек опущено по умолчанию)
    /// </code>
    /// </example>
    /// <exception cref="System.ArgumentException">
    /// Thrown if <paramref name="money" /> is not valid (though this should not occur if created via <see cref="Money.TryCreate(int64, int)" />).
    /// Included as a safety guard.
    /// </exception>
    /// <seealso cref="Money.TryCreate(int64, int)" />
    /// <seealso cref="MoneyErrors.toRussian" />
    let toWords (money: Money) : string =
        // === Рубли ===
        let rubleWords = NumberToWords.toWordList
                           RussianRules.unitMasc
                           RussianRules.unitFem
                           RussianRules.teen
                           RussianRules.ten
                           RussianRules.hundred
                           RussianRules.ranks
                           money.Rubles.Value

        let rubleForm = Declension.form RussianRules.currency (money.Rubles.Value)
        let rubleText = TextOutput.joinWords (rubleWords @ [rubleForm])

        // === Копейки ===
        let kopecks = int money.Kopecks.Value
        let kopeckForm = Declension.form RussianRules.subunit (int64 kopecks)

        let kopeckText =
            if kopecks = 0 then
                RussianRules.zeroSubunit
            else
                let teen = kopecks % 100
                let t = teen / 10
                let u = teen % 10
                let words =
                    [
                        if teen >= 10 && teen <= 19 then
                            match RussianRules.teen teen with
                            | Some word -> yield word
                            | None -> ()
                        else
                            if t > 0 then
                                match RussianRules.ten t with
                                | Some word -> yield word
                                | None -> ()
                            if u > 0 then
                                match RussianRules.unitFem u with
                                | Some word -> yield word
                                | None -> ()
                    ]
                let joined = TextOutput.joinWords words
                sprintf "%s %s" joined kopeckForm

        // === Сборка результата ===
        TextOutput.withConjunction RussianRules.conjunction rubleText kopeckText
