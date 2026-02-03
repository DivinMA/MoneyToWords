namespace MoneyToWords.Domain
/// <summary>
/// Represents an error that occurs during the creation or processing of a monetary amount.
/// Used as the error type in <see cref="Result{T, TError}" /> operations across the domain.
/// </summary>
/// <remarks>
/// Each case includes:
/// <list type="bullet">
///   <item>A stable <see cref="Code" /> for logging, monitoring, and API contracts.</item>
///   <item>A human-readable <see cref="Description" /> in English (for logs and fallback).</item>
/// </list>
/// For user-facing localized messages, use presentation modules like <see cref="MoneyErrors.toRussian" />.
/// </remarks>
[<RequireQualifiedAccess>]
type MoneyError =

    /// <summary>
    /// The rubles value is negative, which is not allowed.
    /// </summary>
    | NegativeRubles

    /// <summary>
    /// The rubles value exceeds the maximum supported amount (10^18 - 1).
    /// </summary>
    | RublesTooLarge of actual: int64

    /// <summary>
    /// The kopecks value is outside the valid range [0, 99].
    /// </summary>
    | InvalidKopecks of actual: int

    /// <summary>
    /// Internal composition error (e.g., both rubles and kopecks are invalid).
    /// </summary>
    | CompositionError of reason: string

    /// <summary>
    /// Gets a stable, machine-readable error code.
    /// Suitable for logging, monitoring, telemetry, and API responses.
    /// </summary>
    /// <returns>
    /// One of:
    /// <list type="table">
    ///   <item><term>MONEY_001</term><description>For <see cref="NegativeRubles" /></description></item>
    ///   <item><term>MONEY_002</term><description>For <see cref="InvalidKopecks" /></description></item>
    ///   <item><term>MONEY_003</term><description>For <see cref="RublesTooLarge" /></description></item>
    ///   <item><term>MONEY_004</term><description>For <see cref="CompositionError" /></description></item>
    /// </list>
    /// </returns>
    member this.Code =
        match this with
        | NegativeRubles -> "MONEY_001"
        | InvalidKopecks _ -> "MONEY_002"
        | RublesTooLarge _ -> "MONEY_003"
        | CompositionError _ -> "MONEY_004"

    /// <summary>
    /// Gets a human-readable error description in English.
    /// Intended for logs, debugging, or fallback when localization is not available.
    /// </summary>
    /// <returns>
    /// A brief explanation of the error, e.g.:
    /// <list type="bullet">
    ///   <item>"Amount cannot be negative"</item>
    ///   <item>"Kopecks must be in range 0 to 99 (actual: &lt;value&gt;)"</item>
    ///   <item>"Rubles value is too large (actual: &lt;value&gt;)"</item>
    ///   <item>"Money composition error" or "Money composition error: &lt;details&gt;"</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// For user-facing messages in Russian, use <see cref="MoneyErrors.toRussian(MoneyError)" />.
    /// This property is not intended for direct UI display.
    /// </remarks>
    member this.Description =
        match this with
        | NegativeRubles -> "Amount cannot be negative"
        | InvalidKopecks actual -> sprintf "Kopecks must be in range 0 to 99 (actual: %d)" actual
        | RublesTooLarge actual -> sprintf "Rubles value is too large (actual: %d)" actual
        | CompositionError reason ->
            if System.String.IsNullOrWhiteSpace(reason) then "Money composition error"
            else sprintf "Money composition error: %s" reason
