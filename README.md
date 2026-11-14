# Deuteros Resurrected

A work-in-progress C# / Godot remake of **Deuteros: The Next Millennium** (1991). The project recreates its resource management, research, production, ships and interplanetary logistics.

## Build and run

Install these matching versions:

- **Godot 4.2.2 .NET** (the standard GDScript-only editor cannot build this project).
- **.NET SDK 6.0.428**, pinned by `global.json`.

## Controls

Use the mouse to select screens and interact with controls. The top navigation bar includes time advancement; right-click closes applicable subwindows or returns to the overview when available. **Escape** opens settings. Some navigation behavior is still tracked in Asana.

## Project layout

- `Godot/Code/Objects/`: game state and domain models.
- `Godot/Code/Platform/`: screen controllers, buttons and shared UI behavior.
- `Godot/Screens/`, `Godot/PreFabs/`: scenes and reusable components.
- `Godot/Sprites/`, `Sounds/`, `Fonts/`, `Themes/`: game assets.
- `SourceData/`, `SourceMaterials/`: historical reference material.

## Contributing and credits

Please put your hand up on our Discord server to get access as a contributor

Original game: Ian Bird / Activision. Repository license: [CC0 1.0](LICENSE).
