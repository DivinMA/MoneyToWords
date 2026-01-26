namespace MoneyToWords.Application

open MoneyToWords.Domain

//// <summary>
/// Converts a 64-bit integer into a list of words representing its value in natural language.
/// Designed for functional composition and language extensibility.
/// </summary>
/// <remarks>
/// This module is **language-agnostic** and accepts all linguistic rules as function parameters.
/// Enables consumers to plug in rules for Russian, English, Spanish, or any language with similar number structure.
/// 
/// Handles numbers from 0 to 999 quadrillion (10^15), grouping by thousands:
/// <list type="bullet">
///   <item>Units, thousands, millions, billions, trillions, quadrillions</item>
///   <item>Uses provided functions for units, teens, tens, hundreds, and rank declension</item>
/// </list>
/// 
/// Example output: <c>["сто"; "двадцать"; "три"; "рубля"]</c>
/// 
/// Designed for high performance and composability with <see cref="TextOutput.joinWords" />.
/// </remarks>
[<RequireQualifiedAccess>]
module NumberToWords =

    /// <summary>
    /// Splits a number in range 0–999 into word parts using provided linguistic functions.
    /// </summary>
    /// <param name="units">Function mapping digit (1–9) to its word form (e.g., "один"). Returns <see cref="None" /> if not applicable.</param>
    /// <param name="teen">Function for teens (10–19), e.g., "одиннадцать". Returns <see cref="None" /> if not applicable.</param>
    /// <param name="ten">Function for tens (20, 30...), e.g., "двадцать". Returns <see cref="None" /> if not applicable.</param>
    /// <param name="hundred">Function for hundreds (100, 200...), e.g., "триста". Returns <see cref="None" /> if not applicable.</param>
    /// <param name="n">The number to convert, must be in range [0, 999].</param>
    /// <returns>A list of words representing the number, in correct order.</returns>
    /// <example>
    /// <code>
    /// let parts = parts999 unitMasc teen ten hundred 123
    /// // → ["сто"; "двадцать"; "три"]
    /// </code>
    /// </example>
    let private parts999
        (units: int -> string option)
        (teen: int -> string option)
        (ten: int -> string option)
        (hundred: int -> string option)
        (n: int)
        : string list =

        if n = 0 then []
        else
            let teenVal = n % 100
            let h = n / 100
            let t = teenVal / 10
            let u = teenVal % 10
            [
                if h > 0 then
                    match hundred h with
                    | Some word -> yield word
                    | None -> ()  // Skip if not defined

                if teenVal >= 10 && teenVal <= 19 then
                    match teen teenVal with
                    | Some word -> yield word
                    | None -> ()
                else
                    if t > 0 then
                        match ten t with
                        | Some word -> yield word
                        | None -> ()
                    if u > 0 then
                        match units u with
                        | Some word -> yield word
                        | None -> ()
            ]

    /// <summary>
    /// Processes a 3-digit chunk (0–999) of a large number with correct rank declension.
    /// </summary>
    /// <param name="unitsMasc">Units in masculine form (e.g., "один", "два"). Used for most ranks.</param>
    /// <param name="unitsFem">Units in feminine form (e.g., "одна", "две"). Used for "тысяча".</param>
    /// <param name="teen">Teens (10–19).</param>
    /// <param name="ten">Tens (20, 30...).</param>
    /// <param name="hundred">Hundreds (100, 200...).</param>
    /// <param name="ranks">Function to get rank word (e.g., "тысяча") based on rank index and chunk value.</param>
    /// <param name="chunk">The 3-digit number to process.</param>
    /// <param name="rank">The rank index: 0=units, 1=thousands, 2=millions, etc.</param>
    /// <returns>A list of words representing the chunk and its rank.</returns>
    /// <example>
    /// <code>
    /// processChunk unitMasc unitFem teen ten hundred ranks 1 1 // → ["одна"; "тысяча"]
    /// processChunk unitMasc unitFem teen ten hundred ranks 23 2 // → ["двадцать"; "три"; "миллиона"]
    /// </code>
    /// </example>
    let private processChunk
        (unitsMasc: int -> string option)
        (unitsFem: int -> string option)
        (teen: int -> string option)
        (ten: int -> string option)
        (hundred: int -> string option)
        (ranks: int -> int64 -> string)
        (chunk: int)
        (rank: int)
        : string list =

        if chunk = 0 then
            []
        else
            // Choose units form: feminine for thousands, masculine otherwise
            let units = if rank = 1 then unitsFem else unitsMasc

            // Get base words for the chunk
            let parts = parts999 units teen ten hundred chunk

            // Add rank word if applicable
            if rank > 0 then
                let rankWord = ranks rank (int64 chunk)
                parts @ [rankWord]
            else
                parts

    /// <summary>
    /// Converts a 64-bit integer into a list of words representing its full value.
    /// Supports numbers from 0 to 999 quadrillion (up to 10^15).
    /// </summary>
    /// <param name="unitsMasc">Function for masculine units (1–9): "один", "два", ...</param>
    /// <param name="unitsFem">Function for feminine units (1–9): "одна", "две", ...</param>
    /// <param name="teen">Function for teens (10–19): "десять", "одиннадцать", ...</param>
    /// <param name="ten">Function for tens (20, 30...): "двадцать", "тридцать", ...</param>
    /// <param name="hundred">Function for hundreds (100, 200...): "сто", "двести", ...</param>
    /// <param name="ranks">Function to get the declinable rank word (e.g., "миллион") based on rank and value.</param>
    /// <param name="n">The number to convert. Must be ≥ 0.</param>
    /// <returns>A list of words in correct order, e.g., ["сто"; "двадцать"; "три"; "рубля"].</returns>
    /// <remarks>
    /// For negative numbers, use <c>abs</c> before calling this function.
    /// Returns ["ноль"] if input is 0.
    /// Uses recursion to process each 3-digit chunk.
    /// </remarks>
    /// <example>
    /// <code>
    /// let words = toWordList
    ///                 RussianRules.unitMasc
    ///                 RussianRules.unitFem
    ///                 RussianRules.teen
    ///                 RussianRules.ten
    ///                 RussianRules.hundred
    ///                 RussianRules.ranks
    ///                 123L
    /// // → ["сто"; "двадцать"; "три"]
    /// </code>
    /// </example>
    let toWordList
        (unitsMasc: int -> string option)
        (unitsFem: int -> string option)
        (teen: int -> string option)
        (ten: int -> string option)
        (hundred: int -> string option)
        (ranks: int -> int64 -> string)
        (n: int64)
        : string list =

        if n = 0L then
            ["ноль"]
        else
            let rec loop (rem: int64) (rank: int) (acc: string list) : string list =
                if rem = 0L then
                    acc
                else
                    let chunk = int (rem % 1000L)
                    let next = rem / 1000L
                    let current = processChunk unitsMasc unitsFem teen ten hundred ranks chunk rank
                    loop next (rank + 1) (current @ acc)

            loop (abs n) 0 []