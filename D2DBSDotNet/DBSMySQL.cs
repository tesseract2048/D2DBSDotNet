using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace D2DBSDotNet
{
    class DBSMySQL
    {
        private MySqlConnection MyConn;
        private string ConnString;
        private Queue CommandQueue;
        private Thread ExecuteThread;
        private object QueueLock;
        private int ExecuteCount;

        public DBSMySQL()
        {
            ExecuteCount = 0;
            QueueLock = new object();
            CommandQueue = new Queue();
            ConnString = "SERVER=" + D2DBS.config["mysql_host"] + ";DATABASE=" + D2DBS.config["mysql_database"] + ";UID=" + D2DBS.config["mysql_user"] + ";PASSWORD=" + D2DBS.config["mysql_pass"];
            ConnectMySQL();
            ExecuteThread = new Thread(new ThreadStart(ExecuteLoop));
            ExecuteThread.Start();
        }

        private bool ConnectMySQL()
        {
            try
                {
                    MyConn = new MySqlConnection(ConnString);
                    MyConn.Open();
                }
                catch (Exception e)
                {
                    D2DBS.log.Write("fatal", "Cannot connect to MySQL Server: " + e.Message);
                    D2DBS.Cleanup();
                    return false;
                }
                D2DBS.log.Write("info", "Connection to MySQL Server established.");
            return true;
        }

        private bool CheckConnection()
        {
            if (MyConn.State != System.Data.ConnectionState.Open)
            {
                MyConn.Dispose();
                return ConnectMySQL();
            }
            return true;
        }

        private void ExecuteLoop()
        {
            while (true)
            {
                if (!CheckConnection()) return;
                while (CommandQueue.Count > 0)
                {
                    string sql;
                    lock (QueueLock)
                    {
                        sql = (string)CommandQueue.Dequeue();
                    }
                    DateTime St = DateTime.Now;
                    MySqlCommand SQLCmd = new MySqlCommand(sql, MyConn);
                    try
                    {
                        SQLCmd.ExecuteNonQuery();
                    }
                    catch(Exception e)
                    {
                        D2DBS.log.Write("error", "Failed to execute SQL statement: " + sql);
                        D2DBS.log.Write("error", "MySQL Error: " + e.Message);
                    }
                    finally
                    {
                        SQLCmd.Dispose();
                    }
                    TimeSpan Elapsed = DateTime.Now - St;
                    if (Elapsed.TotalMilliseconds > 100)
                    {
                        D2DBS.log.Write("debug", "Slow query: " + sql);
                    }
                    Interlocked.Increment(ref ExecuteCount);
                }
                Thread.Sleep(1000);
            }
        }

        public void Execute(string sql)
        {
            lock (QueueLock)
            {
                CommandQueue.Enqueue(sql);
            }
        }

        public string Escape(string str)
        {
            return MySql.Data.MySqlClient.MySqlHelper.EscapeString(str);
        }

        public int GetNumber(string sql)
        {
            MySqlCommand SQLCmd = new MySqlCommand(sql, MyConn);
            MySqlDataReader reader = SQLCmd.ExecuteReader();
            if (reader.HasRows == false) goto fail;
            reader.Read();
            if (reader.IsDBNull(0)) goto fail;
            int ret = reader.GetInt32(0);
            reader.Close();
            SQLCmd.Dispose();
            return ret;
fail:
            reader.Close();
            SQLCmd.Dispose();
            return -1;
        }
    }
}
