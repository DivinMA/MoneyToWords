namespace MoneyToWords.Infrastructure

open MoneyToWords.Domain
open MoneyToWords.Application

/// <summary>
/// Provides linguistic rules for rendering numbers in Russian.
/// Implements functions for units, teens, tens, hundreds, and rank declension.
/// </summary>
/// <remarks>
/// Designed to be used as input to <see cref="NumberToWords.toWordList" />.
/// All functions are pure, total for valid inputs, and optimized for readability and performance.
/// 
/// Key features:
/// <list type="bullet">
///   <item>Support for grammatical gender: masculine ("один") and feminine ("одна") for correct "тысяча" handling.</item>
///   <item>Proper declension of ranks: "миллион", "миллиона", "миллионов".</item>
///   <item>Includes <see cref="Form" /> definitions for currency and subunits.</item>
/// </list>
/// 
/// This module is a ready-to-use implementation for Russian.
/// Consumers can define similar modules for other languages (e.g., <c>SpanishRules</c>).
/// </remarks>
[<RequireQualifiedAccess>]
module RussianRules =

    /// <summary>
    /// Gets the masculine form of a unit (1–9), used for most ranks.
    /// </summary>
    /// <param name="n">Digit from 1 to 9.</param>
    /// <returns>Word like "один", "два", "три", or <see langword="None" /> if out of range.</returns>
    /// <example><c>unitMasc 1</c> → <c>Some "один"</c></example>
    let inline unitMasc (n: int) : string option =
        match n with
        | 1 -> Some "один"
        | 2 -> Some "два"
        | 3 -> Some "три"
        | 4 -> Some "четыре"
        | 5 -> Some "пять"
        | 6 -> Some "шесть"
        | 7 -> Some "семь"
        | 8 -> Some "восемь"
        | 9 -> Some "девять"
        | _ -> None

    /// <summary>
    /// Gets the feminine form of a unit (1–9), used for "тысяча".
    /// </summary>
    /// <param name="n">Digit from 1 to 9.</param>
    /// <returns>Word like "одна", "две", or masculine form for 3+.</returns>
    /// <example><c>unitFem 1</c> → <c>Some "одна"</c>, <c>unitFem 3</c> → <c>Some "три"</c></example>
    let inline unitFem (n: int) : string option =
        match n with
        | 1 -> Some "одна"
        | 2 -> Some "две"
        | n -> unitMasc n  // 3–9 use masculine

    /// <summary>
    /// Gets the word for a teen number (10–19).
    /// </summary>
    /// <param name="n">Number from 10 to 19.</param>
    /// <returns>Word like "десять", "одиннадцать", or <see langword="None" /> if out of range.</returns>
    /// <example><c>teen 11</c> → <c>Some "одиннадцать"</c></example>
    let inline teen (n: int) : string option =
        let teens = [| "десять"; "одиннадцать"; "двенадцать"; "тринадцать"; "четырнадцать";
                       "пятнадцать"; "шестнадцать"; "семнадцать"; "восемнадцать"; "девятнадцать" |]
        if n >= 10 && n <= 19 then Array.tryItem (n - 10) teens
        else None

    /// <summary>
    /// Gets the word for a ten (20, 30, ..., 90).
    /// </summary>
    /// <param name="n">Tens digit (2–9).</param>
    /// <returns>Word like "двадцать", "тридцать", or <see langword="None" /> if out of range.</returns>
    /// <example><c>ten 3</c> → <c>Some "тридцать"</c></example>
    let inline ten (n: int) : string option =
        let tens = [| ""; ""; "двадцать"; "тридцать"; "сорок"; "пятьдесят";
                     "шестьдесят"; "семьдесят"; "восемьдесят"; "девяносто" |]
        Array.tryItem n tens

    /// <summary>
    /// Gets the word for a hundred (100, 200, ..., 900).
    /// </summary>
    /// <param name="n">Hundreds digit (1–9).</param>
    /// <returns>Word like "сто", "двести", or <see langword="None" /> if out of range.</returns>
    /// <example><c>hundred 3</c> → <c>Some "триста"</c></example>
    let inline hundred (n: int) : string option =
        let hundreds = [| ""; "сто"; "двести"; "триста"; "четыреста"; "пятьсот";
                         "шестьсот"; "семьсот"; "восемьсот"; "девятьсот" |]
        Array.tryItem n hundreds

    /// <summary>
    /// Gets the correctly declined form of a rank (thousands, millions, etc.) based on value.
    /// </summary>
    /// <param name="rank">Rank index: 1 = тысячи, 2 = миллионы, etc.</param>
    /// <param name="value">The numeric value of the chunk (used for declension).</param>
    /// <returns>The correct form of the rank word, e.g., "миллион", "миллиона", "миллионов".</returns>
    /// <example>
    /// <code>
    /// ranks 2 1L // → "миллион"
    /// ranks 2 2L // → "миллиона"
    /// ranks 2 5L // → "миллионов"
    /// </code>
    /// </example>
    /// <seealso cref="Declension.form" />
    let ranks (rank: int) (value: int64) : string =
        let formForRank =
            match rank with
            | 1 -> Form("тысяча", "тысячи", "тысяч")
            | 2 -> Form("миллион", "миллиона", "миллионов")
            | 3 -> Form("миллиард", "миллиарда", "миллиардов")
            | 4 -> Form("триллион", "триллиона", "триллионов")
            | 5 -> Form("квадриллион", "квадриллиона", "квадриллионов")
            | _ -> Form("", "", "")  // fallback (should not occur)

        Declension.form formForRank value

    /// <summary>
    /// Form for Russian currency: "рубль", "рубля", "рублей".
    /// </summary>
    let currency: Form = Form("рубль", "рубля", "рублей")

    /// <summary>
    /// Form for Russian subunit: "копейка", "копейки", "копеек".
    /// </summary>
    let subunit: Form = Form("копейка", "копейки", "копеек")

    /// <summary>
    /// Conjunction used between rubles and kopecks: " и ".
    /// </summary>
    let conjunction: string = " и "

    /// <summary>
    /// Full phrase for zero kopecks: "ноль копеек".
    /// Used when kopecks are zero and must be explicitly included.
    /// </summary>
    let zeroSubunit: string = "ноль копеек"
