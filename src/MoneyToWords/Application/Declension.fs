namespace MoneyToWords.Application

open MoneyToWords.Domain

/// <summary>
/// Модуль для выбора правильной формы слова по числу.
/// </summary>
[<RequireQualifiedAccess>]
module Declension =

    /// <summary>
    /// Возвращает подходящую форму из <see cref="Form"/> по числу.
    /// </summary>
    /// <param name="form">Формы слова</param>
    /// <param name="n">Число</param>
    /// <returns>Подходящая форма</returns>
    /// <example>
    /// <code>
    /// Declension.form currencyForm 1L // → "рубль"
    /// Declension.form currencyForm 2L // → "рубля"
    /// </code>
    /// </example>
    let form (Form(one, two, many)) (n: int64) =
        let n = abs n
        let lastDigit = n % 10L
        let lastTwo = n % 100L

        if lastTwo >= 11L && lastTwo <= 19L then
            many  // 11–19 → "рублей"
        elif lastDigit = 1L then
            one   // 1, 21, 31 → "рубль"
        elif lastDigit >= 2L && lastDigit <= 4L then
            two   // 2–4, 22–24 → "рубля"
        else
            many  // 5–9, 0, 25+ → "рублей"