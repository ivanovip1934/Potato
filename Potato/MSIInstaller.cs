using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;

namespace POTATO
{
    [Serializable]
    class MSIInstaller:Installer
    {

        //public string Name { get; private set; }
        //public string PathMsi { get; private set; }
        //public bool x86AppOnX64Os { get; private set; }
        //public string Version { get; private set; }
        //public bool Update { get; private set; }
        //public bool ForceRestart { get; private set; }
        //public string Property { get; private set; }

        //private Random rnd;
        //private Dictionary<string,string> ptrnnames;
        //public string PathToTempFileLog { get; private set; }

        new public event EventHandler<InstallEvent> EventLog;

        public MSIInstaller(string pathX64, string pathX86, string version, bool update, bool forceRestart, string property)
        {
            Version = version;
            PathToInstaller = GetPathToInstaller(pathX64, pathX86);
            Update = update;
            ForceRestart = forceRestart;
            Property = property;
        }

        
        public MSIInstaller(Options options, Dictionary<string,string> ptrnNames)
        {

            x86AppOnX64Os = options.x86AppOnX64Os;
            PathToInstaller = GetPathToInstaller(options.PathX64, options.PathX86);
            Version = (!string.IsNullOrEmpty(options.Version)) ? options.Version : GetInfoMSI(PathToInstaller, "ProductVersion");
            Name = GetInfoMSI(PathToInstaller, "ProductName");           
            Update = options.Update;
            RemoveOtherVersion = options.RemoveOtherVersion;
            ForceRestart = options.ForceRestart;
            Property = options.Property;
            UninstallPropertyEXE = options.UninstallPropertyEXE;
            NeedEXELog = options.NeedEXELog;  
            rnd = new Random(ServiceClass.StartVal());
            ptrnnames = ptrnNames;
            PathToTempFileLog = Path.Combine(Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine), $"LogInstallApp{rnd.Next(1000, 2000)}.log");

        }


        protected override string typeInstaller()
        {
            return ".msi";
        }

        private protected override void UninstallApp(CurApp curapp)
        {
            string uninstallString = curapp.UninstallString.ToLower();

            #region Удаляем Msi пакеты используя msiexec
            if (!string.IsNullOrEmpty(uninstallString) && uninstallString.Contains("msiexec"))
            {
                Console.WriteLine($"Будем удалять {curapp.DisplayName} версии {curapp.DisplayVersion} используя msiexec");
                if (uninstallString.Contains("/i"))
                {
                    Console.WriteLine("Vmesto udaleniy zapustinza ustanovka");
                    uninstallString = uninstallString.ToLower().Replace("/i", "/x");
                }

                if (!uninstallString.Contains("quiet"))
                    uninstallString += " /quiet";

                if (ForceRestart & !uninstallString.Contains("forcerestart"))
                    uninstallString += " /forcerestart";
                uninstallString += $" /log {PathToTempFileLog}";
                Console.WriteLine($"result uninstallString: {uninstallString}");
                int numuninst = Cmd(uninstallString);

                switch (numuninst)
                {
                    case 0:
                        EventLog?.Invoke(this, new InstallEvent("Event: программа удалена", ResultJob.Uninstall));
                        break;
                    case 1641:
                        EventLog?.Invoke(this, new InstallEvent("Event: программа установлена и требуется перезагрузка", ResultJob.Uninstall));
                        Environment.Exit(0);
                        break;
                    default:
                        EventLog?.Invoke(this, new InstallEvent($"Event: ошибка № {numuninst} при удалении программы", ResultJob.ErrorUninstall));
                        Environment.Exit(0);
                        break;
                }

            }
            #endregion

        }
        private protected override string CreateInstallString()
        {
            string installString = $"msiexec /i \"{PathToInstaller}\" /quiet";
            if (!String.IsNullOrEmpty(Property))
                installString += $" {Property}";
            if (ForceRestart)
                installString += " /forcerestart";
            installString += $" /log {PathToTempFileLog}";
            return installString;

        }
       
        string GetInfoMSI(string fileName, string property)
        {
            GetMsiInfo msiinfo = new GetMsiInfo();
            msiinfo.GetMSIInfo(fileName, property);
            return msiinfo.ValueProperty;
        }
        
    }
}
