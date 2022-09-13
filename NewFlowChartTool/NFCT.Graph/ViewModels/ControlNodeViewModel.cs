using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FlowChart.Core;
using FlowChartCommon;

namespace NFCT.Graph.ViewModels
{
    internal class ControlNodeViewModel : BaseNodeViewModel
    {
        static ControlNodeViewModel()
        {
            ControlFuncNames = new HashSet<string>()
            {
                "stopnode", "stopflow", "gotonode"
            };
            
        }
        public ControlNodeViewModel(Node node, GraphPaneViewModel g) : base(node, g)
        {
            ParamNodes = new ObservableCollection<BaseNodeViewModel>();
        }

        public static HashSet<string> ControlFuncNames;

        public string? ControlFuncName { get; set; }
        public ObservableCollection<BaseNodeViewModel> ParamNodes { get; set; }

        public static bool MaybeControlNodeViewModel(string text)
        {
            var left = text.IndexOf('(');
            var right = text.IndexOf(')');
            if (left == -1 || right == -1 || left > right)
            {
                return false;
            }

            var startText = text.Substring(0, left).Trim();
            if (!ControlFuncNames.Contains(startText))
            {
                return false;
            }

            return true;
        }

        public bool ParseText(string? text)
        {
            text ??= ((TextNode)(Node)).Text;
            var left = text.IndexOf('(');
            var right = text.IndexOf(')');
            if (left == -1 || right == -1 || left > right)
            {
                //TODO change node type to TextNode
                return false;
            }

            var startText = text.Substring(0, left).Trim();
            if (!ControlFuncNames.Contains(startText))
            {
                //TODO change node type to TextNode
                return false;
            }

            ControlFuncName = startText;
            var paramStr = text.Substring(left + 1, right - left - 1).Trim();
            var paramList = paramStr.Split(',').ToList().ConvertAll(s => s.Trim());
            ParamNodes.Clear();
            paramList.ForEach(idStr =>
            {
                int id;
                if (Int32.TryParse(idStr, out id))
                {
                    if (id < Owner.Nodes.Count)
                        ParamNodes.Add(Owner.Nodes[id]);
                    else
                        Logger.WARN($"cannot find node with id `{id}`");

                }
                else
                {
                    if (idStr.StartsWith('"'))
                        idStr = idStr.Trim('"');
                    var node = Owner.Graph.GetNode(idStr);
                    if (node == null)
                        Logger.WARN($"cannot find node with uid `{idStr}`");
                    else
                    {
                        ParamNodes.Add(Owner.NodeDict[node]);
                    }
                }
            });
            return true;
        }
    }
}
