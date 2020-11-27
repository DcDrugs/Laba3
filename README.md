Настройка для парсера:
Settings.json
Settings.xml
Settings2.xml

5 режимов работы:
1. Архивация
2. Деархифация
3. Шифрование
4. Разшифровка
5. Архивация и шифрование

Для выбора настройки вам нужно вести строго определенный ключ и папку(их может быть от 0 до 5), куда будет помещен фаил с действием.
Настройка 
  "TargetPath": "...", Здесь вам нужно прописать путь за которым будет проходить логирование изменений(добавлений)
   "SourcetPath": "...", Здесь вам нужно прописать путь за которым будет проходить вставка новых объектов

Установка:

cd C:\Windows\Microsoft.NET\Framework64\v4.0.30319

Установка службы(... - путь до программы):

InstallUtil.exe ...\FTPTargetWatcher.exe

Запуск с конфигурацией(обязательно):

sc start FTPTargetWatcher ...\*.json

Удаление:

InstallUtil.exe /u ...\FTPTargetWatcher.exe


Теперь пройдемся по самой лаб. работе:
/Parser в ней находяться сами парсеры, json , xml v1.0 xml v2.0 и менеджер парсеров.
Оба парсера являються универсальными и легко модифицируемыми. Код являеться самоописывающимся, по этому разобрать в нем не составит труда.

В /Options: находяться все классы настройки запуска.

В /ArchiveAndCryptor: находяться Класс криптования и класс Архивации.

В FTPRequestManager.cs находиться сам класс логирования.
Он в себе содержит сам класс запуска и логгер, который следит за папкой. с помощью событий он вызывает методы настроек, чтобы выполнить поставленные ему задачи.

Из особенностей могу выделить Качественный код для парсеров и рефакторинг кода.

Реализовано:

1. Сервис служба
2. ВСЕ видимые исключительные словлены.
3. Обработчик XML файла универсален (Легко подлежит добавлениям новых фич).
4. Обработчик Json файла универсален(Легко подлежит добавлениям новых фич).
5. Создан класс хранения данных парсеров.
6. Конфигурации ArchiveOptions, CryptingOptions, FileOptions.
7. Менеджер Конфигураций ParserManager.
8. Менеджер конфигурации получает на вход путь к файлу через консольные параметры запуска.
9. Изменения настроек .json .xml допустимо.
10. Наименования свойств модели совпадают с наименованием тэгов/секций/перечислений в XML/JSON файлах
