using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace POTATO
{
    class WriteEvents
    {

        private string pathToDirLog;
        public WriteEvents(string PathToDirLog) {
            this.pathToDirLog = PathToDirLog;
        }
        
        public void WriteLog(object sender, InstallEvent e) {

            // Set VARIABLES
            string logFileName = $"{Environment.MachineName}.log";
            string appName = ((Installer)sender).Name.Replace("\"", "").Replace(",", "").Replace(":", "");
            string tmpFileLog = ((Installer)sender).PathToTempFileLog;
            string fileNameLog = String.Empty;
            string olderrorlog = String.Empty;
            string pathToDirLogApp = Path.Combine(pathToDirLog, appName);

            Dictionary<ResultJob, string> dicPathName = new Dictionary<ResultJob, string>() {
                { ResultJob.Install, Path.Combine(pathToDirLogApp,"install")},
                { ResultJob.ErrorInstall, Path.Combine(pathToDirLogApp, "error-install")},
                { ResultJob.Uninstall,Path.Combine(pathToDirLogApp, "uninstall")},
                { ResultJob.ErrorUninstall,Path.Combine(pathToDirLogApp, "error-uninstall")}
            };

            // END set VARIABLES

            #region Create Directory for store Logs

            if (!Directory.Exists(pathToDirLogApp))
                try{
                    Directory.CreateDirectory(pathToDirLogApp);
                }
                catch (Exception expException){
                    Console.WriteLine(expException.Message);
                    Environment.Exit(1);
                }

            #endregion

            #region Create dicPathName[e.RJ] Directory 
            if (!Directory.Exists(dicPathName[e.RJ]))
                try
                {
                    Directory.CreateDirectory(dicPathName[e.RJ]);
                }
                catch (Exception expException)
                {
                    Console.WriteLine(expException.Message);
                    Environment.Exit(1);
                }
            #endregion


            if (e.RJ == ResultJob.Install | e.RJ == ResultJob.Uninstall)
            {
                fileNameLog = Path.Combine(dicPathName[e.RJ], logFileName);
                olderrorlog = Path.Combine(dicPathName[e.RJ + 2], logFileName);
                if (File.Exists(olderrorlog))
                    File.Delete(olderrorlog);
            }
            else
                fileNameLog = Path.Combine(dicPathName[e.RJ], logFileName);
            if (File.Exists(tmpFileLog))
            {
                while (ServiceClass.IsLocked(tmpFileLog))
                    Thread.Sleep(3000);
                File.Copy(tmpFileLog, fileNameLog, true);
                File.Delete(tmpFileLog);
            }
        }
    }
}
