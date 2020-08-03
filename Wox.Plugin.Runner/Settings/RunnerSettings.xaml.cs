using Flow.Launcher.Plugin;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Wox.Plugin.Runner.Settings
{
    /// <summary>
    /// Interaction logic for RunnerSettings.xaml
    /// </summary>
    public partial class RunnerSettings
    {
        private readonly RunnerSettingsViewModel viewModel;

        public RunnerSettings(RunnerSettingsViewModel viewModel)
        {
            InitializeComponent();

            this.viewModel = viewModel;

            this.viewModel.LoadCommands();

            lbxCommands.ItemsSource = this.viewModel.Commands;
        }

        private void btnBrowsePath_Click( object sender, RoutedEventArgs e )
        {
            var dialog = new OpenFileDialog();
            dialog.DereferenceLinks = false;
            var result = dialog.ShowDialog();
            if ( result == true )
            {
                tbPath.Text = dialog.FileName;
            }
        }

        private void btnBrowseWorkDir_Click( object sender, RoutedEventArgs e ) {
            using (var dialog = new FolderBrowserDialog()) {
                dialog.Description = "Select working directory";
                if (dialog.ShowDialog() == DialogResult.OK) {
                    tbWorkDir.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.Commands.Any(c => string.IsNullOrEmpty(c.Shortcut) || string.IsNullOrEmpty(c.Path)))
            {
                MessageBox.Show("One or more commands is missing a Shortcut or Path. Set a Shortcut and Path and try again.", "", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            viewModel.SaveChanges();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Add();
            lbxCommands.SelectedItem = viewModel.SelectedCommand;
        }

        private void bntDelete(object sender, RoutedEventArgs e)
        {
            var selectedCommand = lbxCommands.SelectedItem as CommandViewModel;

            if (selectedCommand != null)
                viewModel.Delete(selectedCommand);
        }
    }
}
