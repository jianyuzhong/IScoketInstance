
using NSCommonUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CY_SysLog
{
    class Program
    {
//        smtp
//oracle
//mysql
//conn
//ftp
//mssql
//dns
//openvpn
//dhcp
//nntp
//smb
//http
//l2tp
//IEC61850-SV
//IEC61850-MMS
//pptp
//IEC60870-5-104
//ldap
//telnet
//pop3
//ssl
//ssh
//IEC61850-GOOSE
//icmp
//imap
//postgresql
//rdp
//rlogin
//tacacs
//cvs
//krb
        static void Main(string[] args)
        {
            Log.Write(ELogLevel.Info, "SystemStart");
            AppDomain.CurrentDomain.UnhandledException +=new UnhandledExceptionEventHandler (CurrentDomain_UnhandledException);
            MainDeal deal = new MainDeal();
            deal.start();
            //deal.OutPutFile("");
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Write(ELogLevel.Fatal, $"{e.ToString()}");
        }
    }
}
