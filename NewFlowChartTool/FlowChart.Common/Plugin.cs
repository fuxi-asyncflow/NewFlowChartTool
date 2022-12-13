using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.Common
{
    public interface IPlugin
    {
        bool Register(IPluginManager manager);
    }

    public class IPluginManager
    {
        
    }
}
