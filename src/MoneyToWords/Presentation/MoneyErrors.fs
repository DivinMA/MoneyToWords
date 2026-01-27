namespace MoneyToWords.Presentation

open MoneyToWords.Domain

/// <summary>
/// Formats <see cref="MoneyError" /> instances into user-friendly Russian messages.
/// Provides localized error presentation for end users.
/// </summary>
/// <remarks>
/// This module belongs to the <b>Presentation</b> layer and should not be used in domain logic.
/// 
/// Responsibilities:
/// <list type="bullet">
///   <item>Translates machine-readable <see cref="MoneyError" /> cases into natural language.</item>
///   <item>Provides context-rich messages with actual values (e.g., invalid kopecks).</item>
///   <item>Ensures clarity and correctness in Russian grammar and punctuation.</item>
/// </list>
/// 
/// Unlike <see cref="MoneyError.Description" />, which is in English and intended for logs,
/// this module is designed for display in UI, CLI, or API responses to Russian-speaking users.
/// </remarks>
[<RequireQualifiedAccess>]
module MoneyErrors =

    /// <summary>
    /// Converts a <see cref="MoneyError" /> into a human-readable message in Russian.
    /// </summary>
    /// <param name="error">The domain error to format.</param>
    /// <returns>A localized string suitable for end users.</returns>
    /// <example>
    /// <code>
    /// let msg = MoneyErrors.toRussian MoneyError.NegativeRubles
    /// // → "Сумма не может быть отрицательной"
    /// 
    /// let msg2 = MoneyErrors.toRussian (MoneyError.InvalidKopecks(-5))
    /// // → "Копейки не могут быть отрицательными. Указано: -5"
    /// </code>
    /// </example>
    /// <seealso cref="MoneyError" />
    let toRussian = function
        | MoneyError.NegativeRubles ->
            "Сумма не может быть отрицательной"

        | MoneyError.RublesTooLarge value ->
            sprintf "Значение рублей %d слишком велико. Максимум — 999 квадриллионов 999 триллионов 999 миллиардов 999 миллионов 999 тысяч 999." value

        | MoneyError.InvalidKopecks actual when actual < 0 ->
            sprintf "Копейки не могут быть отрицательными. Указано: %d" actual

        | MoneyError.InvalidKopecks actual when actual > 99 ->
            sprintf "Копейки не могут быть больше 99. Указано: %d" actual

        | MoneyError.InvalidKopecks _ ->
            "Некорректное значение копеек"

        | MoneyError.CompositionError reason ->
            sprintf "Ошибка составления денежной суммы: %s" reason
