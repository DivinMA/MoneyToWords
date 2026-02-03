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
| `major` | <code style="background:#B60205; color:white">  </code> | Major breaking change | ✅ Да (major) |
| `minor` | <code style="background:#79B8FF; color:white">  </code> | Minor release | ✅ Да (minor) |
| `patch` | <code style="background:#22863A; color:white">  </code> | Patch release | ✅ Да (patch) |
| `ready-for-review` | <code style="background:#2188FF; color:white">  </code> | Ready for review | ❌ Нет |

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

## 🔁 Релизы и публикация

- Управление релизами автоматизировано и основано на метках PR и Release Drafter.
  - **Лейблы** `major`, `minor`, `patch` определяют семантические изменения версии (используется MinVer).
  - **Release Drafter** собирает заметки релиза и формирует Draft Release при пушах в `main`.
  - **Auto Release**: при пуше в `main` CI вычисляет версию (MinVer) и публикует релиз, создавая тег `v<version>` при необходимости.
  - **Публикация на NuGet**: релиз автоматически упаковывается и публикуется на NuGet.org (workflow `publish.yml`). Для публикации необходимо настроить `NUGET_API_KEY` в Secrets репозитория.

- Рекомендуемая практика для чистой истории: используйте squash-мерджи для PR в `main` (включите соответствующее правило в настройках репозитория). Это делает историю линейной, помогает MinVer правильно определять версии и упрощает CHANGELOG.

- CHANGELOG.md: Release Drafter формирует заметки релизов; если нужно, могу добавить автоматическое обновление файла `CHANGELOG.md` в репозитории при релизе (создавать PR с обновлённым changelog).

---
> ⚙️ ⚙️ Сгенерировано автоматически • система документации