
namespace Wox.Plugin.Runner.ViewModel
{
    public class CommandViewModel
    {
        public CommandViewModel( Command command )
        {
            Command = command;
            description = command.Description;
            shortcut = command.Shortcut;
            path = command.Path;
            workingDirectory = command.WorkingDirectory;
            argumentsFormat = command.ArgumentsFormat;
            runAsAdministrator = command.RunAsAdministrator;
        }

        public Command Command { get; set; }

        private string description;
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
                CheckDirty();
            }
        }

        private string shortcut;
        public string Shortcut
        {
            get
            {
                return shortcut;
            }
            set
            {
                shortcut = value;
                CheckDirty();
            }
        }

        private string path;
        public string Path
        {
            get
            {
                return path;
            }
            set
            {
                path = value;
                CheckDirty();
            }
        }

        private string workingDirectory;

        public string WorkingDirectory
        {
            get {
                return workingDirectory;
                
            }
            set {
                workingDirectory = value;
                CheckDirty();
            }
        }

        private string argumentsFormat;
        public string ArgumentsFormat
        {
            get
            {
                return argumentsFormat;
            }
            set
            {
                argumentsFormat = value;
                CheckDirty();
            }
        }

        private bool runAsAdministrator;
        public bool RunAsAdministrator
        {
            get
            {
                return runAsAdministrator;
            }
            set
            {
                runAsAdministrator = value;
                CheckDirty();
            }
        }

        public bool IsDirty { get; set; } = false;

        public Command GetCommand()
        {
            if ( !IsDirty )
                return Command;
            else
                return new Command
                {
                    Description = Description,
                    Shortcut = Shortcut,
                    Path = Path,
                    WorkingDirectory = WorkingDirectory,
                    ArgumentsFormat = ArgumentsFormat,
                    RunAsAdministrator = RunAsAdministrator
                };
        }

        private void CheckDirty()
        {
            IsDirty =
                ( Description != Command.Description ) ||
                ( Shortcut != Command.Shortcut ) ||
                ( Path != Command.Path ) ||
                ( WorkingDirectory != Command.WorkingDirectory ) ||
                ( ArgumentsFormat != Command.ArgumentsFormat ) ||
                ( RunAsAdministrator != Command.RunAsAdministrator );
        }
    }
}
