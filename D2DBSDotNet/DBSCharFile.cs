using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace D2DBSDotNet
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    struct TCharInfoHeader
    {
        public int magicword;	/* static for check */
        public int version;	/* charinfo file version */
        public int create_time;	/* character creation time */
        public int last_time;	/* character last access time */
        public int checksum;
        public int total_play_time;	/* total in game play time */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6, ArraySubType = UnmanagedType.SysInt)]
        public int[] reserved;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string charname;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string account;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string realmname;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    struct TCharInfoSummary
    {
        public int experience;
        public int charstatus;
        public int charlevel;
        public int charclass;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    struct TCharInfoPortrait
    {
        public short header;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11, ArraySubType = UnmanagedType.I1)]
        public byte[] gfx;
        public byte char_class;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11, ArraySubType = UnmanagedType.I1)]
        public byte[] color;
        public byte level;
        public byte status;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = UnmanagedType.I1)]
        public byte[] u1;
        public byte ladder;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.I1)]
        public byte[] u2;
        public byte end;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    struct TCharInfo
    {
        public TCharInfoHeader header;
        public TCharInfoPortrait portrait;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30, ArraySubType = UnmanagedType.I1)]
        public byte[] pad;
        public TCharInfoSummary summary;
    }

    class DBSCharFile
    {
        private DateTime TimeStampStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public short GetHardcore(ushort Status)
        {
            if ((Status & 0x04) != 0)
                return 1;
            else
                return 0;
        }
        public short GetExpansion(ushort Status)
        {
            if ((Status & 0x20) != 0)
                return 1;
            else
                return 0;
        }
        public short GetLadder(ushort Status)
        {
            if ((Status & 0x40) != 0)
                return 1;
            else
                return 0;
        }

        public byte[] GetCharSave(string AccountName, string CharName)
        {
            try
            {
                string charsave = D2DBS.config["charsavedir"] + @"\" + CharName;
                if (!File.Exists(charsave)) return null;
                FileStream CharStream = new FileStream(charsave, FileMode.Open, FileAccess.Read);
                byte[] buf = new byte[CharStream.Length];
                CharStream.Read(buf, 0, (int)CharStream.Length);
                CharStream.Close();
                CharStream.Dispose();
                if (buf[4] != 0x60 && buf[4] != 0x59) return null;
                if (buf.Length > 130)
                {
                    int checksum = CalcChecksumOrginal(buf);
                    buf[12] = (byte)(checksum & 0xFF);
                    buf[13] = (byte)((checksum >> 8) & 0xFF);
                    buf[14] = (byte)((checksum >> 16) & 0xFF);
                    buf[15] = (byte)((checksum >> 24) & 0xFF);
                }
                return buf;
            }
            catch
            {
                return null;
            }
        }

        public bool SetCharSave(string AccountName, string CharName, byte[] buf)
        {
            try
            {
                string charsave = D2DBS.config["charsavedir"] + @"\" + CharName;
                if (!File.Exists(charsave))
                {
                    throw new Exception("CharSave doesn't exist");
                }
                int checksum = CalcChecksumFixed(buf);
                buf[12] = (byte)(checksum & 0xFF);
                buf[13] = (byte)((checksum >> 8) & 0xFF);
                buf[14] = (byte)((checksum >> 16) & 0xFF);
                buf[15] = (byte)((checksum >> 24) & 0xFF);
                FileStream CharStream = new FileStream(charsave, FileMode.Create, FileAccess.Write);
                CharStream.Write(buf, 0, (int)buf.Length);
                CharStream.Close();
                CharStream.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SetCharInfoRaw(string AccountName, string CharName, byte[] buf)
        {
            try
            {
                string charinfo = D2DBS.config["charinfodir"] + @"\" + AccountName + @"\" + CharName;
                if (!File.Exists(charinfo))
                {
                    throw new Exception("CharInfo doesn't exist");
                }
                FileStream CharStream = new FileStream(charinfo, FileMode.Create, FileAccess.Write);
                CharStream.Write(buf, 0, (int)buf.Length);
                CharStream.Close();
                CharStream.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SetCharInfo(string AccountName, string CharName, TCharInfo CharInfo)
        {
            try
            {
                CharInfo.header.last_time = (int)((DateTime.Now - TimeStampStart).TotalSeconds);
                return SetCharInfoRaw(AccountName, CharName, D2DBS.core.StructToBytes(CharInfo));
            }
            catch
            {
                return false;
            }
        }

        public byte[] GetCharInfoRaw(string AccountName, string CharName)
        {
            try
            {
                string charinfo = D2DBS.config["charinfodir"] + @"\" + AccountName + @"\" + CharName;
                if (!File.Exists(charinfo)) return null;
                FileStream CharStream = new FileStream(charinfo, FileMode.Open, FileAccess.Read);
                byte[] buf = new byte[192];
                CharStream.Read(buf, 0, 192);
                CharStream.Close();
                CharStream.Dispose();
                return buf;
            }
            catch
            {
                return null;
            }
        }

        public TCharInfo GetCharInfo(string AccountName, string CharName)
        {
            byte[] raw = GetCharInfoRaw(AccountName, CharName);
            if (raw == null)
            {
                return new TCharInfo();
            }
            else
            {
                return (TCharInfo)D2DBS.core.BytesToStruct(GetCharInfoRaw(AccountName, CharName), typeof(TCharInfo));
            }
        }

        public int CalcChecksumFixed(byte[] buf)
        {
            int checksum = 0;
            unchecked
            {
                for (int i = 0; i < buf.Length; i++)
                {
                    int ch = buf[i];
                    if (i >= 12 & i < 16)
                        ch = 0;
                    if (checksum < 0) ch += 3;
                    checksum = checksum * 3 + ch;
                }
            }
            return checksum;
        }

        public int CalcChecksumOrginal(byte[] buf)
        {
            int checksum = 0;
            unchecked
            {
                for (int i = 0; i < buf.Length; i++)
                {
                    int ch = buf[i];
                    if (i >= 12 & i < 16)
                        ch = 0;
                    if (checksum < 0) ch += 1;
                    checksum = checksum * 2 + ch;
                }
            }
            return checksum;
        }

    }
}
