using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Wox.Plugin.Runner
{
    interface IConfigurationLoader
    {
        IEnumerable<Command> LoadCommands();
        void SaveCommands( IEnumerable<Command> commands );
    }

    class ConfigurationLoader : IConfigurationLoader
    {
        readonly static string configPath = Path.Combine(Runner.Context.CurrentPluginMetadata.PluginDirectory, "Settings");
        readonly static string configFile = Path.Combine(configPath, "commands.json");

        public ConfigurationLoader()
        {
            Directory.CreateDirectory( configPath );
            if ( !File.Exists( configFile ) )
            {
                File.Create( configFile ).Close();
            }
        }

        public IEnumerable<Command> LoadCommands()
        {
            var commands = JsonConvert.DeserializeObject<IEnumerable<Command>>( File.ReadAllText( configFile ) );
            return commands ?? new List<Command>();
        }

        public void SaveCommands( IEnumerable<Command> commands )
        {
            File.WriteAllText( configFile, JsonConvert.SerializeObject( commands ) );
        }
    }
}
