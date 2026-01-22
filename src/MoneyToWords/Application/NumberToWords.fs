// Application/NumberToWords.fs

namespace MoneyToWords.Application

open MoneyToWords.Domain

/// <summary>
/// Преобразует числовое значение в список слов прописью.
/// </summary>
[<RequireQualifiedAccess>]
module NumberToWords =

    /// <summary>
    /// Разбивает число 0–999 на слова.
    /// </summary>
    /// <param name="rules">Правила языка</param>
    /// <param name="n">Число</param>
    /// <param name="units">Единицы (мужской или женский род)</param>
    /// <returns>Список слов</returns>
    let private parts999 (rules: ILanguageRules) (n: int) (units: string[]) : string list =
        if n = 0 then []
        else
            let teen = n % 100
            let h = n / 100
            let t = teen / 10
            let u = teen % 10
            [
                if h > 0 then yield rules.Hundreds.[h]
                if teen >= 10 && teen <= 19 then
                    yield rules.Teens.[teen - 10]
                else
                    if t > 0 then yield rules.Tens.[t]
                    if u > 0 then yield units.[u]  // ✅ Используется переданный массив
            ]

    /// <summary>
    /// Обрабатывает один "чанк" (0–999) с учётом разряда.
    /// </summary>
    /// <param name="rules">Правила языка</param>
    /// <param name="chunk">Число</param>
    /// <param name="rank">Разряд: 0=единицы, 1=тысячи, ...</param>
    /// <returns>Список слов</returns>
    let private processChunk (rules: ILanguageRules) (chunk: int) (rank: int) : string list =
        if chunk = 0 then
            []
        else
            let isThousand = rank = 1
            let units = if isThousand then rules.UnitsFem else rules.UnitsMasc

            if isThousand then
                if chunk = 1 then
                    ["одна"; "тысяча"]
                elif chunk = 2 then
                    ["две"; "тысячи"]
                else
                    let parts = parts999 rules chunk units
                    let rankWord = Declension.form rules.Ranks.[0] (int64 chunk)
                    parts @ [rankWord]
            else
                let parts = parts999 rules chunk units
                if rank > 0 && rank - 1 < rules.Ranks.Length then
                    let rankWord = Declension.form rules.Ranks.[rank - 1] (int64 chunk)
                    parts @ [rankWord]
                else
                    parts

    /// <summary>
    /// Преобразует число в список слов прописью.
    /// </summary>
    /// <param name="rules">Правила языка</param>
    /// <param name="n">Число</param>
    /// <returns>Список слов</returns>
    let toWordList (rules: ILanguageRules) (n: int64) : string list =
        if n = 0L then
            ["ноль"]
        else
            let rec loop (rem: int64) (rank: int) (acc: string list) : string list =
                if rem = 0L then
                    acc
                else
                    let chunk = int (rem % 1000L)
                    let next = rem / 1000L
                    let current = processChunk rules chunk rank
                    loop next (rank + 1) (current @ acc)
            loop n 0 []