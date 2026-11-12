# Java-компонент профессиональной пробы

Работающий модуль главы 3: читает JSON, сохранённый `PcTrialController`, заново проверяет порядок действий и формирует сводку. Не доверяет полю `Summary` во входном файле. Реального школьника и измерения педагогического эффекта примеры не представляют.

## Запуск

Нужна Java 17 или новее. Для сборки нужен JDK, для выполнения готового JAR достаточно Java runtime.

```powershell
./build.ps1
java -Dfile.encoding=UTF-8 -jar build/vrprof-analyzer.jar examples/completed.json results/completed
java -Dfile.encoding=UTF-8 -jar build/vrprof-analyzer.jar examples/incomplete.json results/incomplete
```

Результат: `analysis.json` и `summary.txt`. Завершённый пример даёт 9/9 шагов, 1 подсказку и 1 ошибку; незавершённый — 4/9, 0 подсказок и 1 ошибку. JSON записан реальным методом `PcTrialController.SaveAttempt` при автоматическом вызове компонента в редакторе Unity. Это не запись ручного VR-прохождения. Команда создания: `Tools > VrProf > Экспортировать проверочные журналы`.

## Устройство

`Exercise`, `TrialStep`, `TrialEvent`, `TrialAttempt`, `TrialResult` — неизменяемые классы record. `UnityJsonAttemptReader` реализует `AttemptReader` и проверяет типы JSON. `AttemptEvaluator` выполняет предметную проверку. `TextSummaryFormatter` реализует `SummaryFormatter`. `TrialAnalyzer` объединяет эти объекты; `Main` отвечает за CLI и файлы результата. Новое представление сводки можно добавить отдельной реализацией интерфейса, не меняя алгоритм.

Контракт: `AttemptId`, `ExerciseId`, `PupilCode`, `MentorCode`, `Events`, `Difficulty`, `Interest`, `StudentFeedback`, `MentorFeedback`. У события `Number`, `Kind`, `StepCode`, `ObjectId`, `Success`. Версия `ram-replacement-v1` соответствует девяти шагам C#. `Events` допускает пустой массив, оценки — только целые числа от 1 до 5, номера событий — подряд от 1. Поле `Summary` необязательно и игнорируется. Неверный пользовательский поступок учитывается как ошибка пробы; неверный формат файла прерывает обработку.

Коды завершения: 0 — файл обработан (в том числе незавершённая проба); 1 — ошибка входного файла или записи результата; 2 — неверное число аргументов. Неизвестные дополнительные поля допускаются. Старый экспорт без идентификаторов нужно повторно сохранить обновлённым контроллером; они не подставляются молча.

## Зависимость

Gson 2.13.2, Apache License 2.0; JAR и лицензия находятся в `lib`. SHA-256 JAR: `dd0ce1b55a3ed2080cb70f9c655850cda86c206862310009dcb5e5c95265a5e0`. Источник: Maven Central, `com.google.code.gson:gson:2.13.2`. Версия зафиксирована для воспроизводимости, утверждения о том, что она самая новая, нет.

## Проверка

Из корня Unity-проекта: `python Verification/verify.py`. Нужны Python 3, Java 17+, .NET SDK 8, предварительно собранный JAR и два экспортированных примера. Проверка сравнивает Java с непосредственно подключённым `Assets/Scripts/TrialLogic.cs` и сохранённой Unity-сводкой. Отчёт: `results/verification.json`.

В разработке и документировании использован Codex (OpenAI). Интеграционные проверки выполнены на реальных исходниках; использование ИИ раскрыто в `Docs/AI_USAGE.md` и пояснительной записке.
