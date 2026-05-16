using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Wox.Plugin.Runner.ViewModel;

public sealed class CommandViewModel : INotifyPropertyChanged
{
    private string _argumentsFormat;

    private string _description;

    private bool _isDirty;

    private string _path;

    private bool _runAsAdministrator;

    private string _shortcut;

    private string _workingDirectory;

    public CommandViewModel(Command command)
    {
        Command = command;
        _description = command.Description;
        _shortcut = command.Shortcut;
        _path = command.Path;
        _workingDirectory = command.WorkingDirectory;
        _argumentsFormat = command.ArgumentsFormat;
        _runAsAdministrator = command.RunAsAdministrator;
    }

    private Command Command { get; }

    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    public string Shortcut
    {
        get => _shortcut;
        set => SetField(ref _shortcut, value);
    }

    public string Path
    {
        get => _path;
        set => SetField(ref _path, value);
    }

    public string WorkingDirectory
    {
        get => _workingDirectory;
        set => SetField(ref _workingDirectory, value);
    }

    public string ArgumentsFormat
    {
        get => _argumentsFormat;
        set => SetField(ref _argumentsFormat, value);
    }

    public bool RunAsAdministrator
    {
        get => _runAsAdministrator;
        set => SetField(ref _runAsAdministrator, value);
    }

    public bool IsDirty
    {
        get => _isDirty;
        set
        {
            if (_isDirty == value) return;
            _isDirty = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DirtyIndicator));
            OnPropertyChanged(nameof(DisplayText));
        }
    }

    public string DirtyIndicator => IsDirty ? "● " : "";

    public string DisplayText => $"{DirtyIndicator}{Description}";
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(propertyName);
        CheckDirty();
    }

    public Command GetCommand()
    {
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
            Description != Command.Description ||
            Shortcut != Command.Shortcut ||
            Path != Command.Path ||
            WorkingDirectory != Command.WorkingDirectory ||
            ArgumentsFormat != Command.ArgumentsFormat ||
            RunAsAdministrator != Command.RunAsAdministrator;
    }
}