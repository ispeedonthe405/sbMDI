using System.Collections.ObjectModel;
using System.Diagnostics;
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
        private RowDefinition TabRow = new();
        private double SingleTabRowHeight = 24;
        private double NumberOfTabRows = 1;
        private ObservableCollection<WindowTabItem> TabItems = [];

        private double _MaxTabWidth = 130;
        public double MaxTabWidth
        {
            get => _MaxTabWidth;
            //set => SetField(ref _MaxTabWidth, value, nameof(MaxTabWidth));
        }
        


        public MdiContainerTabbed() : base()
        {
            //ContainerGrid.SizeChanged += (s, e) => { RecalculateTabRow(); };
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            TabPanel = (TabControl)Template.FindName("TabPanel", this);
            TabPanel.ClipToBounds = true;
            TabPanel.ItemsSource = TabItems;
            TabPanel.SelectionChanged += TabPanel_SelectionChanged;
            ContainerGrid.SizeChanged += (s, e) => RecalculateTabRow();
            TabItems.CollectionChanged += TabItems_CollectionChanged;
            TabRow = (RowDefinition)Template.FindName("TabRow", this);
        }

        /// <summary>
        /// As tab count changes, or as the window changes size, recalculate the height of the grid row
        /// to accomodate the tabs without clipping.
        /// </summary>
        private void RecalculateTabRow()
        {
            // Note: Tabs are assumed to be max width, because obtaining the actual tab
            // header height requires one to jump through flaming hoops
            double totalTabWidth = TabPanel.Items.Count * MaxTabWidth;
            double panelWidth = ContainerGrid.ActualWidth;
            double adjustedPanelSize = (NumberOfTabRows * panelWidth) - (NumberOfTabRows * MaxTabWidth);

            // New row needed; add row height
            if (totalTabWidth > adjustedPanelSize)
            {
                //while (totalTabWidth > adjustedPanelSize)
                {
                    NumberOfTabRows++;
                    adjustedPanelSize = (NumberOfTabRows * panelWidth) - (NumberOfTabRows * MaxTabWidth);
                    double newHeight = TabRow.Height.Value + SingleTabRowHeight;
                    TabRow.Height = new(newHeight, GridUnitType.Pixel);
                }
            }

            // extra row exists; subtract one row height
            else if(totalTabWidth < NumberOfTabRows * panelWidth)
            {
                if (NumberOfTabRows > 1 && ((NumberOfTabRows * panelWidth) / totalTabWidth) > 2)
                {
                    NumberOfTabRows--;

                    //Debug.WriteLine(string.Format("Subtracting a row: Row count = {0}", NumberOfTabRows));
                    
                    double newHeight = TabRow.Height.Value - SingleTabRowHeight;
                    TabRow.Height = new(newHeight, GridUnitType.Pixel);
                }
            }
        }

        private void TabItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Fix all new tabs to max tab width
            if (e.NewItems is not null && e.NewItems[0] is TabItem newTab)
            {
                newTab.MaxWidth = MaxTabWidth;
            }

            RecalculateTabRow();

            //// As the tab count changes, make sure the grid row that contains the tabs
            //// is the right height to accomodate them.
            ////
            //// Note: TabItem is not the actual tab header but the tab page content.
            //// To get the actual tab width one must jump through flaming hoops;
            //// therefore we assume all tab headers are at max width.
            //double totalTabWidth = TabPanel.Items.Count * MaxTabWidth;

            //// Tabs width exceeds cotainer width * rows; add a new row
            //if(totalTabWidth >= (ContainerGrid.ActualWidth * NumberOfTabRows))
            //{
            //    NumberOfTabRows++;
            //    double currentHeight = TabRow.Height.Value;
            //    currentHeight += SingleTabRowHeight;
            //    TabRow.Height = new(currentHeight, GridUnitType.Pixel);
            //    return;
            //}

            //// Total rows height exceeds the space needed by at least one whole row; subtract a row
            //double rowsNeeded = totalTabWidth / ContainerGrid.ActualWidth;
            //double d = Math.Round(rowsNeeded, MidpointRounding.ToPositiveInfinity);
            //if (d < 1) d = 1;
            //double newHeight = SingleTabRowHeight * d;
            //TabRow.Height = new(newHeight, GridUnitType.Pixel);
            //NumberOfTabRows = d;

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
            //ContainerGrid.Children.Clear();
            //ContainerGrid.RowDefinitions.Clear();
            //ContainerGrid.ColumnDefinitions.Clear();

            //switch (TabStripPlacement)
            //{
            //    case Dock.Top:
            //        ContainerGrid.RowDefinitions.Add(TabRow);
            //        ContainerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(MdiData.ButtonPanelHeight, GridUnitType.Pixel) });
            //        ContainerGrid.RowDefinitions.Add(new RowDefinition());

            //        ContainerGrid.Children.Add(TabPanel);
            //        Grid.SetRow(TabPanel, 0);

            //        ContainerGrid.Children.Add(ButtonsPanel);
            //        Grid.SetRow(ButtonsPanel, 1);

            //        ContainerGrid.Children.Add(ClientArea);
            //        Grid.SetRow(ClientArea, 2);
            //        break;

            //    case Dock.Bottom:
            //        ContainerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(MdiData.ButtonPanelHeight, GridUnitType.Pixel) });
            //        ContainerGrid.RowDefinitions.Add(new RowDefinition());
            //        ContainerGrid.RowDefinitions.Add(TabRow);

            //        ContainerGrid.Children.Add(TabPanel);
            //        Grid.SetRow(TabPanel, 2);

            //        ContainerGrid.Children.Add(ButtonsPanel);
            //        Grid.SetRow(ButtonsPanel, 0);

            //        ContainerGrid.Children.Add(ClientArea);
            //        Grid.SetRow(ClientArea, 1);
            //        break;
            //}
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
