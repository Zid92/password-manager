# Как запустить и работать с Password Manager

## Что нужно

- Установленный **.NET 10 SDK** ([скачать](https://dotnet.microsoft.com/download)).
- Репозиторий на диске: открой терминал в папке проекта (`c:\Users\User\source\repos\pm` или где у тебя лежит клон).
- Для **MAUI**: установленный workload. Если MAUI не собирается, выполни один раз:
  ```bash
  dotnet workload install maui
  dotnet workload restore src/PasswordManager.Maui/PasswordManager.Maui.csproj
  ```
  (может занять несколько минут; после перезагрузки ПК — при необходимости — повтори.)

---

## Шаг 1. Запустить API (обязательно)

API — это сервер: без него ни веб, ни WPF, ни MAUI работать не будут.

В терминале из корня репозитория выполни:

```bash
dotnet run --project PasswordManager.Api
```

Либо запусти скрипт (из корня репозитория):
- **PowerShell:** `.\run-api.ps1`
- **CMD:** `run-api.cmd`

Оставь это окно открытым. В консоли появится адрес, например:

```text
Now listening on: http://localhost:5131
```

Клиенты по умолчанию настроены на **http://localhost:5131** (профиль API из `launchSettings.json`). Если запускаешь API с другим портом — поменяй `ApiBaseUrl` в настройках клиентов (см. раздел «Если API слушает не на localhost:5131»).

---

## Шаг 2. Запустить клиент

В **новом** терминале (из той же папки) запусти один из клиентов.

### Вариант А: веб (Blazor)

```bash
dotnet run --project PasswordManager.Blazor
```

В консоли будет что-то вроде:

```text
Now listening on: http://localhost:5180
```

Открой в браузере этот адрес (например `http://localhost:5180`).

### Вариант Б: WPF (Windows)

```bash
dotnet run --project src/PasswordManager/PasswordManager.csproj
```

Откроется окно приложения.

### Вариант В: MAUI (например Windows)

```bash
dotnet run --project src/PasswordManager.Maui/PasswordManager.Maui.csproj -f net10.0-windows10.0.19041.0
```

Откроется окно MAUI-приложения.

Можно запустить несколько клиентов сразу (и Blazor, и WPF, и MAUI) — все будут использовать один и тот же API и одну общую базу паролей.

---

## Шаг 3. Первый вход и работа с паролями

1. **Открыть хранилище**  
   В любом клиенте введи мастер-пароль и нажми «Unlock» / «Разблокировать».  
   Если хранилище на сервере ещё не создано, оно создастся при первом успешном открытии.

2. **Добавить пароль**  
   - В Blazor: зайди в раздел Vault → «New credential» → заполни поля (Title, Username, Password, при желании URL, Notes) → Save.  
   - В WPF/MAUI: кнопка «+» или «Add» → заполни те же поля → Save.

3. **Смотреть и копировать пароли**  
   Список записей показывается в главном экране. Обычно есть действия вроде «Copy user» / «Copy pass» или «Показать пароль» и копирование в буфер.

4. **Редактировать и удалять**  
   В карточке записи используй кнопки Edit и Delete (или аналоги по смыслу).

5. **Поиск**  
   В Blazor в Vault есть поле поиска. В WPF/MAUI — поле поиска вверху списка. Ввод текста фильтрует записи по названию, логину, URL.

6. **Проверка на утечки**  
   В MAUI/WPF обычно есть кнопка «Check» / «Проверить» в тулбаре — запускается проверка паролей через Have I Been Pwned. Скомпрометированные помечаются в списке.

---

## Если API слушает не на localhost:5131

Укажи в клиентах тот адрес, который выводит API при запуске:

| Клиент | Где менять |
|--------|------------|
| **Blazor** | `PasswordManager.Blazor/wwwroot/appsettings.json` → `ApiBaseUrl`. |
| **WPF** | `src/PasswordManager/appsettings.json` → `ApiBaseUrl`. |
| **MAUI** | `src/PasswordManager.Maui/MauiProgram.cs` → константа `ApiBaseUrl`. |

Пример: если API пишет `http://localhost:5131` — везде должен быть `http://localhost:5131`. Для доступа с другого ПК в сети подставь, например, `http://192.168.1.10:5131`.

---

## Порядок при каждом включении

1. Запустить **API** (`dotnet run --project PasswordManager.Api`).
2. Запустить нужные **клиенты** (Blazor, WPF и/или MAUI).
3. В клиенте ввести мастер-пароль и работать с общим хранилищем.

Остановка API закрывает доступ ко хранилищу до следующего запуска API.
