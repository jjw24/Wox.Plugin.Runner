using Flow.Launcher.Plugin;
using System.Collections.ObjectModel;
using System.Linq;

namespace Wox.Plugin.Runner.Settings
{
    public class RunnerSettingsViewModel
    {
        private readonly PluginInitContext? context;

        public RunnerSettingsViewModel() { }

        public RunnerSettingsViewModel( PluginInitContext context )
        {
            this.context = context;
        }

        public void LoadCommands()
        {
            Commands = new ObservableCollection<CommandViewModel>(
                RunnerConfiguration.Commands.Select( c => new CommandViewModel( c ) ) );
        }

        public ObservableCollection<CommandViewModel>? Commands { get; set; }

        public CommandViewModel? SelectedCommand { get; set; }

        public bool CommandIsSelected
        {
            get
            {
                return SelectedCommand != null;
            }
        }

        public void Add()
        {
            var cmd = new CommandViewModel(new Command());
            Commands!.Add(cmd);
            SelectedCommand = cmd;
        }

        public void SaveChanges()
        {
            RunnerConfiguration.Commands = Commands!.Select(c => c.GetCommand());

            context!.API.ShowMsg("Your changes have been saved!");
        }

        public void Delete(CommandViewModel cmdToDelete)
        {
            if (cmdToDelete != null)
            {
                Commands!.Remove(cmdToDelete);
                SelectedCommand = null;
            }
        }
    }
}
