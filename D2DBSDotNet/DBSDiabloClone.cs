using System;
using System.Collections.Generic;
using System.Text;

namespace D2DBSDotNet
{
    [System.Runtime.Remoting.Contexts.Synchronization]
    class DBSDiabloClone : System.ContextBoundObject
    {
        private int SOJCount;
        private int SOJCountTrigger;
        private int param_x;
        private int param_alpha;
        private int param_beta;
        private Random rndSeed = new Random((int)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds + 91);

        public DBSDiabloClone()
        {
            if (D2DBS.config["dc_control"] == "1")
            {
                param_x = int.Parse(D2DBS.config["dc_param_x"]);
                param_alpha = int.Parse(D2DBS.config["dc_param_alpha"]);
                param_beta = int.Parse(D2DBS.config["dc_param_beta"]);
                SOJCountTrigger = D2DBS.mysql.GetNumber("SELECT MAX(`next_counter`) AS c FROM dc_trigger");
                if (SOJCountTrigger <= 0)
                {
                    SOJCountTrigger = GenerateSeed();
                    D2DBS.mysql.Execute("INSERT INTO dc_trigger (`date`, `counter`, `next_counter`) VALUES (UNIX_TIMESTAMP(), '0', '" + SOJCountTrigger.ToString() + "')");
                }
                SOJCount = D2DBS.mysql.GetNumber("SELECT SUM(`count`) AS c FROM diablo_clone");
                D2DBS.log.Write("info", "Diablo Clone feature loaded, counter is " + SOJCount.ToString() + ", next seed is " + SOJCountTrigger.ToString() + ".");
            }
        }

        public int Increment(int step, string realm)
        {
            if (D2DBS.config["dc_control"] != "1") return 0;
            if (step <= 0) return SOJCount;
            D2DBS.mysql.Execute("INSERT LOW_PRIORITY INTO diablo_clone (`date`, `realm`, `count`) VALUES (UNIX_TIMESTAMP(), '" + realm + "', '" + step.ToString() + "') ON DUPLICATE KEY UPDATE `count` = `count` + '" + step.ToString() + "'");
            SOJCount += step;
            RunTrigger();
            return SOJCount;
        }

        public int Get()
        {
            if (D2DBS.config["dc_control"] != "1") return 0;
            return SOJCount;
        }

        private void RunTrigger()
        {
            if (D2DBS.config["dc_control"] != "1") return;
            if (SOJCount >= SOJCountTrigger)
            {
                SOJCountTrigger += GenerateSeed();
                D2DBS.mysql.Execute("INSERT INTO dc_trigger (`date`, `counter`, `next_counter`) VALUES (UNIX_TIMESTAMP(), '" + SOJCount + "', '" + SOJCountTrigger.ToString() + "')");
                D2DBS.packet.TriggerDC();
            }
        }

        public int GenerateSeed()
        {
            return param_x + (int)((double)param_beta * rndSeed.NextDouble()) + (int)(Math.Sqrt((double)(param_beta * param_beta) * rndSeed.NextDouble()));
        }

    }
}
