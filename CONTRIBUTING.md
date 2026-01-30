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
| `chore` | <code style="background:#0052CC; color:white">  </code> | Maintenance task | ✅ Да (patch) |
| `ci` | <code style="background:#5319E7; color:white">  </code> | CI/CD pipeline | ✅ Да (patch) |
| `docs` | <code style="background:#0E8A16; color:white">  </code> | Documentation changes | ✅ Да (patch) |
| `draft` | <code style="background:#6A737D; color:white">  </code> | Work in progress | ❌ Нет |
| `feature` | <code style="background:#1D76DB; color:white">  </code> | New functionality | ✅ Да (minor) |
| `fix` | <code style="background:#D93F0B; color:white">  </code> | Bug fix | ✅ Да (patch) |
| `license` | <code style="background:#0052CC; color:white">  </code> | License compliance | ✅ Да (patch) |
| `major` | <code style="background:#B60205; color:white">  </code> | Major breaking change | ✅ Да (major) |
| `minor` | <code style="background:#79B8FF; color:white">  </code> | Minor release | ✅ Да (minor) |
| `patch` | <code style="background:#22863A; color:white">  </code> | Patch release | ✅ Да (patch) |
| `ready-for-review` | <code style="background:#2188FF; color:white">  </code> | Ready for review | ❌ Нет |
| `refactor` | <code style="background:#795E26; color:white">  </code> | Code restructuring | ✅ Да (patch) |
| `security` | <code style="background:#B60205; color:white">  </code> | Security issue | ✅ Да (major) |
| `test` | <code style="background:#795E26; color:white">  </code> | Test changes | ✅ Да (patch) |

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
> ⚙️ Сгенерировано автоматически • generate-contributing.fsx • 2026-01-30 19:52 UTC