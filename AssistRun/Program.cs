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

namespace AssistRun
{
    public class Program
    {
        private const int N_CHECK_GAP_TIME = 5000;//5秒
        private const int N_RUN_GAP_MAX_TIME = 30;//30秒
        private const string STR_RUN_FILE = "Run.data";
        private static string m_assitProcessName = "";
        private static bool m_bCheck = false;
        private static int m_nLastRunTimeStamp = 0;
        private static bool m_bRunning = false;
        private static Thread m_thread = null;

        private static DateTime m_startTime = new DateTime(1970, 1, 1).ToLocalTime();

        #region Public Method
        //-------------------------------------------------------------------------
        public static void Main(string[] args)
        {
            //0.保存需要辅助的程序名称
            /*if (null == args || args.Length <= 0)
            {
                Console.WriteLine("Start params args is null or Empty !");
                Environment.Exit(0);
                return;
            }

            m_assitProcessName = args[0];*/

            m_assitProcessName = "Render";

            Console.WriteLine("-----------------------------------");
            Console.WriteLine("0.启动辅助运行程序");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("需要辅助运行程序：" + m_assitProcessName);

            //1.干掉同名的其他进程
            __KillSameOtherProcess();

            //2.开启线程
            //__StartThread();

            Console.Read();
        }
        //-------------------------------------------------------------------------
        #endregion

        #region Private Method
        //-------------------------------------------------------------------------
        private static Process[] GetCurRunProcess()
        {
            Process[] processes = Process.GetProcesses();
            StringBuilder builder = new StringBuilder();
            foreach (Process p in processes)
            {
                try
                {
                    builder.AppendLine("名称：" + p.ProcessName + "，启动时间：" + p.StartTime.ToShortTimeString() + "，进程ID：" + p.Id.ToString());
                }
                catch (Exception)
                {
                    //builder.Append(ex.Message.ToString());//某些系统进程禁止访问，所以要加异常处理
                }
            }

            Console.WriteLine(builder.ToString());

            return processes;
        }
        //-------------------------------------------------------------------------
        private static void __KillProcessByName(string strProcessName)
        {
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("3.干掉卡死的程序：" + strProcessName);
            Console.WriteLine("-----------------------------------");
            Process[] processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (null == process)
                {
                    continue;
                }
                if (process.ProcessName.Equals(strProcessName, StringComparison.OrdinalIgnoreCase))
                {
                    process.Kill();
                }
            }
        }
        //-------------------------------------------------------------------------
        private static int __GetTimeStamp(DateTime time)
        {
            return (int)(time - m_startTime).TotalSeconds;
        }
        //-------------------------------------------------------------------------
        private static void __KillSameOtherProcess()
        {
            Process curProcess = Process.GetCurrentProcess();
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("1.干掉同名的其他进程");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("当前进程号: " + curProcess.Id);

            Process[] processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (null == process || false == process.ProcessName.Equals(curProcess.ProcessName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (process.Id != curProcess.Id)
                {
                    process.Kill();
                }
            }
        }
        //-------------------------------------------------------------------------
        private static void __ReadRunStatusFile()
        {
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("2.读取运行状态文件");
            Console.WriteLine("-----------------------------------");
            if (false == File.Exists(STR_RUN_FILE))
            {
                Console.WriteLine("__ReadRunStatusFile:" + STR_RUN_FILE + " not Exists !");
                return;
            }

            string strContent = File.ReadAllText(STR_RUN_FILE);
            if (string.IsNullOrEmpty(strContent))
            {
                Console.WriteLine("__ReadRunStatusFile:" + STR_RUN_FILE + " file content is NullOrEmpty !");
                return;
            }

            string[] datas = strContent.Split('_');
            if (null == datas || datas.Length < 2)
            {
                Console.WriteLine("__ReadRunStatusFile:" + STR_RUN_FILE + " file content Data Invalid ! strContent=" + strContent);
                return;
            }

            m_bCheck = int.Parse(datas[0]) == 1 ? true : false;
            m_nLastRunTimeStamp = int.Parse(datas[1]);

            Console.WriteLine("__ReadRunStatusFile:m_bCheck=" + m_bCheck + ",m_nLastRunTimeStamp=" + m_nLastRunTimeStamp);
        }
        //-------------------------------------------------------------------------
        private static void __StartThread()
        {
            if (null == m_thread)
            {
                m_bRunning = true;
                m_thread = new Thread(__Run);
                Console.WriteLine("-----------------------------------");
                Console.WriteLine("2.开启检查线程");
                Console.WriteLine("-----------------------------------");
            }
        }
        //-------------------------------------------------------------------------
        private static void __Run()
        {
            while (m_bRunning)
            {
                try
                {
                    //3.读取运行状态文件,解析值
                    __ReadRunStatusFile();

                    //4.是否开启检查
                    __CheckRun();
                    Thread.Sleep(N_CHECK_GAP_TIME);
                }
                catch (Exception)
                {

                }
            }
        }
        //-------------------------------------------------------------------------
        private static void __CheckRun()
        {
            if (false == m_bCheck)
            {
                return;
            }

            int nNowTimeStamp = __GetTimeStamp(DateTime.Now);
            if (nNowTimeStamp - m_nLastRunTimeStamp > N_RUN_GAP_MAX_TIME)
            {
                __KillProcessByName(m_assitProcessName);
                __RebootProcess();
                m_bRunning = false;
                Environment.Exit(0);
            }
        }
        //-------------------------------------------------------------------------
        private static void __RebootProcess()
        {
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("4.重新启动程序");
            Console.WriteLine("-----------------------------------");
            string strDefaultPath = System.Environment.CurrentDirectory;
            Console.WriteLine("strDefaultPath=" + strDefaultPath);

            string strRebootPath = __GetEXEPath(strDefaultPath);
            Console.WriteLine("strRebootPath=" + strRebootPath);

            __Reboot(strRebootPath);
        }
        //-------------------------------------------------------------------------
        private static string __GetEXEPath(string strDir)
        {
            string strPath = string.Empty;
            if (false == Directory.Exists(strDir))
            {
                return strPath;
            }

            var filePaths = Directory.GetFiles(strDir, "*.*", SearchOption.AllDirectories);
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
        //-------------------------------------------------------------------------
        private static void __Reboot(string strPath)
        {
            strPath = strPath.Replace("/", "\\");
            if (false == File.Exists(strPath))
            {
                return;
            }

            Process.Start(strPath);
        }
        //-------------------------------------------------------------------------
        #endregion
    }
}
