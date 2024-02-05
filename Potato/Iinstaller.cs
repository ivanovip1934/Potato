using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace POTATO
{
    internal class Installer
    {
        public string Name { get; private protected set; }
        public string PathToInstaller { get; private protected set; }
        public bool x86AppOnX64Os { get; private protected set; }
        public string Version { get; private protected set; }
        public bool Update { get; private protected set; }
        public bool RemoveOtherVersion { get; private protected set; }
        public bool ForceRestart { get; private protected set; }
        public string Property { get; private protected set; }
        public string UninstallPropertyEXE { get; private protected set; } 
        public bool NeedEXELog { get; private protected set; }

        private protected Random rnd;
        private protected Dictionary<string, string> ptrnnames;
        public string PathToTempFileLog { get; private protected set; }

        public event EventHandler<InstallEvent> EventLog;

        public Installer() {
            WriteEvents wrevent = new WriteEvents(GeneralConfig.GetInstance().PathToDirLog);
            EventLog += wrevent.WriteLog;
        }


        
        private protected virtual string CreateInstallString() { return String.Empty; }
        virtual protected string typeInstaller() { return String.Empty; }
        private protected string GetPathToInstaller(string pathX64, string pathX86)
        {
            bool IsOSX64 = Environment.Is64BitOperatingSystem;

            if (!IsOSX64 || this.x86AppOnX64Os) {
                return pathToInstaller(pathX86);
            }
            return pathToInstaller(pathX64);

        }
        private string pathToInstaller(string Path) {
            string typeInstllr = typeInstaller();
            typeInstllr = typeInstllr.Replace(".","").ToUpper();

            if (!String.IsNullOrEmpty(Path))
            {
                if (File.Exists(Path))
                    return Path;
                else
                {
                    Console.WriteLine($"ERROR: Path to {typeInstllr}  => {Path} incorrect");
                    Environment.Exit(0);
                }
            }
            else
            {
                Console.WriteLine($"ERROR: Path to package {typeInstllr}  not set");                
                Environment.Exit(0);
            }
            return String.Empty;
        }
        public virtual void InstallApp()
        {
            // 1. Проверяем: есть ли установленная версия данной программы?
            CurApp curapp = ListProg();
            if (curapp != null)
            {
                Console.WriteLine($"Cur App version {curapp.DisplayVersion}");
                Console.WriteLine($"Cur App UninstallString {curapp.UninstallString}");
                VerCurApp isneedinstall = IsNeedInstall(curapp);
                if (isneedinstall == VerCurApp.IsSame)
                {
                    // Если версия установленного приложения равно версии установочного пакета - то  выходим из приложения.
                    Console.WriteLine("программа установленна той же версии - завершаем работу");
                    Environment.Exit(0);
                }
                else if (isneedinstall == VerCurApp.IsGreate & !RemoveOtherVersion)
                {
                    // Если версия установленного приложения больше версии установочного пакета и не требуется удаления других версий отличных от msi то просто выходим из приложения.
                    Console.WriteLine("программа установленна  более новой версии и не требуется ее удаление - завершаем работу");
                    Environment.Exit(0);
                }
                else
                {
                    // если Update = false - то удаляем текущую установленную программу.
                    if (!Update)
                    {
                        UninstallApp(curapp);
                        if (ForceRestart)
                            Environment.Exit(0);
                    }
                }
            }
            string installString = CreateInstallString();
            Console.WriteLine("install string: " + installString);
            

            int numinst = Cmd(installString);
            Regex tstMsi = new Regex(@"msiexe", RegexOptions.IgnoreCase);
            if (!tstMsi.IsMatch(installString) && !NeedEXELog) { }
            
            else {
                
                switch (numinst)
                {
                    case 0:
                        EventLog?.Invoke(this, new InstallEvent("Event: программа установлена", ResultJob.Install));
                        break;
                    case 1641:
                        EventLog?.Invoke(this, new InstallEvent("Event: программа установлена и требуется перезагрузка", ResultJob.Install));
                        Environment.Exit(0);
                        break;
                    default:
                        EventLog?.Invoke(this, new InstallEvent($"Event: ошибка № {numinst} при установки программы", ResultJob.ErrorInstall));
                        Environment.Exit(0);
                        break;
                }
            }               
        }
        public VerCurApp IsNeedInstall(CurApp curapp)
        {
            string[] arrVersionCurApp = curapp.DisplayVersion.Trim().Split('.');
            string[] arrversionMSI = Version.Trim().Split('.');
            int count = Math.Min(arrVersionCurApp.Count(), arrversionMSI.Count());
            int valueCurApp = 0;
            int valueVerMsi = 0;

            for (int i = 0; i < count; i++)
            {
                valueCurApp = int.Parse(arrVersionCurApp[i]);
                valueVerMsi = int.Parse(arrversionMSI[i]);
                if (valueVerMsi > valueCurApp)
                    return VerCurApp.IsLess;
                else if (valueVerMsi < valueCurApp)
                    return VerCurApp.IsGreate;
            }
            return VerCurApp.IsSame;
        }
        private protected virtual void UninstallApp(CurApp curapp) {

        }

        protected int Cmd(string line)
        {
            int startvalue = ServiceClass.StartVal();
            Thread.Sleep(startvalue);
            Random rnd = new Random(startvalue);

            do
            {
                Thread.Sleep(rnd.Next(1500, 10000));
            } while (ServiceClass.InstallerServiceIsUsing());

            Process newpc = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = $"/c {line}",
                WindowStyle = ProcessWindowStyle.Hidden
            });
            int listpr = newpc.HandleCount;
            newpc.WaitForExit();
            int PrcExitCod = newpc.ExitCode;
            Console.WriteLine(PrcExitCod.ToString());
            return PrcExitCod;
        }

        private protected CurApp ListProg()
        {
#if DEBUG
            Console.WriteLine("Start search installed app");
#endif
            string registry_key = String.Empty;
            if (Environment.Is64BitOperatingSystem)
                registry_key = this.x86AppOnX64Os ?
                    @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall" :
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            else
                registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

            if (string.IsNullOrEmpty(registry_key) || string.IsNullOrEmpty(this.Name))
            {
#if DEBUG
                Console.WriteLine("reg path or appName is null");
#endif
                Environment.Exit(0);
            }

            string prntName = GetPattern(this.Name);
            Regex rgx = new Regex(prntName, RegexOptions.IgnoreCase);

            using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registry_key))
            {
                var appfromreg = from subkey in key.GetSubKeyNames()
                                 let displayName = key.OpenSubKey(subkey).GetValue("DisplayName")?.ToString()
                                 where displayName != null
                                 where rgx.IsMatch(displayName)
                                 select new CurApp
                                 (
                                     displayName,
                                     key.OpenSubKey(subkey).GetValue("DisplayVersion")?.ToString(),
                                     key.OpenSubKey(subkey).GetValue("InstallLocation")?.ToString(),
                                     key.OpenSubKey(subkey).GetValue("UninstallString")?.ToString()                                     
                                 );

                if (appfromreg.Count() == 1)
                {
#if DEBUG

                    Console.WriteLine($"DisplayName: {appfromreg.First().DisplayName}\n" +
                                              $"DisplayVersion: {appfromreg.First().DisplayVersion}\n" +
                                              $"UninstallString: {appfromreg.First().UninstallString}\n" +
                                              $"InstallLocation: {appfromreg.First().InstallLocation}");
#endif
                    return appfromreg.First();
                }
                return null;
            }
        }

        private protected string GetPattern(string name)
        {
            if (ptrnnames.ContainsKey(name))
                return ptrnnames[name];

            foreach (KeyValuePair<string, string> ptrn in this.ptrnnames)
            {
                if (IsName(name, ptrn))
                    return ptrn.Value;
            }
            return name;
        }

        private protected static bool IsName(string appname, KeyValuePair<string, string> patter)
        {
            Regex rgx = new Regex(patter.Value, RegexOptions.IgnoreCase);
            return rgx.IsMatch(appname);
        }
        
    }
}
