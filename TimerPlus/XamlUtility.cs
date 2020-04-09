using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using TimerPlus.Native;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TimerPlus
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool param;
            if (!bool.TryParse(System.Convert.ToString(parameter), out param))
            {
                param = false;
            }
            if ((value as bool?).GetValueOrDefault() != param)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DateTimeToMonthStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime param = (DateTime)value;
            return param.ToString("MMMM yyyy");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan param = (TimeSpan)value;
            bool showDiff;
            bool validParam = bool.TryParse(System.Convert.ToString(parameter), out showDiff);
            if (!validParam || !showDiff)
            {
                return param.ToString(@"hh\:mm\:ss");
            }
            if (param.TotalSeconds > 0)
            {
                return param.ToString(@"\+hh\:mm\:ss");
            }
            else if (param.TotalSeconds < 0)
            {
                return param.ToString(@"\-hh\:mm\:ss");
            }
            else
            {
                return param.ToString(@"hh\:mm\:ss");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TimeSpanToColorZoneModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan param = (TimeSpan)value;

            if (param.TotalHours <= 0)
            {
                return MaterialDesignThemes.Wpf.ColorZoneMode.Standard;
            }
            else if (param.TotalHours < 2.5)
            {
                return MaterialDesignThemes.Wpf.ColorZoneMode.PrimaryLight;
            }
            else if (param.TotalHours < 3.5)
            {
                return MaterialDesignThemes.Wpf.ColorZoneMode.PrimaryMid;
            }
            else if (param.TotalHours < 4.5)
            {
                return MaterialDesignThemes.Wpf.ColorZoneMode.PrimaryDark;
            }
            else
            {
                return MaterialDesignThemes.Wpf.ColorZoneMode.Accent;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TimeSpanToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan param = (TimeSpan)value;

            if (param.TotalSeconds <= 0)
            {
                return (SolidColorBrush)Application.Current.TryFindResource("SecondaryAccentBrush");
            }
            else if (param.TotalMinutes <= 5)
            {
                return (SolidColorBrush)Application.Current.TryFindResource("PrimaryHueDarkBrush");
            }
            else if (param.TotalMinutes <= 15)
            {
                return (SolidColorBrush)Application.Current.TryFindResource("PrimaryHueMidBrush");
            }
            else
            {
                return (SolidColorBrush)Application.Current.TryFindResource("MaterialDesignBody");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class WindowBlur
    {
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled", typeof(bool), typeof(WindowBlur),
            new PropertyMetadata(false, OnIsEnabledChanged));

        public static void SetIsEnabled(DependencyObject element, bool value)
        {
            element.SetValue(IsEnabledProperty, value);
        }

        public static bool GetIsEnabled(DependencyObject element)
        {
            return (bool)element.GetValue(IsEnabledProperty);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                if (true.Equals(e.OldValue))
                {
                    GetWindowBlur(window)?.Detach();
                    window.ClearValue(WindowBlurProperty);
                }
                if (true.Equals(e.NewValue))
                {
                    var blur = new WindowBlur();
                    blur.Attach(window);
                    window.SetValue(WindowBlurProperty, blur);
                }
            }
        }

        public static readonly DependencyProperty WindowBlurProperty = DependencyProperty.RegisterAttached(
            "WindowBlur", typeof(WindowBlur), typeof(WindowBlur),
            new PropertyMetadata(null, OnWindowBlurChanged));

        public static void SetWindowBlur(DependencyObject element, WindowBlur value)
        {
            element.SetValue(WindowBlurProperty, value);
        }

        public static WindowBlur GetWindowBlur(DependencyObject element)
        {
            return (WindowBlur)element.GetValue(WindowBlurProperty);
        }

        private static void OnWindowBlurChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                (e.OldValue as WindowBlur)?.Detach();
                (e.NewValue as WindowBlur)?.Attach(window);
            }
        }

        private Window _window;

        private void Attach(Window window)
        {
            _window = window;
            var source = (HwndSource)PresentationSource.FromVisual(window);
            if (source == null)
            {
                window.SourceInitialized += OnSourceInitialized;
            }
            else
            {
                AttachCore();
            }
        }

        private void Detach()
        {
            try
            {
                DetachCore();
            }
            finally
            {
                _window = null;
            }
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            ((Window)sender).SourceInitialized -= OnSourceInitialized;
            AttachCore();
        }

        private void AttachCore()
        {
            EnableBlur(_window);
        }

        private void DetachCore()
        {
            _window.SourceInitialized += OnSourceInitialized;
        }

        private static void EnableBlur(Window window)
        {
            var windowHelper = new WindowInteropHelper(window);

            var accent = new AccentPolicy
            {
                AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND
            };

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);
    }

    namespace Native
    {
        internal enum AccentState
        {
            ACCENT_DISABLED,
            ACCENT_ENABLE_GRADIENT,
            ACCENT_ENABLE_TRANSPARENTGRADIENT,
            ACCENT_ENABLE_BLURBEHIND,
            ACCENT_INVALID_STATE,
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        internal enum WindowCompositionAttribute
        {
            WCA_ACCENT_POLICY = 19,
        }
    }
}
