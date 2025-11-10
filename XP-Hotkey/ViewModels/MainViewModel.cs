using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using XP_Hotkey.Models;
using XP_Hotkey.Services;

namespace XP_Hotkey.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly SnippetService _snippetService;
    private readonly VariableProcessor _variableProcessor;
    private readonly FormDialogService _formDialogService;
    private readonly BackupService _backupService;

    private ObservableCollection<Snippet> _snippets = new();
    private Snippet? _selectedSnippet;
    private string _searchText = string.Empty;
    private string _selectedCategory = "All";
    private bool _showFavoritesOnly;

    public MainViewModel(
        SnippetService snippetService,
        VariableProcessor variableProcessor,
        FormDialogService formDialogService,
        BackupService backupService)
    {
        _snippetService = snippetService;
        _variableProcessor = variableProcessor;
        _formDialogService = formDialogService;
        _backupService = backupService;

        LoadSnippets();

        AddSnippetCommand = new RelayCommand(_ => AddSnippet());
        EditSnippetCommand = new RelayCommand(_ => EditSnippet(), _ => SelectedSnippet != null);
        DeleteSnippetCommand = new RelayCommand(_ => DeleteSnippet(), _ => SelectedSnippet != null);
        DuplicateSnippetCommand = new RelayCommand(_ => DuplicateSnippet(), _ => SelectedSnippet != null);
        ToggleFavoriteCommand = new RelayCommand(_ => ToggleFavorite(), _ => SelectedSnippet != null);
        ExportCommand = new RelayCommand(_ => ExportSnippets());
        ImportCommand = new RelayCommand(_ => ImportSnippets());
        RefreshCommand = new RelayCommand(_ => LoadSnippets());
    }

    public ObservableCollection<Snippet> Snippets
    {
        get => _snippets;
        set
        {
            _snippets = value;
            OnPropertyChanged();
        }
    }

    public Snippet? SelectedSnippet
    {
        get => _selectedSnippet;
        set
        {
            _selectedSnippet = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PreviewText));
            ((RelayCommand)EditSnippetCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DeleteSnippetCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DuplicateSnippetCommand).RaiseCanExecuteChanged();
            ((RelayCommand)ToggleFavoriteCommand).RaiseCanExecuteChanged();
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
            FilterSnippets();
        }
    }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            _selectedCategory = value;
            OnPropertyChanged();
            FilterSnippets();
        }
    }

    public bool ShowFavoritesOnly
    {
        get => _showFavoritesOnly;
        set
        {
            _showFavoritesOnly = value;
            OnPropertyChanged();
            FilterSnippets();
        }
    }

    public string PreviewText
    {
        get
        {
            if (SelectedSnippet == null)
                return string.Empty;

            return _variableProcessor.ProcessVariables(SelectedSnippet.Text, SelectedSnippet);
        }
    }

    public ObservableCollection<string> Categories { get; } = new();

    public ICommand AddSnippetCommand { get; }
    public ICommand EditSnippetCommand { get; }
    public ICommand DeleteSnippetCommand { get; }
    public ICommand DuplicateSnippetCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand ImportCommand { get; }
    public ICommand RefreshCommand { get; }

    private void LoadSnippets()
    {
        var allSnippets = _snippetService.GetAllSnippets();
        Snippets = new ObservableCollection<Snippet>(allSnippets);
        UpdateCategories();
        FilterSnippets();
    }

    private void FilterSnippets()
    {
        var filtered = _snippetService.GetAllSnippets().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(s =>
                s.Shortcut.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                s.Text.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (s.Description != null && s.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                s.Tags.Any(t => t.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
        }

        if (ShowFavoritesOnly)
        {
            filtered = filtered.Where(s => s.IsFavorite);
        }

        if (SelectedCategory != "All")
        {
            filtered = filtered.Where(s => s.Categories.Contains(SelectedCategory));
        }

        Snippets = new ObservableCollection<Snippet>(filtered);
    }

    private void UpdateCategories()
    {
        var categories = _snippetService.GetAllSnippets()
            .SelectMany(s => s.Categories)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        Categories.Clear();
        Categories.Add("All");
        foreach (var category in categories)
        {
            Categories.Add(category);
        }
    }

    private void AddSnippet()
    {
        var snippet = new Snippet();
        if (ShowSnippetEditDialog(snippet))
        {
            try
            {
                _snippetService.AddSnippet(snippet);
                LoadSnippets();
            }
            catch (InvalidOperationException ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Fehler beim Hinzufügen: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void EditSnippet()
    {
        if (SelectedSnippet == null) return;

        var snippet = CloneSnippet(SelectedSnippet);
        if (ShowSnippetEditDialog(snippet))
        {
            try
            {
                _snippetService.UpdateSnippet(snippet);
                LoadSnippets();
            }
            catch (InvalidOperationException ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Fehler beim Aktualisieren: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void DeleteSnippet()
    {
        if (SelectedSnippet == null) return;

        var result = System.Windows.MessageBox.Show(
            $"Möchten Sie das Snippet '{SelectedSnippet.Shortcut}' wirklich löschen?",
            "Snippet löschen",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _snippetService.DeleteSnippet(SelectedSnippet.Id);
            LoadSnippets();
        }
    }

    private void DuplicateSnippet()
    {
        if (SelectedSnippet == null) return;
        _snippetService.DuplicateSnippet(SelectedSnippet.Id);
        LoadSnippets();
    }

    private void ToggleFavorite()
    {
        if (SelectedSnippet == null) return;
        SelectedSnippet.IsFavorite = !SelectedSnippet.IsFavorite;
        _snippetService.UpdateSnippet(SelectedSnippet);
        LoadSnippets();
    }

    private void ExportSnippets()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "JSON files (*.json)|*.json|CSV files (*.csv)|*.csv",
            DefaultExt = "json"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                if (dialog.FileName.EndsWith(".csv"))
                {
                    _backupService.ExportToCsv(dialog.FileName);
                }
                else
                {
                    _backupService.ExportToJson(dialog.FileName);
                }

                System.Windows.MessageBox.Show("Export erfolgreich!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Fehler beim Export: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void ImportSnippets()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json|CSV files (*.csv)|*.csv",
            DefaultExt = "json"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                if (dialog.FileName.EndsWith(".json"))
                {
                    _snippetService.ImportFromJson(dialog.FileName);
                }
                else
                {
                    // CSV import would need to be implemented
                    System.Windows.MessageBox.Show("CSV-Import wird noch nicht unterstützt.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                LoadSnippets();
                System.Windows.MessageBox.Show("Import erfolgreich!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Fehler beim Import: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private bool ShowSnippetEditDialog(Snippet snippet)
    {
        var dialog = new Views.SnippetEditDialog(snippet)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (dialog.ShowDialog() == true && dialog.Result != null)
        {
            snippet.Shortcut = dialog.Result.Shortcut;
            snippet.Text = dialog.Result.Text;
            snippet.Description = dialog.Result.Description;
            return true;
        }
        return false;
    }

    private Snippet CloneSnippet(Snippet source)
    {
        return new Snippet
        {
            Id = source.Id,
            Shortcut = source.Shortcut,
            Text = source.Text,
            Description = source.Description,
            Categories = new List<string>(source.Categories),
            Tags = new List<string>(source.Tags),
            IsFavorite = source.IsFavorite,
            CaseSensitive = source.CaseSensitive,
            UseRegex = source.UseRegex,
            FormFields = source.FormFields.Select(f => new FormField
            {
                Name = f.Name,
                Label = f.Label,
                Type = f.Type,
                Required = f.Required,
                DefaultValue = f.DefaultValue,
                Placeholder = f.Placeholder,
                ValidationPattern = f.ValidationPattern,
                ValidationMessage = f.ValidationMessage
            }).ToList(),
            Enabled = source.Enabled,
            Hotkey = source.Hotkey
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    public void Execute(object? parameter)
    {
        _execute(parameter);
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

