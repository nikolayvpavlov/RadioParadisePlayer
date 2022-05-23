﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadioParadisePlayer.Helpers
{
    internal class NotBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool && !(bool)value)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (value is Visibility && (Visibility)value != Visibility.Visible);
        }
    }
}
