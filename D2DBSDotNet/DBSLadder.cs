using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections;

namespace D2DBSDotNet
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    struct TLadderInfo
    {
        public uint experience; //4
        public ushort status;   //2
        public byte level;      //1
        public byte char_class; //1
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string charname; //16
        public TLadderInfo(bool clean)
        {
            experience = 0;
            status = 0;
            level = 0;
            char_class = 0;
            charname = "";
        }
    };
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TLadderIndex
    {
        public int type;
        public int offset;
        public int number;
    };
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TLadderHeader
    {
        public int maxtype;
        public int checksum;
    }

    class DBSLadder
    {
        private const int MAX_TYPE = 35;
        private string LadderFile = D2DBS.config["ladderdir"] + @"\ladder.D2DV";
        private string LadderBakFile = D2DBS.config["ladderdir"] + @"\ladderbk.D2DV";

        private List<List<TLadderInfo>> Ladder = new List<List<TLadderInfo>>();
        private List<Hashtable> CharIndex = new List<Hashtable>();

        private DateTime LastSave = DateTime.Now;
        private System.Timers.Timer FlushTimer;
        private object LadderLock;

        public DBSLadder()
        {
            LadderLock = new object();
            for (int i = 0; i < MAX_TYPE; i++)
            {
                List<TLadderInfo> TypeLadder = new List<TLadderInfo>();
                Ladder.Add(TypeLadder);
                CharIndex.Add(new Hashtable());
            }
            if (!LoadLadderFile())
            {
                SaveLadderFile();
            }
            FlushTimer = new System.Timers.Timer();
            FlushTimer.Interval = int.Parse(D2DBS.config["laddersave_interval"]) * 1000;
            FlushTimer.Elapsed += FlushTimerElapsed;
            FlushTimer.Start();
        }

        private void FlushTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            FlushLadder();
        }

        public void FlushLadder()
        {
            D2DBS.log.Write("info", "Flushing ladder...");
            lock (LadderLock)
            {
                for (int i = 0; i < MAX_TYPE; i++)
                {
                    SortLadder(i);
                }
                SaveLadderFile();
            }
        }

        public void UpdateLadder(uint experience, ushort status, byte level, byte char_class, string charname)
        {
            short hardcore, expansion;
            int ladder_overall_type, ladder_class_type;

            hardcore = D2DBS.charfile.GetHardcore(status);
            expansion = D2DBS.charfile.GetExpansion(status);

            if (expansion == 0 && char_class > 0x04)
            {
                D2DBS.log.Write("error", "Non-expansion char `" + charname + "` with expansion class");
                return;
            }
            if (expansion == 1 && char_class > 0x06)
            {
                D2DBS.log.Write("error", "Expansion char `" + charname + "` with invaild class");
                return;
            }
            if (hardcore == 1 && expansion == 1)
            {
                ladder_overall_type = 0x13;
            }
            else if (hardcore == 0 && expansion == 1)
            {
                ladder_overall_type = 0x1B;
            }
            else if (hardcore == 1 && expansion == 0)
            {
                ladder_overall_type = 0x00;
            }
            else if (hardcore == 0 && expansion == 0)
            {
                ladder_overall_type = 0x09;
            }
            else
            {
                ladder_overall_type = 0x00;
            }
            ladder_class_type = ladder_overall_type + char_class + 1;
            InsertLadder(ladder_overall_type, experience, status, level, char_class, charname);
            InsertLadder(ladder_class_type, experience, status, level, char_class, charname);
        }

        private void InsertLadder(int type, uint experience, ushort status, byte level, byte char_class, string charname)
        {
            lock (LadderLock)
            {
                if (CharIndex[type].Contains(charname))
                {
                    int index = (int)CharIndex[type][charname];
                    TLadderInfo LadderInfo = Ladder[type][index];
                    LadderInfo.char_class = char_class;
                    LadderInfo.experience = experience;
                    LadderInfo.level = level;
                    LadderInfo.status = status;
                    Ladder[type][index] = LadderInfo;
                }
                else
                {
                    TLadderInfo LadderInfo = new TLadderInfo(true);
                    LadderInfo.charname = charname;
                    LadderInfo.char_class = char_class;
                    LadderInfo.experience = experience;
                    LadderInfo.level = level;
                    LadderInfo.status = status;
                    CharIndex[type][charname] = Ladder[type].Count;
                    Ladder[type].Add(LadderInfo);
                }
            }
        }

        private void SortLadder(int type)
        {
            lock (LadderLock)
            {
                int ladder_max = GetNumberByType(type);
                Ladder[type].Sort(new LadderSort());
                if (Ladder[type].Count > ladder_max)
                {
                    Ladder[type].RemoveRange(ladder_max, Ladder[type].Count - ladder_max);
                }
                CharIndex[type].Clear();
                for (int i = 0; i < ladder_max; i++)
                {
                    CharIndex[type][Ladder[type][i].charname] = i;
                }
            }
        }

        private int GetNumberByType(int i)
        {
            int number;
            if (i == 0x00 || i == 0x09 || i == 0x13 || i == 0x1B)
            {
                number = 1000;
            }
            else if ((i > 0x00 && i <= 0x05) || (i > 0x09 && i <= 0x0E) || (i > 0x13 && i <= 0x1A) || (i > 0x1B && i <= 0x22))
            {
                number = 200;
            }
            else
            {
                number = 0;
            }
            return number;
        }

        private bool LoadLadderFile()
        {
            if (!File.Exists(LadderFile)) return false;
            try
            {
                TLadderHeader Header = new TLadderHeader();
                TLadderIndex[] LadderIndex = new TLadderIndex[35];
                TLadderInfo LadderInfo = new TLadderInfo();
                FileStream LadderStream = new FileStream(LadderFile, FileMode.Open, FileAccess.Read);
                BinaryReader LadderReader = new BinaryReader(LadderStream);

                byte[] buf = new Byte[Marshal.SizeOf(Header)];
                int i, j, count = 0;
                LadderReader.Read(buf, 0, buf.Length);
                Header = (TLadderHeader)D2DBS.core.BytesToStruct(buf, Header.GetType());
                for (i = 0; i < MAX_TYPE; i++)
                {
                    buf = new Byte[Marshal.SizeOf(LadderIndex[i])];
                    LadderReader.Read(buf, 0, buf.Length);
                    LadderIndex[i] = (TLadderIndex)D2DBS.core.BytesToStruct(buf, LadderIndex[i].GetType());
                }
                for (i = 0; i < MAX_TYPE; i++)
                {
                    Ladder[i].Clear();
                    CharIndex[i].Clear();
                    for (j = 0; j < LadderIndex[i].number; j++)
                    {
                        buf = new Byte[Marshal.SizeOf(LadderInfo)];
                        LadderReader.Read(buf, 0, buf.Length);
                        LadderInfo = (TLadderInfo)D2DBS.core.BytesToStruct(buf, LadderInfo.GetType());
                        CharIndex[i][LadderInfo.charname] = Ladder[i].Count;
                        Ladder[i].Add(LadderInfo);
                        count++;
                    }
                }
                LadderStream.Close();
                LadderStream.Dispose();
                D2DBS.log.Write("info", "Ladder loaded successfully, " + count.ToString() + " char(s) in total.");
                return true;
            }
            catch
            {
                D2DBS.log.Write("error", "Failed to load ladder file, rebuild it.");
            }
            return false;
        }

        private bool SaveLadderFile()
        {
            lock (LadderLock)
            {
                TLadderIndex[] LadderIndex = new TLadderIndex[35];
                TLadderHeader Header = new TLadderHeader();
                TLadderInfo emptydata = new TLadderInfo(true);
                int i, j;
                int count = 0;
                int start = Marshal.SizeOf(Header) + Marshal.SizeOf(LadderIndex[0]) * 35;
                for (i = 0; i < MAX_TYPE; i++)
                {
                    LadderIndex[i].type = i;
                    LadderIndex[i].offset = start;
                    LadderIndex[i].number = GetNumberByType(i);
                    start += LadderIndex[i].number * 24;
                }
                byte[] buf;
                MemoryStream MSLadderBuffer = new MemoryStream();
                BinaryWriter LadderBuffer = new BinaryWriter(MSLadderBuffer);
                Header.maxtype = MAX_TYPE;
                LadderBuffer.Write(D2DBS.core.StructToBytes(Header));
                for (i = 0; i < MAX_TYPE; i++)
                {
                    LadderBuffer.Write(D2DBS.core.StructToBytes(LadderIndex[i]));
                }
                for (i = 0; i < MAX_TYPE; i++)
                {
                    for (j = 0; j < LadderIndex[i].number; j++)
                    {
                        if (j >= Ladder[i].Count)
                            LadderBuffer.Write(D2DBS.core.StructToBytes(emptydata));
                        else
                        {
                            LadderBuffer.Write(D2DBS.core.StructToBytes(Ladder[i][j]));
                            count++;
                        }
                    }
                }
                LadderBuffer.Close();

                buf = MSLadderBuffer.ToArray();
                MSLadderBuffer.Close();
                MSLadderBuffer.Dispose();

                int Checksum = CalcChecksum(buf);
                Header.checksum = Checksum;
                buf[4] = (byte)(Checksum & 0xFF);
                buf[5] = (byte)((Checksum >> 8) & 0xFF);
                buf[6] = (byte)((Checksum >> 16) & 0xFF);
                buf[7] = (byte)((Checksum >> 24) & 0xFF);

                if (D2DBS.config["enablebackup"] != "0" && File.Exists(LadderFile))
                {
                    try
                    {
                        if (File.Exists(LadderBakFile)) File.Delete(LadderBakFile);
                        File.Move(LadderFile, LadderBakFile);
                        D2DBS.log.Write("info", "Ladder backup executed.");
                    }
                    catch (Exception e)
                    {
                        D2DBS.log.Write("error", "Backup failed: " + e.Message);
                    }
                }

                FileStream LadderStream = new FileStream(LadderFile, FileMode.Create, FileAccess.Write);
                LadderStream.Write(buf, 0, buf.Length);
                LadderStream.Close();
                LadderStream.Dispose();
                D2DBS.log.Write("info", "Ladder saved successfully, " + count.ToString() + " char(s) in total.");
                return true;
            }
        }

        private int CalcChecksum(byte[] buf)
        {
            const int offset = 4;
            int checksum;
            int i;
            uint ch;

            unchecked
            {
                checksum = 0;
                for (i = 0; i < buf.Length; i++)
                {
                    ch = buf[i];
                    if (i >= offset && i < offset + 4) ch = 0;
                    if (checksum < 0)
                        ch++;
                    checksum = 2 * checksum + (int)ch;
                }
            }
            return checksum;
        }
    }
    class LadderSort : IComparer<TLadderInfo>
    {
        public int Compare(TLadderInfo obj1, TLadderInfo obj2)
        {
            if (obj1.experience < obj2.experience)
                return 1;
            else if (obj1.experience > obj2.experience)
                return -1;
            else
                return 0;
        }
    }
}
