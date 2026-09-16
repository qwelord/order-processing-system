# OrderFlow pig easter egg

Готовая пасхалка для текущего тёмного интерфейса OrderFlow.

## Файлы

- `pig.svg` — изображение свиньи.
- `pig-oink.mp3` — короткий звук хрюканья.
- `pig-widget.html` — HTML-разметка.
- `pig-widget.css` — стили, hover/focus/click-анимации и адаптивность.
- `pig-widget.js` — звук, анимация и сохранение счётчика в localStorage.

## Установка

В текущем `NotificationService.WebApi/wwwroot` создай папку `pig-widget` и положи туда:

- `pig.svg`
- `pig-oink.mp3`
- `pig-widget.css`
- `pig-widget.js`

В `NotificationService.WebApi/wwwroot/index.html`:

1. Перед `</head>` добавь:

```html
<link rel="stylesheet" href="/pig-widget/pig-widget.css">
```

2. Перед `</body>` добавь содержимое `pig-widget.html`.

3. Перед `</body>` после разметки свиньи добавь:

```html
<script src="/pig-widget/pig-widget.js"></script>
```

Свинья появится в правом нижнем углу. Звук запускается только по клику. Счётчик сохраняется между перезагрузками страницы.

## Сброс счётчика

В DevTools Console:

```js
localStorage.removeItem('orderflow-pig-oinks');
```
