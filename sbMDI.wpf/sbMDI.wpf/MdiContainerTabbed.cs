using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;



namespace sbMDI.wpf
{
    /// <summary>
    /// Why not just bind TabControl.ItemsSource to Container.Children?
    /// The framework throws an exception, complaining about Visual element
    /// already having a parent (Canvas ClientArea). 
    /// 
    /// So to populate the tabs in sync with the child windows, we have to abstract it. 
    /// Either that, or there's some Jedi databinding trick I don't know about.
    /// </summary>
    internal class WindowTabItem
    {
        public MdiChild? Window { get; set; }
    }

    /// <summary>
    /// An MDI-like container with tabs corresponding to each child window
    /// </summary>
    public class MdiContainerTabbed : MdiContainerBase
    {
        private TabControl TabPanel = new();
        private ObservableCollection<WindowTabItem> TabItems = [];

        private UInt32 _MaxTabWidth = 32;
        public UInt32 MaxTabWidth
        {
            get => _MaxTabWidth;
            set
            {
                _MaxTabWidth = value;
                OnPropertyChanged(nameof(MaxTabWidth));
            }
        }

        private UInt32 _TabPanelHeight = 30;
        public UInt32 TabPanelHeight
        {
            get => _TabPanelHeight;
            set 
            { 
                _TabPanelHeight = value;
                OnPropertyChanged(nameof(TabPanelHeight));
            }
        }

        protected Brush _TabPanelBrush = SystemColors.MenuBarBrush;
        public Brush TabPanelBrush
        {
            get => _TabPanelBrush;
            set
            {
                _TabPanelBrush = value;
                OnPropertyChanged(nameof(TabPanelBrush));
            }
        }


        public MdiContainerTabbed() : base()
        {
            Binding b1 = new(nameof(TabPanelBrush));
            TabPanel.SetBinding(BackgroundProperty, b1);
            TabPanel.ItemsSource = TabItems;
            TabPanel.DisplayMemberPath = "Window.Title";
            TabPanel.SelectionChanged += TabPanel_SelectionChanged;
        }

        private void TabPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems is not null && e.AddedItems.Count > 0)
            {
                if (e.AddedItems[0] is WindowTabItem tab && tab.Window is not null)
                {
                    SetActiveWindow(tab.Window);
                }
            }
        }

        private void MdiContainerTabbed_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
        }

        /// <summary>
        /// Being more granular with the content grid layout allows the tab bar position to change
        /// at runtime without destroying the universe. Note that only the container controls are
        /// wiped out and rebuilt. The child window collection isn't touched.
        /// </summary>
        protected override void CreateContentGrid()
        {
            RecreateContentGrid();
        }
        
        private void RecreateContentGrid()
        {
            ContentGrid.Children.Clear();
            ContentGrid.RowDefinitions.Clear();
            ContentGrid.ColumnDefinitions.Clear();

            switch (TabPanel.TabStripPlacement)
            {
                case Dock.Top:
                    ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(TabPanelHeight, GridUnitType.Pixel) });
                    ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(MdiData.ButtonPanelHeight, GridUnitType.Pixel) });
                    ContentGrid.RowDefinitions.Add(new RowDefinition());

                    ContentGrid.Children.Add(TabPanel);
                    Grid.SetRow(TabPanel, 0);

                    ContentGrid.Children.Add(ButtonPanel);
                    Grid.SetRow(ButtonPanel, 1);

                    ContentGrid.Children.Add(ClientArea);
                    Grid.SetRow(ClientArea, 2);
                    break;

                case Dock.Bottom:
                    ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(MdiData.ButtonPanelHeight, GridUnitType.Pixel) });
                    ContentGrid.RowDefinitions.Add(new RowDefinition());
                    ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(TabPanelHeight, GridUnitType.Pixel) });

                    ContentGrid.Children.Add(TabPanel);
                    Grid.SetRow(TabPanel, 2);

                    ContentGrid.Children.Add(ButtonPanel);
                    Grid.SetRow(ButtonPanel, 0);

                    ContentGrid.Children.Add(ClientArea);
                    Grid.SetRow(ClientArea, 1);
                    break;
            }
        }

        protected override void OnResized(object sender, SizeChangedEventArgs e)
        {

        }

        protected override void OnChildAdded(MdiChild window)
        {
            TabItems.Add(new WindowTabItem() { Window = window });
        }

        protected override void OnChildRemoved(MdiChild window)
        {
            var tab = TabItems.Where(i => i.Window == window).FirstOrDefault();
            if(tab is not null)
            {
                TabItems.Remove(tab);
            }
        }

        protected override void OnChildActivated(MdiChild window)
        {
            var tab = TabItems.Where(i => i.Window == window).FirstOrDefault();
            if (tab is not null)
            {
                TabPanel.SelectedValue = tab;
            }
        }
    }
}
