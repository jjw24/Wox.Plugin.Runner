
This is a port of the Wox plugin [Runner](https://github.com/jessebarocio/Wox.Plugin.Runner) created by Jesse Barocio (@jessebarocio), and based on the work from the [fork](https://github.com/CrazyCoder/Wox.Plugin.Runner) by Serge Baranov (@CrazyCoder).

This port is intended to be used for [Flow Launcher](https://github.com/Flow-Launcher/Flow.Launcher). It will not work for Wox.

**New with this port:** 

Use {*} flag in your setting argument to allow infinite arguments passed through the query window. For this to work the setting argument needs to end with `{*}`

Use {0} in your setting argument to pass just one argument. For this to work the setting argument needs to end with `{0}`, e.g. `-p {0}`. You can also specify multiple additional arguments e.g. `-h {0} -p {1}` with query `r shortcut1 myremotecomp 22`, this will pass the arguments in as `-h myremotecomp -p 22`.

**Installation**

To use this plugin, from your Flow Launcher search `pm install runner`

**Important:**

The original Wox plugin saves its setting file (commands.json) in `%localappdata%\Wox.Plugin.Runner`, so to keep using the same commands you had, you need to move that file manually to the location above inside the `Wox.Plugin.Runner\Settings` folder after installing.

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
