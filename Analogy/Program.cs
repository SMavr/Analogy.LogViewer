using Analogy.DataTypes;
using Analogy.Forms;
using Analogy.Managers;
using DevExpress.LookAndFeel;
using DevExpress.Utils.Drawing.Helpers;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Analogy.Common.DataTypes;
using Analogy.Common.Interfaces;
using Analogy.CommonControls.DataTypes;
using Analogy.Interfaces;

namespace Analogy
{
    static class Program
    {
        public const int WM_COPYDATA = 0x004A;

        [DllImport("user32", EntryPoint = "SendMessageA")]
        private static extern int SendMessage(IntPtr Hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        private static IAnalogyUserSettings Settings => UserSettingsManager.UserSettings;
        private static string AssemblyLocation;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
            DevExpress.UserSkins.BonusSkins.Register();
            WindowsFormsSettings.LoadApplicationSettings();
            WindowsFormsSettings.SetDPIAware();
            AnalogyLogManager.Instance.LogInformation($"OS: {Environment.OSVersion.Version}", nameof(Program));
            if (Environment.OSVersion.Version.Major >= 10)
            {
                WindowsFormsSettings.ForceDirectXPaint();
            }
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            AssemblyLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            WindowsFormsSettings.DefaultFont = Settings.FontSettings.UIFont;
            WindowsFormsSettings.DefaultMenuFont = Settings.FontSettings.MenuFont;
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            if (Settings.EnableFirstChanceException)
            {
                AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            }
            Settings.OnEnableFirstChanceExceptionChanged += (s, e) =>
            {
                if (e)
                {
                    AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
                }
                else
                {
                    AppDomain.CurrentDomain.FirstChanceException -= CurrentDomain_FirstChanceException;
                }
            };
            Application.SetCompatibleTextRenderingDefault(false);
            Application.EnableVisualStyles();
#if NETCOREAPP3_1 || NET
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
#endif
            Settings.IncreaseNumberOfLaunches();
            if (!string.IsNullOrEmpty(Settings.ApplicationSkinName))
            {
                if (!string.IsNullOrEmpty(Settings.ApplicationSvgPaletteName))
                {
                    UserLookAndFeel.Default.SetSkinStyle(Settings.ApplicationSkinName, Settings.ApplicationSvgPaletteName);
                }
                else
                {
                    UserLookAndFeel.Default.SetSkinStyle(Settings.ApplicationSkinName);
                }

                switch (Settings.ApplicationStyle)
                {
                    case AnalogyLookAndFeelStyle.Flat:
                        UserLookAndFeel.Default.Style = LookAndFeelStyle.Flat;
                        break;
                    case AnalogyLookAndFeelStyle.UltraFlat:
                        UserLookAndFeel.Default.Style = LookAndFeelStyle.UltraFlat;
                        break;
                    case AnalogyLookAndFeelStyle.Style3D:
                        UserLookAndFeel.Default.Style = LookAndFeelStyle.Style3D;
                        break;
                    case AnalogyLookAndFeelStyle.Office2003:
                        UserLookAndFeel.Default.Style = LookAndFeelStyle.Office2003;
                        break;
                    case AnalogyLookAndFeelStyle.Skin:
                        UserLookAndFeel.Default.Style = LookAndFeelStyle.Skin;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            UserLookAndFeel.Default.StyleChanged += (s, e) =>
            {
                UserLookAndFeel laf = (UserLookAndFeel)s;
                Settings.ApplicationSkinName = laf.ActiveSkinName;
                switch (laf.Style)
                {
                    case LookAndFeelStyle.Flat:
                        Settings.ApplicationStyle = AnalogyLookAndFeelStyle.Flat;
                        break;
                    case LookAndFeelStyle.UltraFlat:
                        Settings.ApplicationStyle = AnalogyLookAndFeelStyle.UltraFlat;
                        break;
                    case LookAndFeelStyle.Style3D:
                        Settings.ApplicationStyle = AnalogyLookAndFeelStyle.Style3D;
                        break;
                    case LookAndFeelStyle.Office2003:
                        Settings.ApplicationStyle = AnalogyLookAndFeelStyle.Office2003;
                        break;
                    case LookAndFeelStyle.Skin:
                        Settings.ApplicationStyle = AnalogyLookAndFeelStyle.Skin;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                Settings.ApplicationSvgPaletteName = laf.ActiveSvgPaletteName;

            };
            //// Register the Winter Joy skin. 
            //System.Reflection.Assembly asm = typeof(DevExpress.UserSkins.WinterJoy).Assembly;
            //DevExpress.Skins.SkinManager.Default.RegisterAssembly(asm);
            //// Apply the skin. 
            //DevExpress.XtraEditors.WindowsFormsSettings.DefaultLookAndFeel.SetSkinStyle("Winter Joy");
            if (Settings.SingleInstance && Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {

                if (Environment.GetCommandLineArgs().Length == 2)
                {
                    var otherAnalogy = GetAlreadyRunningInstance();
                    if (otherAnalogy != null)
                    {
                        SendDataMessage(otherAnalogy, Environment.GetCommandLineArgs()[1]);
                    }
                }
                else
                {
                    XtraMessageBox.Show("Single instance is on. Exiting this instance", "Analogy");
                }

                return;
            }

            if (Settings.IsFirstRun)
            {
                FirstTimeRunForm f = new FirstTimeRunForm();
                f.ShowDialog();
            }
            if (Settings.MainFormType == MainFormType.RibbonForm)
            {
                Application.Run(new MainForm());
            }
            else
            {
                Application.Run(new FluentDesignMainForm());
            }

        }

        private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            /*
            Assembly assembly = args.LoadedAssembly;

            File.AppendAllText("assemblies.txt", assembly.FullName + Environment.NewLine);
            var attribute = assembly.GetCustomAttributes(true).OfType<System.Runtime.Versioning.TargetFrameworkAttribute>().FirstOrDefault();
            if (attribute != null)
                File.AppendAllText("assemblies.txt", attribute.FrameworkDisplayName + Environment.NewLine);
            */
        }
        public static void SendDataMessage(Process targetProcess, string msg)
        {
            //Copy the string message to a global memory area in unicode format
            IntPtr _stringMessageBuffer = Marshal.StringToHGlobalUni(msg);

            //Prepare copy data structure
            NativeMethods.COPYDATASTRUCT _copyData = new NativeMethods.COPYDATASTRUCT();
            _copyData.dwData = IntPtr.Zero;
            _copyData.lpData = _stringMessageBuffer;
            _copyData.cbData =
                msg.Length * 2; //Number of bytes required for marshalling this string as a series of unicode characters
            IntPtr _copyDataBuff = IntPtrAlloc(_copyData);

            //Send message to the other process
            SendMessage(targetProcess.MainWindowHandle, WM_COPYDATA, IntPtr.Zero, _copyDataBuff);

            Marshal.FreeHGlobal(_copyDataBuff);
            Marshal.FreeHGlobal(_stringMessageBuffer);
        }

        // Allocate a pointer to an arbitrary structure on the global heap.
        private static IntPtr IntPtrAlloc<T>(T param)
        {
            IntPtr retval = Marshal.AllocHGlobal(Marshal.SizeOf(param));
            Marshal.StructureToPtr(param, retval, false);
            return retval;
        }

        private static Process GetAlreadyRunningInstance()
        {
            var current = Process.GetCurrentProcess();
            return Process.GetProcessesByName(current.ProcessName).FirstOrDefault(p => p.Id != current.Id);
        }

        private static void CurrentDomain_FirstChanceException(object sender,
            System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            AnalogyLogger.Instance.LogWarning(e.Exception.ToString(), nameof(CurrentDomain_FirstChanceException));
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            AnalogyLogger.Instance.LogException("Error: " + e.ExceptionObject, e.ExceptionObject as Exception, nameof(CurrentDomain_UnhandledException));
            MessageBox.Show("Error: " + e.ExceptionObject, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            AnalogyLogger.Instance.LogException("Error: " + e.Exception, e.Exception, nameof(Application_ThreadException));
            MessageBox.Show("Error: " + e.Exception, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }


        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // missing resources are... missing
            if (args.Name.Contains(".resources"))
            {
                return null;
            }

            // check for assemblies already loaded
            Assembly? assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == args.Name);
            if (assembly != null)
            {
                return assembly;
            }

            // Try to load by filename - split out the filename of the full assembly name
            // and append the base path of the original assembly (ie. look in the same dir)
            // NOTE: this doesn't account for special search paths but then that never
            //           worked before either.
            string filename = args.Name.Split(',')[0] + ".dll".ToLower();



            var paths = FactoriesManager.Instance.ProbingPaths.Select(Path.GetDirectoryName).Except(new List<string> { AssemblyLocation }).Distinct()
                .ToList();
            paths.AddRange(AnalogyNonPersistSettings.Instance.AdditionalAssembliesDependenciesLocations);
            foreach (var path in paths)
            {
                string asmFile = FindFileInPath(filename, path);
                if (!string.IsNullOrEmpty(asmFile))
                {
                    try
                    {
                        string finalPath = Path.GetDirectoryName(asmFile);
                        if (!string.IsNullOrEmpty(finalPath) && paths.Count == 1)
                        {
                            Environment.CurrentDirectory = finalPath;
                        }

                        return Assembly.LoadFrom(asmFile);
                    }
                    catch
                    {
                        //ignore
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Looks for the first match in a file structure
        /// </summary>
        /// <param name="filename">The filename only to look for</param>
        /// <param name="path">Path to start with</param>
        /// <returns>Fully qualified path of the file found or NULL</returns>
        private static string FindFileInPath(string filename, string path)
        {

            foreach (var fullFile in Directory.GetFiles(path))
            {
                var file = Path.GetFileName(fullFile);
                if (file.Equals(filename, StringComparison.InvariantCultureIgnoreCase))
                {
                    return fullFile;
                }
            }

            foreach (var dir in Directory.GetDirectories(path))
            {
                var file = FindFileInPath(filename, dir);
                if (!string.IsNullOrEmpty(file))
                {
                    return file;
                }
            }

            return null;
        }


    }
}
