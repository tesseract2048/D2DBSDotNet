using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace D2DBSDotNet
{
    static class D2DBS
    {
        public static DBSCore core;
        public static DBSNet net;
        public static DBSPacket packet;
        public static DBSCharLock charlock;
        public static DBSCharFile charfile;
        public static DBSLog log;
        public static DBSMySQL mysql;
        public static DBSConfig config;
        public static DBSLadder ladder;
        public static DBSSoap soap;
        public static DBSDiabloClone diabloClone;
        public static bool ifService = false;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            string[] args = Environment.GetCommandLineArgs();

            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "-s" || args[i] == "--service")
                {
                    Environment.CurrentDirectory = Application.StartupPath;
                    ifService = true;
                    break;
                }
            }

            core = new DBSCore();

            if (ifService)
            {
                System.ServiceProcess.ServiceBase.Run(new D2DBSService());
            }
            else
            {
                Application.ApplicationExit += new EventHandler(AppCleanup);
                core.Init();

                DBSTest test = new DBSTest();
                test.RunTests();
                Application.Run();
            }
        }

        static void AppCleanup(object sender, System.EventArgs e){
            Cleanup();
        }

        public static void Cleanup()
        {
            core.Cleanup();
            D2DBS.log.Write("info", "Sending term signal...");
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}