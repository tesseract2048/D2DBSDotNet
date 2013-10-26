using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace D2DBSDotNet
{
    struct TLockNode
    {
        public string CharName;
        public int GSId;
        public TLockNode(string _CharName, int _GSId)
        {
            CharName = _CharName;
            GSId = _GSId;
        }
    }

    [System.Runtime.Remoting.Contexts.Synchronization]
    class DBSCharLock : System.ContextBoundObject
    {
        private const int MAX_GS = 32;
        private Hashtable LockTable = new Hashtable();
        private List<Hashtable> GSLockTable = new List<Hashtable>();

        public DBSCharLock()
        {
            for (int i = 0; i < MAX_GS; i++)
                GSLockTable.Add(new Hashtable());
            D2DBS.log.Write("info", "Character lock table built for up to " + MAX_GS.ToString() + " game server(s).");
        }

        public int GetLockedCount()
        {
            return LockTable.Count;
        }

        public bool QueryCharLock(string CharName)
        {
            return LockTable.Contains(CharName);
        }

        public bool SetCharLock(string CharName, int GSId)
        {
            if (LockTable.Contains(CharName) == true)
            {
                return false;
            }
            TLockNode NewLock = new TLockNode(CharName, GSId);
            LockTable.Add(CharName, NewLock);
            if (GSId != -1) GSLockTable[GSId].Add(CharName, NewLock);
            D2DBS.log.Write("info", "Locked char `" + CharName + "` for gs " + GSId.ToString());
            return true;
        }

        public int GetGSIdByLock(string CharName)
        {
            if (LockTable.Contains(CharName) == false)
            {
                return -1;
            }
            TLockNode CurrLock = (TLockNode)LockTable[CharName];
            return CurrLock.GSId;
        }

        public bool UnlockChar(string CharName)
        {
            if (LockTable.Contains(CharName) == false)
            {
                return true;
            }
            TLockNode CurrLock = (TLockNode)LockTable[CharName];
            if (CurrLock.GSId != -1) GSLockTable[CurrLock.GSId].Remove(CharName);
            LockTable.Remove(CharName);
            D2DBS.log.Write("info", "Unlocked char `" + CharName + "`");
            return true;
        }

        public bool UnlockAllCharByGSId(int GSId)
        {
            List<string> PendingUnlock = new List<string>();
            foreach (DictionaryEntry LockEntry in GSLockTable[GSId])
            {
                PendingUnlock.Add((string)LockEntry.Key);
            }
            for (int i = 0; i < PendingUnlock.Count; i++)
            {
                GSLockTable[GSId].Remove(PendingUnlock[i]);
                LockTable.Remove(PendingUnlock[i]);
            }
            D2DBS.log.Write("info", "Unlocked all chars on gs " + GSId.ToString());
            return true;
        }
    }
}
