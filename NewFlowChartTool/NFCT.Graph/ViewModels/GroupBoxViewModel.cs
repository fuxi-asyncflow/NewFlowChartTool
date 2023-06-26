using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Core;
using FlowChart.Layout;
using Prism.Mvvm;

namespace NFCT.Graph.ViewModels
{
    public class GroupBoxViewModel : BindableBase, IGroup
    {
        public GroupBoxViewModel(Group group, GraphPaneViewModel graph)
        {
            Nodes = new List<BaseNodeViewModel>();
            Group = group;
            Owner = graph;
        }

        public Group Group { get; set; }
        public GraphPaneViewModel Owner { get; set; }
        public List<BaseNodeViewModel> Nodes { get; set; }
        public List<INode> InsideNodes
        {
            get => Nodes.ConvertAll(node => (INode)node);
        }

        public double Top { get; set; }
        public double Left { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        private const double margin = 20.0;

        public void Resize()
        {
            double left = double.MaxValue;
            double top = double.MaxValue;
            double right = double.MinValue;
            double bottom = double.MinValue;

            Nodes.ForEach(node =>
            {
                if (left > node.Left) left = node.Left;
                if (top > node.Top) top = node.Top;
                var r = node.Left + node.Width;
                if(right < r) right = r;
                var b = node.Top + node.Height;
                if(bottom < b) bottom = b;
            });

            Top = top - margin;
            Left = left - margin;
            Width = right - Left + margin;
            Height = bottom - Top + margin;
            RaisePropertyChanged(nameof(Top));
            RaisePropertyChanged(nameof(Left));
            RaisePropertyChanged(nameof(Width));
            RaisePropertyChanged(nameof(Height));

        }

        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public void RemoveNode(BaseNodeViewModel nodeVm)
        {
            Nodes.Remove(nodeVm);
        }

        public void AddNode(BaseNodeViewModel nodeVm)
        {
            Nodes.Add(nodeVm);
        }

    }
}
