using System.Collections.ObjectModel;
using XP_Hotkey.Models;
using UserControl = System.Windows.Controls.UserControl;
using DataGrid = System.Windows.Controls.DataGrid;

namespace XP_Hotkey.Controls;

public partial class SnippetListControl : UserControl
{
    public static readonly System.Windows.DependencyProperty SnippetsProperty =
        System.Windows.DependencyProperty.Register(
            nameof(Snippets),
            typeof(ObservableCollection<Snippet>),
            typeof(SnippetListControl),
            new System.Windows.PropertyMetadata(null, OnSnippetsChanged));

    public static readonly System.Windows.DependencyProperty SelectedSnippetProperty =
        System.Windows.DependencyProperty.Register(
            nameof(SelectedSnippet),
            typeof(Snippet),
            typeof(SnippetListControl),
            new System.Windows.PropertyMetadata(null));

    public SnippetListControl()
    {
        InitializeComponent();
        SnippetsDataGrid.SelectionChanged += (s, e) =>
        {
            SelectedSnippet = SnippetsDataGrid.SelectedItem as Snippet;
        };
    }

    public ObservableCollection<Snippet>? Snippets
    {
        get => (ObservableCollection<Snippet>?)GetValue(SnippetsProperty);
        set => SetValue(SnippetsProperty, value);
    }

    public Snippet? SelectedSnippet
    {
        get => (Snippet?)GetValue(SelectedSnippetProperty);
        set => SetValue(SelectedSnippetProperty, value);
    }

    private static void OnSnippetsChanged(System.Windows.DependencyObject d, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (d is SnippetListControl control && e.NewValue is ObservableCollection<Snippet> snippets)
        {
            control.SnippetsDataGrid.ItemsSource = snippets;
        }
    }
}

