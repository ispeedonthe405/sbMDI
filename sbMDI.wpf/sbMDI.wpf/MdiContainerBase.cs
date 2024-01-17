using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace sbMDI.wpf
{

    /// <summary>
    /// The common base of the MdiContainer types
    /// </summary>
    public abstract class MdiContainerBase : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged is not null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private ObservableCollection<MdiChild> _Children = [];
        protected ObservableCollection<MdiChild> Children { get => _Children; }

        private Grid _ContentGrid = new();
        protected Grid ContentGrid { get => _ContentGrid; }

        private DockPanel _ButtonPanel = new();
        protected DockPanel ButtonPanel { get => _ButtonPanel; }

        private Canvas _ClientArea = new();
        protected Canvas ClientArea { get => _ClientArea; }

        protected Brush _WindowBrush = SystemColors.WindowBrush;
        public Brush WindowBrush
        {
            get => _WindowBrush;
            set { _WindowBrush = value; OnPropertyChanged(nameof(WindowBrush)); }
        }

        protected Brush _ClientAreaBrush = SystemColors.AppWorkspaceBrush;
        public Brush ClientAreaBrush
        {
            get => _ClientAreaBrush;
            set { _ClientAreaBrush = value; OnPropertyChanged(nameof(ClientAreaBrush)); }
        }

        protected Brush _ButtonPanelBrush = SystemColors.MenuBarBrush;
        public Brush ButtonPanelBrush
        {
            get => _ButtonPanelBrush;
            set { _ButtonPanelBrush = value; OnPropertyChanged(nameof(ButtonPanelBrush)); }        
        }

        protected MdiChild? ActiveWindow { get; set; } = null;

        protected double WindowOffset = 10.0;

        protected Button ButtonClose = new();
        protected Button ButtonMinimize = new();
        protected Button ButtonMaximize = new();



        public MdiContainerBase()
        {
            Binding b1 = new(nameof(WindowBrush));
            SetBinding(BackgroundProperty, b1);
            Binding b2 = new(nameof(ClientAreaBrush));
            ClientArea.SetBinding(BackgroundProperty, b2);
            Binding b3 = new(nameof(ButtonPanelBrush));
            ButtonPanel.SetBinding(BackgroundProperty, b3);

            Children.CollectionChanged += Children_CollectionChanged;
            SizeChanged += MdiContainerBase_SizeChanged;
            Loaded += MdiContainerBase_Loaded;
            GotFocus += MdiContainerBase_GotFocus;

            CreateContentGrid();
            CreateButtonPanel();
            Content = ContentGrid;
        }

        public MdiChild NewChildWindow()
        {
            MdiChild window = new(this);
            Children.Add(window);
            ActivateWindow(window);
            return window;
        }

        public void SetActiveWindow(MdiChild window)
        {
            //Debug.WriteLine("SetActiveWindow making " + window.Title + " active");
            ActivateWindow(window);
        }

        private void ChooseNewActiveWindow()
        {
            if (Children.Count == 0)
            {
                ActiveWindow = null;
                return;
            }

            int topZ = -999;
            MdiChild? topWnd = null;
            foreach (UIElement child in ClientArea.Children)
            {
                int z = Panel.GetZIndex(child);
                if (z > topZ)
                {
                    topZ = z;
                    topWnd = child as MdiChild;
                }
            }
            if (topWnd is not null)
            {
                Debug.WriteLine("ChooseNewActiveWindow selects " + topWnd.Title);
                ActivateWindow(topWnd);
            }
        }

        // Avoids unnecessary code duplication
        private void ActivateWindow(MdiChild window)
        {
            Debug.WriteLine("Activating " + window.Title);
            ActiveWindow = window;
            Panel.SetZIndex(ActiveWindow, Children.Count + 1);
            ActiveWindow.BringIntoView();
            ActiveWindow.Focus();
        }

        private void MdiContainerBase_GotFocus(object sender, RoutedEventArgs e)
        {
            ActiveWindow?.Focus();
        }

        private void MdiContainerBase_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateButtonsStatus();
        }


        protected void CreateButtonPanel()
        {
            ButtonPanel.LastChildFill = false;
            ButtonPanel.Children.Clear();

            // These ugly buttons are temporary. I'll theme them later.
            ButtonClose.Content = " X ";
            ButtonClose.Click += ButtonClose_Click;
            ButtonClose.Margin = new Thickness(3, 1, 3, 1);
            DockPanel.SetDock(ButtonClose, Dock.Right);
            ButtonPanel.Children.Add(ButtonClose);

            ButtonMaximize.Content = " [] ";
            ButtonMaximize.Click += ButtonMaximize_Click;
            ButtonMaximize.Margin = new Thickness(3, 1, 3, 1);
            DockPanel.SetDock(ButtonMaximize, Dock.Right);
            ButtonPanel.Children.Add(ButtonMaximize);

            ButtonMinimize.Content = " - ";
            ButtonMinimize.Click += ButtonMinimize_Click;
            ButtonMinimize.Margin = new Thickness(3, 1, 3, 1);
            DockPanel.SetDock(ButtonMinimize, Dock.Right);
            ButtonPanel.Children.Add(ButtonMinimize);
        }

        private void ReassignZIndex()
        {
            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                if (child != null)
                {
                    Panel.SetZIndex(child, i);
                }
            }
            if (ActiveWindow is not null)
            {
                ActivateWindow(ActiveWindow);
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveWindow is not null)
            {
                Debug.WriteLine("Closing " + ActiveWindow.Title);
                ClientArea.Children.Remove(ActiveWindow);
                Children.Remove(ActiveWindow);
            }
            ChooseNewActiveWindow();
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            foreach (var window in Children)
            {
                window.WindowState = WindowState.Normal;
            }
            ActiveWindow?.Focus();
        }

        private void ButtonMaximize_Click(object sender, RoutedEventArgs e)
        {
            foreach (var window in Children)
            {
                window.WindowState = WindowState.Maximized;
            }
            ActiveWindow?.Focus();
        }

        protected void UpdateButtonsStatus()
        {
            bool enable = Children.Count > 0;
            foreach (Control child in ButtonPanel.Children)
            {
                child.IsEnabled = enable;
            }
        }



        /// <summary>
        /// Container types may choose to respond to these events.
        /// For example, Tabbed MDI container creates and destroys
        /// its tabs in response to Added and Removed.
        /// </summary>
        protected abstract void CreateContentGrid();
        protected abstract void OnResized(object sender, SizeChangedEventArgs e);
        protected abstract void OnChildAdded(MdiChild window);
        protected abstract void OnChildRemoved(MdiChild window);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MdiContainerBase_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (var wnd in Children)
            {
                wnd.Resize();
            }
            OnResized(sender, e);
        }

        /// <summary>
        /// Override if a given container class needs to do more when the Children collection changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Children_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is null)
            {
                return;
            }
            if (e.NewItems[0] is not MdiChild window)
            {
                return;
            }

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                Debug.WriteLine("adding " + window.Title);
                Panel.SetZIndex(window, Children.Count);
                ActivateWindow(window);
                ClientArea.Children.Add(window);
                UpdateButtonsStatus();
                OnChildAdded(window);
            }

            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                Debug.WriteLine("Removing " + window.Title);
                if (Children.Count == 0)
                {
                    ActiveWindow = null;
                }
                else
                {
                    ReassignZIndex();
                    ChooseNewActiveWindow();
                }
                ClientArea.Children.Remove(window);
                UpdateButtonsStatus();
                OnChildRemoved(window);
            }
        }
    }
}

