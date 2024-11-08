using LogViewer.Enums;
using System;
using System.Runtime.Serialization;

namespace LogViewer.MVVM.Models
{
    /// <summary>
    /// Информация о параметре шаблона
    /// </summary>
    [Serializable]
    [DataContract]
    public class LogTemplateItemInfo
    {
        /// <summary>
        /// Параметр
        /// </summary>
        public eImportTemplateParameters Parameter { get; set; }

        /// <summary>
        /// Группа параметра
        /// </summary>
        public string Group { get; set; }
    }
}