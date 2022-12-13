using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.Common.Report
{
    public class ReporterEvent
    {
        public virtual string EventName { get; }
        public DateTime Time { get; private set; }
        public Dictionary<string, string>? Parameters { get; set; }
    }

    public class StartupEvent : ReporterEvent
    {
        public override string EventName => "StartUp";
    }

    public class OpenProjectEvent : ReporterEvent
    {
        public OpenProjectEvent(string projectName)
        {
            Parameters.Add("project_name", projectName);
        }

        public override string EventName => "OpenProject";
    }

    public class OpenGraphEvent : ReporterEvent
    {
        public OpenGraphEvent(string graphPath)
        {
            Parameters.Add("graph_name", graphPath);
        }
    }

    public class Reporter
    {

    }
}
