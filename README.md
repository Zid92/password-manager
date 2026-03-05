# Password Manager

Безопасный менеджер паролей для Windows с поддержкой глобальных горячих клавиш и интеллектуальным ранжированием.

![.NET 10](https://img.shields.io/badge/.NET-10.0-purple)
![Windows](https://img.shields.io/badge/Platform-Windows-blue)
![License](https://img.shields.io/badge/License-MIT-green)

## Возможности

- **Безопасное хранение** — пароли шифруются с помощью AES-256, база данных защищена SQLCipher
- **Мастер-пароль** — единый пароль для доступа к хранилищу
- **Windows Hello** — биометрическая аутентификация (отпечаток пальца, распознавание лица)
- **Глобальные горячие клавиши** — быстрая вставка логина/пароля в любое приложение (по умолчанию `Ctrl+Alt+P`)
- **Интеллектуальное ранжирование** — запоминает какие пароли использовались в каких приложениях
- **Проверка утечек** — интеграция с [Have I Been Pwned](https://haveibeenpwned.com/) для проверки паролей
- **System Tray** — работает в фоне, минимизируется в системный трей
- **Material Design** — современный и красивый интерфейс

## Скриншоты

*Скриншоты будут добавлены позже*

## Требования

- Windows 10/11
- .NET 10 Runtime

## Установка

### Из исходного кода

```bash
git clone https://github.com/yourusername/password-manager.git
cd password-manager
dotnet build
dotnet run --project src/PasswordManager
```

### Сборка релиза

```bash
dotnet publish src/PasswordManager -c Release -r win-x64 --self-contained
```

## Использование

### Первый запуск

1. Запустите приложение
2. Создайте мастер-пароль (минимум 8 символов)
3. Опционально включите Windows Hello для быстрой разблокировки

### Добавление пароля

1. Нажмите кнопку `+` в правом нижнем углу
2. Заполните поля (название, логин, пароль, URL)
3. Нажмите "Сохранить"

### Быстрая вставка

1. Откройте приложение, где нужно ввести логин/пароль
2. Нажмите `Ctrl+Alt+P` (или вашу настроенную комбинацию)
3. Выберите нужную запись из списка
4. Нажмите `Enter` для вставки логина и пароля
   - `Ctrl+U` — только логин
   - `Ctrl+P` — только пароль

### Проверка паролей на утечки

1. В главном окне нажмите иконку щита
2. Дождитесь завершения проверки
3. Скомпрометированные пароли будут отмечены красным значком

## Безопасность

- Пароли хранятся в зашифрованной базе данных SQLite (SQLCipher)
- Используется AES-256 для шифрования паролей
- Мастер-ключ производится из мастер-пароля с помощью PBKDF2 (100,000 итераций)
- При использовании Windows Hello, ключ защищён DPAPI и хранится в Windows Credential Manager
- Проверка утечек использует k-anonymity (на сервер отправляются только первые 5 символов хэша)

## Технологии

- [.NET 10](https://dotnet.microsoft.com/) — платформа
- [WPF](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/) — UI фреймворк
- [MaterialDesignInXAML](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) — Material Design стили
- [CommunityToolkit.Mvvm](https://docs.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) — MVVM фреймворк
- [SQLite + SQLCipher](https://www.zetetic.net/sqlcipher/) — зашифрованная база данных
- [Hardcodet.NotifyIcon.Wpf](https://github.com/hardcodet/wpf-notifyicon) — system tray
- [Have I Been Pwned API](https://haveibeenpwned.com/API/v3) — проверка утечек

## Лицензия

MIT License. См. файл [LICENSE](LICENSE).

## Вклад

Pull requests приветствуются! Для крупных изменений сначала создайте issue.
