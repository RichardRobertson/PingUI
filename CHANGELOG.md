# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

# [3.0.1] - 2024-06-28

### Fixed

- Properly initialize LocalOrAppDataJsonConfiguration.AutomaticTagEntries

# [3.0.0] - 2024-06-28

### Added

- Visible counter on the right edge of the ping result history if a given status repeats
- Can use enter and escape to confirm or cancel dialogs respectively ([#19](https://github.com/RichardRobertson/PingUI/issues/19))
- Target tagging system ([#20](https://github.com/RichardRobertson/PingUI/issues/20))
	- Tag entry bar on edit target view
	- Tags display in tooltip when hovering over a target in details view
	- Filter button added to top of window
	- Drag arrange is disabled while list is filtered
	- Automatic tagging system based on the label or address

### Changed

- Replaced close button path glyph with `SymbolIcon` on main window and settings popup

# [2.0.0] - 2024-03-12

### Added

- New menu drawer to remove clutter from window controls area
- New condensed view available in the menu drawer ([#14](https://github.com/RichardRobertson/PingUI/issues/14))
	- Includes details view option to return to the original appearance
- Delete all targets button added to menu drawer ([#16](https://github.com/RichardRobertson/PingUI/issues/16))
- Ability to drag targets on the main detail view to rearrange them ([#17](https://github.com/RichardRobertson/PingUI/issues/17))

### Changed

- Settings and start/stop all target pings moved to menu drawer

### Removed

- Move Up and Move Down context menu items for targets

# [1.4.0] - 2024-02-26

### Added

- Added dropdown to start/stop all target pings at once ([#15](https://github.com/RichardRobertson/PingUI/issues/15))

### Fixed

- Fixed scrollbar on main view ([#13](https://github.com/RichardRobertson/PingUI/issues/13))

## [1.3.1] - 2024-02-14

### Added

- Settings panel
	- Checkbox to check online for updates ([#12](https://github.com/RichardRobertson/PingUI/Issues/12))
	- Checkbox to remember window location

- Ability to restore window location across program launches

## [1.3.0] - 2024-01-26

### Added

- Set focus to textbox in edit target dialog ([#11](https://github.com/RichardRobertson/PingUI/Issues/11))

### Changed

- Precompiled binaries are now AOT compiled for win-x64

### Fixed

- Localize ping results ([#10](https://github.com/RichardRobertson/PingUI/issues/10))

## [1.2.1] - 2023-11-10

### Fixed

- Incorrect JSON parsing in update check ([#9](https://github.com/RichardRobertson/PingUI/issues/9))

## [1.2.0] - 2023-11-9

### Added

- Automatic zip and msi package creation on release publish
- Ability to check for updates on startup ([#8](https://github.com/RichardRobertson/PingUI/issues/8))
- New application icon to get rid of default Avalonia logo

## [1.1.0] - 2023-10-27

### Added

- Repeat count parameter for `FakeTogglePinger` which assists in testing
- Ability to move targets up and down in the list ([#4](https://github.com/RichardRobertson/PingUI/issues/4))
- Ability to resolve hostnames instead of only typing in IP addresses ([#2](https://github.com/RichardRobertson/PingUI/issues/2))

### Changed

- Updated Nuget package references
- History is now only preserved when `TargetViewModel.Target.Address` does not change ([#5](https://github.com/RichardRobertson/PingUI/issues/5))
- `TargetViewModel.Target` can now be updated ([#7](https://github.com/RichardRobertson/PingUI/issues/7))

### Fixed

- `TargetViewModel` alerts only trigger once per state change ([#1](https://github.com/RichardRobertson/PingUI/issues/1))
- `IPinger` implementation scheduling fix ([#3](https://github.com/RichardRobertson/PingUI/issues/3))
- Prevent adding two identical targets to the list ([#6](https://github.com/RichardRobertson/PingUI/issues/6))

## [1.0.0] - 2023-9-27

First release
