using System.Windows;
using XP_Hotkey.Models;
using XP_Hotkey.Services;

namespace XP_Hotkey.Views;

public partial class StatisticsWindow : Window
{
    public StatisticsWindow(SnippetService snippetService)
    {
        InitializeComponent();

        var snippets = snippetService.GetAllSnippets()
            .OrderByDescending(s => s.Statistics.UseCount)
            .Select(s => new
            {
                Kürzel = s.Shortcut,
                Verwendungen = s.Statistics.UseCount,
                Zuletzt_verwendet = s.LastUsed == DateTime.MinValue ? "Nie" : s.LastUsed.ToString("dd.MM.yyyy HH:mm"),
                Erstellt = s.Created.ToString("dd.MM.yyyy")
            })
            .ToList();

        StatisticsDataGrid.ItemsSource = snippets;
    }
}

