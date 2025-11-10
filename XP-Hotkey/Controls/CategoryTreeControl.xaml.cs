using System.Collections.ObjectModel;
using System.Linq;
using UserControl = System.Windows.Controls.UserControl;

namespace XP_Hotkey.Controls;

public partial class CategoryTreeControl : UserControl
{
    public static readonly System.Windows.DependencyProperty CategoriesProperty =
        System.Windows.DependencyProperty.Register(
            nameof(Categories),
            typeof(ObservableCollection<string>),
            typeof(CategoryTreeControl),
            new System.Windows.PropertyMetadata(null, OnCategoriesChanged));

    public CategoryTreeControl()
    {
        InitializeComponent();
    }

    public ObservableCollection<string>? Categories
    {
        get => (ObservableCollection<string>?)GetValue(CategoriesProperty);
        set => SetValue(CategoriesProperty, value);
    }

    private static void OnCategoriesChanged(System.Windows.DependencyObject d, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (d is CategoryTreeControl control && e.NewValue is ObservableCollection<string> categories)
        {
            control.CategoryTreeView.Items.Clear();
            foreach (var category in categories.OrderBy(c => c))
            {
                control.CategoryTreeView.Items.Add(category);
            }
        }
    }
}

