using LogViewer.MVVM.Models;
using System.Collections.Generic;

namespace LogViewer.MVVM.ViewModels
{
    public class ImportLogsProcessViewModel : BaseViewModel
    {
        public List<ImportLogFile> ImportFiles { get; set; } = new List<ImportLogFile>();
    }
}