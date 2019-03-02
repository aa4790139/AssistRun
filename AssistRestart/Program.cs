//*************************************************************************
//     创建日期:     2019-3-1
//     创建作者:     Harry
//     版权所有:     剑齿虎
//     开发版本:     V1.0
//     文件名称:     Program.cs
//     概要说明:     
//     责任说明:
//*************************************************************************
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace AssitRestart
{
    class Program
    {
        static void Main(string[] args)
        {
            string strCurProcessPath = Process.GetCurrentProcess().MainModule.FileName;
            Console.WriteLine("strCurProcessPath=" + strCurProcessPath);

            string strDefaultPath = System.Environment.CurrentDirectory;
            Console.WriteLine("strDefaultPath=" + strDefaultPath);

            string strPath = __GetEXEPath(strDefaultPath);
            Console.WriteLine("strPath="+ strPath);

            try
            {
                __RestartQH(strPath);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("__RestartQH:ex=" + ex.StackTrace);
            }

            Environment.Exit(0);
        }

        private static void __RestartQH(string strPath)
        {
            strPath = strPath.Replace("/", "\\");
            if (false == File.Exists(strPath))
            {
                return;
            }

            Thread.Sleep(4000);

            string strExe = Path.GetExtension(strPath);
            string strProcessName = Path.GetFileName(strPath).Replace(strExe, "");
            var alreadyOpen = Process.GetProcesses().Where(p => p.MainWindowHandle != IntPtr.Zero
                && p.ProcessName == strProcessName).Count() > 0;
            if (false == alreadyOpen)
            {
                Process.Start(strPath);
            }

            Console.Read();
        }

        private static string __GetEXEPath(string strDir)
        {
            string strPath = string.Empty;
            if (false == Directory.Exists(strDir))
            {
                return strPath;
            }

            var filePaths = Directory.GetFiles(strDir, "*.*", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < filePaths.Length; i++)
            {
                var strFilePath = filePaths[i];
                if (false == string.IsNullOrEmpty(strFilePath) && strFilePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    strPath = strFilePath;
                    break;
                }
            }

            return strPath;
        }
    }
}
