using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace Wox.Plugin.Runner
{
    static class ConfigurationLoader
    {
        readonly static string configPath = Environment.ExpandEnvironmentVariables(
            @$"%appdata%\FlowLauncher\Settings\Plugins\{Runner.Context.CurrentPluginMetadata.Name}");
        readonly static string configFile = Path.Combine(configPath, "commands.json");

        /// <summary>
        /// Backwards compatibility code, remove after release 2.4.0
        /// </summary>
        public static void LoadCommandsFileToSettings(Settings settings)
        {
            if (!File.Exists(configFile))
                return;

            var text = File.ReadAllText(configFile);
            var originalCommands = new List<Command>();
            if (!string.IsNullOrEmpty(text))
                originalCommands = JsonSerializer.Deserialize<List<Command>>(text) ?? new List<Command>();

            foreach (var command in originalCommands)
            {
                settings.Commands.Add(command);
            }

            var backupFile = Path.Combine(configPath, "commands_backup.json");
            File.Move(configFile, backupFile);
        }
    }
}
