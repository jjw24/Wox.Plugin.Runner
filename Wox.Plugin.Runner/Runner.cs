using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Flow.Launcher.Plugin;
using Wox.Plugin.Runner.ViewModel;

namespace Wox.Plugin.Runner;

public class Runner : IPlugin, ISettingProvider
{
    internal static PluginInitContext Context = null!;
    internal static Settings Settings = null!;
    private RunnerSettingsViewModel? _viewModel;

    public void Init(PluginInitContext context)
    {
        Context = context;
        Settings = context.API.LoadSettingJsonStorage<Settings>();
        _viewModel = new RunnerSettingsViewModel(Context);
    }

    public List<Result> Query(Query query)
    {
        List<Result> results;

        var search = query.Search;

        // triggers when an action keyword is set
        // shows all possible plugin commands
        if (string.IsNullOrEmpty(search))
        {
            results = Settings.Commands
                .Select(c =>
                    new Result
                    {
                        Score = 50,
                        Title = c.Description,
                        SubTitle = $"[{c.Shortcut}] {GetPathPreview(c.Path)}",
                        Action = _ => RunCommand(c),
                        IcoPath = !string.IsNullOrEmpty(c.Path) && File.Exists(c.Path) ? c.Path : "Images/gear.png"
                    })
                .ToList();
        }
        // triggers when no action keyword is set
        else
        {
            var splittedSearch = search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var shortcut = splittedSearch[0];

            var terms = splittedSearch[1..];

            // exact match found and shows to the user command is being run with arguments
            // score gets a boost based on how close the number of terms match. For example, if you have 3
            // identical keywords, each with a different number of parameters then they will be sorted based on
            // closeness to the number of parameters the user has typed.
            //
            // for example if the user types `pt hello there`, then a {0} and {1} version will have the lowest ranking
            // while the version with {2} will have the highest, followed by {3}, {4}...{*}.
            results = Settings.Commands.Where(c => c.Shortcut == shortcut)
                .Select(c => new Result
                {
                    Score = 50 + (terms.Length <= c.TermsCount ? terms.Length - c.TermsCount : -50),
                    Title = terms.Length > 0
                        ? $"{c.Description} → {string.Join(" ", terms)}"
                        : c.Description,
                    SubTitle = terms.Length > 0
                        ? $"[{c.Shortcut}] Run with arguments: {string.Join(" ", terms)}"
                        : $"[{c.Shortcut}] {GetPathPreview(c.Path)}",
                    Action = _ => RunCommand(c, terms),
                    IcoPath = !string.IsNullOrEmpty(c.Path) && File.Exists(c.Path) ? c.Path : "Images/gear.png"
                })
                .ToList();

            // no exact match found, tries to find a fuzzy match against existing plugin commands
            if (!results.Any()) results = FuzzySearchCommand(shortcut, terms);
        }

        return results;
    }

    public Control CreateSettingPanel()
    {
        return new RunnerSettings(_viewModel!);
    }

    private List<Result> FuzzySearchCommand(string shortcut, string[] terms)
    {
        return Settings.Commands.Select(c => new Result
            {
                Score = Context.API.FuzzySearch(shortcut, c.Shortcut).Score,
                Title = c.Description,
                SubTitle = $"[{c.Shortcut}] {GetPathPreview(c.Path)}",
                Action = _ => RunCommand(c, terms),
                IcoPath = !string.IsNullOrEmpty(c.Path) && File.Exists(c.Path) ? c.Path : "Images/gear.png"
            }).Where(r => r.Score > 0)
            .ToList();
    }

    private static string GetPathPreview(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return "No path specified";

        const int maxLength = 60;
        if (path.Length <= maxLength)
            return path;

        // Show start and end of path with ellipsis in middle
        var partLength = (maxLength - 3) / 2;
        return path.Substring(0, partLength) + "..." + path.Substring(path.Length - partLength);
    }

