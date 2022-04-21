using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using FlowChart.Core;
using FlowChart.Layout;

namespace NFCT.Graph.ViewModels
{
    public class GraphLayoutAdapter : IGraph
    {
        GraphPaneViewModel _g;
        public GraphLayoutAdapter(GraphPaneViewModel g) { _g = g; }
      
        public IEnumerable<INode> Nodes => _g.Nodes;

        public IEnumerable<IEdge> Edges => _g.Connectors;

        public double Width { set => _g.Width = value; }
        public double Height { set => _g.Height = value; }
    }
    public class GraphPaneViewModel : BindableBase
    {        

        private FlowChart.Core.Graph _graph;
        public FlowChart.Core.Graph Graph { get => _graph; }
        public GraphPaneViewModel(FlowChart.Core.Graph graph)
        {
            _graph = graph;
            Nodes = new ObservableCollection<TextNodeViewModel>();
            NodeDict = new Dictionary<TextNode, TextNodeViewModel>();
            Connectors = new ObservableCollection<ConnectorViewModel>();
            Initialize();
        }

        public void AddNode(Node node)
        {
            var tNode = node as TextNode;
            if (tNode == null) return;
            var vm = new TextNodeViewModel(tNode);
            Nodes.Add(vm);
            NodeDict.Add(tNode, vm);
        }

        public void Connect(Connector conn)
        {
            var start = conn.Start;
            var end = conn.End;
            TextNodeViewModel startVm, endVm;
            if (NodeDict.TryGetValue(start as TextNode, out startVm) &&  NodeDict.TryGetValue(end as TextNode, out endVm))
            {
                Connectors.Add(new ConnectorViewModel(conn, startVm, endVm));
            }           
          
        }

        public void Initialize()
        {
            Nodes.Clear();
            NodeDict.Clear();
            _graph.Nodes.ForEach(node => AddNode(node));
            _graph.Connectors.ForEach(Connect);

            for(int i=0;i<Nodes.Count;i++)
            {
                var node = Nodes[i];
                node.CX = 100;
                node.CY = 40*i;
                node.Width = i * 40;
                node.Height = 30;                
            }
            NeedLayout = true;
        }

        public string Name { get => _graph.Name; }

        public string Description { get => _graph.Description;}

        public ObservableCollection<TextNodeViewModel> Nodes { get; set; }
        public Dictionary<TextNode, TextNodeViewModel> NodeDict { get; set; }
        public ObservableCollection<ConnectorViewModel> Connectors { get; set; }
        public bool NeedLayout { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public void Relayout()
        {
            var graph = new GraphLayoutAdapter(this);
            MsaglLayout layout = new MsaglLayout();
            layout.Layout(graph);
        }
    }
}
