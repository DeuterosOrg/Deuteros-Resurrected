# Deuteros Resurrected

A work-in-progress C# / Godot remake of **Deuteros: The Next Millennium** (1991). The project recreates its resource management, research, production, ships and interplanetary logistics.

Development and bug reports are tracked in [Deuteros Development on Asana](https://app.asana.com/1/507237966097081/project/1214891399253076). See the [backlog assessment](docs/asana-triage.md) for the current implementation gaps and questions requiring original-game evidence. Save/load and several later-game features are incomplete.

## Build and run

Install these matching versions:

- **Godot 4.2.1 .NET** (the standard GDScript-only editor cannot build this project).
- **.NET SDK 6.0.428**, pinned by `global.json`.
- **Python 3.9+** for the portable validation and download scripts.

The version pins reproduce the existing project. A supported engine/.NET upgrade should be evaluated separately from gameplay fixes.

Download [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) for your machine's architecture. The SDK includes the required runtime. Install the matching Godot binary from the [official release](https://github.com/godotengine/godot/releases/tag/4.2.1-stable), or use our checksum-verified installer:

```sh
python3 scripts/install_godot.py
dotnet build Godot/Deuteros.csproj
```

The installer prints the Godot executable path. Put that executable on `PATH` as `godot`, or set `GODOT` to its full path when using the validation script. On Windows, use `python` in place of `python3`.

Open `Godot/project.godot` in the .NET editor and press **F5**, or run:

```sh
godot --path Godot --editor
godot --path Godot
```

Allow the first editor import to finish before running. For an SDK installed in a custom directory, set `DOTNET_ROOT` to that directory and add it to `PATH` before launching Godot.

## Validate changes

```sh
python3 scripts/validate.py --godot /full/path/to/Godot
```

This builds C#, imports assets, runs the regression scene against the real game code, and starts/exits the actual entry scene. It checks engine logs as well as process exit codes. Logs are saved in `artifacts/validation/`. See [testing details](docs/testing.md) for coverage and [validation results](docs/validation-results.md) for verified fixes and known runtime limitations.

## Controls

Use the mouse to select screens and interact with controls. The top navigation bar includes time advancement; right-click closes applicable subwindows or returns to the overview when available. **Escape** opens settings. Some navigation behavior is still tracked in Asana.

## Project layout

- `Godot/Code/Objects/`: game state and domain models.
- `Godot/Code/Platform/`: screen controllers, buttons and shared UI behavior.
- `Godot/Screens/`, `Godot/PreFabs/`: scenes and reusable components.
- `Godot/Sprites/`, `Sounds/`, `Fonts/`, `Themes/`: game assets.
- `Godot/Tests/`: engine-hosted C# regression tests and startup smoke script.
- `scripts/`: portable installation and validation commands.
- `SourceData/`, `SourceMaterials/`: historical reference material.

## Save and load

The disk menu now provides five local save slots, overwrite/load confirmation and backups. See [save files](docs/save-files.md) for storage, recovery and format details.

## Windows export

For native Windows verification and continued backlog work, follow the [Windows agent brief](docs/windows-agent-brief.md).

Install matching .NET export templates, then validate and export:

```sh
python3 scripts/install_godot.py --templates
python3 scripts/validate.py --godot /full/path/to/Godot --export-windows
```

The output is `artifacts/windows/Deuteros.exe` and its supporting files. Distribute the entire output directory. The added CI workflow is configured to check Linux and Windows and retain validation logs and the Windows build as workflow artifacts; export success alone does not verify Windows gameplay.

## Contributing and credits

Read [CONTRIBUTING.md](CONTRIBUTING.md). The companion [deuteros-parallel](https://github.com/WizzoUK2/deuteros-parallel) project contains additional reverse-engineering research and tests; some access may require permission. Its documented divergences must be considered when using it as behavioral evidence.

Original game: Ian Bird / Activision. Repository license: [CC0 1.0](LICENSE).