    private bool RunCommand(Command command, IEnumerable<string>? terms = null)
    {
        try
        {
            var args = GetProcessArguments(command, terms);
            
            // Validate that the file exists
            if (string.IsNullOrEmpty(args.FileName))
            {
                Context.API.ShowMsg("Error: Command Path Missing",
                    $"The command '{command.Description}' does not have a valid path configured.\n\n" +
                    "Please edit the command in the settings and specify the executable path.");
                return false;
            }

            if (!File.Exists(args.FileName) && !IsShellCommand(args.FileName))
            {
                Context.API.ShowMsg("Error: File Not Found",
                    $"The file does not exist:\n{args.FileName}\n\n" +
                    $"Command: {command.Description}\n" +
                    $"Please verify the path in the plugin settings.");
                Context.API.LogWarn(nameof(Runner), $"File not found: {args.FileName}");
                return false;
            }

            var startInfo = new ProcessStartInfo(args.FileName, args.Arguments)
            {
                UseShellExecute = true
            };

            if (command.RunAsAdministrator)
                startInfo.Verb = "runas";

            // Working directory if set via settings will be args.WorkingDirectory
            // If not set, args.WorkingDirectory will default to the directory of the executable
            if (Directory.Exists(args.WorkingDirectory))
            {
                startInfo.WorkingDirectory = args.WorkingDirectory;
            }
            else
            {
                Context.API.ShowMsg("Error: Working Directory Not Found",
                    $"The working directory does not exist:\n{args.WorkingDirectory}\n\n" +
                    $"The command will run from the application's directory instead.");
                Context.API.LogWarn(nameof(Runner), $"Working directory not found: {args.WorkingDirectory}");
            }

            Process.Start(startInfo);
        }
        catch (Win32Exception w32Ex)
        {
            // If a command needs elevation and the user hits "No" on the UAC dialog
            if (w32Ex.Message == "The operation was canceled by the user")
                return false;

            // File not found or other Win32 errors
            if (w32Ex.NativeErrorCode == 2) // ERROR_FILE_NOT_FOUND
            {
                Context.API.ShowMsg("Error: Cannot Start Process",
                    $"Windows cannot find the specified file:\n{command.Path}\n\n" +
                    $"Error: {w32Ex.Message}\n\n" +
                    "If you're trying to run a shell command (like 'echo' or 'dir'), " +
                    "you need to run it through cmd.exe or powershell.exe.\n\n" +
                    "Example: C:\\Windows\\System32\\cmd.exe\n" +
                    "Arguments: /c echo \"Test\"");
            }
            else
            {
                Context.API.ShowMsg("Error: Failed to Start Process",
                    $"An error occurred while trying to start:\n{command.Path}\n\n" +
                    $"Error Code: {w32Ex.NativeErrorCode}\n" +
                    $"Message: {w32Ex.Message}");
            }
            
            Context.API.LogException(nameof(Runner), "Failed to start process", w32Ex);
            return false;
        }
        catch (FormatException ex)
        {
            Context.API.ShowMsg("Error: Invalid Arguments Format",
                $"The arguments format for command '{command.Description}' is invalid.\n\n" +
                "Please check the Arguments field in the plugin settings.\n\n" +
                "Use {0}, {1}, {2}... for positional arguments or {*} for all arguments.");
            Context.API.LogException(nameof(Runner), "Argument format was invalid", ex);
            return false;
        }
        catch (Exception ex)
        {
            Context.API.ShowMsg("Error: Unexpected Error",
                $"An unexpected error occurred while running:\n{command.Description}\n\n" +
                $"Error: {ex.Message}\n\n" +
                "Check the Flow Launcher logs for more details.");
            Context.API.LogException(nameof(Runner), "Unexpected error running command", ex);
            return false;
        }

        return true;
    }

    private static bool IsShellCommand(string fileName)
    {
        // Check if it's a known shell built-in command or relative path
        var name = Path.GetFileName(fileName).ToLowerInvariant();
        var shellCommands = new[] { "cmd", "cmd.exe", "powershell", "powershell.exe", "pwsh", "pwsh.exe" };
        return shellCommands.Contains(name) || !Path.IsPathRooted(fileName);
    }

    private ProcessArguments GetProcessArguments(Command c, IEnumerable<string>? terms)
    {
        var argString = string.Empty;

        if (!string.IsNullOrEmpty(c.ArgumentsFormat))
        {
            // command's arguments HAS an infinite flag, thus user is able to manually pass infinite amount of arguments
            if (c.ArgumentsFormat.Contains("{*}"))
                // add user specified arguments to the arguments to be passed
                argString = c.ArgumentsFormat.Replace("{*}", terms != null ? string.Join(" ", terms) : "");
            // command's arguments HAS flag/s, thus user is able to manually pass in arguments e.g. settings: {0} {1}
            // or command's arguments HAS set normal text arguments e.g. settings: -h myremotecomp -p 22
            else
                argString = terms != null
                    ? string.Format(c.ArgumentsFormat, terms.ToArray<object?>())
                    : c.ArgumentsFormat;
        }

        var workingDir = c.WorkingDirectory;
        if (workingDir == "{explorer}")
        {
            var openExplorerPaths = ExplorerPathsService.GetOpenExplorerPaths();
            workingDir = openExplorerPaths.FirstOrDefault();
        }

        if (string.IsNullOrEmpty(workingDir))
            // Use directory where executable is based.
            workingDir = Path.GetDirectoryName(c.Path);

        return new ProcessArguments
        {
            FileName = c.Path,
            Arguments = argString,
            WorkingDirectory = workingDir
        };
    }

    private sealed class ProcessArguments
    {
        public string FileName { get; init; } = "";
        public string Arguments { get; init; } = "";
        public string? WorkingDirectory { get; init; }
    }
}