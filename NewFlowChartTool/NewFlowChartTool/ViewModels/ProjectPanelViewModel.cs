using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using FlowChart.Core;

namespace NewFlowChartTool.ViewModels
{
    internal class ProjectTreeItemViewModel : BindableBase
    {
        public ProjectTreeItemViewModel(Item item)
        {
            _item = item;
        }
        readonly Item _item;
        public string Name { get => _item.Name; }
        public string Description { get => _item.Description; }
    }
    internal class ProjectPanelViewModel : BindableBase
    {
    }
}
