using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace D2DBSDotNet
{
    class DBSConfig
    {
        private System.Collections.Hashtable ConfigTable = new System.Collections.Hashtable();
        public DBSConfig(string ConfigFile)
        {
            StreamReader ConfigReader = new StreamReader(ConfigFile);
            while (ConfigReader.EndOfStream == false)
            {
                string ConfigLine = ConfigReader.ReadLine().Trim();
                if (ConfigLine == null || ConfigLine.Length == 0 || ConfigLine[0] == '#' || ConfigLine.IndexOf('=') == -1) continue;
                string ItemName = ConfigLine.Substring(0, ConfigLine.IndexOf('=')).Trim();
                string ItemVal = ConfigLine.Substring(ConfigLine.IndexOf('=') + 1, ConfigLine.Length - ConfigLine.IndexOf('=') - 1).Trim();
                this[ItemName] = ItemVal;
            }
        }
        public string this[string Name]{
            get
            {
                if (ConfigTable.Contains(Name))
                    return (string)ConfigTable[Name];
                else
                    return null;
            }
            set{
                lock (ConfigTable.SyncRoot)
                {
                    ConfigTable[Name] = value;
                }
            }
        }
    }
}
