using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TimerPlus
{
    public static class Helper
    {
        public static Random rnd = new Random((int)DateTime.Now.Ticks);

        public static string FromAppPath(this string relativePath)
        {
            return Path.Combine(System.Windows.Forms.Application.StartupPath, relativePath);
        }

        public static string RandomString(int stringLength)
        {
            const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789_-";
            char[] chars = new char[stringLength];

            for (int i = 0; i < stringLength; i++)
            {
                chars[i] = allowedChars[rnd.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }

        public static bool IsEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsBlank(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static void HideBoundingBox(object root)
        {
            Control control = root as Control;
            if (control != null)
                control.FocusVisualStyle = null;

            if (root is DependencyObject)
            {
                foreach (object child in LogicalTreeHelper.GetChildren((DependencyObject)root))
                {
                    HideBoundingBox(child);
                }
            }
        }
    }
}
