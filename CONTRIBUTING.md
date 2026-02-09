# How to Contribute

Спасибо, что хочешь помочь! Мы ценим каждый вклад.

Проект использует **полностью автоматизированную систему управления**, чтобы упростить участие.

---

## 🏷️ Система лейблов

Мы используем два типа лейблов:
### 🔹 Поведенческие (без префикса)
Участвуют в **автоматическом версионировании** и **changelog**.

| Лейбл | Цвет | Описание | Участвует в релизе? |
|------|------|--------|-------------------|
| `changes-requested` | <code style="background:#D93F0B; color:white">  </code> | Changes requested | ❌ Нет |
| `draft` | <code style="background:#6A737D; color:white">  </code> | Work in progress | ❌ Нет |
| `ready-for-review` | <code style="background:#2188FF; color:white">  </code> | Ready for review | ❌ Нет |
| `version:major` | <code style="background:#B60205; color:white">  </code> | Major breaking change (automated) | ✅ Да (major) |
| `version:minor` | <code style="background:#79B8FF; color:white">  </code> | Minor release (automated) | ✅ Да (minor) |
| `version:patch` | <code style="background:#22863A; color:white">  </code> | Patch release (automated) | ✅ Да (patch) |

---

#### `type:...` — тип задачи

| Лейбл | Описание |
|------|--------|
| `type:bug` | Reported bug |
| `type:discussion` | Architecture discussion |
| `type:feature` | New feature request |
| `type:good-first-issue` | Good for beginners |
| `type:help-wanted` | Help wanted |
| `type:question` | Question |
| `type:refactor` | Code refactoring |

#### `area:...` — область кода

| Лейбл | Описание |
|------|--------|
| `area:ci` | CI/CD workflows |
| `area:core` | Core library code |
| `area:docs` | Documentation |
| `area:examples` | Example projects |
| `area:tests` | Test code |

#### `priority:...` — приоритет

| Лейбл | Описание |
|------|--------|
| `priority:critical` | Critical priority |
| `priority:high` | High priority |
| `priority:low` | Low priority |
| `priority:medium` | Medium priority |

#### `effort:...` — сложность

| Лейбл | Описание |
|------|--------|
| `effort:huge` | Huge effort (week+) |
| `effort:large` | Large effort (2+d) |
| `effort:medium` | Medium effort (1d) |
| `effort:small` | Small effort (1-2h) |


---

## 📝 Шаблоны задач и PR

При создании:
- **Issue** — выберите тип: баг, фича, вопрос
- **PR** — заполнится шаблон автоматически

---

## 🌲 Ветки

Используйте:
- `feat/...` — новые возможности
- `fix/...` — исправления
- `docs/...` — документация
- `chore/...` — техническое обслуживание
- `refactor/...` — рефакторинг

Пример: `feat/add-currency-support`

---

## 📦 Коммиты

Используйте [Conventional Commits](https://www.conventionalcommits.org/): `<type>: <описание> [тело]`

Поддерживаемые типы:
- `feat`: новая функция
- `fix`: исправление
- `docs`: документация
- `chore`: обслуживание
- `refactor`: рефакторинг
- `test`: тесты
- `ci`: CI/CD

Пример: `feat: add support for EUR currency`

---

## 🔍 Как узнать версию библиотеки?

```bash
dotnet list package | grep MoneyToWordsFSharpLib
```

🔄 Автоматизация
Лейблы ставятся автоматически по ветке и файлам.
Не нужно вручную ставить feature, area:core и т.д.
CI проверяет, что есть patch/minor/major.

Если возникнут вопросы — см. задачу [#11: Настройка автоматизации](https://github.com/DivinMA/MoneyToWords/issues/11)

---
> ⚙️ ⚙️ Сгенерировано автоматически • система документации