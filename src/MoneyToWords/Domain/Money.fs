namespace MoneyToWords.Domain

/// <summary>
/// Количество рублей (целое, неотрицательное, ≤ 999_999_999_999_999_999).
/// </summary>
[<Struct>]
type Rubles private (value: int64) =

    /// <summary>
    /// Возвращает числовое значение рублей.
    /// </summary>
    member _.Value = value

    /// <summary>
    /// Пытается создать <see cref="Rubles"/>.
    /// </summary>
    /// <param name="value">Число рублей</param>
    /// <returns><see cref="Result{T, TError}"/> с <see cref="Rubles"/> или сообщением об ошибке</returns>
    static member TryCreate(value: int64) =
        if value < 0L then Error "Rubles cannot be negative."
        elif value >= 1_000_000_000_000_000_000L then Error "Rubles too large (max 999 quadrillion)."
        else Ok (Rubles value)

    /// <inheritdoc />
    override this.ToString() = string this.Value

/// <summary>
/// Количество копеек (0–99).
/// </summary>
[<Struct>]
type Kopecks private (value: byte) =

    /// <summary>
    /// Возвращает числовое значение копеек.
    /// </summary>
    member _.Value = value

    /// <summary>
    /// Пытается создать <see cref="Kopecks"/>.
    /// </summary>
    /// <param name="value">Количество копеек</param>
    /// <returns><see cref="Result{T, TError}"/> с <see cref="Kopecks"/> или сообщением об ошибке</returns>
    static member TryCreate(value: int) =
        if value < 0 || value > 99 then Error "Kopecks must be between 0 and 99."
        else Ok (Kopecks(byte value))

    /// <inheritdoc />
    override this.ToString() = sprintf "%02d" this.Value

/// <summary>
/// Денежная сумма в рублях и копейках.
/// </summary>
[<Struct>]
type Money = {

    /// <summary>
    /// Количество рублей.
    /// </summary>
    Rubles: Rubles

    /// <summary>
    /// Количество копеек.
    /// </summary>
    Kopecks: Kopecks
} with

    /// <summary>
    /// Пытается создать <see cref="Money"/>.
    /// </summary>
    /// <param name="rubles">Рубли</param>
    /// <param name="kopecks">Копейки</param>
    /// <returns><see cref="Result{T, TError}"/> с <see cref="Money"/> или ошибкой</returns>
    static member TryCreate(rubles: int64, kopecks: int) =
        match Rubles.TryCreate rubles, Kopecks.TryCreate kopecks with
        | Ok r, Ok k -> Ok { Rubles = r; Kopecks = k }
        | Error rMsg, Ok _ -> Error rMsg
        | Ok _, Error kMsg -> Error kMsg
        | Error rMsg, Error kMsg -> Error (rMsg + " " + kMsg)

    /// <summary>
    /// Преобразует сумму в <see cref="decimal"/>.
    /// </summary>
    /// <returns>Сумма в формате рубль.копейка</returns>
    member this.ToDecimal() = decimal this.Rubles.Value + decimal this.Kopecks.Value / 100m

    /// <inheritdoc />
    override this.ToString() = sprintf "%.2f" (this.ToDecimal())