using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using XP_Hotkey.Models;
using Label = System.Windows.Controls.Label;
using MessageBox = System.Windows.MessageBox;

namespace XP_Hotkey.Views;

public partial class FormDialog : Window
{
    private readonly List<FormField> _formFields;
    private readonly Dictionary<string, System.Windows.Controls.TextBox> _textBoxes = new();

    public Dictionary<string, string> Result { get; private set; } = new();

    public FormDialog(List<FormField> formFields)
    {
        InitializeComponent();
        _formFields = formFields;
        BuildForm();
    }

    private void BuildForm()
    {
        foreach (var field in _formFields)
        {
            var label = new Label
            {
                Content = field.Label + (field.Required ? " *" : ""),
                Margin = new Thickness(0, 5, 0, 2)
            };
            FormFieldsPanel.Children.Add(label);

            System.Windows.Controls.TextBox textBox;
            if (field.Type == "number")
            {
                textBox = new System.Windows.Controls.TextBox
                {
                    Margin = new Thickness(0, 0, 0, 10)
                };
                textBox.PreviewTextInput += (s, e) =>
                {
                    e.Handled = !char.IsDigit(e.Text, 0);
                };
            }
            else
            {
                textBox = new System.Windows.Controls.TextBox
                {
                    Margin = new Thickness(0, 0, 0, 10)
                };
            }

            if (!string.IsNullOrEmpty(field.Placeholder))
            {
                // WPF doesn't have placeholder, but we can use a watermark effect
            }

            if (!string.IsNullOrEmpty(field.DefaultValue))
            {
                textBox.Text = field.DefaultValue;
            }

            _textBoxes[field.Name] = textBox;
            FormFieldsPanel.Children.Add(textBox);
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        Result.Clear();

        foreach (var field in _formFields)
        {
            if (!_textBoxes.TryGetValue(field.Name, out var textBox))
                continue;

            var value = textBox.Text.Trim();

            if (field.Required && string.IsNullOrEmpty(value))
            {
                MessageBox.Show($"Das Feld '{field.Label}' ist erforderlich.", "Validierung", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!string.IsNullOrEmpty(field.ValidationPattern) && !string.IsNullOrEmpty(value))
            {
                var regex = new Regex(field.ValidationPattern);
                if (!regex.IsMatch(value))
                {
                    var message = !string.IsNullOrEmpty(field.ValidationMessage) 
                        ? field.ValidationMessage 
                        : $"Das Feld '{field.Label}' hat ein ungültiges Format.";
                    MessageBox.Show(message, "Validierung", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            Result[field.Name] = value;
        }

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

