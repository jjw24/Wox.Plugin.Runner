using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Wox.Plugin.Runner
{
    interface IConfigurationLoader
    {
        IEnumerable<Command> LoadCommands();
        void SaveCommands(IEnumerable<Command> commands);
    }

    class ConfigurationLoader : IConfigurationLoader
    {
        readonly static string configPath = Environment.ExpandEnvironmentVariables(
            @$"%appdata%\FlowLauncher\Settings\Plugins\{Runner.Context!.CurrentPluginMetadata.Name}");
        readonly static string configFile = Path.Combine(configPath, "commands.json");

        public ConfigurationLoader()
        {

            Directory.CreateDirectory(configPath);
            if (!File.Exists(configFile))
            {
                File.Create(configFile).Close();
            }
        }

        public IEnumerable<Command> LoadCommands()
        {
            var text = File.ReadAllText(configFile);
            if (!string.IsNullOrEmpty(text))
                return JsonSerializer.Deserialize<IEnumerable<Command>>(text) ?? new List<Command>();

            return new List<Command>();
        }

        public void SaveCommands(IEnumerable<Command> commands)
        {
            File.WriteAllText(configFile, JsonSerializer.Serialize(commands));
        }
    }
}
