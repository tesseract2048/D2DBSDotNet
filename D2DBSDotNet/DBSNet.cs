using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace D2DBSDotNet
{
    public class ConnInfo
    {
        public TcpClient Client;
        public string RemoteAddr;
        public NetworkStream NetStream;
        public BinaryWriter NetWriter;
        public BinaryReader NetReader;
        public int GSId;
        public bool Disposed;
        public object StreamLocker;
        public ConnInfo(TcpClient _Client)
        {
            Client = _Client;
            RemoteAddr = ((System.Net.IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString();
            NetStream = Client.GetStream();
            NetReader = new BinaryReader(NetStream);
            NetWriter = new BinaryWriter(NetStream);
            GSId = -1;
            Disposed = false;
            StreamLocker = new object();
        }
        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            if (GSId > -1)
            {
                D2DBS.charlock.UnlockAllCharByGSId(GSId);
                D2DBS.log.Write("error", "Unlocked all characters on GS " + GSId.ToString());
            }
            NetWriter.Close();
            NetReader.Close();
            NetStream.Close();
            NetStream.Dispose();
            Client.Client.Close();
            Client.Close();
        }
    }

    class DBSNet
    {
        TcpListener NetListener;
        Thread ListenThread;
        bool Running;
        List<string> GSAddr;
        Dictionary<int, ConnInfo> GSConn;

        public DBSNet(int Port)
        {
            Running = true;
            GSConn = new Dictionary<int, ConnInfo>();
            try
            {
                NetListener = new TcpListener(new IPEndPoint(IPAddress.Any, Port));
                NetListener.Start(64);
                ListenThread = new Thread(new ThreadStart(ListenLoop));
                ListenThread.Start();
                D2DBS.log.Write("info", "Listening on Port " + Port.ToString());
            }
            catch (Exception e)
            {
                D2DBS.log.Write("fatal", "Cannot listen on port " + Port.ToString() + ": " + e.Message);
                D2DBS.Cleanup();
            }
            GSAddr = new List<string>(D2DBS.config["gameservlist"].Split(','));
        }

        public ConnInfo GetConnByGSId(int GSId)
        {
            return GSConn[GSId];
        }

        private void ClientLoop(object Param)
        {
            ConnInfo Conn = new ConnInfo((TcpClient)Param);
            TcpClient Client = Conn.Client;
            string RemoteAddr = Conn.RemoteAddr;
            NetworkStream NetStream = Conn.NetStream;
            BinaryReader NetReader = Conn.NetReader;
            BinaryWriter NetWriter = Conn.NetWriter;
            try
            {
                Client.ReceiveBufferSize = 65535;
                Client.SendBufferSize = 65535;
                int ConnState = 0;
                while (Conn.Disposed == false && Client.Connected == true)
                {
                    if (ConnState == 0)
                    {
                        byte ConnClass = NetReader.ReadByte();
                        if (ConnClass == 0x88)
                        {
                            for (int i = 0; i < GSAddr.Count; i++)
                            {
                                if (GSAddr[i] == RemoteAddr)
                                {
                                    if (GSConn.ContainsKey(i))
                                    {
                                        D2DBS.log.Write("warn", "Previous D2GS connection found, dispose.");
                                        GSConn[i].Dispose();
                                    }
                                    if (GSConn.ContainsKey(i) == false)
                                        GSConn.Add(i, Conn);
                                    else
                                        GSConn[i] = Conn;
                                    ConnState = 1;
                                    Conn.GSId = i;
                                    D2DBS.log.Write("info", "D2GS connection from " + RemoteAddr + " accepted (GSId: " + i.ToString() + ")");
                                    break;
                                }
                            }
                            if (ConnState == 0)
                            {
                                throw new Exception("D2GS connection from " + RemoteAddr + " not authorized");
                            }
                        }
                        else
                        {
                            throw new Exception("Unknown connection class 0x" + ConnClass.ToString("X"));
                        }
                    }
                    else
                    {
                        int size;
                        while ((size = NetReader.ReadInt16()) > 0)
                        {
                            byte[] buf = new byte[size];
                            int leftsize = size;
                            int pos = 0;
                            buf[0] = (byte)(size & 0xFF);
                            buf[1] = (byte)((size >> 8) & 0xFF);
                            pos += 2;
                            leftsize -= 2;
                            while (Conn.Disposed == false && Client.Connected == true && leftsize > 0)
                            {
                                int readlen = NetReader.Read(buf, pos, leftsize);
                                leftsize -= readlen;
                                pos += readlen;
                            }
                            if (leftsize == 0)
                            {
                                List<byte> packetbuf = new List<byte>(buf);
                                int Code = D2DBS.core.DBSHandlePacket(Conn, packetbuf);
                                if (Code == -1)
                                {
                                    throw new Exception("Failed to handle packet");
                                }
                            }
                            else
                            {
                                throw new Exception("Error occured while receiving packet");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                D2DBS.log.Write("error", "Connection from " + RemoteAddr + " closed: " + e.Message + " (GSId: " + Conn.GSId.ToString() + ")");
            }
            D2DBS.log.Write("error", "D2GS Connection from " + RemoteAddr + " closed (GSId: " + Conn.GSId.ToString() + ")");
            Conn.Dispose();
        }

        public bool BroadcastPacket(byte[] buffer)
        {
            for (int i = 0; i < GSAddr.Count; i++)
            {
                if (GSConn.ContainsKey(i))
                {
                    lock (GSConn[i].StreamLocker)
                    {
                        GSConn[i].NetWriter.Write(buffer);
                        GSConn[i].NetWriter.Flush();
                    }
                }
            }
            return true;
        }

        private void ListenLoop()
        {
            while (Running)
            {
                TcpClient Client = NetListener.AcceptTcpClient();
                Thread ClientThread = new Thread(new ParameterizedThreadStart(ClientLoop));
                ClientThread.Start(Client);
            }
        }
    }
}
