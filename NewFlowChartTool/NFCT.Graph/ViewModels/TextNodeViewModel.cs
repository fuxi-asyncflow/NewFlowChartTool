﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using FlowChart.AST;
using Prism.Mvvm;
using FlowChart.Core;
using FlowChartCommon;
using Prism.Regions;


namespace NFCT.Graph.ViewModels
{
    public class TextTokenViewModel : BindableBase
    {
        public TextTokenViewModel(string text)
        {
            Text = text;
        }
        public string Text { get; set; }
        public TextToken.TokenType Type { get; set; }
        public string? TipText { get; set; }
        public Brush Color => CanvasNodeResource.NodeTokenBrushes[(int)Type];

        public void OnThemeSwitch()
        {
            RaisePropertyChanged(nameof(Color));
        }
    }

    public class TextNodeViewModel : BaseNodeViewModel
    {
#if DEBUG
        public TextNodeViewModel()
        {
            var graph = new FlowChart.Core.Graph("design");
            graph.AddNode(new StartNode());
            Node = new TextNode() { Text = "hello" };
            Owner = new GraphPaneViewModel(graph);
            BgType = NodeBgType.CONDITION;
            Tokens = new ObservableCollection<TextTokenViewModel>();
            Tokens.Add(new TextTokenViewModel("$a") { Type = TextToken.TokenType.Variable });
            Tokens.Add(new TextTokenViewModel(" = ") { Type = TextToken.TokenType.Default });
            Tokens.Add(new TextTokenViewModel("Func") { Type = TextToken.TokenType.Member });
            Tokens.Add(new TextTokenViewModel("(") { Type = TextToken.TokenType.Default });
            Tokens.Add(new TextTokenViewModel("123") { Type = TextToken.TokenType.Number });
            Tokens.Add(new TextTokenViewModel(", ") { Type = TextToken.TokenType.Default });
            Tokens.Add(new TextTokenViewModel("\"hello\"") { Type = TextToken.TokenType.String });
            Tokens.Add(new TextTokenViewModel(")") { Type = TextToken.TokenType.Default });
        }
#endif
        public new TextNode Node { get; set; }
        public string Text { get => Node.Text; }
        public ObservableCollection<TextTokenViewModel> Tokens { get; set; }
        public TextNodeViewModel(TextNode node, GraphPaneViewModel g) :base(node, g)
        {
            Node = node;
            Tokens = new ObservableCollection<TextTokenViewModel>();
        }

        public override void ExitEditingMode(NodeAutoCompleteViewModel acVm, bool save)
        {
            // avoid func is called twice: first called by Key.Esc, then called by LostFocus
            if (IsEditing == false)
                return;
            Logger.DBG($"[{nameof(TextNodeViewModel)}] ExitEditingMode");
            if (save)
            {
                Node.Text = acVm.Text;
                Logger.DBG($"node text is change to {Node.Text}");
                RaisePropertyChanged(nameof(Text));
                Owner.NeedLayout = true;
                Owner.Build();
            }

            IsEditing = false;
        }

        public override string ToString()
        {
            return $"[{nameof(TextNodeViewModel)}] {Text}";
        }

        public override void OnThemeSwitch()
        {
            RaisePropertyChanged(nameof(BgType));
            //TODO some graph theme take effect after close and open again
            foreach (var textTokenViewModel in Tokens)
            {
                textTokenViewModel.OnThemeSwitch();
            }
        }

        public override void OnParseEnd(ParseResult pr)
        {
            Logger.DBG($"[OnParseEnd] Tokens count : {pr.Tokens}");
            if (pr.Tokens != null)
            {
                Tokens.Clear();
                pr.Tokens.ForEach(token =>
                {
                    Tokens.Add(new TextTokenViewModel(Text.Substring(token.Start, token.End - token.Start))
                    {
                        Type = token.Type
                    });
                });
            }

            ErrorMessage = pr.IsError ? pr.ErrorMessage : null;
            Owner.NeedLayout = true;
        }
    }

    public class StartNodeViewModel : BaseNodeViewModel
    {
        public new StartNode Node { get; set; }
        public StartNodeViewModel(StartNode node, GraphPaneViewModel g) : base(node, g)
        {
            Node = node;
        }
    }

}
