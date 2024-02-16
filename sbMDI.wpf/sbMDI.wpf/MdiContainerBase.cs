using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace sbMDI.wpf
{

    /// <summary>
    /// The common base of the MdiContainer types.
    /// The window-frame buttons are not drawn on the child windows.
    /// Instead, they're part of the container window.
    /// </summary>
    public abstract class MdiContainerBase : UserControl, INotifyPropertyChanged
    {
        ///////////////////////////////////////////////////////////
        #region INotifyPropertyChanged
        /////////////////////////////

        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetField<TField>(ref TField field, TField value, string propertyName)
        {
            if (EqualityComparer<TField>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            OnPropertyChanged(propertyName);
        }

        /////////////////////////////
        #endregion INotifyPropertyChanged
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Events / Actions
        /////////////////////////////

        public event Action<MdiChild, MdiChild>? ActiveChildChangedEvent;
        
        
        /////////////////////////////
        #endregion Events / Actions
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        protected ObservableCollection<MdiChild> Children { get; } = [];

        private Grid _ContainerGrid = new();
        protected Grid ContainerGrid { get => _ContainerGrid; set => _ContainerGrid = value; }

        private Canvas _ClientArea = new();
        protected Canvas ClientArea { get => _ClientArea; set => _ClientArea = value; }

        public double ClientAreaWidth { get => ClientArea.ActualWidth; }

        public double ClientAreaHeight { get => ClientArea.ActualHeight; }

        public Size ClientAreaSize { get => new(ClientAreaWidth, ClientAreaHeight); }
        
        public static readonly DependencyProperty ActiveMdiChildProperty =
            DependencyProperty.Register("ActiveMdiChild", typeof(MdiChild), typeof(MdiContainerBase),
            new UIPropertyMetadata(null, new PropertyChangedCallback(ActiveMdiChildValueChanged)));
        public MdiChild? ActiveMdiChild
        {
            get { return (MdiChild)GetValue(ActiveMdiChildProperty); }
            internal set { SetValue(ActiveMdiChildProperty, value); }
        }
        private static void ActiveMdiChildValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MdiContainerBase container = (MdiContainerBase)sender;
            MdiChild newChild = (MdiChild)e.NewValue;
            MdiChild oldChild = (MdiChild)e.OldValue;

            if (newChild == null || newChild == oldChild)
            {
                return;
            }

            if (container.ActiveChildChangedEvent is not null)
            {
                container.ActiveChildChangedEvent(newChild, oldChild);
            }

            if (oldChild != null && oldChild.WindowState == WindowState.Maximized)
            {
                newChild.WindowState = WindowState.Maximized;
            }

            int maxZindex = 0;
            for (int i = 0; i < container.Children.Count; i++)
            {
                int zindex = Panel.GetZIndex(container.Children[i]);
                if (zindex > maxZindex)
                {
                    maxZindex = zindex;
                }
                if (container.Children[i] != newChild)
                {
                    container.Children[i].Focused = false;
                }
                else
                {
                    newChild.Focused = true;
                }
            }

            Panel.SetZIndex(newChild, maxZindex + 1);

            container.MdiChildTitleChanged?.Invoke(container, new RoutedEventArgs());
        }

        public event RoutedEventHandler MdiChildTitleChanged;

        protected double WindowOffset = 10.0;

        protected StackPanel ButtonsPanel = new();
        protected Button ButtonClose = new();
        protected Button ButtonMinimize = new();
        protected Button ButtonMaximize = new();

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////




        ///////////////////////////////////////////////////////////
        #region Construction & Init
        /////////////////////////////
        
        static MdiContainerBase()
        {
            Application.Current.Resources.MergedDictionaries.Add(
                new ResourceDictionary
                {
                    Source = new Uri(@"/sbMDI.wpf;component/ThemeDefault.xaml", UriKind.Relative)
                });
        }

        public MdiContainerBase()
        {
            Children.CollectionChanged += Children_CollectionChanged;
            SizeChanged += MdiContainerBase_SizeChanged;
            Loaded += MdiContainerBase_Loaded;
            GotFocus += MdiContainerBase_GotFocus;
            MdiChildTitleChanged += MdiContainerBase_MdiChildTitleChanged;

            CreateContainerGrid();
            Content = ContainerGrid;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ContainerGrid = (Grid)Template.FindName("ContainerGrid", this);
            ClientArea = (Canvas)Template.FindName("ClientArea", this);
            ButtonsPanel = (StackPanel)Template.FindName("ButtonsPanel", this);
            ButtonMinimize = (Button)Template.FindName("MinimizeButton", this);
            ButtonMaximize = (Button)Template.FindName("MaximizeButton", this);
            ButtonClose = (Button)Template.FindName("CloseButton", this);

            ButtonMinimize.Click += ButtonMinimize_Click;
            ButtonMaximize.Click += ButtonMaximize_Click;
            ButtonClose.Click += ButtonClose_Click;

            // Initial button panel state
            ButtonsPanel.Visibility = Visibility.Collapsed;
        }

        /////////////////////////////
        #endregion Construction & Init
        ///////////////////////////////////////////////////////////




        ///////////////////////////////////////////////////////////
        #region Window Interface
        /////////////////////////////

        public MdiChild NewChildWindow()
        {
            var window = new MdiChild(this);
            Children.Add(window);
            return window;
        }

        public MdiChild NewChildWindow(string title)
        {
            var window = new MdiChild(this) { Title = title };
            Children.Add(window);
            return window;
        }

        public MdiChild NewChildWindow(UserControl content)
        {
            var window = new MdiChild(this) { Content = content };
            Children.Add(window);
            return window;
        }

        public MdiChild NewChildWindow(UserControl content, string title)
        {
            var window = new MdiChild(this) { Content = content, Title = title };
            Children.Add(window);
            return window;
        }

        public void CloseChildWindow(MdiChild window)
        {
            Children.Remove(window);
        }

        public void CloseAllChildWindows()
        {
            foreach(var window in Children)
            {
                Children.Remove(window);
            }
        }

        public void SetActiveWindow(MdiChild window)
        {
            ActivateWindow(window);
        }

        public MdiChild? FindWindow(string title)
        {
            return Children.Where(i => i.Title == title).FirstOrDefault();
        }

        public List<MdiChild>? FindWindows(string title)
        {
            return Children.Where(i => i.Title == title).ToList();
        }

        public MdiChild? FindWindow(Type T)
        {
            return Children.Where(i => i.Content.GetType() == T).FirstOrDefault();
        }

        public List<MdiChild>? FindWindows(Type T)
        {
            return Children.Where(i => i.Content.GetType() == T).ToList();
        }

        public MdiChild? FindWindow(object DataContext)
        {
            return Children.Where(i => i.Content.DataContext == DataContext).FirstOrDefault();
        }

        public List<MdiChild>? FindWindows(object DataContext)
        {
            return Children.Where(i => i.Content.DataContext == DataContext).ToList();
        }

        public MdiChild? FindWindow(Type T, object DataContext)
        {
            return Children.Where(i => i.Content.GetType() == T && i.Content.DataContext == DataContext).FirstOrDefault();
        }

        public List<MdiChild>? FindWindows(Type T, object DataContext)
        {
            return Children.Where(i => i.Content.GetType() == T && i.Content.DataContext == DataContext).ToList();
        }

        /////////////////////////////6
        #endregion Window Interface
        ///////////////////////////////////////////////////////////
        



        ///////////////////////////////////////////////////////////
        #region Abstracts
        /////////////////////////////

        /// <summary>
        /// Each container has a unique layout for the same core components
        /// (primarily the ClientArea and ButtonBar when the children are maximized).
        /// The layout for each type is handled in the override.
        /// </summary>
        protected abstract void CreateContainerGrid();


        /// <summary>
        /// Container types may choose to respond to these events.
        /// For example, Tabbed MDI container creates and destroys
        /// its tabs in response to Added and Removed.
        /// </summary>
        protected abstract void OnResized(object sender, SizeChangedEventArgs e);
        protected abstract void OnChildAdded(MdiChild window);
        protected abstract void OnChildRemoved(MdiChild window);
        protected abstract void OnChildActivated(MdiChild window);

        /////////////////////////////
        #endregion Abstracts
        ///////////////////////////////////////////////////////////
        



        ///////////////////////////////////////////////////////////
        #region Events
        /////////////////////////////

        private void MdiContainerBase_MdiChildTitleChanged(object sender, RoutedEventArgs e)
        {
            
        }

        private void MdiContainerBase_GotFocus(object sender, RoutedEventArgs e)
        {
            ActiveMdiChild?.Focus();
        }

        private void MdiContainerBase_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateButtonsStatus();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveMdiChild is not null)
            {
                //Debug.WriteLine("Closing " + ActiveMdiChild.Title);
                Children.Remove(ActiveMdiChild);
                ChooseNewActiveWindow();
            }
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            foreach (var window in Children)
            {
                window.WindowState = WindowState.Normal;
            }
            ActiveMdiChild?.Focus();
        }

        private void ButtonMaximize_Click(object sender, RoutedEventArgs e)
        {
            foreach (var window in Children)
            {
                window.WindowState = WindowState.Maximized;
            }
            ActiveMdiChild?.Focus();
        }

        private void MdiContainerBase_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (var wnd in Children)
            {
                wnd.ApplyWindowState();
            }
            OnResized(sender, e);
        }

        private void Children_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems is null)
                {
                    return;
                }
                if (e.NewItems[0] is not MdiChild window)
                {
                    return;
                }

                //Debug.WriteLine("adding " + window.Title);
                if(window.Container is null)
                {
                    window.Container = this;
                }
                window.WindowStateChanged += Child_WindowStateChanged;
                Panel.SetZIndex(window, Children.Count);
                OnChildAdded(window);
                ClientArea.Children.Add(window);
                ActivateWindow(window);
                UpdateButtonsStatus();
            }

            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems is null)
                {
                    return;
                }
                if (e.OldItems[0] is not MdiChild window)
                {
                    return;
                }

                //Debug.WriteLine("Removing " + window.Title);
                if (Children.Count == 0)
                {
                    ActiveMdiChild = null;
                }
                else
                {
                    ReassignZIndex();
                    ChooseNewActiveWindow();
                }
                OnChildRemoved(window);
                ClientArea.Children.Remove(window);
                UpdateButtonsStatus();
            }
        }

        private void Child_WindowStateChanged(object sender, WindowStateEventArgs e)
        {
            if(e.State == WindowState.Maximized)
            {
                ButtonsPanel.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                foreach (var child in Children)
                {
                    if(child.WindowState == WindowState.Maximized)
                    {
                        ButtonsPanel.Visibility = Visibility.Visible;
                        return;
                    }
                }
            }

            // If we got here, there are zero maxed MdiChild windows
            ButtonsPanel.Visibility = Visibility.Collapsed;
        }

        /////////////////////////////
        #endregion Events
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Window Internal
        /////////////////////////////

        protected MdiChild? FindTopmostWindow()
        {
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
            return topWnd;
        }

        private void ChooseNewActiveWindow()
        {
            if (Children.Count == 0)
            {
                ActiveMdiChild = null;
                return;
            }

            var window = FindTopmostWindow();
            if(window is not null)
            {
                //Debug.WriteLine("ChooseNewActiveWindow selects " + window.Title);
                ActivateWindow(window);
            }
        }

        // Avoids unnecessary code duplication
        private void ActivateWindow(MdiChild window)
        {
            //Debug.WriteLine("Activating " + window.Title);
            ActiveMdiChild = window;
            ReassignZIndex();
            ActiveMdiChild.BringIntoView();
            ActiveMdiChild.Focus();
            OnChildActivated(window);
        }

        private void ReassignZIndex()
        {
            //for (int i = 0; i < Children.Count; i++)
            //{
            //    var child = Children[i];
            //    if (child != null)
            //    {
            //        Panel.SetZIndex(child, i);
            //    }
            //}
            //if (ActiveMdiChild is not null)
            //{
            //    Panel.SetZIndex(ActiveMdiChild, Children.Count + 1);
            //}
        }

        protected void UpdateButtonsStatus()
        {
            bool enable = Children.Count > 0;
            foreach (Control child in ButtonsPanel.Children)
            {
                child.IsEnabled = enable;
            }

            if(Children.Count == 0)
            {
                ButtonsPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                foreach (var window in Children)
                {
                    if (window.WindowState == WindowState.Maximized)
                    {
                        ButtonsPanel.Visibility = Visibility.Visible;
                        return;
                    }
                }
            }
        }

        /////////////////////////////
        #endregion Window Internal
        ///////////////////////////////////////////////////////////
        
    }
}

