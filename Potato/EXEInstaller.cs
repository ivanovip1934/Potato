using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace POTATO
{
     class EXEInstaller:Installer
    {
        public string PathToCleanApp { get; private set; }
        public string PropertyCleanApp { get; private set; }
        public bool FirstCleanApp { get; private set; }





        new public event EventHandler<InstallEvent> EventLog;

        public EXEInstaller(EXEOptions options, Dictionary<string, string> ptrnNames) {
            
            Name = options.Name;
            x86AppOnX64Os = options.x86AppOnX64Os;
            PathToInstaller = GetPathToInstaller(options.PathX64, options.PathX86);
            Version = options.Version;
            Update = options.Update;
            ForceRestart = options.ForceRestart;
            Property = options.Property;
            PathToCleanApp = options.PathToCleanApp;
            PropertyCleanApp = options.PropertyCleanApp;
            FirstCleanApp = options.FirstCleanApp;
            UninstallPropertyEXE = options.UninstallPropertyEXE;
            NeedEXELog = options.NeedEXELog;  
            rnd = new Random(ServiceClass.StartVal());
            ptrnnames = ptrnNames;
            PathToTempFileLog = Path.Combine(Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine), $"LogInstallApp{rnd.Next(1000, 2000)}.log");            
        }

        protected override string typeInstaller()
        {
            return ".exe";
        }
        private protected override string CreateInstallString()
        {
            string installString = $"{PathToInstaller}";
            if (!String.IsNullOrEmpty(Property))
                installString += $" {Property}";
            if (NeedEXELog)
            {
                installString = installString.ToUpper().Replace("%PATHLOG%", PathToTempFileLog);
            }
            return installString;
        }

        private protected string CreateUninstallString( string reguninstallString) { 
            string uninstallString = $"{reguninstallString} {UninstallPropertyEXE}";
            if (NeedEXELog)
            {
                uninstallString = uninstallString.ToUpper().Replace("%PATHLOG%", PathToTempFileLog);
            }
            return uninstallString;
        }

        private protected override void UninstallApp(CurApp curapp)
        {
            string uninstallString = CreateUninstallString(curapp.UninstallString);
            Console.WriteLine(uninstallString);            
            int numuninst = Cmd(uninstallString);
            if (NeedEXELog)
            {
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
                        EventLog?.Invoke(this, new InstallEvent($"Event: ошибка N {numuninst} при удалении программы", ResultJob.ErrorUninstall));
                        Environment.Exit(0);
                        break;
                }
            }

        }
    }
}
