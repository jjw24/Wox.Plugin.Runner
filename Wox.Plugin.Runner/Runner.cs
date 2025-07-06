using Flow.Launcher.Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Wox.Plugin.Runner.ViewModel;

namespace Wox.Plugin.Runner
{
    public class Runner : IPlugin, ISettingProvider
    {
        internal static PluginInitContext Context = null!;
        RunnerSettingsViewModel? viewModel;
        internal static Settings _settings;

        public void Init(PluginInitContext context)
        {
            Context = context;
            _settings = context.API.LoadSettingJsonStorage<Settings>();
            viewModel = new RunnerSettingsViewModel(Context);
        }

        public List<Result> Query(Query query)
        {
            var results = new List<Result>();

            var search = query.Search;

            // triggers when an action keyword is set
            // shows all possible plugin commands
            if (string.IsNullOrEmpty(search))
            {
                results = _settings.Commands
                    .Select(c =>
                        new Result()
                        {
                            Score = 50,
                            Title = c.Shortcut,
                            SubTitle = c.Description,
                            Action = e =>
                            {
                                return RunCommand(e, c);
                            },
                            IcoPath = c.Path
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
                results = _settings.Commands.Where(c => c.Shortcut == shortcut)
                    .Select(c => new Result()
                    {
                        Score = 50 + (terms.Length <= c.TermsCount ? terms.Length - c.TermsCount : -50),
                        Title = "Run " + (c.Description ?? $"shortcut {c.Shortcut}") +
                                (terms.Count() > 0 ? $" with arguments: {string.Join(" ", terms)}" : string.Empty),
                        SubTitle = c.Description,
                        Action = e => RunCommand(e, c, terms),
                        IcoPath = c.Path
                    })
                    .ToList();

                // no exact match found, tries to find a fuzzy match against existing plugin commands
                if (!results.Any())
                {
                    results = FuzzySearchCommand(shortcut, terms);
                }
            }

            return results;
        }

        private List<Result> FuzzySearchCommand(string shortcut, string[] terms)
        {
            return _settings.Commands.Select(c => new Result()
            {
                Score = Context.API.FuzzySearch(shortcut, c.Shortcut).Score,
                Title = c.Shortcut,
                SubTitle = c.Description,
                Action = e => RunCommand(e, c, terms),
                IcoPath = c.Path
            }).Where(r => r.Score > 0)
            .ToList();
        }

        public Control CreateSettingPanel()
        {
            return new RunnerSettings(viewModel!);
        }

        private bool RunCommand(ActionContext e, Command command, IEnumerable<string>? terms = null)
        {
            try
            {
                var args = GetProcessArguments(command, terms);
                var startInfo = new ProcessStartInfo(args.FileName, args.Arguments)
                {
                    UseShellExecute = true
                };
                if (args.WorkingDirectory != null)
                {
                    startInfo.WorkingDirectory = args.WorkingDirectory;
                }
                Process.Start(startInfo);
            }
            catch (Win32Exception w32Ex)
            {
                // If a command needs elevation and the user hits "No" on the UAC dialog an exception is thrown
                // with this message. We want to ignore this exception but throw any others.
                if (w32Ex.Message != "The operation was canceled by the user")
                    throw;
            }
            catch (FormatException ex)
            {
                Context.API.ShowMsg("There was a problem. Please check the arguments format for the command.");
                Context.API.LogException(nameof(Runner), "Argument format was invalid", ex);
            }
            return true;
        }

        private ProcessArguments GetProcessArguments(Command c, IEnumerable<string>? terms)
        {
            var argString = string.Empty;

            if (!string.IsNullOrEmpty(c.ArgumentsFormat))
            {
                // command's arguments HAS an infinite flag, thus user is able to manually pass infinite amount of arguments
                if (c.ArgumentsFormat.Contains("{*}"))
                {
                    // add user specified arguments to the arguments to be passed
                    argString = c.ArgumentsFormat.Replace("{*}", terms != null ? string.Join(" ", terms) : "");
                }
                // command's arguments HAS flag/s, thus user is able to manually pass in arguments e.g. settings: {0} {1}
                // or command's arguments HAS set normal text arguments e.g. settings: -h myremotecomp -p 22
                else
                {
                    argString = terms != null
                                    ? string.Format(c.ArgumentsFormat, terms.ToArray())
                                    : c.ArgumentsFormat;
                }
            }

            var workingDir = c.WorkingDirectory;
            if (workingDir == "{explorer}")
            {
                var openExplorerPaths = ExplorerPathsService.GetOpenExplorerPaths();
                workingDir = openExplorerPaths.FirstOrDefault();
            }

            if (string.IsNullOrEmpty(workingDir))
            {
                // Use directory where executable is based.
                workingDir = Path.GetDirectoryName(c.Path);
            }

            return new ProcessArguments
            {
                FileName = c.Path,
                Arguments = argString,
                WorkingDirectory = workingDir
            };
        }

        class ProcessArguments
        {
            public string FileName { get; set; } = "";
            public string Arguments { get; set; } = "";
            public string? WorkingDirectory { get; set; }
        }
    }
}