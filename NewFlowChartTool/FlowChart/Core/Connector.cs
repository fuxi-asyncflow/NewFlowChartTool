using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.Core
{
    public class Connector
    {
        public enum ConnectorType
        {
            FAILURE = 0,
            SUCCESS = 1,
            ALWAYS = 2,
            DELETE = 3
        }
        public Node Start { get; set; }
        public Node End { get; set; }
        public ConnectorType ConnType { get; set; }
    }
}
