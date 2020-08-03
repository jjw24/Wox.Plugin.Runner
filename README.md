
This is a port of the Wox plugin [Runner](https://github.com/jessebarocio/Wox.Plugin.Runner) created by Jesse Barocio @jessebarocio based on the [fork](https://github.com/CrazyCoder/Wox.Plugin.Runner) from Serge Baranov @CrazyCoder.

This port is intended to be used for [Flow Launcher](https://github.com/Flow-Launcher/Flow.Launcher). It will not work for Wox.

To download this plugin, go to the latest [release](https://github.com/jjw24/Wox.Plugin.Runner/releases/latest) and extract the zip file to Flow's user data's plugin directory.

Changes contained in this port:

- Upgraded to .Net Core 3.1
- Changed Wox's plugin library to Flow's plugin library
- Removed MvvmLightLibs library
- Changed the location of the setting file commands.json
- Remove obsolete Runner.Configurator

Below are the changes from @CrazyCoder.

-------------------
# Wox.Plugin.Runner

A plugin that allows you to create simple command shortcuts in [Wox](http://getwox.com).

![Demo](demo.gif)

## This fork changes

This fork merges the changes from 2 other forks:
* Working directory support comes from [@mars888](https://github.com/mars888/Wox.Plugin.Runner).
* Support for correctly passing arguments when using global pattern (`*`) is by [@slav](https://github.com/slav/Wox.Plugin.Runner). For some reason it didn't work properly for me, so I had to fix it.
* A bug where arguments were not saved in the original version is fixed
* An icon from the configured command will be displayed instead of the fixed Runner icon

## Configuration

Configure the plugin in the Wox Settings window. Each item has four settings:

* Description - the human-readable command name that shows up in the Wox command list.
* Shortcut - the shortcut or alias you want to use.
* Path - the path to the program, or script you want to run.
* Working dir - directory where to start the process, Path program directory is used when empty
* Arguments - the arguments format to use when launching the program or script. This is a [.NET format string](https://msdn.microsoft.com/en-us/library/txafckwd.aspx).

## Examples

The following sets up a shortcut that opens a remote desktop session:

* Description - `Remote Desktop`
* Shortcut - `rdp`
* Path - `mstsc`
* Arguments - `/v:{0}`

Now all I need to do is type in `r rdp myserver`, hit enter, and a remote desktop session would be launched.
