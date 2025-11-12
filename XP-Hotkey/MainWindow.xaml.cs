using System.Windows;
using System.Windows.Input;
using XP_Hotkey.Services;
using XP_Hotkey.ViewModels;
using XP_Hotkey.Views;
using MessageBox = System.Windows.MessageBox;

namespace XP_Hotkey;

public partial class MainWindow : Window
{
    private readonly SnippetService? _snippetService;
    private MainViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(
        SnippetService snippetService,
        VariableProcessor variableProcessor,
        FormDialogService formDialogService,
        BackupService backupService) : this()
    {
        _snippetService = snippetService;
        _viewModel = new MainViewModel(snippetService, variableProcessor, formDialogService, backupService);
        DataContext = _viewModel;

        // Register Ctrl+Shift+Q for Quick Add Dialog
        var quickAddCommand = new RoutedCommand();
        quickAddCommand.InputGestures.Add(new KeyGesture(Key.Q, ModifierKeys.Control | ModifierKeys.Shift));
        CommandBindings.Add(new CommandBinding(quickAddCommand, QuickAddDialog_Execute));
    }

    private void QuickAddDialog_Execute(object sender, ExecutedRoutedEventArgs e)
    {
        ShowQuickAddDialog();
    }

    private void QuickAddMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ShowQuickAddDialog();
    }

    public void ShowQuickAddDialog()
    {
        if (_snippetService == null) return;

        var dialog = new QuickAddDialog();
        if (dialog.ShowDialog() == true && dialog.Result != null)
        {
            try
            {
                _snippetService.AddSnippet(dialog.Result);
                _viewModel?.RefreshSnippets();
                MessageBox.Show("Snippet erfolgreich hinzugefügt!", "Erfolg",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Hinzufügen des Snippets:\n{ex.Message}",
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
