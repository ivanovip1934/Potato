using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace POTATO
{
    internal class ServiceClass
    {
        public static bool InstallerServiceIsUsing()
        {

            try
            {
                using (var mutex = Mutex.OpenExisting(@"Global\_MSIExecute"))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                // Mutex not found; MSI isn't running
            }
            return false;

        }
        public static int StartVal()
        {
            int startvalue = 0;
            RandomNumberGenerator rng = new RNGCryptoServiceProvider();
            byte[] tokenData = new byte[1];
            rng.GetBytes(tokenData);
            string Varstr = String.Empty;
            for (int i = 0; i <= tokenData.Length - 1; i++)
            {
                Varstr += tokenData[i].ToString();
            }
            startvalue = int.Parse(Varstr);
            return startvalue;
        }
        public static bool  IsLocked(string fileName)
        {
            try
            {
                using (FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    fs.Close();
                    // Здесь вызываем свой метод, работаем с файлом
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (ex.HResult == -2147024894)
                    return false;
            }
            return true;
        }
    }
}
