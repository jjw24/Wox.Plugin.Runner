using Flow.Launcher.Plugin;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Wox.Plugin.Runner.Services;
using Wox.Plugin.Runner.Settings;

namespace Wox.Plugin.Runner
{
    public class Runner : IPlugin, ISettingProvider
    {
        PluginInitContext initContext;
        bool isGlobal;

        public void Init( PluginInitContext context )
        {
            if ( !SimpleIoc.Default.IsRegistered<IMessageService>() )
            {
                SimpleIoc.Default.Register<IMessageService>( () => new MessageService() );
            }
            initContext = context;
            isGlobal = context.CurrentPluginMetadata.ActionKeywords.Contains(Flow.Launcher.Plugin.Query.GlobalPluginWildcardSign);
        }

        public List<Result> Query( Query query )
        {
            var results = new List<Result>();
            if (query.Terms.Length < 2 && !this.isGlobal) return results;

            var commandName = query.Terms[isGlobal ? 0 : 1];
            var terms = query.Terms.ToList();
            terms.RemoveAt(0); // remove command name
            var matches = RunnerConfiguration.Commands.Where( c => c.Shortcut.StartsWith( commandName ) )
                .Select( c => new Result()
                {
                    Score = int.MaxValue / 2,
                    Title = c.Description + " " + String.Join( " ", terms ),
                    Action = e => RunCommand( e, terms, c ),
                    IcoPath = c.Path
                } );
            results.AddRange( matches );
            return results;
        }

        public Control CreateSettingPanel()
        {
            return new RunnerSettings( new RunnerSettingsViewModel( initContext ) );
        }

        private bool RunCommand( ActionContext e, List<string> terms, Command command )
        {
            try
            {
                var args = GetProcessArguments( command, terms );
                var startInfo = new ProcessStartInfo(args.FileName, args.Arguments);
                if (args.WorkingDirectory != null) {
                    startInfo.WorkingDirectory = args.WorkingDirectory;
                }
                Process.Start(startInfo);
                // Process.Start( args.FileName, args.Arguments );
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
                SimpleIoc.Default.GetInstance<IMessageService>().ShowErrorMessage(
                    "There was a problem. Please check the arguments format for the command." );
            }
            return true;
        }

        private ProcessArguments GetProcessArguments( Command c, List<string> terms )
        {
            var argString = String.Empty;
            if ( !String.IsNullOrEmpty( c.ArgumentsFormat ) )
            {
                var arguments = terms;

                if ( !isGlobal )
                    arguments.RemoveAt( 0 );
                if ( arguments.Count > 0 )
                {
                    argString = String.Format( c.ArgumentsFormat, arguments.ToArray() );
                }
                else
                    argString = String.Empty;
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
