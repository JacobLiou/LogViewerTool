using System;
using System.Globalization;

namespace LogViewer.Localization
{
    /// <summary>
    /// Аргументы события смены языка
    /// </summary>
    public class LanguageEventArgs : EventArgs
    {
        public CultureInfo CultureInfo { get; private set; }

        public LanguageEventArgs(CultureInfo cultureInfo)
        {
            CultureInfo = cultureInfo;
        }
    }
}