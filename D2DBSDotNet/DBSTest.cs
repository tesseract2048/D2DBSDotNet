using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace D2DBSDotNet
{
    class DBSTest
    {
        Random rndSeed = new Random();

        public void RunTests()
        {
            /*string x = "";
            for (int i = 0; i < 1000; i++)
                x += D2DBS.diabloClone.GenerateSeed().ToString() + "\r\n";
            System.IO.File.WriteAllText(@"E:\dc_seed.txt", x);*/
            //D2DBS.mysql.Execute("SELECT * FROM game");
            //TestMySQL();
            //TestPacket();
            //TestNet();
            //TestPacket();
            //D2DBS.log.Write("debug", "Test Finished.");
        }

        public void TestMySQL()
        {
            Thread[] TestThread = new Thread[20];
            for (int i = 0; i < 20; i++)
            {
                TestThread[i] = new Thread(new ParameterizedThreadStart(TestMySQL_1));
                TestThread[i].Start();
            }
        }

        public void TestMySQL_1(object param)
        {
            for (int i = 0; i < 1000; i++)
            {
                D2DBS.mysql.Execute("insert into test values ('')");
            }
        }

        TcpClient Client = new TcpClient();

        public void TestPacket()
        {
            Client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6114));
            Client.GetStream().Write(new byte[] { 0x88 }, 0, 1);
            Thread[] TestThread = new Thread[6];
            for (int i = 0; i < 6; i++)
            {
                TestThread[i] = new Thread(new ParameterizedThreadStart(TestPacket_1));
            }
            TestThread[0].Start("nonzlp");
            TestThread[1].Start("nonlad");
            TestThread[2].Start("exphc");
            TestThread[3].Start("stdhc");
            TestThread[4].Start("nonnon");
            TestThread[5].Start("nec-binarie");
        }

        public void TestPacket_1(object param)
        {
            string charname = (string)param;
            for (int i = 0; i < 25; i++)
            {
                lock (Client.GetStream())
                {
                    D2DBS.packet.MakeTestPacket(Client.GetStream(), "binarie", charname);
                    D2DBS.packet.MakeTestPacket2(Client.GetStream(), "binarie", charname);
                    D2DBS.packet.MakeTestPacket3(Client.GetStream(), charname);
                }
            }
        }

        public void TestNet()
        {
            TcpClient Client1 = new TcpClient();
            Client1.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6114));
            Client1.GetStream().Write(new byte[] { 0x88 }, 0, 1);
            TcpClient Client2 = new TcpClient();
            Client2.Connect(new IPEndPoint(IPAddress.Parse("10.232.0.128"), 6114));
            Client2.GetStream().Write(new byte[] { 0x88 }, 0, 1);
            for (int i = 0; i < 100; i++)
            {
                D2DBS.packet.MakeTestPacket2(Client1.GetStream(), "gmddr", "wxsor-sire");
                D2DBS.packet.MakeTestPacket2(Client2.GetStream(), "binarie", "nec-binarie");
                D2DBS.packet.MakeTestPacket2_1(Client2.GetStream(), "wxsor-sire");
                D2DBS.packet.MakeTestPacket2_1(Client1.GetStream(), "nec-binarie");
                D2DBS.packet.MakeTestPacket4(Client1.GetStream(), "nec-binarie");
            }
        }

        public void TestCharFile()
        {
            CharFile_1("gmddr", "wxsor-sire");
            CharFile_1("binarie", "nec-binarie");
            CharFile_1("binarie", "nonnon");
            CharFile_1("binarie", "nonzlp");
            CharFile_1("binarie", "nonlad");
            CharFile_1("binarie", "stdhc");
            CharFile_1("binarie", "exphc");
        }

        private void CharFile_1(string AccountName, string CharName)
        {
            TCharInfo CharInfo = D2DBS.charfile.GetCharInfo(AccountName, CharName);
            ushort Status = (ushort)CharInfo.summary.charstatus;
            int expansion = D2DBS.charfile.GetExpansion(Status);
            int hardcore = D2DBS.charfile.GetHardcore(Status);
            int Ladder = D2DBS.charfile.GetLadder(Status);
            D2DBS.log.Write("debug", "Charinfo for `" + CharName + "`(*" + AccountName + "): name " + CharInfo.header.charname + " acc " + CharInfo.header.account + " level " + CharInfo.portrait.level.ToString() + " class " + CharInfo.portrait.char_class.ToString() + " explow " + CharInfo.summary.experience + " exp " + expansion.ToString() + ", hc " + hardcore.ToString() + ", lad " + Ladder.ToString());
        }

        public void TestCharLock()
        {
            //Step 1: Multi-Thread Lock Set
            const string DummyName = "TEST_1";
            const int ThreadNum = 64;
            Thread[] TestThread = new Thread[ThreadNum];
            Thread WatchDog = new Thread(new ParameterizedThreadStart(CharLock_Watch));
            for (int i = 0; i < ThreadNum; i++)
            {
                TestThread[i] = new Thread(new ParameterizedThreadStart(CharLock_1));
            }
            WatchDog.Start();
            for (int i = 0; i < ThreadNum; i++)
            {
                TestThread[i].Start();
            }
            //Step 2: Clear Lock for GS 0
            D2DBS.charlock.SetCharLock(DummyName, 1);
            Thread.Sleep(5000);
            D2DBS.charlock.UnlockAllCharByGSId(0);
            if (D2DBS.charlock.SetCharLock(DummyName, 1) == false)
            {
                D2DBS.charlock.UnlockChar(DummyName);
                int GSId = D2DBS.charlock.GetGSIdByLock(DummyName);
                if (GSId == -1)
                {
                    D2DBS.log.Write("debug", "CharLock Unit Test Passed!");
                }
                else
                {
                    D2DBS.log.Write("debug", "CharLock Unit Test Failed at brunch 2, check your code!");
                }
            }
            else
            {
                D2DBS.log.Write("debug", "CharLock Unit Test Failed at brunch 1, check your code!");
            }
        }

        public void CharLock_1(object param)
        {
            for (int i = 0; i < 1000; i++)
            {
                string name = GenName(10);
                D2DBS.charlock.SetCharLock(name, 0);
            }
        }

        public void CharLock_Watch(object param)
        {
            while (true)
            {
                D2DBS.log.Write("debug", "CharLock Benchmark: " + D2DBS.charlock.GetLockedCount().ToString());
                Thread.Sleep(500);
            }
        }

        private string GenName(int len)
        {
            string name = "";
            for (int i = 0; i < len; i++) name += (char)(65 + rndSeed.Next() % 26);
            return name;
        }

        public void TestLadder()
        {
            int expansion = rndSeed.Next() % 2;
            int hardcore = rndSeed.Next() % 2;
            int char_class = 0;
            string name = "";
            uint exp = (uint)rndSeed.Next();
            byte level = (byte)(rndSeed.Next()%100);
            short status = 0;
            if (expansion == 1)
            {
                status += 0x20;
                char_class = rndSeed.Next() % 7;
            }
            else
            {
                char_class = rndSeed.Next() % 5;
            }
            if (hardcore == 1)
            {
                status += 0x04;
            }

            D2DBS.log.Write("debug", "Test charname " + name + " level " + level.ToString() + " exp " + exp.ToString() + " class " + char_class.ToString() + " status " + status.ToString());
            D2DBS.ladder.UpdateLadder(exp, (ushort)status, level, (byte)char_class, name);
        }
    }
}
