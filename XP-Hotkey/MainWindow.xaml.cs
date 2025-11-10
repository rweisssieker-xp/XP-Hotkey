using System.Windows;
using XP_Hotkey.Services;
using XP_Hotkey.ViewModels;

namespace XP_Hotkey;

public partial class MainWindow : Window
{
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
        DataContext = new MainViewModel(snippetService, variableProcessor, formDialogService, backupService);
    }
}
