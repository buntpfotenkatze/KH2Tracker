using System;
using System.Globalization;
using System.Windows.Data;

namespace KhTracker;

public class HideZeroConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value as int? == 0 ? " " : value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value as string == " " ? 0 : value;
    }
}

public class ObtainedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value as bool? == true)
        {
            return 1;
        }
        else
        {
            return 0.45;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value as int? == 1;
    }
}

public class WeaponConverter : IValueConverter
{
    private const string CusPath = "pack://application:,,,/CustomImages/System/stats/";
    private string enabledPath1 = "Images/System/stats/"; //sword
    private string enabledPath2 = "Images/System/stats/"; //shield
    private string enabledPath3 = "Images/System/stats/"; //staff
    private readonly bool cusMode = Properties.Settings.Default.CustomIcons;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        //get the correct path
        {
            if (cusMode)
            {
                if (MainWindow.CustomSwordFound)
                    enabledPath1 = CusPath;
                if (MainWindow.CustomShieldFound)
                    enabledPath2 = CusPath;
                if (MainWindow.CustomStaffFound)
                    enabledPath3 = CusPath;
            }
        }

        return (string)value switch
        {
            "Sword" => enabledPath1 + "sword.png",
            "Shield" => enabledPath2 + "shield.png",
            "Staff" => enabledPath3 + "staff.png",
            _ => null
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        //get the correct path
        {
            if (cusMode)
            {
                if (MainWindow.CustomSwordFound)
                    enabledPath1 = CusPath;
                if (MainWindow.CustomShieldFound)
                    enabledPath2 = CusPath;
                if (MainWindow.CustomStaffFound)
                    enabledPath3 = CusPath;
            }
        }

        if ((string)value == enabledPath1 + "sword.png")
        {
            return "Sword";
        }
        else if ((string)value == enabledPath2 + "shield.png")
        {
            return "Shield";
        }
        else if ((string)value == enabledPath3 + "staff.png")
        {
            return "Staff";
        }
        else
        {
            return "";
        }
    }
}
