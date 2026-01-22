namespace MoneyToWords.Domain

/// <summary>
/// Форма слова: одна/две/много (например: "рубль", "рубля", "рублей").
/// </summary>
type Form = Form of string * string * string

/// <summary>
/// Интерфейс правил языка для преобразования чисел в слова.
/// Позволяет расширять поддержку языков без изменения логики.
/// </summary>
type ILanguageRules = {

    /// <summary>
    /// Единицы, мужской род: "один", "два"
    /// </summary>
    UnitsMasc: string[]

    /// <summary>
    /// Единицы, женский род: "одна", "две"
    /// </summary>
    UnitsFem: string[]

    /// <summary>
    /// Числа 10–19: "десять", "одиннадцать"
    /// </summary>
    Teens: string[]

    /// <summary>
    /// Десятки: "двадцать", "тридцать"
    /// </summary>
    Tens: string[]

    /// <summary>
    /// Сотни: "сто", "двести"
    /// </summary>
    Hundreds: string[]

    /// <summary>
    /// Формы разрядов: "тысяча", "миллион", ...
    /// </summary>
    Ranks: Form[]

    /// <summary>
    /// Формы валюты: "рубль", "рубля", "рублей"
    /// </summary>
    Currency: Form

    /// <summary>
    /// Формы подвалюты: "копейка", "копейки", "копеек"
    /// </summary>
    Subunit: Form

    /// <summary>
    /// Союз между рублями и копейками (например: " и ")
    /// </summary>
    Conjunction: string
}