using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.Common
{
    public class CrashDump
    {
        private static string GetInternalIP()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }

        public static string GetExceptionString(Exception? e, bool br = true)
        {
            string msg = "Exception is Null";
            if (e == null)
                return msg;

            msg = "Message: \n" + e.Message + "\n" + "Stack: \n" + e.StackTrace + "\nTarget: \n" + e.TargetSite + "\n";
            if (e.InnerException != null)
            {
                var ie = e.InnerException;
                string innermsg = "\nInnerException Message: \n" + ie.Message + "\n" + "Stack: \n" + ie.StackTrace +
                                  "\nTarget: \n" +
                                  ie.TargetSite;
                msg += "\n----\n";
                msg += innermsg;
            }
            if (br) 
                msg = msg.Replace("\n", "<br/>\r\n");
            return msg;
        }
    }
}
