# Project architecture

The project separates presentation from game functionality.

## Presentation layer
- `Forms/*.Designer.cs`: WinForms control creation, layout, fonts, colors and responsive positioning.
- `Forms/*.cs`: event handling and form behavior only.
- `Controls/`: reusable visual controls such as the Tetris logo, board renderer and previews.

## Game layer
- `Game/TetrisGameEngine.cs`: gameplay rules, movement, line clearing, scoring, hold and game modes.
- `Game/BoardModel.cs`: board state and row/column operations.
- `Game/SpecialItems/`: Arcane Chaos relic logic.

## Managers / data layer
- `Managers/AudioManager.cs`: music, SFX and volume.
- `Managers/ScoreManager.cs`: TXT score read/write.
- `Models/`: data records and preferences.

This separation keeps interface code independent from gameplay rules and data handling.
