using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace D2DBSDotNet
{
    class DBSPacket
    {
        public enum EPacketType
        {
            GSSaveCharSaveRequest = 0x30,
            GSSaveCharInfoRequest = 0x40,
            GSGetDataRequest = 0x31,
            GSUpdateLadder = 0x32,
            GSCharLock = 0x33,
            GSEchoReply = 0x34,
            GSCloseSignal = 0x37,
            GSKickReply = 0x39,
            DBSSaveDataReply = 0x30,
            DBSGetDataReply = 0x31,
            DBSEchoRequest = 0x34,
            DBSKickRequest = 0x38,
            GSPerformanceCounter = 0x91,
            GSRateExceeded = 0x92,
            GSEmergency = 0x93,
            GSSOJCounterUpdate = 0x96,
            DBSSOJCounterUpdate = 0x97,
            DBSDCTrigger = 0x98
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct THeader
        {
            public short size;
            public byte type;
            public int seqno;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TGSSaveCharSaveRequest
        {
            public THeader h;
            public short datalen;
            public short ist;
            public short changedist;
            /* AccountName */
            /* CharName */
            /* GameName(if charsave) */
            /* IPAddr(if charsave) */
            /* ItemRecord(if charsave) */
            /* data */
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TGSSaveCharInfoRequest
        {
            public THeader h;
            public short datalen;
            /* AccountName */
            /* CharName */
            /* data */
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TGSGetDataRequest
        {
            public THeader h;
            public byte datatype;
            /* AccountName */
            /* CharName */
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TGSUpdateLadder
        {
            public THeader h;
            public byte charlevel;
            public uint charexplow;
            public uint charexphigh;
            public byte charclass;
            public ushort charstatus;
            /* CharName */
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TGSCharLock
        {
            public THeader h;
            public byte lockstatus;
            /* CharName */
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TGSCloseSignal
        {
            public THeader h;
            public byte ifclose;
            /* GameName */
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TGSKickReply
        {
            public THeader h;
            public byte result;
            /* CharName */
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TDBSKickRequest
        {
            public THeader h;
            /* CharName */
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TGSEchoReply
        {
            public THeader h;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TDBSEchoRequest
        {
            public THeader h;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TDBSGetDataReply
        {
            public THeader h;
            public byte result;
            public int charcreatetime;
            public byte allowladder;
            public byte datatype;
            public short datalen;
            /* CharName */
            /* data */
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TDBSSaveDataReply
        {
            public THeader h;
            public byte result;
            public byte datatype;
            /* CharName */
        }

        //New..
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TGSPerformanceCounter
        {
            public THeader h;
            public int BusyPool;
            public int IdlePool;
            public int ThreadNum;
            public int HandleNum;
            public int GENum;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TGSRateExceeded
        {
            public THeader h;
            public int RequestRate;
            /* AccountName */
            /* CharName */
            /* IPAddr */
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TGSEmergency
        {
            public THeader h;
            public int EmergencyType;
            public int Param;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TGSSOJCounterUpdate
        {
            public THeader h;
            public int increment;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TDBSSOJCounterUpdate
        {
            public THeader h;
            public int soj_counter;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TDBSDCTrigger
        {
            public THeader h;
        }

        public int HandlePacket(ConnInfo Conn, List<byte> buf)
        {
            THeader h = new THeader();
            if (buf.Count < Marshal.SizeOf(h)) return -1;
            h = (THeader)D2DBS.core.BytesToStruct(buf.ToArray(), Marshal.SizeOf(h), h.GetType());
            if (buf.Count != h.size) return -1;
            switch ((EPacketType)h.type)
            {
                case EPacketType.GSGetDataRequest:
                    return HandleGetDataRequest(Conn, buf);
                case EPacketType.GSCloseSignal:
                    return HandleCloseSignal(Conn, buf);
                case EPacketType.GSCharLock:
                    return HandleCharLock(Conn, buf);
                case EPacketType.GSKickReply:
                    return HandleKickReply(Conn, buf);
                case EPacketType.GSSaveCharInfoRequest:
                    return HandleSaveCharInfo(Conn, buf);
                case EPacketType.GSSaveCharSaveRequest:
                    return HandleSaveCharSave(Conn, buf);
                case EPacketType.GSUpdateLadder:
                    return HandleUpdateLadder(Conn, buf);
                case EPacketType.GSPerformanceCounter:
                    return HandlePerformanceCounter(Conn, buf);
                case EPacketType.GSRateExceeded:
                    return HandleRateExceeded(Conn, buf);
                case EPacketType.GSEmergency:
                    return HandleEmergency(Conn, buf);
                case EPacketType.GSSOJCounterUpdate:
                    return HandleSOJCounterUpdate(Conn, buf);
                case EPacketType.GSEchoReply:
                    return 0;
                default:
                    return -1;
            }
        }

        private int HandlePerformanceCounter(ConnInfo Conn, List<byte> buf)
        {
            TGSPerformanceCounter packet = new TGSPerformanceCounter();
            int pos = Marshal.SizeOf(packet);
            packet = (TGSPerformanceCounter)D2DBS.core.BytesToStruct(buf.ToArray(), Marshal.SizeOf(packet), packet.GetType());
            D2DBS.mysql.Execute("INSERT DELAYED INTO performance (`date`,`gs_addr`,`busy_pool`, `idle_pool`, `thread_num`, `handle_num`, `ge_num`) VALUES (UNIX_TIMESTAMP(), '" + Conn.RemoteAddr + "','" + packet.BusyPool.ToString() + "','" + packet.IdlePool.ToString() + "','" + packet.ThreadNum.ToString() + "','" + packet.HandleNum.ToString() + "','" + packet.GENum.ToString() + "')");
            return 0;
        }

        private int HandleEmergency(ConnInfo Conn, List<byte> buf)
        {
            TGSEmergency packet = new TGSEmergency();
            int pos = Marshal.SizeOf(packet);
            packet = (TGSEmergency)D2DBS.core.BytesToStruct(buf.ToArray(), Marshal.SizeOf(packet), packet.GetType());
            D2DBS.mysql.Execute("INSERT DELAYED INTO emergency (`date`,`gs_addr`,`type`, `param`) VALUES (UNIX_TIMESTAMP(), '" + Conn.RemoteAddr + "','" + packet.EmergencyType.ToString() + "','" + packet.Param.ToString() + "')");
            return 0;
        }

        private int HandleRateExceeded(ConnInfo Conn, List<byte> buf)
        {
            TGSRateExceeded packet = new TGSRateExceeded();
            int pos = Marshal.SizeOf(packet);
            string[] Names = BytesToString(buf.GetRange(pos, buf.Count - pos)).Split('\0');
            packet = (TGSRateExceeded)D2DBS.core.BytesToStruct(buf.ToArray(), Marshal.SizeOf(packet), packet.GetType());
            D2DBS.mysql.Execute("INSERT DELAYED INTO rate_exceeded (`date`,`gs_addr`,`charname`, `accname`, `ipaddr`, `rate`) VALUES (UNIX_TIMESTAMP(), '" + Conn.RemoteAddr + "','" + Names[0] + "','" + Names[1] + "','" + Names[2] + "','" + packet.RequestRate.ToString() + "')");
            return 0;
        }

        private int HandleCloseSignal(ConnInfo Conn, List<byte> buf)
        {
            TGSCloseSignal packet = new TGSCloseSignal();
            int pos = Marshal.SizeOf(packet);
            packet = (TGSCloseSignal)D2DBS.core.BytesToStruct(buf.ToArray(), Marshal.SizeOf(packet), packet.GetType());
            string[] Names = BytesToString(buf.GetRange(pos, buf.Count - pos)).Split('\0');
            string GameName = Names[0];
            string CharVersion = D2DBS.config["char_version"];
            if (packet.ifclose == 0)
            {
                D2DBS.mysql.Execute("INSERT DELAYED INTO game (`version`, `name`,`startdate`,`enddate`) VALUES ('" + CharVersion + "', '" + D2DBS.mysql.Escape(GameName) + "', UNIX_TIMESTAMP(), '0')");
                D2DBS.log.Write("info", "Game `" + GameName + "` closed on gs " + Conn.GSId.ToString());
            }
            else
            {
                D2DBS.mysql.Execute("UPDATE LOW_PRIORITY `game` SET `enddate` = UNIX_TIMESTAMP() WHERE `version` = '" + CharVersion + " AND `name` = '" + D2DBS.mysql.Escape(GameName) + "' ORDER BY startdate DESC LIMIT 1");
                D2DBS.log.Write("info", "Game `" + GameName + "` started on gs " + Conn.GSId.ToString());
            }
            return 0;
        }

        private int HandleCharLock(ConnInfo Conn, List<byte> buf)
        {
            TGSCharLock packet = new TGSCharLock();
            int pos = Marshal.SizeOf(packet);
            packet = (TGSCharLock)D2DBS.core.BytesToStruct(buf.ToArray(), Marshal.SizeOf(packet), packet.GetType());
            string[] Names = BytesToString(buf.GetRange(pos, buf.Count - pos)).Split('\0');
            string CharName = Names[0];
            D2DBS.log.Write("info", "Charlock `" + CharName + "` set to " + packet.lockstatus.ToString() + " for gs " + Conn.GSId);
            if (packet.lockstatus == 0)
            {
                D2DBS.charlock.UnlockChar(CharName);
            }
            else
            {
                D2DBS.charlock.SetCharLock(CharName, Conn.GSId);
            }
            return 0;
        }

        private int HandleKickReply(ConnInfo Conn, List<byte> buf)
        {
            TGSKickReply packet = new TGSKickReply();
            int pos = Marshal.SizeOf(packet);
            packet = (TGSKickReply)D2DBS.core.BytesToStruct(buf.ToArray(), Marshal.SizeOf(packet), packet.GetType());
            string[] Names = BytesToString(buf.GetRange(pos, buf.Count - pos)).Split('\0');
            string CharName = Names[0];
            if (packet.result == 1)
            {
                D2DBS.charlock.UnlockChar(CharName);
                D2DBS.log.Write("info", "Char `" + CharName + "` kicked from gs " + Conn.GSId.ToString());
            }
            return 0;
        }

        private int HandleSaveCharInfo(ConnInfo Conn, List<byte> buf)
        {
            TGSSaveCharInfoRequest packet = new TGSSaveCharInfoRequest();
            int pos = Marshal.SizeOf(packet);
            packet = (TGSSaveCharInfoRequest)D2DBS.core.BytesToStruct(buf.ToArray(), Marshal.SizeOf(packet), packet.GetType());
            string[] Names = BytesToString(buf.GetRange(pos, buf.Count - pos - packet.datalen - 1)).Split('\0');
            string AccountName = Names[0];
            string CharName = Names[1];
            string CharVersion = D2DBS.config["char_version"];
            byte[] data = buf.GetRange(buf.Count - packet.datalen, packet.datalen).ToArray();
            TCharInfo CharInfo = (TCharInfo)D2DBS.core.BytesToStruct(data, typeof(TCharInfo));

            TDBSSaveDataReply rpacket = new TDBSSaveDataReply();
            byte[] rawcharname = StringToChars(CharName);
            rpacket.h.seqno = packet.h.seqno;
            rpacket.h.type = (byte)EPacketType.DBSSaveDataReply;
            rpacket.h.size = (short)(Marshal.SizeOf(rpacket) + rawcharname.Length);
            rpacket.datatype = 2;
            if (D2DBS.charfile.SetCharInfo(AccountName, CharName, CharInfo) == true)
            {
                D2DBS.mysql.Execute("INSERT DELAYED INTO `charstat` (`version`, `charname`, `accname`, `charclass`, `level`, `ist`, `lastupdate`) SELECT '" + CharVersion + "', '" + CharName + "', '" + AccountName + "', '" + CharInfo.summary.charclass.ToString() + "', '" + CharInfo.summary.charlevel.ToString() + "', '0', UNIX_TIMESTAMP() FROM `charstat` WHERE NOT EXISTS (SELECT `charname` FROM `charstat` WHERE `version` = '" + CharVersion + "' `charname` = '" + CharName + "') LIMIT 1");
                D2DBS.mysql.Execute("UPDATE LOW_PRIORITY `charstat` SET `accname` = '" + AccountName + "', `charclass` = '" + CharInfo.summary.charclass.ToString() + "', `level` = '" + CharInfo.summary.charlevel.ToString() + "', `lastupdate` = UNIX_TIMESTAMP() WHERE version = '" + CharVersion + "' AND charname = '" + CharName + "'");
                rpacket.result = 0;
                D2DBS.log.Write("info", "Saved charinfo `" + CharName + "`(*" + AccountName + ") for gs " + Conn.GSId.ToString());
            }
            else
            {
                rpacket.result = 1;
                D2DBS.log.Write("info", "Failed to saved charinfo `" + CharName + "`(*" + AccountName + ") for gs " + Conn.GSId.ToString());
            }
            lock (Conn.StreamLocker)
            {
                Conn.NetWriter.Write(D2DBS.core.StructToBytes(rpacket));
                Conn.NetWriter.Write(rawcharname);
                Conn.NetWriter.Flush();
            }
            return 0;
        }

        private int HandleSaveCharSave(ConnInfo Conn, List<byte> buf)
        {
            TGSSaveCharSaveRequest packet = new TGSSaveCharSaveRequest();
            int pos = Marshal.SizeOf(packet);
            packet = (TGSSaveCharSaveRequest)D2DBS.core.BytesToStruct(buf.ToArray(), Marshal.SizeOf(packet), packet.GetType());
            string[] Names = BytesToString(buf.GetRange(pos, buf.Count - pos - packet.datalen)).Split('\0');
            string AccountName = Names[0];
            string CharName = Names[1];
            string GameName = Names[2];
            string IPAddr = Names[3];
            string ItemLog = Names[4];
            string CharVersion = D2DBS.config["char_version"];
            byte result = 1;
            byte[] data = buf.GetRange(buf.Count - packet.datalen, packet.datalen).ToArray();

            if (data[0] != 0x55 || data[1] != 0xAA)
            {
                D2DBS.log.Write("error", "Invaild charsave `" + CharName + "`(*" + AccountName + ")@" + GameName + " for gs " + Conn.GSId.ToString());
            }
            else
            {
                if (D2DBS.charfile.SetCharSave(AccountName, CharName, data) == true)
                {
                    D2DBS.mysql.Execute("UPDATE LOW_PRIORITY charstat SET `ist` = '" + packet.ist.ToString() + "' WHERE version = '" + CharVersion + "' AND charname = '" + CharName + "'");

                    if (ItemLog.Length > 0)
                    {
                        D2DBS.mysql.Execute("INSERT DELAYED INTO itemrecord (`date`,`version`,`game`,`charname`,`accname`,`ipaddr`,`itemchange`,`istchange`) VALUES (UNIX_TIMESTAMP(), '" + CharVersion + "', '" + D2DBS.mysql.Escape(GameName) + "', '" + CharName + "', '" + AccountName + "', '" + IPAddr + "', '" + D2DBS.mysql.Escape(ItemLog) + "','" + packet.changedist.ToString() + "')");
                    }

                    result = 0;
                    D2DBS.log.Write("info", "Saved charsave `" + CharName + "`(*" + AccountName + ") for gs " + Conn.GSId.ToString());
                }
                else
                {
                    D2DBS.log.Write("info", "Failed to saved charsave `" + CharName + "`(*" + AccountName + ") for gs " + Conn.GSId.ToString());
                }
            }

            TDBSSaveDataReply rpacket = new TDBSSaveDataReply();
            byte[] rawcharname = StringToChars(CharName);
            rpacket.h.seqno = packet.h.seqno;
            rpacket.h.type = (byte)EPacketType.DBSSaveDataReply;
            rpacket.h.size = (short)(Marshal.SizeOf(rpacket) + rawcharname.Length);
            rpacket.datatype = 1;
            rpacket.result = result;
            lock (Conn.StreamLocker)
            {
                Conn.NetWriter.Write(D2DBS.core.StructToBytes(rpacket));
                Conn.NetWriter.Write(rawcharname);
                Conn.NetWriter.Flush();
            }
            return 0;
        }

        private int HandleSOJCounterUpdate(ConnInfo Conn, List<byte> buf)
        {
            TGSSOJCounterUpdate packet = new TGSSOJCounterUpdate();
            int pos = Marshal.SizeOf(packet);
            string[] Names = BytesToString(buf.GetRange(pos, buf.Count - pos)).Split('\0');
            packet = (TGSSOJCounterUpdate)D2DBS.core.BytesToStruct(buf.ToArray(), Marshal.SizeOf(packet), packet.GetType());
            D2DBS.diabloClone.Increment(packet.increment, Names[0]);

            TDBSSOJCounterUpdate rpacket = new TDBSSOJCounterUpdate();
            rpacket.h.seqno = packet.h.seqno;
            rpacket.h.type = (byte)EPacketType.DBSSOJCounterUpdate;
            rpacket.h.size = (short)(Marshal.SizeOf(rpacket));
            rpacket.soj_counter = D2DBS.diabloClone.Get();
            lock (Conn.StreamLocker)
            {
                Conn.NetWriter.Write(D2DBS.core.StructToBytes(rpacket));
                Conn.NetWriter.Flush();
            }
            return 0;
        }

        private int HandleUpdateLadder(ConnInfo Conn, List<byte> buf)
        {
            TGSUpdateLadder packet = new TGSUpdateLadder();
            int pos = Marshal.SizeOf(packet);
            packet = (TGSUpdateLadder)D2DBS.core.BytesToStruct(buf.ToArray(), Marshal.SizeOf(packet), packet.GetType());
            string[] Names = BytesToString(buf.GetRange(pos, buf.Count - pos)).Split('\0');
            string CharName = Names[0];
            D2DBS.ladder.UpdateLadder(packet.charexplow, packet.charstatus, packet.charlevel, packet.charclass, CharName);
            D2DBS.log.Write("info", "Ladder of `" + CharName + "`(L=" + packet.charlevel.ToString() + ",C=" + packet.charclass.ToString() + ",S=" + packet.charstatus.ToString() + ") updated for gs " + Conn.GSId.ToString());
            return 0;
        }

        private int HandleGetDataRequest(ConnInfo Conn, List<byte> buf)
        {
            TGSGetDataRequest packet = new TGSGetDataRequest();
            int pos = Marshal.SizeOf(packet);
            packet = (TGSGetDataRequest)D2DBS.core.BytesToStruct(buf.ToArray(), Marshal.SizeOf(packet), packet.GetType());
            string[] Names = BytesToString(buf.GetRange(pos, buf.Count - pos)).Split('\0');
            string AccountName = Names[0];
            string CharName = Names[1];
            TCharInfo CharInfo = new TCharInfo();
            byte result = 1;
            byte[] data = null;
            if (packet.datatype == 1)
            {
                if (D2DBS.charlock.SetCharLock(CharName, Conn.GSId))
                {
                    data = D2DBS.charfile.GetCharSave(AccountName, CharName);
                    if (data == null)
                    {
                        result = 1;
                        D2DBS.log.Write("error", "Failed to load charsave `" + CharName + "`(*" + AccountName + ") for gs " + Conn.GSId.ToString());
                        D2DBS.charlock.UnlockChar(CharName);
                    }
                    else
                    {
                        result = 0;
                        D2DBS.log.Write("info", "Loaded charsave `" + CharName + "`(*" + AccountName + ") for gs " + Conn.GSId.ToString());
                    }
                }
                else
                {
                    int LockGSId = D2DBS.charlock.GetGSIdByLock(CharName);
                    result = 1;
                    D2DBS.log.Write("warn", "Char `" + CharName + "`(*" + AccountName + ") already locked on gs " + LockGSId.ToString() + " for gs " + Conn.GSId.ToString());
                    //KickPlayer(D2DBS.net.GetConnByGSId(LockGSId), CharName);
                }
            }
            else
            {
                data = D2DBS.charfile.GetCharInfoRaw(AccountName, CharName);
                D2DBS.log.Write("info", "Loaded charinfo `" + CharName + "`(*" + AccountName + ") for gs " + Conn.GSId.ToString());
            }
            if (result == 0)
            {
                byte[] RAWCharInfo = D2DBS.charfile.GetCharInfoRaw(AccountName, CharName);
                if (RAWCharInfo == null)
                {
                    D2DBS.log.Write("error", "Failed to load charinfo `" + CharName + "`(*" + AccountName + ") for gs " + Conn.GSId.ToString());
                    D2DBS.charlock.UnlockChar(CharName);
                    result = 1;
                }
                else
                {
                    CharInfo = (TCharInfo)D2DBS.core.BytesToStruct(RAWCharInfo, typeof(TCharInfo));
                }
            }
            TDBSGetDataReply rpacket = new TDBSGetDataReply();
            if (result == 0)
            {
                int ladder_time = int.Parse(D2DBS.config["ladderinit_time"]);
                if (D2DBS.charfile.GetLadder((ushort)CharInfo.summary.charstatus) == 1)
                {
                    if (CharInfo.header.create_time < ladder_time)
                    {
                        rpacket.allowladder = 0;
                        D2DBS.log.Write("info", "Char `" + CharName + "`(*" + AccountName + ") expired for ladder, converting to non-ladder");
                    }
                    else
                    {
                        rpacket.allowladder = 1;
                    }
                }
                else
                {
                    rpacket.allowladder = 0;
                }
                rpacket.charcreatetime = CharInfo.header.create_time;
                rpacket.datalen = (short)data.Length;
            }
            else
            {
                rpacket.allowladder = 0;
                rpacket.charcreatetime = 0;
                rpacket.datalen = 0;
            }
            byte[] rawcharname = StringToChars(CharName);
            rpacket.h.seqno = packet.h.seqno;
            rpacket.h.type = (byte)EPacketType.DBSGetDataReply;
            rpacket.h.size = (short)(Marshal.SizeOf(rpacket) + rawcharname.Length + rpacket.datalen);
            rpacket.datatype = packet.datatype;
            rpacket.result = result;
            lock (Conn.StreamLocker)
            {
                Conn.NetWriter.Write(D2DBS.core.StructToBytes(rpacket));
                Conn.NetWriter.Write(rawcharname);
                if (rpacket.datalen > 0) Conn.NetWriter.Write(data);
                Conn.NetWriter.Flush();
            }
            return 0;
        }

        public void KickPlayer(ConnInfo Conn, string CharName)
        {
            if (Conn.Disposed) return;
            D2DBS.log.Write("info", "Kicking `" + CharName + "`");
            TDBSKickRequest kpacket = new TDBSKickRequest();
            byte[] RAWCharName = StringToChars(CharName);
            kpacket.h.seqno = 0;
            kpacket.h.type = (byte)EPacketType.DBSKickRequest;
            kpacket.h.size = (short)(Marshal.SizeOf(kpacket.GetType()) + RAWCharName.Length);
            lock (Conn.StreamLocker)
            {
                Conn.NetWriter.Write(D2DBS.core.StructToBytes(kpacket));
                Conn.NetWriter.Write(RAWCharName);
                Conn.NetWriter.Flush();
            }
        }

        public void TriggerDC()
        {
            TDBSDCTrigger rpacket = new TDBSDCTrigger();
            rpacket.h.seqno = 0;
            rpacket.h.type = (byte)EPacketType.DBSDCTrigger;
            rpacket.h.size = (short)(Marshal.SizeOf(rpacket));
            D2DBS.net.BroadcastPacket(D2DBS.core.StructToBytes(rpacket));
        }

        public void MakeTestPacket(Stream network, string AccountName, string CharName)
        {
            byte[] rawAccount = StringToChars(AccountName);
            byte[] rawChar = StringToChars(CharName);
            TGSGetDataRequest packet = new TGSGetDataRequest();
            packet.h.seqno = 1;
            packet.h.size = (short)(rawAccount.Length + rawChar.Length + Marshal.SizeOf(packet));
            packet.h.type = (byte)EPacketType.GSGetDataRequest;
            packet.datatype = 2;
            byte[] rawPacket = D2DBS.core.StructToBytes(packet);
            network.Write(rawPacket, 0, rawPacket.Length);
            network.Write(rawAccount, 0, rawAccount.Length);
            network.Write(rawChar, 0, rawChar.Length);
        }

        public void MakeTestPacket2(Stream network, string AccountName, string CharName)
        {
            byte[] rawAccount = StringToChars(AccountName);
            byte[] rawChar = StringToChars(CharName);
            TGSGetDataRequest packet = new TGSGetDataRequest();
            packet.h.seqno = 1;
            packet.h.size = (short)(rawAccount.Length + rawChar.Length + Marshal.SizeOf(packet));
            packet.h.type = (byte)EPacketType.GSGetDataRequest;
            packet.datatype = 1;
            byte[] rawPacket = D2DBS.core.StructToBytes(packet);
            network.Write(rawPacket, 0, rawPacket.Length);
            network.Write(rawAccount, 0, rawAccount.Length);
            network.Write(rawChar, 0, rawChar.Length);
        }

        public void MakeTestPacket2_1(Stream network, string CharName)
        {
            byte[] rawChar = StringToChars(CharName);
            TGSKickReply packet = new TGSKickReply();
            packet.h.seqno = 1;
            packet.h.size = (short)(rawChar.Length + Marshal.SizeOf(packet));
            packet.h.type = (byte)EPacketType.GSKickReply;
            packet.result = 1;
            byte[] rawPacket = D2DBS.core.StructToBytes(packet);
            network.Write(rawPacket, 0, rawPacket.Length);
            network.Write(rawChar, 0, rawChar.Length);
        }

        public void MakeTestPacket3(Stream network, string CharName)
        {
            byte[] rawChar = StringToChars(CharName);
            TGSKickReply packet = new TGSKickReply();
            packet.h.seqno = 1;
            packet.h.size = (short)(rawChar.Length + Marshal.SizeOf(packet));
            packet.h.type = (byte)EPacketType.GSKickReply;
            packet.result = 1;
            byte[] rawPacket = D2DBS.core.StructToBytes(packet);
            network.Write(rawPacket, 0, rawPacket.Length);
            network.Write(rawChar, 0, rawChar.Length);
        }

        public void MakeTestPacket4(Stream network, string CharName)
        {
            byte[] rawChar = StringToChars(CharName);
            TGSCloseSignal packet = new TGSCloseSignal();
            packet.h.seqno = 1;
            packet.h.size = (short)(rawChar.Length + Marshal.SizeOf(packet));
            packet.h.type = (byte)EPacketType.GSCloseSignal;
            packet.ifclose = 0;
            byte[] rawPacket = D2DBS.core.StructToBytes(packet);
            network.Write(rawPacket, 0, rawPacket.Length);
        }

        private string BytesToString(List<byte> src)
        {
            return System.Text.Encoding.ASCII.GetString(src.ToArray());
        }

        private byte[] StringToChars(string src)
        {
            return System.Text.Encoding.ASCII.GetBytes(src + '\0');
        }
    }

}
