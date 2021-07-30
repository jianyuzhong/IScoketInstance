using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CY_SysLog
{
    public  class StaticObject
    {
        public static string  output_path { get; set; }
        public static string sokect_ip { get; set; }
        public static long sokect_ip_long { get; set; }
        public static string header { get; set; }
        public static List<string> payload  { get; set; }
        public static List<string> base64header { get; set; }

        public static int sokect_port { get; set; }
        public static long count { get; set; } = 0;


    }
}
