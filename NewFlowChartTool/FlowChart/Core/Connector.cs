using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.Core
{
    [DebuggerDisplay("Conn[{Start}->{End}]")]
    public class Connector
    {
        public enum ConnectorType
        {
            FAILURE = 0,
            SUCCESS = 1,
            ALWAYS = 2,
            DELETE = 3
        }

        public Connector(Node start, Node end, ConnectorType type)
        {
            Start = start;
            End = end;
            _conntype = type; // don't raise event in ctor
        }


        public Node Start { get; set; }
        public Node End { get; set; }

        private ConnectorType _conntype;

        public Graph OwnerGraph => Start.OwnerGraph;

        public ConnectorType ConnType
        {
            get => _conntype;
            set
            {
                if (value == _conntype) return;
                var oldValue = _conntype;
                _conntype = value;
                OwnerGraph.RaiseConnectorTypeChangeEvent(this, oldValue);
                //ConnectorTypeChangeEvent?.Invoke(this,  oldValue, _conntype);
            }
        }
    }
}
