using Flow.Launcher.Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Wox.Plugin.Runner.Settings;

namespace Wox.Plugin.Runner
{
    public class Runner : IPlugin, ISettingProvider
    {
        internal static PluginInitContext Context;
        RunnerSettingsViewModel viewModel;

        public void Init( PluginInitContext context )
        {
            Context = context;
            viewModel = new RunnerSettingsViewModel(Context);
        }

        public List<Result> Query( Query query )
        {
            var results = new List<Result>();

            if (string.IsNullOrEmpty(query.Search))
                return results;

            var search = query.Search;

            var splittedSearch = search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var shortcut = splittedSearch[0];

            var terms = splittedSearch[1..];

            var matches = 
                RunnerConfiguration.Commands
                .Where(c => c.Shortcut == shortcut)
                .Select(c => 
                    new Result()
                    {
                        Score = 50,
                        Title = "Run " + (c.Description ?? $"shortcut {c.Shortcut}") + 
                            (terms.Count() > 0 ? $" with arguments: {string.Join(" ", terms)}" : string.Empty) ,
                        SubTitle = c.Description,
                        Action = e => RunCommand(e, terms, c),
                        IcoPath = c.Path
                    });
            return matches.ToList();
        }

        public Control CreateSettingPanel()
        {
            return new RunnerSettings(viewModel);
        }

        private bool RunCommand( ActionContext e, IEnumerable<string> terms, Command command )
        {
            try
            {
                var args = GetProcessArguments(command, terms);
                var startInfo = new ProcessStartInfo(args.FileName, args.Arguments);
                if (args.WorkingDirectory != null) {
                    startInfo.WorkingDirectory = args.WorkingDirectory;
                }
                Process.Start(startInfo);
            }
            catch ( Win32Exception w32Ex )
            {
                // If a command needs elevation and the user hits "No" on the UAC dialog an exception is thrown
                // with this message. We want to ignore this exception but throw any others.
                if ( w32Ex.Message != "The operation was canceled by the user" )
                    throw;
            }
            catch ( FormatException )
            {
                Context.API.ShowMsg("There was a problem. Please check the arguments format for the command.");
            }
            return true;
        }

        private ProcessArguments GetProcessArguments( Command c, IEnumerable<string> terms )
        {
            var argString = string.Empty;

            if (!string.IsNullOrEmpty(c.ArgumentsFormat))
            {
                if (c.ArgumentsFormat.EndsWith("{*}"))
                {
                    argString = c.ArgumentsFormat.Remove(c.ArgumentsFormat.Length-3, 3) + string.Join(" ", terms);
                }
                else 
                {
                    argString = string.Format(c.ArgumentsFormat, terms.ToArray());
                }
            }

            var workingDir = c.WorkingDirectory;
            if (string.IsNullOrEmpty(workingDir)) {
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
            public string FileName { get; set; }
            public string Arguments { get; set; }
            public string WorkingDirectory { get; set; }
        }
    }
}
