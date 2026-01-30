# 💰 MoneyToWords — Сумма прописью
[![.NET](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com)
[![NuGet](https://img.shields.io/nuget/v/MoneyToWordsFSharpLib.svg)](https://www.nuget.org/packages/MoneyToWordsFSharpLib)
[![Build](https://github.com/DivinMA/MoneyToWords/actions/workflows/ci.yml/badge.svg)](https://github.com/DivinMA/MoneyToWords/actions)
[![Release](https://github.com/DivinMA/MoneyToWords/actions/workflows/release-draft.yml/badge.svg)](https://github.com/DivinMA/MoneyToWords/actions)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE.txt)

**MoneyToWords** — надёжная библиотека на F#, преобразующая числа в текст с правильным склонением:  
- "один рубль",
- "двадцать одна тысяча",
- "сто двадцать три миллиона".

> 💡 Этот проект создан для изучения F#, принципов функционального программирования, механизмов GitHub и паттернов проектирования.
> Выполнен по **принципам production-ready кода**:  
> - полное тестирование,
> - чистая архитектура,
> - документация
> - готовность к использованию в реальных системах.

Подходит для:
- Формирования банковских документов
- Чеков и счётов
- Финансовых отчётов
- Генерации суммы прописью в печатных формах

---

## ✨ Особенности

- ✅ Поддержка чисел до **триллионов**
- 🇷🇺 Правильное склонение: `рубль`, `рубля`, `рублей`
- 🔤 Женский род для тысяч: `одна тысяча`, `двадцать одна тысяча`
- 🧪 100% покрытие тестами
- 🧩 Легко расширить для других языков
- 📦 Готова к использованию как NuGet-пакет

---

## 🚀 Примеры использования

```fsharp
	toWords 1L 0L // → "один рубль и ноль копеек"
	toWords 3L 50L // → "три рубля и пятьдесят копеек"
	toWords 21_000L 0L // → "двадцать одна тысяча рублей и ноль копеек"	
	toWords 112_234L 75L // → "сто двенадцать тысяч двести тридцать четыре рубля и семьдесят пять копеек"
	toWords 0L 1L // → "ноль рублей и одна копейка"
```

---

## 📦 Использование
```fsharp
	open MoneyToWords

	let result = toWords 42_000L 50L
	match result with
	| Ok text -> printfn "Сумма: %s" text
	| Error msg -> printfn "Ошибка: %s" msg
```

---

## 🧩 Как это работает

Число разбивается на группы по 3 цифры (тысячи, миллионы и т.д.), каждая обрабатывается отдельно:
```
123 456 789
  |   |   └─ 789 → "семьсот восемьдесят девять"
  |   └───── 456 → "четыреста пятьдесят шесть тысяч"
  └───────── 123 → "сто двадцать три миллиона"
```

Для каждой группы:

- Используются правильные формы: тысяча, тысячи, тысяч
- Учитываются особые случаи: одна тысяча, две тысячи
- Правильное склонение чисел: пятнадцать, а не пятьнадцать

---

## 🧪 Тестирование
> Проект полностью покрыт тестами:

Юнит-тесты — все граничные случаи:

- 0, 1, 2, 5
- 11–19
- 21, 22, 25 — правильное окончание
- 100, 1000, 999_999_999_999_999
- Свойства (FsCheck) — проверка на тысячах случайных значений:

- Склонение по последней цифре
- Исключения для 11–14
- Поддержка больших чисел

> Все тесты проходят на CI при каждом коммите.

## 🛠 Установка
```bash
	dotnet add package MoneyToWords
```

---

## 🧱 Сборка и тесты
```bash
	dotnet build
	dotnet test
```

Проект использует:
- F# .NET 10 LTS
- xUnit, FsCheck, Unquote

## 📄 Лицензия
MIT — свободно используй в любых проектах, коммерческих и открытых.

---

## 🙏 Поддержка
- 🐞 Нашёл баг? → [Сообщи об ошибке](https://github.com/DivinMA/MoneyToWords/issues/new?template=bug_report.yml)
- 💡 Есть идея? → [Предложи фичу](https://github.com/DivinMA/MoneyToWords/issues/new?template=feature_request.yml)
- ❓ Вопрос? → [Задай вопрос](https://github.com/DivinMA/MoneyToWords/issues/new?template=question.yml)
- 🤝 Вклад? → [Создай PR](https://github.com/DivinMA/MoneyToWords/pulls)

---

## 🙌 Автор
@DivinMA — [GitHub](https://github.com/DivinMA) | [Telegram](@Michael_Divin)

---

## 🟢 Статус
✅ Стабильная версия: v1.0.0
✅ CI/CD:
CI
✅ NuGet:
NuGet

---

## 💡 Благодарности
Спасибо, что используешь этот проект!

Если он помог — поставь ⭐ — это вдохновляет на развитие.
