using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Flow.Launcher.Plugin;
using JetBrains.Annotations;
using Wox.Plugin.Runner.Infrastructure;

namespace Wox.Plugin.Runner.ViewModel;

public sealed class RunnerSettingsViewModel : INotifyPropertyChanged
{
    private readonly PluginInitContext? _context;
    private ObservableCollection<CommandViewModel>? _commands;
    private bool _hasUnsavedChanges;
    private string _searchText = string.Empty;
    private CommandViewModel? _selectedCommand;

    public RunnerSettingsViewModel(PluginInitContext context)
    {
        _context = context;
        SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
    }

    [UsedImplicitly] public ICommand SaveCommand { get; }

    public ObservableCollection<CommandViewModel>? Commands
    {
        get => _commands;
        private set
        {
            if (_commands == value) return;
            if (_commands != null) _commands.CollectionChanged -= Commands_CollectionChanged;
            _commands = value;
            if (_commands != null) _commands.CollectionChanged += Commands_CollectionChanged;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FilteredCommands));
        }
    }

    public ObservableCollection<CommandViewModel> FilteredCommands
    {
        get
        {
            if (Commands == null || string.IsNullOrWhiteSpace(SearchText))
                return Commands ?? new ObservableCollection<CommandViewModel>();

            var filtered = Commands.Where(c =>
                c.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                c.Shortcut.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                c.Path.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            ).ToList();

            return new ObservableCollection<CommandViewModel>(filtered);
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value) return;
            _searchText = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FilteredCommands));
        }
    }

    public CommandViewModel? SelectedCommand
    {
        get => _selectedCommand;
        set
        {
            if (_selectedCommand == value) return;
            _selectedCommand = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CommandIsSelected));
        }
    }

    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        private set
        {
            if (_hasUnsavedChanges == value) return;
            _hasUnsavedChanges = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public bool CommandIsSelected => SelectedCommand != null;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void LoadCommands()
    {
        Commands = new ObservableCollection<CommandViewModel>(
            Runner.Settings.Commands.Select(c => new CommandViewModel(c)));

        // Subscribe to collection changes to track unsaved state
        Commands.CollectionChanged += Commands_CollectionChanged;

        // Subscribe to each command's property changes
        foreach (var cmd in Commands) cmd.PropertyChanged += Command_PropertyChanged;

        HasUnsavedChanges = false;
    }

    private void Commands_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        HasUnsavedChanges = true;

        // Subscribe to new items
        if (e.NewItems != null)
            foreach (CommandViewModel cmd in e.NewItems)
                cmd.PropertyChanged += Command_PropertyChanged;

        // Unsubscribe from removed items
        if (e.OldItems != null)
            foreach (CommandViewModel cmd in e.OldItems)
                cmd.PropertyChanged -= Command_PropertyChanged;
    }

    private void Command_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Only mark as unsaved if the change resulted in a dirty command
        if (sender is CommandViewModel { IsDirty: true }) HasUnsavedChanges = true;
    }

    private bool CanExecuteSave()
    {
        return HasUnsavedChanges;
    }

    private void ExecuteSave()
    {
        if (CanExecuteSave()) SaveChanges();
    }

    public void Add()
    {
        var cmd = new CommandViewModel(new Command());
        Commands!.Add(cmd);
        SelectedCommand = cmd;
        HasUnsavedChanges = true;
    }

    public void SaveChanges()
    {
        Runner.Settings.Commands.Clear();
        foreach (var cmd in Commands!.Select(c => c.GetCommand())) Runner.Settings.Commands.Add(cmd);

        _context!.API.SaveSettingJsonStorage<Settings>();

        // Reset dirty flags
        if (Commands != null)
            foreach (var cmd in Commands)
                cmd.IsDirty = false;

        HasUnsavedChanges = false;

        _context.API.ShowMsg("Your changes have been saved!");
    }

    public void Delete(CommandViewModel cmdToDelete)
    {
        Commands!.Remove(cmdToDelete);
        SelectedCommand = null;
        HasUnsavedChanges = true;
    }

    public void MoveCommand(int oldIndex, int newIndex)
    {
        if (Commands == null || oldIndex < 0 || newIndex < 0 ||
            oldIndex >= Commands.Count || newIndex >= Commands.Count || oldIndex == newIndex)
            return;

        var item = Commands[oldIndex];
        Commands.RemoveAt(oldIndex);
        Commands.Insert(newIndex, item);
        HasUnsavedChanges = true;
    }

    public void DiscardChanges()
    {
        LoadCommands();
    }
}