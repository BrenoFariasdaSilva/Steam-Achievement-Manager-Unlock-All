<div align="center">
  
# [Steam-Achievement-Manager-Unlock-All](https://github.com/BrenoFariasdaSilva/Steam-Achievement-Manager-Unlock-All) <img src="https://github.com/BrenoFariasdaSilva/Steam-Achievement-Manager-Unlock-All/blob/b9551c46ea13749dea35c19897396fa462a444e7/.assets/Icons/Steam.svg"  width="3%" height="3%">

</div>

<div align="center">
  
---

This is a modernized fork of [gibbed's Steam Achievement Manager (SAM)](https://github.com/gibbed/SteamAchievementManager), with a fully automated achievement unlocking system. This fork introduces a new project, **SAM.PickerAuto**, which enables one-click, unattended unlocking of all achievements for all games in your Steam library.
  
---

</div>

<div align="center">

![GitHub Code Size in Bytes](https://img.shields.io/github/languages/code-size/BrenoFariasdaSilva/Steam-Achievement-Manager-Unlock-All)
![GitHub Commits](https://img.shields.io/github/commit-activity/t/BrenoFariasdaSilva/Steam-Achievement-Manager-Unlock-All/master)
![GitHub Last Commit](https://img.shields.io/github/last-commit/BrenoFariasdaSilva/Steam-Achievement-Manager-Unlock-All)
![GitHub Forks](https://img.shields.io/github/forks/BrenoFariasdaSilva/Steam-Achievement-Manager-Unlock-All)
![GitHub Language Count](https://img.shields.io/github/languages/count/BrenoFariasdaSilva/Steam-Achievement-Manager-Unlock-All)
![GitHub License](https://img.shields.io/github/license/BrenoFariasdaSilva/Steam-Achievement-Manager-Unlock-All)
![GitHub Stars](https://img.shields.io/github/stars/BrenoFariasdaSilva/Steam-Achievement-Manager-Unlock-All)
![GitHub Contributors](https://img.shields.io/github/contributors/BrenoFariasdaSilva/Steam-Achievement-Manager-Unlock-All)
![GitHub Created At](https://img.shields.io/github/created-at/BrenoFariasdaSilva/Steam-Achievement-Manager-Unlock-All)
![wakatime](https://wakatime.com/badge/github/BrenoFariasdaSilva/Steam-Achievement-Manager-Unlock-Allsvg)

</div>

<div align="center">
  
![RepoBeats Statistics](https://repobeats.axiom.co/api/embed/f833f8caf83561972596ad8ce7aee46c5e58a164.svg "Repobeats analytics image")

</div>


## Features

- **Original SAM Functionality**: Manage, unlock, and edit Steam achievements and statistics for any game, with a portable WinForms UI.
- **SAM.PickerAuto**: A new executable (`SAM.PickerAuto.exe`) that automates the achievement unlocking process for all games in your list, requiring only a single click.
- **Unlock All Button**: Added to the main toolbar, this button triggers a fully automated, asynchronous process that:
  - Iterates through every visible game in the list (respects current filters/search).
  - Launches `SAM.Game.exe` in headless mode for each AppID, passing a `--unlock-all` argument.
  - Waits for the achievement list to load, selects all achievements, commits them to Steam, and closes the process before moving to the next game.
  - Runs in the background without freezing the UI, with progress and results shown in the status bar.
- **Headless Mode for SAM.Game**: `SAM.Game.exe` now supports a `--unlock-all` argument for silent, non-interactive unlocking (no UI, no dialogs, exit code signals result).
- **Robust Inter-Process Automation**: All automation is handled via process invocation and public API, with no UI automation or message-pumping hacks.

## Directory Structure

```
SAM.API/           # Steamworks API interop and callbacks
SAM.Game/          # Achievement manager for a single game (now supports headless automation)
SAM.Picker/        # Standard game picker UI
SAM.PickerAuto/    # Automated picker with Unlock All button and automation logic
SAM.sln            # Solution file (includes all four projects)
```

## Building the Solution

1. **Project Integration**
	- `SAM.PickerAuto` is a direct copy of `SAM.Picker`, renamed and registered in `SAM.sln` with a unique GUID.
	- `SAM.PickerAuto.csproj` is updated with `<AssemblyName>SAM.PickerAuto</AssemblyName>` so it builds to `SAM.PickerAuto.exe`.
	- All four projects are included in the solution and build together. No project references are required between Picker/PickerAuto.

2. **.sln and .csproj Changes**
	- `SAM.sln` contains entries for all four projects, including the new `SAM.PickerAuto` with its own build configs.
	- `SAM.PickerAuto.csproj` is renamed and updated as above.

## Usage

### Standard Mode

Run `SAM.Picker.exe` to manually select and unlock achievements for individual games, as in the original SAM.

### Automated Mode (Unlock All)

1. Run `SAM.PickerAuto.exe`.
2. Use the search/filter as desired to limit the games to process.
3. Click the **Unlock All** button (located to the right of the game filtering button in the toolbar).
4. The app will:
	- Disable the Unlock All and Refresh buttons.
	- For each game in the filtered list:
	  - Launch `SAM.Game.exe <AppID> --unlock-all` in headless mode (no window).
	  - Wait for the process to finish (fully async, UI remains responsive).
	  - Update the status bar with progress and exit code.
	- When complete, re-enable the buttons and show a summary dialog.

#### Exit Codes (from SAM.Game.exe in headless mode)

- `0`: Success (all achievements unlocked)
- `1`: Steam/stats error
- `2`: Client initialization failure
- `3`: Steamworks DLL not found

## Implementation Details

### UI Changes

- In `SAM.PickerAuto/GamePicker.Designer.cs`, a new `ToolStripButton` labeled **Unlock All** is added immediately after the game filtering button. It is wired to an async handler (`OnUnlockAll`).

### Automation Logic

- In `SAM.PickerAuto/GamePicker.cs`, the `OnUnlockAll` handler:
  - Snapshots the filtered game list.
  - Disables relevant UI buttons.
  - For each game, launches `SAM.Game.exe <AppID> --unlock-all` with `UseShellExecute=false` and `CreateNoWindow=true`.
  - Awaits process exit asynchronously.
  - Updates the status bar with progress.
  - Re-enables UI and shows a summary when done.

### Headless Unlocking (SAM.Game)

- In `SAM.Game/Program.cs`, a new code path is added:
  - If the second argument is `--unlock-all`, the app runs in headless mode (no forms, no dialogs).
  - Calls into a new static class `AutoUnlocker` to perform the unlock logic:
	 - Waits for Steam stats to load.
	 - Loads all achievement IDs.
	 - Unlocks all eligible achievements and commits them.
	 - Exits with the appropriate code.

### Inter-Process Communication

- No IPC or message-passing is required. All automation is handled by launching `SAM.Game.exe` with arguments and monitoring its exit code.

## File Changes Overview

**1. SAM.sln**
	- Added project entry for `SAM.PickerAuto` with unique GUID and build configs.

**2. SAM.PickerAuto.csproj**
	- Renamed from `SAM.Picker.csproj`.
	- Set `<AssemblyName>SAM.PickerAuto</AssemblyName>`.

**3. GamePicker.Designer.cs (SAM.PickerAuto)**
	- Added `Unlock All` button to the toolbar after the filter button.

**4. GamePicker.cs (SAM.PickerAuto)**
	- Added `using System.Threading.Tasks;`.
	- Implemented `OnUnlockAll` async handler as described above.

**5. Program.cs (SAM.Game)**
	- Added headless mode: parses `--unlock-all` argument, runs unlock logic, exits with code, no UI.

**6. AutoUnlocker.cs (SAM.Game)**
	- New static class for headless automation logic: waits for stats, unlocks all, commits, returns status.

## Development Setup (Visual Studio / Build Instructions)

This section explains how to install the required tools, open the project, and build the solution to generate the executables, including `SAM.PickerAuto.exe`.

---

### 1. Install Visual Studio (Required)

#### Recommended Version
- Visual Studio Community (free)

#### Download
- https://visualstudio.microsoft.com/vs/community/

#### Required Workload (MANDATORY)
During installation, select:

- **.NET Desktop Development**

This is required for:
- WinForms (UI projects)
- C# build tools
- MSBuild integration

---

### 2. Required Components

Inside Visual Studio Installer, ensure the following are selected:

#### Individual Components
- .NET Framework 4.x targeting pack (match solution target version)
- MSBuild
- Windows 10/11 SDK
- C# and Visual Basic Roslyn compiler
- Git for Windows (optional but recommended)

---

### 3. Opening the Project

1. Open Visual Studio
2. Click:

```
File → Open → Project/Solution
```

3. Select:

```
SAM.sln
```

4. Wait for:
- NuGet restore (automatic)
- Project indexing
- Solution load completion

---

### 4. Restore Dependencies

If required:

```
Build → Restore NuGet Packages
```

or right-click solution:

```
Restore NuGet Packages
```

---

### 5. Build the Solution

#### Build Entire Solution

```
Build → Build Solution
```

#### Keyboard Shortcut (Windows)
```
Ctrl + Shift + B
```

This will compile:
- SAM.API
- SAM.Game
- SAM.Picker
- SAM.PickerAuto

---

#### Cancel Build
```
Ctrl + Break
```

---

#### Compile Active Project Only
```
Ctrl + F7
```

---

#### Run Code Analysis (Optional)
```
Alt + F11
```

---

### 6. Output Location

After successful build, executables will be generated in:

```
/bin/Debug/
/bin/Release/
```

Depending on configuration:

- `SAM.Picker.exe`
- `SAM.PickerAuto.exe`
- `SAM.Game.exe`

---

### 7. Running the Application

#### Standard Mode
Run:
```
SAM.Picker.exe
```

#### Automated Mode
Run:
```
SAM.PickerAuto.exe
```

Then click:
```
Unlock All
```

---

### 8. Build Configuration Notes

Ensure configuration is set correctly in Visual Studio:

- Debug → for development
- Release → for production build

Use top toolbar:

```
Solution Configuration: Release
Solution Platform: Any CPU
```

---

### 9. Common Build Issues

#### Missing .NET Target
- Install required .NET Framework via Visual Studio Installer

#### NuGet Errors
- Restore packages manually

#### Build Fails on SAM.Game
- Ensure Steamworks dependencies are present in project references

## Attribution

Original project by [gibbed](https://github.com/gibbed/SteamAchievementManager). Most icons are from the [Fugue Icons](https://p.yusukekamiyamane.com/) set.

This fork adds full automation and headless achievement unlocking. See code comments and commit history for detailed implementation notes.

## License

### Apache License 2.0

This project is licensed under the [Apache License 2.0](LICENSE). This license permits use, modification, distribution, and sublicense of the code for both private and commercial purposes, provided that the original copyright notice and a disclaimer of warranty are included in all copies or substantial portions of the software. It also requires a clear attribution back to the original author(s) of the repository. For more details, see the [LICENSE](LICENSE) file in this repository.
