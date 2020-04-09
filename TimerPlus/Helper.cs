using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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

        public static string GetTypeName(this SessionType sType)
        {
            if (sType != null) return sType.Name;
            else return "Unknown";
        }

        public static int RemoveAll<T>(
        this ObservableCollection<T> coll, Func<T, bool> condition)
        {
            var itemsToRemove = coll.Where(condition).ToList();

            foreach (var itemToRemove in itemsToRemove)
            {
                coll.Remove(itemToRemove);
            }

            return itemsToRemove.Count;
        }

        public static IEnumerable<DateTime> AllDatesInMonth(DateTime month)
        {
            int days = DateTime.DaysInMonth(month.Year, month.Month);
            for (int day = 1; day <= days; day++)
            {
                yield return new DateTime(month.Year, month.Month, day);
            }
        }

        public static IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
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

        static GregorianCalendar _gc = new GregorianCalendar();
        public static int GetWeekOfMonth(this DateTime time)
        {
            DateTime first = new DateTime(time.Year, time.Month, 1);
            return time.GetWeekOfYear() - first.GetWeekOfYear() + 1;
        }

        public static int GetWeekOfYear(this DateTime time)
        {
            return _gc.GetWeekOfYear(time, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        }

        public static DateTime FirstDayOfMonth(this DateTime value)
        {
            return value.Date.AddDays(1 - value.Day);
        }

        public static DateTime LastDayOfMonth(this DateTime value)
        {
            return value.FirstDayOfMonth().AddMonths(1).AddDays(-1);
        }
    }
}
