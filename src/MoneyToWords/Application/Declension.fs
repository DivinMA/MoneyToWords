namespace MoneyToWords.Application

open MoneyToWords.Domain

/// <summary>
/// Provides functions for selecting the correct grammatical form of a noun based on a number.
/// Used for proper declension in languages like Russian, where word form depends on the number's value.
/// </summary>
/// <remarks>
/// Follows the common pattern:
/// <list type="bullet">
///   <item><description>1 → singular: "один рубль"</description></item>
///   <item><description>2–4 → genitive singular: "два рубля"</description></item>
///   <item><description>0, 5–9, 11–14 → genitive plural: "пять рублей"</description></item>
/// </list>
/// This module is language-agnostic and works with any <see cref="Form" /> definition.
/// Designed for functional composition and reuse across formatting logic.
/// </remarks>
[<RequireQualifiedAccess>]
module Declension =

    /// <summary>
    /// Selects the appropriate word form from a <see cref="Form" /> based on the numeric value.
    /// </summary>
    /// <param name="form">A <see cref="Form" /> tuple defining singular, 2–4, and plural forms.</param>
    /// <param name="n">The number used to determine the correct form.</param>
    /// <returns>
    /// The correct form as a <see cref="string" />, chosen by:
    /// <list type="number">
    ///   <item>If <c>n % 100</c> is between 11 and 19 → use <b>many</b> form (e.g., "рублей").</item>
    ///   <item>Else if <c>n % 10 = 1</c> → use <b>one</b> form (e.g., "рубль").</item>
    ///   <item>Else if <c>n % 10</c> is 2, 3, or 4 → use <b>twoFour</b> form (e.g., "рубля").</item>
    ///   <item>Otherwise → use <b>many</b> form.</item>
    /// </list>
    /// Negative numbers are treated as positive.
    /// </returns>
    /// <example>
    /// <code>
    /// let currency = Form("рубль", "рубля", "рублей")
    /// Declension.form currency 1L // → "рубль"
    /// Declension.form currency 2L // → "рубля"
    /// Declension.form currency 5L // → "рублей"
    /// Declension.form currency 21L // → "рубль"
    /// Declension.form currency 112L // → "рублей"
    /// </code>
    /// </example>
    /// <seealso cref="Form" />
    let form (Form(one, twoFour, many)) (n: int64) =
        let n = abs n
        let lastDigit = n % 10L
        let lastTwo = n % 100L

        if lastTwo >= 11L && lastTwo <= 19L then
            many
        elif lastDigit = 1L then
            one
        elif lastDigit >= 2L && lastDigit <= 4L then
            twoFour
        else
            many
