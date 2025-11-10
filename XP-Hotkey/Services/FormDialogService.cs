using System.Collections.Generic;
using System.Linq;
using System.Windows;
using XP_Hotkey.Models;
using XP_Hotkey.Views;
using Application = System.Windows.Application;

namespace XP_Hotkey.Services;

public class FormDialogService
{
    public Dictionary<string, string>? ShowFormDialog(List<FormField> formFields, Window? owner = null)
    {
        if (formFields == null || formFields.Count == 0)
            return new Dictionary<string, string>();

        var dialog = new FormDialog(formFields)
        {
            Owner = owner ?? Application.Current.MainWindow
        };

        if (dialog.ShowDialog() == true)
        {
            return dialog.Result;
        }

        return null;
    }

    public string ProcessFormFields(string text, Dictionary<string, string> formValues)
    {
        if (string.IsNullOrEmpty(text) || formValues == null)
            return text;

        var result = text;
        foreach (var kvp in formValues)
        {
            result = result.Replace($"{{{kvp.Key}}}", kvp.Value);
        }

        return result;
    }
}

