using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Runtime.InteropServices;

namespace D2DBSDotNet
{
    class DBSSoap
    {
        [DllImport("LibD2ItemTool.dll")]
        public static extern int xhg_readcsv(IntPtr filename);
        [DllImport("LibD2ItemTool.dll")]
        public static extern int xhg_xchg(IntPtr filename);
        [DllImport("LibD2ItemTool.dll")]
        public static extern int item_appendfromfile(IntPtr charsave, IntPtr d2i, int count);
        [DllImport("LibD2ItemTool.dll")]
        public static extern int item_readcsv(IntPtr filename, int n_normcode, int n_ubercode, int n_ultracode, int n_inv_width, int n_inv_height, int n_stackable, int catalog);
        [DllImport("LibD2ItemTool.dll")]
        public static extern int item_jsonlist(IntPtr charsave, IntPtr buffer);
        [DllImport("LibD2ItemTool.dll")]
        public static extern int item_savelistfromjson(IntPtr charsave, IntPtr json);
        [DllImport("LibD2ItemTool.dll")]
        public static extern int item_appendfromjson(IntPtr charsave, IntPtr json);
        [DllImport("LibD2ItemTool.dll")]
        public static extern int item_stripbyindex(IntPtr charsave, int index, int socket_index);
        [DllImport("LibD2ItemTool.dll")]
        public static extern int item_stripmulbyindex(IntPtr charsave, IntPtr index, IntPtr socket_index, int count);
        [DllImport("kernel32")]
        public static extern bool AllocConsole();

        private HttpListener listener;
        private List<string> soap_allowed_client;

        private void sendContent(HttpListenerContext context, string content)
        {
            byte[] buf = System.Text.Encoding.UTF8.GetBytes(content);
            context.Response.OutputStream.Write(buf, 0, buf.Length);
            context.Response.OutputStream.Close();
            context.Response.OutputStream.Dispose();
        }

        static int seqno = 0;

