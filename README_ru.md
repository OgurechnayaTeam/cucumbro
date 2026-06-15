# Cucumbro

<p align="center">
  <a href="README.md">English</a> | <strong>Русский</strong>
</p>

<p align="center">
  <a href="docs/%D0%A0%D1%83%D0%BA%D0%BE%D0%B2%D0%BE%D0%B4%D1%81%D1%82%D0%B2%D0%BE%20%D0%BF%D0%BE%D0%BB%D1%8C%D0%B7%D0%BE%D0%B2%D0%B0%D1%82%D0%B5%D0%BB%D1%8F.docx">Руководство пользователя</a>
  ·
  <a href="docs/%D0%A0%D1%83%D0%BA%D0%BE%D0%B2%D0%BE%D0%B4%D1%81%D1%82%D0%B2%D0%BE%20%D0%BF%D1%80%D0%BE%D0%B3%D1%80%D0%B0%D0%BC%D0%BC%D0%B8%D1%81%D1%82%D0%B0.docx">Руководство программиста</a>
</p>

<p align="center">
  <a href="https://ogurechnayateam.github.io/cucumbro/">
    <img src="https://avatars.githubusercontent.com/u/271751028?s=200&v=4" alt="Иконка Cucumbro" width="96" height="96">
  </a>
</p>

<h2 align="center">
  <a href="https://ogurechnayateam.github.io/cucumbro/">Играть в Cucumbro на GitHub Pages</a>
</h2>

Cucumbro - это прототип 2D-экшен игры на Unity, действие которой происходит в небольших процедурно генерируемых подземельях. Игрок исследует комнаты, сражается с овощными врагами, собирает предметы и может начинать забеги с разными классами оружия.

## Возможности

- Процедурная генерация подземелий и комнат.
- Управление игроком через Unity Input System.
- Несколько скриптов игрока и оружия, включая катану, пистолет, щит и снаряды.
- Спавн врагов, их активация и базовое боевое поведение.
- Подбираемые предметы: еда, батарейки и щиты.
- Несколько сцен Unity для тестирования и разработки.

## Информация о проекте

- Контекст курса: проект разработан в рамках курса "Проектный практикум" в Уральском федеральном университете (УрФУ).
- Сайт курса: [студенту.прокомпетенции.рф](https://студенту.прокомпетенции.рф/)
- Документ выбора проектов: [список проектов](https://docs.google.com/spreadsheets/d/1bYXj4j211c6HlchBshuYqBvuIFWunA0nDUBZBSRdT-U/htmlview) (проект номер 7).
- Движок: Unity 6000.4.0f1
- Render pipeline: Universal Render Pipeline 2D
- Основные ассеты и скрипты: `Assets/`
- Настройки проекта: `ProjectSettings/`
- Манифест пакетов: `Packages/manifest.json`

## Использованные источники

- Процедурная генерация подземелья была реализована на основе учебного репозитория Sunny Valley Studio: <https://github.com/SunnyValleyStudio/Unity_2D_Procedural_Dungoen_Tutorial>.

## Использование ИИ

Генеративные ИИ-инструменты использовались как помощники при разработке и оформлении документации: для разбора проблем Unity/C#, проверки настройки GitHub workflow, улучшения README и вычитки текста отчета. Финальные решения, интеграция кода и проверка проекта выполнялись командой.

## Как запустить

1. Откройте проект в Unity 6000.4.0f1 или совместимой версии Unity 6.
2. Дождитесь восстановления пакетов из `Packages/manifest.json`.
3. Откройте сцену `Assets/Scenes/SampleScene.unity`.
4. Нажмите Play, чтобы протестировать текущий прототип.

## Web-сборка и GitHub Pages

В репозитории добавлены GitHub Actions workflows для WebGL-релизов и публикации на GitHub Pages.

1. Добавьте в настройки репозитория GitHub секреты для активации Unity: `UNITY_LICENSE`, `UNITY_EMAIL` и `UNITY_PASSWORD`.
2. В настройках репозитория выберите GitHub Actions как источник GitHub Pages.
3. Отправьте версионный тег, например `v0.1.0`.
4. Workflow `Build WebGL Release` соберет Unity WebGL player и прикрепит `cucumbro-webgl.zip` к GitHub Release.
5. Workflow `Deploy Pages From Latest Release` скачает `cucumbro-webgl.zip` из последнего релиза и опубликует его на GitHub Pages.

## Структура репозитория

- `Assets/Scripts/` - игровые скрипты игрока, врагов, оружия, UI и логики уровней.
- `Assets/_Scripts/` - скрипты процедурной генерации подземелий и данные ScriptableObject.
- `Assets/Prefabs/` - префабы игрока, врагов, оружия, предметов и UI.
- `Assets/Tiles/` - тайлы и палитры для полов и стен подземелий.
- `ProjectSettings/` - конфигурация проекта Unity.

## Лицензия

Проект распространяется по [MIT License](LICENSE).
