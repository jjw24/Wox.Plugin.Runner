using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using MessageBox = System.Windows.Forms.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using DragEventArgs = System.Windows.DragEventArgs;
using DragDropEffects = System.Windows.DragDropEffects;
using ListBox = System.Windows.Controls.ListBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Wox.Plugin.Runner.ViewModel;

/// <summary>
///     Interaction logic for RunnerSettings.xaml
/// </summary>
public partial class RunnerSettings
{
    private readonly RunnerSettingsViewModel _viewModel;
    private Point? _dragStartPoint;

    public RunnerSettings(RunnerSettingsViewModel viewModel)
    {
        InitializeComponent();

        this._viewModel = viewModel;

        // Set the DataContext to the viewModel
        DataContext = this._viewModel;

        this._viewModel.LoadCommands();

        // Enable drag-drop
        LbxCommands.AllowDrop = true;
        LbxCommands.PreviewMouseLeftButtonDown += LbxCommands_PreviewMouseLeftButtonDown;
        LbxCommands.MouseMove += LbxCommands_MouseMove;
        LbxCommands.Drop += LbxCommands_Drop;

        // Handle unsaved changes warning
        Unloaded += RunnerSettings_Unloaded;
    }

    private void RunnerSettings_Unloaded(object sender, RoutedEventArgs e)
    {
        if (!_viewModel.HasUnsavedChanges) return;
        var result = MessageBox.Show(
            "You have unsaved changes. Do you want to save them?",
            "Unsaved Changes",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result == DialogResult.Yes) btnSaveChanges_Click(this, new RoutedEventArgs());
    }

    private void btnBrowsePath_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            DereferenceLinks = false
        };
        var result = dialog.ShowDialog();
        if (result == true) TbPath.Text = dialog.FileName;
    }

    private void btnBrowseWorkDir_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new FolderBrowserDialog();
        dialog.Description = "Select working directory";
        if (dialog.ShowDialog() == DialogResult.OK) TbWorkDir.Text = dialog.SelectedPath;
    }

    private void btnSaveChanges_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.Commands != null &&
            _viewModel.Commands.Any(c => string.IsNullOrEmpty(c.Shortcut) || string.IsNullOrEmpty(c.Path)))
        {
            MessageBox.Show(
                "One or more commands is missing a Shortcut or Path. Set a Shortcut and Path and try again.", "",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _viewModel.SaveChanges();
    }

    private void btnAdd_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Add();
        LbxCommands.SelectedItem = _viewModel.SelectedCommand;
    }

    private void BntDelete(object sender, RoutedEventArgs e)
    {
        if (LbxCommands.SelectedItem is CommandViewModel selectedCommand)
            _viewModel.Delete(selectedCommand);
    }

    #region Drag and Drop

    private void LbxCommands_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
    }

    private void LbxCommands_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || !_dragStartPoint.HasValue) return;
        var mousePos = e.GetPosition(null);
        var diff = _dragStartPoint.Value - mousePos;

        if (!(Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance) &&
            !(Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)) return;
        var listBox = sender as ListBox;
        var listBoxItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);

        if (listBoxItem == null || listBox == null) return;
        var command = listBox.ItemContainerGenerator.ItemFromContainer(listBoxItem) as CommandViewModel;
        if (command == null) return;
        _dragStartPoint = null;
        DragDrop.DoDragDrop(listBoxItem, command, DragDropEffects.Move);
    }

    private void LbxCommands_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(typeof(CommandViewModel))) return;
        var droppedData = e.Data.GetData(typeof(CommandViewModel)) as CommandViewModel;
        var target = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);

        if (droppedData == null || target == null || _viewModel.Commands == null) return;
        var targetData = LbxCommands.ItemContainerGenerator.ItemFromContainer(target) as CommandViewModel;
        if (targetData == null) return;
        var oldIndex = _viewModel.Commands.IndexOf(droppedData);
        var newIndex = _viewModel.Commands.IndexOf(targetData);

        if (oldIndex != newIndex) _viewModel.MoveCommand(oldIndex, newIndex);
    }

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        do
        {
            if (current is T t) return t;
            if (current == null) return null;
            current = VisualTreeHelper.GetParent(current);
        } while (current != null);

        return null;
    }

    #endregion
}