        private void makeDump(string charname, string filename, string operation)
        {
            if (!System.IO.Directory.Exists(D2DBS.config["soap_dump_dir"] + @"\" + charname))
                System.IO.Directory.CreateDirectory(D2DBS.config["soap_dump_dir"] + @"\" + charname);
            System.IO.File.Copy(filename, D2DBS.config["soap_dump_dir"] + @"\" + charname + @"\" + operation + "_" + (DateTime.Now - new DateTime(1970, 1, 1, 8, 0, 0)).TotalSeconds.ToString() + "_" + (seqno++).ToString());
        }

        private void soapWorker()
        {
            IntPtr ptr1;
            IntPtr ptr2;
            string charname;
            string charfile;

            listener.Start();
            while(true)
            {
                HttpListenerContext request = listener.GetContext();
                try
                {
                    if (soap_allowed_client.Contains(request.Request.RemoteEndPoint.Address.ToString()) == false)
                    {
                        sendContent(request, "NOT_ALLOWED");
                        goto request_fin;
                    }
                    if (request.Request.Url.Query == null || request.Request.Url.Query.Length < 1)
                    {
                        sendContent(request, "ERROR");
                        goto request_fin;
                    }
                    D2DBS.log.Write("debug", "soap: " + request.Request.Url.ToString() + " seqno: " + seqno.ToString());
                    string[] query_params = request.Request.Url.Query.Substring(1).Split('&');
                    if (query_params == null || query_params.Length < 1) sendContent(request, "ERROR");
                    charname = query_params[0];
                    charfile = Environment.CurrentDirectory + @"\" + D2DBS.config["charsavedir"].Replace('/', '\\') + @"\" + query_params[0];
                    ptr1 = Marshal.StringToHGlobalAnsi(charfile);
                    switch (request.Request.Url.AbsolutePath)
                    {
                        case "/api/acquire-lock":
                            if (D2DBS.charlock.QueryCharLock(charname))
                            {
                                sendContent(request, "FAILED");
                            }
                            else
                            {
                                D2DBS.charlock.SetCharLock(charname, -1);
                                sendContent(request, "OK");
                            }
                            break;
                        case "/api/release-lock":
                            if (D2DBS.charlock.QueryCharLock(charname))
                            {
                                D2DBS.charlock.UnlockChar(charname);
                                sendContent(request, "OK");
                            }
                            else
                            {
                                sendContent(request, "FAILED");
                            }
                            break;
                        case "/api/query-lock":
                            if (D2DBS.charlock.QueryCharLock(charname))
                            {
                                sendContent(request, "LOCKED");
                            }
                            else
                            {
                                sendContent(request, "FREE");
                            }
                            break;
                        case "/api/strip-item":
                            makeDump(charname, charfile, "strip-item");
                            if (D2DBS.charlock.QueryCharLock(charname))
                            {
                                int itemid = int.Parse(query_params[1]);
                                int socket_index = itemid;
                                if (query_params.Length > 2) socket_index = int.Parse(query_params[2]);
                                if (item_stripbyindex(ptr1, itemid, socket_index) != 0)
                                {
                                    sendContent(request, "FAILED-2");
                                }
                                else
                                {
                                    sendContent(request, "OK");
                                }
                            }
                            else
                            {
                                sendContent(request, "FAILED");
                            }
                            break;
                        case "/api/strip-mul-item":
                            makeDump(charname, charfile, "strip-mul-item");
                            if (D2DBS.charlock.QueryCharLock(charname))
                            {
                                string[] itemid = query_params[1].Split(',');
                                string[] socket_indexs = query_params[2].Split(',');
                                ptr2 = Marshal.AllocHGlobal(itemid.Length * 4);
                                IntPtr ptr3 = Marshal.AllocHGlobal(socket_indexs.Length * 4);
                                int count = 0;
                                for (int i = 0; i < itemid.Length; i++)
                                {
                                    int id;
                                    if (Int32.TryParse(itemid[i], out id))
                                    {
                                        int si = id;
                                        Int32.TryParse(socket_indexs[i], out si);
                                        Marshal.WriteInt32(new IntPtr(ptr3.ToInt32() + (count) * 4), si);
                                        Marshal.WriteInt32(new IntPtr(ptr2.ToInt32() + (count++) * 4), id);
                                    }
                                }
                                if (item_stripmulbyindex(ptr1, ptr2, ptr3, count) != 0)
                                {
                                    sendContent(request, "FAILED-2");
                                }
                                else
                                {
                                    sendContent(request, "OK");
                                }
                                Marshal.FreeHGlobal(ptr2);
                                Marshal.FreeHGlobal(ptr3);
                            }
                            else
                            {
                                sendContent(request, "FAILED");
                            }
                            break;
                        case "/api/xchg-item":
                            sendContent(request, xhg_xchg(ptr1).ToString());
                            break;
                        case "/api/query-item":
                            IntPtr buf = Marshal.AllocHGlobal(524288);
                            int len = item_jsonlist(ptr1, buf);
                            if (len > -1)
                            {
                                sendContent(request, Marshal.PtrToStringAnsi(buf));
                            }
                            else
                            {
                                sendContent(request, "{}");
                            }
                            Marshal.FreeHGlobal(buf);
                            break;
                        case "/api/add-item":
                            if (query_params.Length < 3)
                            {
                                sendContent(request, "ERROR");
                                break;
                            }
                            if (!System.IO.File.Exists("d2i\\" + query_params[1] + ".d2i"))
                            {
                                sendContent(request, "ERROR");
                                break;
                            }
                            makeDump(charname, charfile, "add-item");
                            ptr2 = Marshal.StringToHGlobalAnsi("d2i\\" + query_params[1] + ".d2i");
                            if (item_appendfromfile(ptr1, ptr2, Int32.Parse(query_params[2])) == 0)
                            {
                                sendContent(request, "OK");
                            }
                            else
                            {
                                sendContent(request, "FAILED");
                            }
                            Marshal.FreeHGlobal(ptr2);
                            break;
                        case "/api/add-json-item":
                            if (request.Request.HttpMethod != "POST")
                            {
                                sendContent(request, "CORRPTED");
                            }
                            System.IO.StreamReader reader = new System.IO.StreamReader(request.Request.InputStream);
                            string json = reader.ReadToEnd();
                            reader.Close();
                            reader.Dispose();
                            ptr2 = Marshal.StringToHGlobalAnsi(json);
                            makeDump(charname, charfile, "add-json-item");
                            if (item_appendfromjson(ptr1, ptr2) == 0)
                            {
                                sendContent(request, "OK");
                            }
                            else
                            {
                                sendContent(request, "FAILED");
                            }
                            Marshal.FreeHGlobal(ptr2);
                            break;
                        case "/api/spawn_dc":
                            D2DBS.packet.TriggerDC();
                            sendContent(request, "OK");
                            break;
                    }
                }
                catch (Exception e)
                {
                    D2DBS.log.Write("fatal", "soap exception: " + e.ToString());
                }
            request_fin:
                request.Response.Close();
            }
        }

        public DBSSoap()
        {
            xhg_readcsv(Marshal.StringToHGlobalAnsi("item_exchange.csv"));
            item_readcsv(Marshal.StringToHGlobalAnsi("armor.txt"), 23, 24, 25, 28, 29, 45, 0);
            item_readcsv(Marshal.StringToHGlobalAnsi("weapons.txt"), 34, 35, 36, 41, 42, 43, 1);
            item_readcsv(Marshal.StringToHGlobalAnsi("misc.txt"), 13, -1, -1, 17, 18, 43, 2);
            listener = new HttpListener();
            soap_allowed_client = new List<string>();
            if (D2DBS.config["soap_client"] != null)
            {
                soap_allowed_client.AddRange(D2DBS.config["soap_client"].Split(','));
            }
            listener.Prefixes.Add("http://+:" + ((D2DBS.config["soap_port"] == null) ? "53979" : D2DBS.config["soap_port"]) + "/");
            System.Threading.Thread worker = new System.Threading.Thread(new System.Threading.ThreadStart(soapWorker), 0x800000);
            worker.Start();
        }
    }
}
