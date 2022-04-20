using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using FlowChart.Core;

namespace NFCT.Graph.ViewModels
{
    public class GraphPaneViewModel : BindableBase
    {        

        private FlowChart.Core.Graph _graph;
        public FlowChart.Core.Graph Graph { get => _graph; }
        public GraphPaneViewModel(FlowChart.Core.Graph graph)
        {
            _graph = graph;
            Nodes = new ObservableCollection<TextNodeViewModel>();
            Initialize();
        }

        public void Initialize()
        {
            Nodes.Clear();
            _graph.Nodes.ForEach(node => Nodes.Add(new TextNodeViewModel(node)));

            for(int i=0;i<Nodes.Count;i++)
            {
                var node = Nodes[i];
                node.CX = 100;
                node.CY = 40*i;
                node.Width = i * 40;
                node.Height = 30;                
            }
        }

        public string Name { get => _graph.Name; }

        public string Description { get => _graph.Description;}

        public ObservableCollection<TextNodeViewModel> Nodes { get; set; }


    }
}
