using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Prism.Mvvm;

namespace NFCT.Graph.ViewModels
{
    public class PromptItemViewModel : BindableBase
    {
        #region Static

        static PromptItemViewModel()
        {
            InactivatedBackGroundBrush = Application.Current.FindResource("HightlightBackGroundBrush") as SolidColorBrush;
            DefaultBackGroundBrush = Application.Current.FindResource("BackGroundBrush") as SolidColorBrush;
            SelectedBackGroundBrush = Application.Current.FindResource("NodeActionBackGround") as SolidColorBrush; 
        }

        private static Brush SelectedBackGroundBrush;
        private static Brush InactivatedBackGroundBrush;
        private static Brush DefaultBackGroundBrush;

        #endregion
        public bool IsFunction { get; }

        private string _text;
        public string Text
        {
            get => IsFunction ? $"{_text}()" : _text;
            set => _text = value;
        }

        public Brush BackGround => IsSelected ? 
            (NodeAutoCompleteViewModel.IsActivate ? SelectedBackGroundBrush : InactivatedBackGroundBrush) 
            : DefaultBackGroundBrush;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { SetProperty(ref _isSelected, value); RaisePropertyChanged(nameof(BackGround));}
        }
    }
    public class NodeAutoCompleteViewModel : BindableBase
    {
        public NodeAutoCompleteViewModel()
        {
            Prompts = new ObservableCollection<PromptItemViewModel>();

            Prompts.Add(new PromptItemViewModel() {Text = "foo"});
            Prompts.Add(new PromptItemViewModel() { Text = "bar" });
            Prompts.Add(new PromptItemViewModel() { Text = "abc" });
            Prompts.Add(new PromptItemViewModel() { Text = "123" });
        }

        public static bool IsActivate { get; set; }
        public TextNodeViewModel Node { get; set; }
        private string _text;
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value, nameof(Text));
        }

        public ObservableCollection<PromptItemViewModel> Prompts { get; set; }




    }
}
