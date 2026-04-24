# Steam Achievement Manager (SAM) — Automated Fork

This is a modernized fork of [gibbed's Steam Achievement Manager (SAM)](https://github.com/gibbed/SteamAchievementManager), with a fully automated achievement unlocking system. This fork introduces a new project, **SAM.PickerAuto**, which enables one-click, unattended unlocking of all achievements for all games in your Steam library.

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

## Attribution

Original project by [gibbed](https://github.com/gibbed/SteamAchievementManager). Most icons are from the [Fugue Icons](https://p.yusukekamiyamane.com/) set.

This fork adds full automation and headless achievement unlocking. See code comments and commit history for detailed implementation notes.
