using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace NFCT.Common
{
    public static class FocusExtension
    {
        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached(
                "IsFocused", typeof(bool), typeof(FocusExtension),
                new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));

        private static void OnIsFocusedPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var uie = (UIElement)d;
            if ((bool)e.NewValue)
            {
                uie.Focus(); // Don't care about false values.
                Keyboard.Focus(uie);
            }
        }
    }

    // From http://blog.scottlogic.com/2010/07/21/exposing-and-binding-to-a-silverlight-scrollviewers-scrollbars.html
    public static class ScrollViewerBinding
    {
        #region VerticalOffset attached property

        /// <summary>
        /// Gets the vertical offset value
        /// </summary>
        public static double GetVerticalOffset(DependencyObject depObj)
        {
            return (double)depObj.GetValue(VerticalOffsetProperty);
        }

        /// <summary>
        /// Sets the vertical offset value
        /// </summary>
        public static void SetVerticalOffset(DependencyObject depObj, double value)
        {
            depObj.SetValue(VerticalOffsetProperty, value);
        }

        /// <summary>
        /// VerticalOffset attached property
        /// </summary>
        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached("VerticalOffset", typeof(double),
            typeof(ScrollViewerBinding), new PropertyMetadata(0.0, OnVerticalOffsetPropertyChanged));

        #endregion

        #region HorizontalOffset attached property

        /// <summary>
        /// Gets the horizontal offset value
        /// </summary>
        public static double GetHorizontalOffset(DependencyObject depObj)
        {
            return (double)depObj.GetValue(HorizontalOffsetProperty);
        }

        /// <summary>
        /// Sets the horizontal offset value
        /// </summary>
        public static void SetHorizontalOffset(DependencyObject depObj, double value)
        {
            depObj.SetValue(HorizontalOffsetProperty, value);
        }

        /// <summary>
        /// HorizontalOffset attached property
        /// </summary>
        public static readonly DependencyProperty HorizontalOffsetProperty =
            DependencyProperty.RegisterAttached("HorizontalOffset", typeof(double),
            typeof(ScrollViewerBinding), new PropertyMetadata(0.0, OnHorizontalOffsetPropertyChanged));

        #endregion

        #region VerticalScrollBar attached property

        /// <summary>
        /// An attached property which holds a reference to the vertical scrollbar which
        /// is extracted from the visual tree of a ScrollViewer
        /// </summary>
        private static readonly DependencyProperty VerticalScrollBarProperty =
            DependencyProperty.RegisterAttached("VerticalScrollBar", typeof(ScrollBar),
            typeof(ScrollViewerBinding), new PropertyMetadata(null));


        /// <summary>
        /// An attached property which holds a reference to the horizontal scrollbar which
        /// is extracted from the visual tree of a ScrollViewer
        /// </summary>
        private static readonly DependencyProperty HorizontalScrollBarProperty =
            DependencyProperty.RegisterAttached("HorizontalScrollBar", typeof(ScrollBar),
            typeof(ScrollViewerBinding), new PropertyMetadata(null));

        #endregion


        /// <summary>
        /// Invoked when the VerticalOffset attached property changes
        /// </summary>
        private static void OnVerticalOffsetPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var sv = d as ScrollViewer;
            if (sv != null)
            {
                // check whether we have a reference to the vertical scrollbar
                if (sv.GetValue(VerticalScrollBarProperty) == null)
                {
                    // if not, handle LayoutUpdated, which will be invoked after the
                    // template is applied and extract the scrollbar
                    sv.LayoutUpdated += (s, ev) =>
                    {
                        if (sv.GetValue(VerticalScrollBarProperty) == null)
                        {
                            GetScrollBarsForScrollViewer(sv);
                        }
                    };
                }
                else
                {
                    // update the scrollviewer offset
                    sv.ScrollToVerticalOffset((double)e.NewValue);
                }
            }
        }

        /// <summary>
        /// Invoked when the HorizontalOffset attached property changes
        /// </summary>
        private static void OnHorizontalOffsetPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var sv = d as ScrollViewer;
            if (sv != null)
            {
                // check whether we have a reference to the vertical scrollbar
                if (sv.GetValue(HorizontalScrollBarProperty) == null)
                {
                    // if not, handle LayoutUpdated, which will be invoked after the
                    // template is applied and extract the scrollbar
                    sv.LayoutUpdated += (s, ev) =>
                    {
                        if (sv.GetValue(HorizontalScrollBarProperty) == null)
                        {
                            GetScrollBarsForScrollViewer(sv);
                        }
                    };
                }
                else
                {
                    // update the scrollviewer offset
                    sv.ScrollToHorizontalOffset((double)e.NewValue);
                }
            }
        }

        /// <summary>
        /// Attempts to extract the scrollbars that are within the scrollviewers
        /// visual tree. When extracted, event handlers are added to their ValueChanged events.
        /// </summary>
        private static void GetScrollBarsForScrollViewer(ScrollViewer scrollViewer)
        {
            ScrollBar scroll = GetScrollBar(scrollViewer, Orientation.Vertical);
            if (scroll != null)
            {
                // save a reference to this scrollbar on the attached property
                scrollViewer.SetValue(VerticalScrollBarProperty, scroll);

                // scroll the scrollviewer
                scrollViewer.ScrollToVerticalOffset(ScrollViewerBinding.GetVerticalOffset(scrollViewer));

                // handle the changed event to update the exposed VerticalOffset
                scroll.ValueChanged += (s, e) =>
                {
                    SetVerticalOffset(scrollViewer, e.NewValue);
                };
            }

            scroll = GetScrollBar(scrollViewer, Orientation.Horizontal);
            if (scroll != null)
            {
                // save a reference to this scrollbar on the attached property
                scrollViewer.SetValue(HorizontalScrollBarProperty, scroll);

                // scroll the scrollviewer
                scrollViewer.ScrollToHorizontalOffset(ScrollViewerBinding.GetHorizontalOffset(scrollViewer));

                // handle the changed event to update the exposed HorizontalOffset
                scroll.ValueChanged += (s, e) =>
                {
                    scrollViewer.SetValue(HorizontalOffsetProperty, e.NewValue);
                };
            }
        }

        /// <summary>
        /// Searches the descendants of the given element, looking for a scrollbar
        /// with the given orientation.
        /// </summary>

        private static ScrollBar GetScrollBar(Visual fe, Orientation orientation)
        {
            // 广度优先搜索算法寻找 scrollviewer 的 scrollbar
            Queue<Visual> queue = new Queue<Visual>();
            queue.Enqueue(fe);
            while (queue.Count > 0)
            {
                Visual e = queue.Dequeue();
                var scrollBar = e as ScrollBar;
                if (scrollBar != null && scrollBar.Orientation == orientation)
                {
                    return scrollBar;
                }
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(e); i++)
                {
                    queue.Enqueue((Visual)VisualTreeHelper.GetChild(e, i));
                }
            }
            return null;
            // 原作者使用的是他自己开发的 LinqToVisualTree 工具来寻找子控件中的ScrollBar
            // 但该方式使用的是深度优先的方式，会取得错误的scrollbar
            //return fe
            //          .Descendants()
            //          .OfType<ScrollBar>()
            //          .SingleOrDefault(s => s.Orientation == orientation);
        }
    }

    public static class SizeObserver
    {
        public static readonly DependencyProperty ObserveProperty = DependencyProperty.RegisterAttached(
            "Observe",
            typeof(bool),
            typeof(SizeObserver),
            new FrameworkPropertyMetadata(OnObserveChanged));

        public static readonly DependencyProperty ObservedWidthProperty = DependencyProperty.RegisterAttached(
            "ObservedWidth",
            typeof(double),
            typeof(SizeObserver));

        public static readonly DependencyProperty ObservedHeightProperty = DependencyProperty.RegisterAttached(
            "ObservedHeight",
            typeof(double),
            typeof(SizeObserver));

        public static bool GetObserve(FrameworkElement frameworkElement)
        {
            return (bool)frameworkElement.GetValue(ObserveProperty);
        }

        public static void SetObserve(FrameworkElement frameworkElement, bool observe)
        {
            frameworkElement.SetValue(ObserveProperty, observe);
        }

        public static double GetObservedWidth(FrameworkElement frameworkElement)
        {
            return (double)frameworkElement.GetValue(ObservedWidthProperty);
        }

        public static void SetObservedWidth(FrameworkElement frameworkElement, double observedWidth)
        {
            frameworkElement.SetValue(ObservedWidthProperty, observedWidth);
        }

        public static double GetObservedHeight(FrameworkElement frameworkElement)
        {
            return (double)frameworkElement.GetValue(ObservedHeightProperty);
        }

        public static void SetObservedHeight(FrameworkElement frameworkElement, double observedHeight)
        {
            frameworkElement.SetValue(ObservedHeightProperty, observedHeight);
        }

        private static void OnObserveChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = (FrameworkElement)dependencyObject;

            if ((bool)e.NewValue)
            {
                frameworkElement.SizeChanged += OnFrameworkElementSizeChanged;
                UpdateObservedSizesForFrameworkElement(frameworkElement);
            }
            else
            {
                frameworkElement.SizeChanged -= OnFrameworkElementSizeChanged;
            }
        }

        private static void OnFrameworkElementSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateObservedSizesForFrameworkElement((FrameworkElement)sender);
        }

        private static void UpdateObservedSizesForFrameworkElement(FrameworkElement frameworkElement)
        {
            // WPF 4.0 onwards
            frameworkElement.SetCurrentValue(ObservedWidthProperty, frameworkElement.ActualWidth);
            frameworkElement.SetCurrentValue(ObservedHeightProperty, frameworkElement.ActualHeight);
        }
    }
}
