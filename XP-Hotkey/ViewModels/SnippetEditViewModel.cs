using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using XP_Hotkey.Models;
using XP_Hotkey.Services;

namespace XP_Hotkey.ViewModels;

public class SnippetEditViewModel : INotifyPropertyChanged
{
    private readonly SnippetService? _snippetService;
    private Snippet _snippet;
    private string _shortcut = string.Empty;
    private string _text = string.Empty;
    private string _description = string.Empty;
    private ObservableCollection<string> _categories = new();
    private ObservableCollection<string> _tags = new();
    private bool _isFavorite;
    private bool _caseSensitive;
    private bool _useRegex;
    private bool _enabled = true;
    private string? _hotkey;
    private ObservableCollection<FormField> _formFields = new();

    public SnippetEditViewModel(Snippet snippet, SnippetService? snippetService = null)
    {
        _snippet = snippet;
        _snippetService = snippetService;
        
        Shortcut = snippet.Shortcut;
        Text = snippet.Text;
        Description = snippet.Description ?? string.Empty;
        Categories = new ObservableCollection<string>(snippet.Categories);
        Tags = new ObservableCollection<string>(snippet.Tags);
        IsFavorite = snippet.IsFavorite;
        CaseSensitive = snippet.CaseSensitive;
        UseRegex = snippet.UseRegex;
        Enabled = snippet.Enabled;
        Hotkey = snippet.Hotkey;
        FormFields = new ObservableCollection<FormField>(snippet.FormFields);

        SaveCommand = new RelayCommand(_ => Save());
        CancelCommand = new RelayCommand(_ => Cancel());
        AddCategoryCommand = new RelayCommand(_ => AddCategory());
        RemoveCategoryCommand = new RelayCommand(_ => RemoveCategory(), _ => SelectedCategory != null);
        AddTagCommand = new RelayCommand(_ => AddTag());
        RemoveTagCommand = new RelayCommand(_ => RemoveTag(), _ => SelectedTag != null);
        AddFormFieldCommand = new RelayCommand(_ => AddFormField());
        RemoveFormFieldCommand = new RelayCommand(_ => RemoveFormField(), _ => SelectedFormField != null);
    }

    public string Shortcut
    {
        get => _shortcut;
        set
        {
            _shortcut = value;
            OnPropertyChanged();
        }
    }

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            OnPropertyChanged();
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<string> Categories
    {
        get => _categories;
        set
        {
            _categories = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<string> Tags
    {
        get => _tags;
        set
        {
            _tags = value;
            OnPropertyChanged();
        }
    }

    public bool IsFavorite
    {
        get => _isFavorite;
        set
        {
            _isFavorite = value;
            OnPropertyChanged();
        }
    }

    public bool CaseSensitive
    {
        get => _caseSensitive;
        set
        {
            _caseSensitive = value;
            OnPropertyChanged();
        }
    }

    public bool UseRegex
    {
        get => _useRegex;
        set
        {
            _useRegex = value;
            OnPropertyChanged();
        }
    }

    public bool Enabled
    {
        get => _enabled;
        set
        {
            _enabled = value;
            OnPropertyChanged();
        }
    }

    public string? Hotkey
    {
        get => _hotkey;
        set
        {
            _hotkey = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<FormField> FormFields
    {
        get => _formFields;
        set
        {
            _formFields = value;
            OnPropertyChanged();
        }
    }

    public string? SelectedCategory { get; set; }
    public string? SelectedTag { get; set; }
    public FormField? SelectedFormField { get; set; }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand AddCategoryCommand { get; }
    public ICommand RemoveCategoryCommand { get; }
    public ICommand AddTagCommand { get; }
    public ICommand RemoveTagCommand { get; }
    public ICommand AddFormFieldCommand { get; }
    public ICommand RemoveFormFieldCommand { get; }

    public Snippet? Result { get; private set; }
    public bool DialogResult { get; private set; }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(Shortcut))
        {
            return;
        }

        _snippet.Shortcut = Shortcut.Trim();
        _snippet.Text = Text;
        _snippet.Description = Description.Trim();
        _snippet.Categories = Categories.ToList();
        _snippet.Tags = Tags.ToList();
        _snippet.IsFavorite = IsFavorite;
        _snippet.CaseSensitive = CaseSensitive;
        _snippet.UseRegex = UseRegex;
        _snippet.Enabled = Enabled;
        _snippet.Hotkey = Hotkey;
        _snippet.FormFields = FormFields.ToList();
        _snippet.Modified = DateTime.Now;

        Result = _snippet;
        DialogResult = true;
    }

    private void Cancel()
    {
        DialogResult = false;
    }

    private void AddCategory()
    {
        // Would show input dialog in real implementation
        Categories.Add("Neue Kategorie");
    }

    private void RemoveCategory()
    {
        if (SelectedCategory != null)
        {
            Categories.Remove(SelectedCategory);
        }
    }

    private void AddTag()
    {
        // Would show input dialog in real implementation
        Tags.Add("Neuer Tag");
    }

    private void RemoveTag()
    {
        if (SelectedTag != null)
        {
            Tags.Remove(SelectedTag);
        }
    }

    private void AddFormField()
    {
        FormFields.Add(new FormField
        {
            Name = "field" + FormFields.Count,
            Label = "Neues Feld",
            Type = "text",
            Required = false
        });
    }

    private void RemoveFormField()
    {
        if (SelectedFormField != null)
        {
            FormFields.Remove(SelectedFormField);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

