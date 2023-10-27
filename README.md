# PingUI

A simple GUI for continuously pinging targets.

## Installation

PingUI is a portable application which has releases compiled for 64bit Windows. Simply download a zip file from [releases](https://github.com/RichardRobertson/PingUI/releases), extract all the files together, and execute `PingUI.exe`.

For other environments, please see [Compiling](#compiling) below.

## Usage

On launch, the user will be presented with an empty window with a `+` button. Clicking `+` will show a dialog asking for information on the new target. At the top, the IP address can be set. Both IPv4 and IPv6 are allowed. If you type a hostname, the app will attempt to resolve it using the system DNS settings and offer IP address suggestions to click. In the middle, a human readable label can be assigned to this target. At the bottom, you can enter an amount of time to wait between pings, whether successful or not. It must be at least one second and no greater than 23 hours, 59 minutes, and 59 seconds.

After you have a target on the list, you may start or stop the ping loop by using the play-pause toggle button. When a target receives a failure result, it will highlight red and show an alert icon. Clicking the alert bell will dismiss the alert until the next time the status reports a failure. Clicking on a target but not on one of the buttons will expand a list below showing the times that the status changed. This will indicate when it started receiving specific success or failure results.

Each target item has a right click menu with options to edit the target, clear its history, or delete it entirely.

On exit, all targets will be saved to a configuration file. By default, this will be located at `%APPDATA%\PingUI\config.json` on Windows. To use a portable configuration file simply create a new file named `config.json` in the same directory as `PingUI.exe` and type `{}` inside it. The application will detect this file as a valid JSON document and store targets and settings there.

## Contributing

Issue reports and pull requests are welcome. When submitting code, please follow the settings in [.editorconfig](.editorconfig).

### Compiling

To compile, use `dotnet build` or `dotnet publish` while in the repository directory. Publish is currently set up to create a trimmed, self contained, single-file build. Native libraries will still be published as separate files and must be included with the executable when running.

For environments other than 64 bit Windows, change the `RuntimeIdentifier` value in [PingUI.csproj](PingUI/PingUI.csproj) and then compile. It is set to `win-x64` by default in the repository, and I do not have a means to test other environments.
