using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Core;
using FlowChart.Debug;

namespace NFCT.Common.Services
{
    public interface IDebugService
    {
        void SetBreakPoint(Node node, bool isBreakPoint);
        void ContinueBreakPoint(GraphInfo graphInfo);
    }
}
