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
        private StackPanel MainPanel = 
            new() 
            { 
                Orientation = Orientation.Vertical, 
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

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

        private static GridLength _SingleTabPanelHeight = new(24, GridUnitType.Pixel);
        public GridLength SingleTabPanelHeight
        {
            get => _SingleTabPanelHeight;
            set 
            {
                _SingleTabPanelHeight = value;
                OnPropertyChanged(nameof(SingleTabPanelHeight));
            }
        }

        private Dock _TabStripPlacement = Dock.Bottom;
        public Dock TabStripPlacement
        {
            get => _TabStripPlacement;
            set
            {
                _TabStripPlacement = value;
                TabPanel.TabStripPlacement = value;
                OnPropertyChanged(nameof(TabStripPlacement));
            }
        }

        private RowDefinition TabRow = new() { Height = _SingleTabPanelHeight };


        public MdiContainerTabbed() : base()
        {
            TabPanel.Height = 26;
            TabPanel.ClipToBounds = false;
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
        protected override void CreateContainerGrid()
        {
            RecreateContentGrid();
        }

        private void RecreateContentGrid()
        {
            ContainerGrid.Children.Clear();
            ContainerGrid.RowDefinitions.Clear();
            ContainerGrid.ColumnDefinitions.Clear();

            switch (TabStripPlacement)
            {
                case Dock.Top:
                    ContainerGrid.RowDefinitions.Add(TabRow);
                    ContainerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(MdiData.ButtonPanelHeight, GridUnitType.Pixel) });
                    ContainerGrid.RowDefinitions.Add(new RowDefinition());

                    ContainerGrid.Children.Add(TabPanel);
                    Grid.SetRow(TabPanel, 0);

                    ContainerGrid.Children.Add(ButtonPanel);
                    Grid.SetRow(ButtonPanel, 1);

                    ContainerGrid.Children.Add(ClientArea);
                    Grid.SetRow(ClientArea, 2);
                    break;

                case Dock.Bottom:
                    ContainerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(MdiData.ButtonPanelHeight, GridUnitType.Pixel) });
                    ContainerGrid.RowDefinitions.Add(new RowDefinition());
                    ContainerGrid.RowDefinitions.Add(TabRow);

                    ContainerGrid.Children.Add(TabPanel);
                    Grid.SetRow(TabPanel, 2);

                    ContainerGrid.Children.Add(ButtonPanel);
                    Grid.SetRow(ButtonPanel, 0);

                    ContainerGrid.Children.Add(ClientArea);
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

        /// <summary>
        /// TabControl automatically resizes itself into new rows when the tab
        /// count increases. Here we're making sure that the grid row the control
        /// lives in dynamically resizes along with it.
        /// </summary>
        private void AdjustTabPanel()
        {
            
        }
    }
}
