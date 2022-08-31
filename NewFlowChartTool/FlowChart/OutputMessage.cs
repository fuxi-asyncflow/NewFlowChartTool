using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Core;

namespace FlowChart.Misc
{
    public enum OutputMessageType
    {
        Default = 0,
        Warning = 1,
        Error = 2,
    }
    public interface IOutputMessage
    {
        public void Output(string msg, OutputMessageType msgType =
                OutputMessageType.Default
            , Node? node = null, Graph? graph = null);
    }

    public static class OutputMessage
    {
        public static IOutputMessage? Inst { get; set; }
    }
}
