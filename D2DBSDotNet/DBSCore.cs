using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace D2DBSDotNet
{
    class DBSCore
    {
        private System.Timers.Timer timer;

        public void Init()
        {
            D2DBS.config = new DBSConfig(@"conf\d2dbs.conf");
            D2DBS.log = new DBSLog();
            D2DBS.ladder = new DBSLadder();
            D2DBS.charfile = new DBSCharFile();
            D2DBS.charlock = new DBSCharLock();
            D2DBS.mysql = new DBSMySQL();
            D2DBS.packet = new DBSPacket();
            D2DBS.diabloClone = new DBSDiabloClone();

            D2DBS.log.Write("info", "Basic module loaded successfully, starting up network.");
            D2DBS.net = new DBSNet(Int32.Parse(D2DBS.config["servaddrs"].Split(':')[1]));

            if (D2DBS.config["enable_soap"] == "1")
            {
                D2DBS.soap = new DBSSoap();
            }

            timer = new System.Timers.Timer();
            timer.AutoReset = true;
            timer.Interval = 30000;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(TimerElapsed);
            timer.Start();
        }

        private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //GC.Collect();
        }

        public void Cleanup()
        {
            D2DBS.log.Write("info", "Cleaning up...");
            if (D2DBS.ladder != null) D2DBS.ladder.FlushLadder();
        }

        public int DBSHandlePacket(ConnInfo Conn, List<byte> buf)
        {
            try
            {
                return D2DBS.packet.HandlePacket(Conn, buf);
            }
            catch (Exception e)
            {
                D2DBS.log.Write("error", "Error occured while handling packet: " + e.ToString());
                return -1;
            }
        }

        public byte[] StructToBytes(object obj)
        {
            int rawsize = Marshal.SizeOf(obj);
            IntPtr buffer = Marshal.AllocHGlobal(rawsize);
            Marshal.StructureToPtr(obj, buffer, false);
            byte[] rawdatas = new byte[rawsize];
            Marshal.Copy(buffer, rawdatas, 0, rawsize);
            Marshal.FreeHGlobal(buffer);
            return rawdatas;
        }

        public object BytesToStruct(byte[] buf, int len, Type type)
        {
            object rtn;
            IntPtr buffer = Marshal.AllocHGlobal(len);
            Marshal.Copy(buf, 0, buffer, len);
            rtn = Marshal.PtrToStructure(buffer, type);
            Marshal.FreeHGlobal(buffer);
            return rtn;
        }

        public void BytesToStruct(byte[] buf, int len, object rtn)
        {
            IntPtr buffer = Marshal.AllocHGlobal(len);
            Marshal.Copy(buf, 0, buffer, len);
            Marshal.PtrToStructure(buffer, rtn);
            Marshal.FreeHGlobal(buffer);
        }

        public void BytesToStruct(byte[] buf, object rtn)
        {
            BytesToStruct(buf, buf.Length, rtn);
        }

        public object BytesToStruct(byte[] buf, Type type)
        {
            return BytesToStruct(buf, buf.Length, type);
        }
    }
}
