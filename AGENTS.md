# Repository Guidelines

## Project Structure & Module Organization

This is a Godot 4.2 C# game. `Godot/project.godot` is the editor entry point; `Godot/Screens/Master.tscn` is the main scene.

- `Godot/Code/`: gameplay orchestration (`GameCore.cs`, `CoreData.cs`), domain models in `Objects/`, scene controllers and UI helpers in `Platform/`, and shared utilities in `Utility/`.
- `Godot/Screens/` and `Godot/PreFabs/`: scenes and reusable scene components.
- `Godot/Sprites/`, `Sounds/`, `Fonts/`, and `Themes/`: runtime assets.
- `SourceData/` and `SourceMaterials/`: reference material, separate from the runtime project.

## Build, Test, and Development Commands

Use the Godot 4.2.1 .NET edition and a .NET SDK capable of building the project's `net6.0` desktop target. The project references `Godot.NET.Sdk/4.2.1`.

Run these commands from the repository root, with the .NET-enabled Godot executable available as `godot`:

- `dotnet restore Godot/Deuteros.csproj`: restore dependencies, including Newtonsoft.Json.
- `dotnet build Godot/Deuteros.csproj`: compile C# and report errors.
- `godot --path Godot --editor`: open the project and import assets.
- `godot --path Godot`: launch the configured main scene after building; alternatively, press F5 in the editor.

Export presets contain developer-specific output paths; select a local destination before exporting.

## Coding Style & Naming Conventions

Match surrounding formatting: many scene scripts use tabs, while some model and utility files use four spaces. Keep braces on separate lines and avoid unrelated reformatting. Use PascalCase for types, methods, properties, and matching filenames; camelCase for parameters and locals. Interfaces use an `I` prefix. Preserve Godot callback names such as `_Ready`, and keep scene node paths synchronized with scripts. No formatter or linter configuration is checked in.

## Testing Guidelines

There is no automated test project, test naming convention, or coverage threshold. Build before submitting, then manually exercise affected screens and gameplay. Check scene transitions, time advancement, and save/load behavior when relevant. Record reproduction and verification steps in the PR. The existing GitHub workflow sends Discord notifications; it does not validate builds.

## Commit & Pull Request Guidelines

History uses informal, action-oriented messages such as `fix compile error` and `add methanoid attacks`, without a mandatory prefix scheme. Keep commits focused and summaries specific. PRs should describe behavior changes, link relevant issues, include build/manual-check results, and provide screenshots for visible UI changes.
