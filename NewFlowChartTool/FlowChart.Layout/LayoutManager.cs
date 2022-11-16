using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Layout.MyLayout;

namespace FlowChart.Layout
{
    public class LayoutManager
    {
        static LayoutManager()
        {
            Instance = new LayoutManager();
        }
        public LayoutManager()
        {
            LayoutDict = new Dictionary<string, Func<ILayout>>();
            LayoutDict.Add("msagl", () => new MsaglLayout());
            LayoutDict.Add("min_depth", () => new MyLayout3());
            LayoutDict.Add("max_depth", () => new MyLayout2());
            LayoutDict.Add("layout_group", () => new MyLayoutGroup());
        }

        public static LayoutManager Instance { get; private set; }

        public Dictionary<string, Func<ILayout>> LayoutDict { get; set; }


    }
}
