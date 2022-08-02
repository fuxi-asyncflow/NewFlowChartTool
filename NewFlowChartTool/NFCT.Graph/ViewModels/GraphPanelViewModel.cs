﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Prism.Mvvm;
using FlowChart.Core;
using FlowChart.Layout;
using FlowChartCommon;
using NLog.Fluent;
using Prism.Commands;

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

        public FlowChart.Core.Graph Graph => _graph;

        public GraphPaneViewModel(FlowChart.Core.Graph graph)
        {
            _graph = graph;
            Nodes = new ObservableCollection<BaseNodeViewModel>();
            NodeDict = new Dictionary<Node, BaseNodeViewModel>();
            Connectors = new ObservableCollection<ConnectorViewModel>();
            ChangeLayoutCommand = new DelegateCommand(ChangeAutoLayout);
            Initialize();
        }

        public void AddNode(Node node)
        {
            var vm = BaseNodeViewModel.CreateNodeViewModel(node, this);
            Nodes.Add(vm);
            NodeDict.Add(node, vm);
        }

        public void Connect(Connector conn)
        {
            var start = conn.Start;
            var end = conn.End;
            BaseNodeViewModel? startVm, endVm;
            if (NodeDict.TryGetValue(start, out startVm) && NodeDict.TryGetValue(end, out endVm))
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
            IsFirstLayout = true;
            NeedLayout = true;
        }

        public string Name => _graph.Name;
        public string FullPath => _graph.Path;
        

        public string Description => _graph.Description;

        public ObservableCollection<BaseNodeViewModel> Nodes { get; set; }
        public Dictionary<Node, BaseNodeViewModel> NodeDict { get; set; }
        public ObservableCollection<ConnectorViewModel> Connectors { get; set; }
        public bool NeedLayout { get; set; }
        public bool IsFirstLayout { get; set; }
        private double _width;
        public double Width { get => _width; set => SetProperty(ref _width, value); }

        public double _height;
        public double Height { get => _height; set => SetProperty(ref _height, value); }

        public bool AutoLayout
        {
            get => _graph.AutoLayout;
        }

        public DelegateCommand ChangeLayoutCommand { get; set; }

        public void ChangeAutoLayout()
        {
            if (AutoLayout)
            {
                _graph.AutoLayout = false;
                RaisePropertyChanged(nameof(AutoLayout));
            }
            else
            {
                var result = MessageBox.Show("Switch to AutoLayout, all nodes position will change", "Be careful",
                    MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                    return;
                _graph.AutoLayout = true;
                NeedLayout = true;
                RaisePropertyChanged(nameof(AutoLayout));
            }
        }

        public bool Relayout()
        {
            Logger.DBG($"Relayout for graph {Name}");
            var graph = new GraphLayoutAdapter(this);
            if (NodeDict.Count <= 0.0 || NodeDict.First().Value.Width <= 0.0)
                return false;
            MsaglLayout layout = new MsaglLayout();
            try
            {
                layout.Layout(graph);
                if (Height < 0.001 || Width < 0.001)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[error] layout failed {e.Message}");
                return false;
            }

            return true;
        }

        #region SCROLL

        private double _scrollX;
        private double _scrollY;

        public double ScrollX
        {
            get => _scrollX;
            set => SetProperty(ref _scrollX, value);
        }

        public double ScrollY
        {
            get => _scrollY;
            set => SetProperty(ref _scrollY, value);
        }

        private double _scrollViewerWidth;
        private double _scrollViewerHeight;

        public double ScrollViewerWidth
        {
            get => _scrollViewerWidth;
            set => SetProperty(ref _scrollViewerWidth, value);
        }

        public double ScrollViewerHeight
        {
            get => _scrollViewerHeight;
            set => SetProperty(ref _scrollViewerHeight, value);
        }

        public void MoveNodeToCenter(BaseNodeViewModel? nodeVm)
        {
            if (nodeVm == null)
                return;
            // Logger.DBG($"MoveNodeToCenter {nodeVm.Left} {ScrollViewerWidth} {nodeVm}");
            ScrollX = nodeVm.Left + (nodeVm.Width - ScrollViewerWidth) / 2;
            ScrollY = nodeVm.Top - ScrollViewerHeight / 2;

        }
        #endregion

        #region FUNCTION

        private BaseNodeViewModel? SelectedNode { get; set; }
        public void SelectNode(BaseNodeViewModel nodeVm)
        {
            if (SelectedNode != null)
                SelectedNode.IsSelect = false;
            SelectedNode = nodeVm;
            nodeVm.IsSelect = true;
        }

        public void ClearSelectedItems(string items)
        {
            if (items == "all")
            {
                var node = SelectedNode;
                SelectedNode = null;
                if(node != null)
                    node.IsSelect = false;
            }
        }

        #endregion
    }
}
