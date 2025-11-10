using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace XP_Hotkey.Utilities;

public class CategoriesConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is List<string> categories)
        {
            return string.Join(", ", categories);
        }
        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return str.Split(',').Select(s => s.Trim()).ToList();
        }
        return new List<string>();
    }
}

