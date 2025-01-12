﻿using LogViewer.Helpers;
using LogViewer.Localization;
using LogViewer.MVVM.Models;
using LogViewer.MVVM.Views;
using Microsoft.Win32;
using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LogViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static bool IsManualStartup { get; private set; } = false;
        public ResourceDictionary ThemeDictionary => Resources.MergedDictionaries[1];

        public void ChangeTheme(Uri uri)
        {
            ThemeDictionary.MergedDictionaries[1] = new ResourceDictionary { Source = uri };
        }

        private static Mutex mutex = new Mutex(true, "{8F6F0AC4-B9A1-45fd-A8CF-72F04E6BDE8F}");

        [STAThread]
        protected override void OnStartup(StartupEventArgs e)
        {
            logger.Trace("OnStartup");

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            if (e.Args.Any(x => (x.EndsWith(".txt") || x.EndsWith(".log")) && File.Exists(x)))
            {
                IsManualStartup = true;

                try
                {
                    if (!mutex.WaitOne(TimeSpan.Zero, true))
                    {
                        var proc = Process.GetCurrentProcess();
                        var processName = proc.ProcessName.Replace(".vshost", "");
                        var runningProcess = Process.GetProcesses()
                            .FirstOrDefault(x => (x.ProcessName == processName || x.ProcessName == proc.ProcessName || x.ProcessName == proc.ProcessName + ".vshost") && x.Id != proc.Id);

                        if (runningProcess == null)
                        {
                            var app = new App();
                            app.InitializeComponent();
                            var window = new MainWindow();
                            MVVM.Views.MainWindow.HandleParameter(e.Args);
                            app.Run(window);
                            return;
                        }

                        UnsafeNative.SendMessage(runningProcess.MainWindowHandle, string.Join(" ", e.Args));

                        Environment.Exit(0);
                    }
                }
                catch (Exception exception)
                {
                    logger.Warn(exception, "OnStartup check mutext exception (open with arguments)");
                }
            }

            Settings.Instance.Load();

            if (Settings.Instance.OnlyOneAppInstance)
            {
                try
                {
                    if (mutex.WaitOne(TimeSpan.Zero, true))
                    {
                        mutex.ReleaseMutex();
                    }
                    else
                    {
                        logger.Trace("OnStartup: one instance of Log Viewer already started");
                        MessageBox.Show(Locals.OnlyOneInstanceCanBeStartedMessageBoxText);
                        Environment.Exit(0);
                    }
                }
                catch (Exception exception)
                {
                    logger.Warn(exception, "OnStartup check mutext exception");
                }
            }

            try
            {
                if (!IsAssociated())
                {
                    var filePath = Process.GetCurrentProcess().MainModule.FileName;

                    SetAssociation(".txt", "LogViewer", filePath);
                    SetAssociation(".log", "LogViewer", filePath);
                }
            }
            catch (Exception exception)
            {
                logger.Warn(exception, "OnStartup set file association exception");
            }

            base.OnStartup(e);

            Task.Run(() =>
            {
                // даем время прогрузиться окну
                Thread.Sleep(5000);
                UpdateManager.StartCheckUpdate();
            });
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            logger.Fatal($"Unhandled exception {e.ExceptionObject as Exception}");
            Environment.Exit(1);
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            logger.Fatal($"Unhandled Exception: {e.Exception}");
            Environment.Exit(2);
        }

        #region File Assotiation

        private bool IsAssociated()
        {
            return Registry.CurrentUser.OpenSubKey(@"Software\Classes\LogViewer", false) != null;
        }

        [DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        private const int SHCNE_ASSOCCHANGED = 0x8000000;
        private const int SHCNF_FLUSH = 0x1000;

        private void SetAssociation(string extension, string progId, string applicationFilePath)
        {
            bool madeChanges = false;

            madeChanges |= SetKeyDefaultValue($@"Software\Classes\{progId}\shell\open\command", "\"" + applicationFilePath + "\" \"%1\"");
            madeChanges |= SetProgIdValue($@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{extension}\OpenWithProgids", progId);

            if (madeChanges) SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
        }

        private bool SetProgIdValue(string path, string progId)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(path))
            {
                if (key.GetValueNames().All(x => x != progId))
                {
                    key.SetValue(progId, Encoding.Unicode.GetBytes(string.Empty), RegistryValueKind.Binary);
                    return true;
                }
            }
            return false;
        }

        private bool SetKeyDefaultValue(string keyPath, string value)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(keyPath))
            {
                if (key.GetValue(null) as string != value)
                {
                    key.SetValue(null, value);
                    return true;
                }
            }
            return false;
        }

        #endregion File Assotiation

        public void Dispose()
        {
            UpdateManager.Dispose();
        }
    }
}