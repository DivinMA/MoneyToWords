namespace MoneyToWords.Infrastructure

open System
open System.Runtime.CompilerServices

/// <summary>
/// Эффективный вывод строк через <see cref="Span{T}"/>.
/// </summary>
[<RequireQualifiedAccess>]
module TextOutput =

    /// <summary>
    /// Максимальный размер буфера (в символах).
    /// </summary>
    let private MaxSize = 2048

    /// <summary>
    /// Присоединяет строку к буферу.
    /// </summary>
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    let private append (span: Span<char>) (pos: int) (text: string) =
        text.AsSpan().CopyTo(span.Slice(pos))
        pos + text.Length

    /// <summary>
    /// Присоединяет слово с пробелом, если позиция > 0.
    /// </summary>
    let private appendWithSpace (span: Span<char>) (pos: int) (word: string) =
        let p = if pos > 0 then span.[pos] <- ' '; pos + 1 else pos
        append span p word

    /// <summary>
    /// Преобразует список слов в одну строку.
    /// </summary>
    /// <param name="words">Слова</param>
    /// <returns>Строка через пробелы</returns>
    let joinWords (words: string list) : string =
        if List.isEmpty words then "ноль"
        else
            let buffer = Array.zeroCreate<char> MaxSize
            let mutable pos = 0
            for word in words do
                pos <- appendWithSpace (Span buffer) pos word
            String(buffer, 0, pos)

    /// <summary>
    /// Соединяет две части с союзом, если вторая не начинается с "ноль".
    /// </summary>
    /// <param name="conj">Союз (например: " и ")</param>
    /// <param name="a">Первая часть</param>
    /// <param name="b">Вторая часть</param>
    /// <returns>Объединённая строка</returns>
    let withConjunction (conj: string) (a: string) (b: string) : string =
        //if b.StartsWith "ноль" then a
        //else 
        a + conj + b