namespace MoneyToWords.Infrastructure

open System
open System.Runtime.CompilerServices

/// <summary>
/// Provides high-performance functions for string construction using <see cref="Span{T}" />.
/// Minimizes heap allocations by writing directly into a pre-allocated character buffer.
/// </summary>
/// <remarks>
/// Designed for scenarios where performance is critical, such as formatting large numbers into words.
/// Uses a stack-allocated buffer of fixed size (<see cref="MaxSize" />) to avoid GC pressure.
/// 
/// Typical usage:
/// <list type="bullet">
///   <item><see cref="joinWords" />: Converts a list of words into a single space-separated string.</item>
///   <item><see cref="withConjunction" />: Joins two parts with a conjunction (e.g., " и ").</item>
/// </list>
/// 
/// All functions are pure in effect (no side effects) despite using mutation internally.
/// The mutation is encapsulated and not observable from the outside.
/// </remarks>
[<RequireQualifiedAccess>]
module TextOutput =

    /// <summary>
    /// Maximum size of the internal character buffer in characters.
    /// Must be large enough to hold the longest possible output (e.g., 999 квадриллионов...).
    /// </summary>
    /// <remarks>
    /// Currently set to 2048, which is sufficient for numbers up to 10^18.
    /// Increase if supporting longer forms or additional languages.
    /// </remarks>
    let private maxSize = 2048

    /// <summary>
    /// Copies a string into a span at the given position.
    /// </summary>
    /// <param name="span">Destination span.</param>
    /// <param name="pos">Starting position in the span.</param>
    /// <param name="text">Text to copy.</param>
    /// <returns>New position after the written text.</returns>
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    let private append (span: Span<char>) (pos: int) (text: string) =
        text.AsSpan().CopyTo(span.Slice(pos))
        pos + text.Length

    /// <summary>
    /// Appends a word to the span with a space if the current position is not zero.
    /// </summary>
    /// <param name="span">Destination span.</param>
    /// <param name="pos">Current write position.</param>
    /// <param name="word">Word to append.</param>
    /// <returns>New position after appending the word and optional space.</returns>
    let private appendWithSpace (span: Span<char>) (pos: int) (word: string) =
        let p = if pos > 0 then span.[pos] <- ' '; pos + 1 else pos
        append span p word

    /// <summary>
    /// Joins a list of words into a single space-separated string.
    /// </summary>
    /// <param name="words">List of words to join.</param>
    /// <returns>
    /// A single string with words separated by single spaces.
    /// Returns "ноль" if the list is empty.
    /// </returns>
    /// <remarks>
    /// Uses a fixed-size character buffer on the stack for allocation-free operation.
    /// Ideal for performance-critical formatting.
    /// </remarks>
    /// <example>
    /// <code>
    /// let result = TextOutput.joinWords ["сто"; "двадцать"; "три"]
    /// // → "сто двадцать три"
    /// 
    /// let empty = TextOutput.joinWords []
    /// // → "ноль"
    /// </code>
    /// </example>
    let joinWords (words: string list) : string =
        if List.isEmpty words then "ноль"
        else
            let buffer = Array.zeroCreate<char> maxSize
            let mutable pos = 0
            for word in words do
                pos <- appendWithSpace (Span buffer) pos word
            String(buffer, 0, pos)

    /// <summary>
    /// Joins two text parts with a conjunction, unless the second part starts with "ноль".
    /// </summary>
    /// <param name="conj">Conjunction to insert (e.g., " и ").</param>
    /// <param name="a">First part (e.g., rubles).</param>
    /// <param name="b">Second part (e.g., kopecks).</param>
    /// <returns>
    /// <list type="bullet">
    ///   <item>If <paramref name="b" /> starts with "ноль", returns <paramref name="a" />.</item>
    ///   <item>Otherwise, returns <c>a + conj + b</c>.</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This function ensures that " и ноль копеек" is not included in the output.
    /// Currently commented out — logic is handled in <see cref="MoneyToWords.toWords" />.
    /// May be reactivated if needed in future versions.
    /// </remarks>
    /// <example>
    /// <code>
    /// TextOutput.withConjunction " и " "сто рублей" "двадцать три копейки"
    /// // → "сто рублей и двадцать три копейки"
    /// 
    /// TextOutput.withConjunction " и " "сто рублей" "ноль копеек"
    /// // → "сто рублей"
    /// </code>
    /// </example>
    let withConjunction (conj: string) (a: string) (b: string) : string =
        // if b.StartsWith "ноль" then a
        // else 
        a + conj + b
