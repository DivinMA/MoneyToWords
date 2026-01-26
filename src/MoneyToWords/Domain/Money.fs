namespace MoneyToWords.Domain
/// <summary>
/// Represents the rubles part of a monetary amount.
/// Value must be a non-negative integer and less than 10^18.
/// Ensures domain invariants are preserved at the type level.
/// </summary>
/// <remarks>
/// This is a <c>struct</c> to avoid heap allocation and improve performance.
/// Instances are immutable and value-based (struct equality).
/// Use <see cref="Rubles.TryCreate(int64)" /> for validated construction.
/// </remarks>
[<Struct>]
type Rubles private (value: int64) =

    /// <summary>
    /// Gets the numeric value of rubles.
    /// </summary>
    /// <returns>The raw <see cref="int64" /> value.</returns>
    member _.Value = value

    /// <summary>
    /// Attempts to create a valid <see cref="Rubles" /> instance from a 64-bit integer.
    /// </summary>
    /// <param name="value">The number of rubles to validate.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}" /> where:
    /// <list type="bullet">
    ///   <item><description><see cref="Result{T, TError}.Ok" /> containing <see cref="Rubles" /> if value is in range [0, 10^18).</description></item>
    ///   <item><description><see cref="Result{T, TError}.Error" /> with <see cref="MoneyError.NegativeRubles" /> if value &lt; 0.</description></item>
    ///   <item><description><see cref="Result{T, TError}.Error" /> with <see cref="MoneyError.RublesTooLarge" /> if value ≥ 10^18.</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// let result = Rubles.TryCreate(500L)
    /// match result with
    /// | Ok rubles -> printfn "Valid: %d rubles" rubles.Value
    /// | Error err -> printfn "Error: %s [%s]" err.Description err.Code
    /// </code>
    /// </example>
    static member TryCreate(value: int64) =
        if value < 0L then Error MoneyError.NegativeRubles
        elif value >= 1_000_000_000_000_000_000L then Error (MoneyError.RublesTooLarge value)
        else Ok (Rubles value)

    /// <summary>
    /// Returns the string representation of the rubles value.
    /// </summary>
    /// <returns>The value as a decimal string (e.g., "123").</returns>
    override this.ToString() = string this.Value


/// <summary>
/// Represents the kopecks part of a monetary amount.
/// Value must be in the range [0, 99], as kopecks are subunits of a ruble.
/// </summary>
/// <remarks>
/// Stored as a <c>byte</c> for memory efficiency.
/// Immutable and struct-based for performance.
/// Use <see cref="Kopecks.TryCreate(int)" /> for validated construction.
/// </remarks>
[<Struct>]
type Kopecks private (value: byte) =

    /// <summary>
    /// Gets the numeric value of kopecks.
    /// </summary>
    /// <returns>The raw <see cref="byte" /> value.</returns>
    member _.Value = value

    /// <summary>
    /// Attempts to create a valid <see cref="Kopecks" /> instance from an integer.
    /// </summary>
    /// <param name="value">The number of kopecks to validate.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}" /> where:
    /// <list type="bullet">
    ///   <item><description><see cref="Result{T, TError}.Ok" /> containing <see cref="Kopecks" /> if value ∈ [0, 99].</description></item>
    ///   <item><description><see cref="Result{T, TError}.Error" /> with <see cref="MoneyError.InvalidKopecks" /> otherwise.</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// let result = Kopecks.TryCreate(99)
    /// match result with
    /// | Ok k -> printfn "Valid: %02d kopecks" k.Value
    /// | Error err -> printfn "Error: %s" err.Description
    /// </code>
    /// </example>
    static member TryCreate(value: int) =
        if value < 0 || value > 99 then Error (MoneyError.InvalidKopecks value)
        else Ok (Kopecks(byte value))

    /// <summary>
    /// Returns the string representation of kopecks with leading zero if needed.
    /// </summary>
    /// <returns>A two-digit string (e.g., "05", "99").</returns>
    override this.ToString() = sprintf "%02d" this.Value


/// <summary>
/// Represents a monetary amount in rubles and kopecks.
/// This is the main value object of the domain, ensuring both parts are valid.
/// Immutable, struct-based, and designed for performance and correctness.
/// </summary>
/// <remarks>
/// Follows Domain-Driven Design (DDD) principles:
/// <list type="bullet">
///   <item>Value Object semantics (equality by value).</item>
///   <item>Guaranteed validity via <see cref="TryCreate(int64, int)" />.</item>
///   <item>No side effects or mutable state.</item>
/// </list>
/// Prefer using <see cref="Money.TryCreate(int64, int)" /> over direct record construction.
/// </remarks>
[<Struct>]
type Money = {

    /// <summary>
    /// The rubles part of the monetary amount.
    /// Guaranteed to be valid (non-negative, within range).
    /// </summary>
    Rubles: Rubles

    /// <summary>
    /// The kopecks part of the monetary amount.
    /// Guaranteed to be in range [0, 99].
    /// </summary>
    Kopecks: Kopecks
} with

    /// <summary>
    /// Attempts to create a valid <see cref="Money" /> instance from rubles and kopecks.
    /// Validates both components and returns a combined result.
    /// </summary>
    /// <param name="rubles">Number of rubles. Must be ≥ 0 and &lt; 10^18.</param>
    /// <param name="kopecks">Number of kopecks. Must be in [0, 99].</param>
    /// <returns>
    /// A <see cref="Result{T, TError}" /> where:
    /// <list type="bullet">
    ///   <item><description><see cref="Result{T, TError}.Ok" /> with <see cref="Money" /> if both values are valid.</description></item>
    ///   <item><description><see cref="Result{T, TError}.Error" /> with <see cref="MoneyError.NegativeRubles" /> or <see cref="MoneyError.RublesTooLarge" /> if rubles are invalid.</description></item>
    ///   <item><description><see cref="Result{T, TError}.Error" /> with <see cref="MoneyError.InvalidKopecks" /> if kopecks are out of range.</description></item>
    ///   <item><description><see cref="Result{T, TError}.Error" /> with <see cref="MoneyError.CompositionError" /> if both are invalid.</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// let result = Money.TryCreate(123L, 45)
    /// match result with
    /// | Ok money -> printfn "Amount: %s" (MoneyToWords.toWords money)
    /// | Error err -> printfn "Invalid amount: %s [%s]" err.Description err.Code
    /// </code>
    /// </example>
    static member TryCreate(rubles: int64, kopecks: int) : Result<Money, MoneyError> =
        let rResult = Rubles.TryCreate rubles
        let kResult = Kopecks.TryCreate kopecks

        match rResult, kResult with
        | Ok r, Ok k -> Ok { Rubles = r; Kopecks = k }
        | Error rErr, Ok _ -> Error rErr
        | Ok _, Error kErr -> Error kErr
        | Error _, Error _ -> Error (MoneyError.CompositionError "Invalid rubles and kopecks")

    /// <summary>
    /// Converts the monetary amount to a <see cref="decimal" /> value.
    /// Useful for arithmetic operations, database storage, or interoperability.
    /// </summary>
    /// <returns>The amount in decimal form (e.g., 123.45m).</returns>
    member this.ToDecimal() =
        decimal this.Rubles.Value + decimal this.Kopecks.Value / 100m

    /// <summary>
    /// Returns the string representation of the amount in "X.YY" format.
    /// </summary>
    /// <returns>A string like "123.45".</returns>
    override this.ToString() = sprintf "%.2f" (this.ToDecimal())