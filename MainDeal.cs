using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSCommonUtilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CY_SysLog
{
    public class MainDeal
    {
        ISocket socket_service = null;
        string tempfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");
        public MainDeal()
        {
            StaticObject.output_path = ConfigurationManager.AppSettings["output_path"];
            StaticObject.sokect_ip = ConfigurationManager.AppSettings["ip"];
            StaticObject.header = ConfigurationManager.AppSettings["header"];
            if (!Directory.Exists(StaticObject.output_path)) Directory.CreateDirectory(StaticObject.output_path);
            StaticObject.payload = ConfigurationManager.AppSettings["payload"].Split(';').ToList();
            StaticObject.base64header = ConfigurationManager.AppSettings["base64header"].Split(';').ToList();


            if (!Directory.Exists(tempfile)) Directory.CreateDirectory(tempfile);
            StaticObject.sokect_ip = ConfigurationManager.AppSettings["ip"];

            IPAddress iPAddress = IPAddress.Parse(StaticObject.sokect_ip);
            StaticObject.sokect_ip_long = iPAddress.Address;
            StaticObject.sokect_port =int.Parse(ConfigurationManager.AppSettings["port"]);
            socket_service = new ISocket(StaticObject.sokect_ip_long, StaticObject.sokect_port);

        }
        public void start()
        {
            socket_service.Start();
            while (true)
            {

                //socket_service._writable = false;
                List<string> data = socket_service.Output();
                if (data!=null)
                {
                    OutPutData(data);
                }
                else
                {
                    Log.Write(ELogLevel.Info, "waitting for get data");
                }
                //socket_service._writable = true;
                Thread.Sleep(1000);
            }
        }
        public void OutPutData(List<string> data)
        {
            if (data == null || data.Count() == 0) return;
            foreach (var item in data)
            {
                string singledata = item.Split(new string[] { " - - - " }, StringSplitOptions.RemoveEmptyEntries).ToList().LastOrDefault();

                OutPutFile(singledata);
            }
        }
        public void OutPutFile(string sdata)
        {
            try
            {
                //sdata = File.ReadAllText(@"C:\Users\钟建宇\Desktop\cy_syslog\Log\new 6.txt");
                Log.Write(ELogLevel.Info, $"{sdata}");
                JObject data = (JObject)JsonConvert.DeserializeObject(sdata);
                List<DataModel> payload = new List<DataModel>();
                Dictionary<string, string> commlod = new Dictionary<string, string>();
                foreach (JProperty item in data.Properties())
                {
                    try
                    {
                        string name = item.Name;
                        string val = item.Value.ToString();

                        if (StaticObject.payload.Contains(name))
                        {
                            val = $"=?utf-8?b?{Convert.ToBase64String(Encoding.UTF8.GetBytes(val))}";
                            DataModel data1 = new DataModel()
                            {
                                datatype = name,
                                dataval = val
                            };
                            payload.Add(data1);
                        }
                        else
                        {
                            if (StaticObject.base64header.Contains(name))
                            {
                                val = $"=?utf-8?b?{Convert.ToBase64String(Encoding.UTF8.GetBytes(val))}";
                            }
                            if (name == "ts")
                            {
                                name = "time";
                                val = val.Split('.')[0];
                                val = GetTime(long.Parse(val)).ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            commlod.Add(name, val.Replace("\n", "\\n").Replace("\r", "\\r"));
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                StringBuilder sb = new StringBuilder();
                if (payload.Count==0)
                {
                    foreach (KeyValuePair<string, string> item1 in commlod)
                    {
                        sb.Append($"{item1.Key}:{item1.Value}\r\n");
                    }
                    StaticObject.count++;
                }
                else
                {
                    foreach (DataModel item in payload)
                    {
                        sb.Append($"objtype:{item.datatype}\r\n");
                        sb.Append($"objval:{item.dataval}\r\n");
                        foreach (KeyValuePair<string, string> item1 in commlod)
                        {
                            sb.Append($"{item1.Key}:{item1.Value}\r\n");
                        }
                        sb.Append("\r\n");
                        StaticObject.count++;
                    }
                }
                string tfilename = $"{Guid.NewGuid().ToString()}.cy_syslog";
                File.WriteAllText(Path.Combine(tempfile, tfilename), sb.ToString(),Encoding.UTF8);
                File.Move(Path.Combine(tempfile, tfilename), Path.Combine(StaticObject.output_path, tfilename));
                Log.Write(ELogLevel.Info, $"\r\ntime:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} count:{StaticObject.count++}");
            }
            catch (Exception ex)
            {
                Log.Write(ELogLevel.Info, $"Handdle str \r\n{sdata }\r\n  with essage{ex.ToString()}");
            }
        }
        private DateTime GetTime(long timeStamp)
        {
            while (timeStamp < 10000000000000000)
            {
                timeStamp = timeStamp * 10;
            }
            TimeSpan toNow = new TimeSpan(timeStamp);
            return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).Add(toNow);
        }
    }
}
