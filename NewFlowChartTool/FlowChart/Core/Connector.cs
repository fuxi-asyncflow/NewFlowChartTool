﻿using System;
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

        private ConnectorType _conntype;

        public ConnectorType ConnType
        {
            get => _conntype;
            set
            {
                if (value == _conntype) return;
                var oldValue = _conntype;
                _conntype = value;
                ConnectorTypeChangeEvent?.Invoke(this,  oldValue, _conntype);
            }
        }

        public delegate void
            ConnectorTypeChangeDelegate(Connector conn, ConnectorType oldValue, ConnectorType newValue);
        public event ConnectorTypeChangeDelegate? ConnectorTypeChangeEvent;

        public delegate void ConnectorDelegate(Connector conn);
        public event ConnectorDelegate ConnectorRemoveEvent;

        public void OnDestroy()
        {
            ConnectorRemoveEvent?.Invoke(this);
        }
    }
}
