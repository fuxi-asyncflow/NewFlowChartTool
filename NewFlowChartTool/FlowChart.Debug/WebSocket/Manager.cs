using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Net;
using FlowChart.Common;
using Logger = FlowChart.Common.Logger;

namespace FlowChart.Debug.WebSocket
{
    public class Manager : DebugManager<Client>
    {
        public Manager() : base()
        {
            
        }

    }
}
