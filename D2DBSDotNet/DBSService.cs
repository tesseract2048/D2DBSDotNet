using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace D2DBSDotNet
{
    public struct SERVICE_STATUS
    {
        public int serviceType;
        public int currentState;
        public int controlsAccepted;
        public int win32ExitCode;
        public int serviceSpecificExitCode;
        public int checkPoint;
        public int waitHint;
    }

    public enum State
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }
    public class D2DBSService : System.ServiceProcess.ServiceBase
    {
        [DllImport("ADVAPI32.DLL", EntryPoint = "SetServiceStatus")]
        public static extern bool SetServiceStatus(IntPtr hServiceStatus, SERVICE_STATUS lpServiceStatus);
        private SERVICE_STATUS myServiceStatus;
        public D2DBSService()
        {
            CanPauseAndContinue = true;
            CanHandleSessionChangeEvent = true;
            ServiceName = "D2DBS";
        }
        private void InitializeComponent()
        {
            this.CanPauseAndContinue = false;
            this.CanShutdown = true;
            this.CanHandleSessionChangeEvent = false;
            this.ServiceName = "D2DBSService";
        }
        protected override void OnStart(string[] args)
        {
            IntPtr handle = this.ServiceHandle;
            D2DBS.core.Init();
            myServiceStatus.currentState = (int)State.SERVICE_RUNNING;
            SetServiceStatus(handle, myServiceStatus);
        }

        protected override void OnStop()
        {
            this.ExitCode = 0;
            D2DBS.Cleanup();
        }
    }
}
