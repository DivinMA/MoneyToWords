namespace MoneyToWords.Domain

/// <summary>
/// Represents a noun form that changes based on number in languages with grammatical gender and number cases.
/// Used for proper declension in Russian and similar languages.
/// </summary>
/// <remarks>
/// The three forms represent:
/// <list type="number">
///   <item><description>Singular (1): "рубль", "копейка"</description></item>
///   <item><description>Plural for 2–4: "рубля", "копейки"</description></item>
///   <item><description>Plural for 5–9, 0, 11–14: "рублей", "копеек"</description></item>
/// </list>
/// This pattern applies to units like currency, time, countable nouns.
/// Example: <c>Form("год", "года", "лет")</c>, <c>Form("человек", "человека", "человек")</c>.
/// </remarks>
/// <param name="One">Form used with numbers ending in 1 (except 11): 1, 21, 31...</param>
/// <param name="TwoFour">Form used with numbers ending in 2–4 (except 12–14): 2, 3, 4, 22, 23, 24...</param>
/// <param name="Many">Form used with numbers ending in 0, 5–9, 11–14</param>
type Form = Form of One: string * TwoFour: string * Many: string