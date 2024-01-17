using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace sbMDI.wpf
{
    public class MdiContainerTabbed : MdiContainerBase
    {
        private DockPanel _TabPanel = new();
        public DockPanel TabPanel { get => _TabPanel; }

        private ObservableCollection<Button> _Tabs = [];
        public ObservableCollection<Button> Tabs { get => _Tabs; }

        public enum eTabBarPosition
        {
            Top,
            Bottom,
            Left,
            Right
        }
        private eTabBarPosition _TabBarPosition = eTabBarPosition.Bottom;
        public eTabBarPosition TabBarPosition
        {
            get => _TabBarPosition;
            set
            {
                _TabBarPosition = value;
                RestructureContentGrid();
            }
        }

        private UInt32 _MaxTabWidth = 32;
        public UInt32 MaxTabWidth
        {
            get => _MaxTabWidth;
            set
            {
                _MaxTabWidth = value;
                AdjustTabDisplay();
            }
        }

        private UInt32 _TabPanelHeight = 30;
        public UInt32 TabPanelHeight
        {
            get => _TabPanelHeight;
            set 
            { 
                _TabPanelHeight = value;
                AdjustTabDisplay();
            }
        }

        protected Brush _TabPanelBrush = SystemColors.MenuBarBrush;
        public Brush TabPanelBrush
        {
            get => _TabPanelBrush;
            set => _TabPanelBrush = value;
        }


        public MdiContainerTabbed() : base()
        {
            TabPanel.Background = TabPanelBrush;
        }

        private void MdiContainerTabbed_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            BuildTabs();
        }

        protected override void CreateContentGrid()
        {
            TabPanel.LastChildFill = false;
            RestructureContentGrid();
        }

        /// <summary>
        /// The layout grid and control-row selection depend on the chosen position of the tab bar
        /// </summary>
        private void RestructureContentGrid()
        {
            ContentGrid.Children.Clear();
            ContentGrid.RowDefinitions.Clear();
            ContentGrid.ColumnDefinitions.Clear();

            switch (TabBarPosition)
            {
                case eTabBarPosition.Top:
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

                case eTabBarPosition.Bottom:
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

                case eTabBarPosition.Left:
                    ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(MdiData.ButtonPanelHeight, GridUnitType.Pixel) });
                    ContentGrid.RowDefinitions.Add(new RowDefinition());
                    break;

                case eTabBarPosition.Right:
                    ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(MdiData.ButtonPanelHeight, GridUnitType.Pixel) });
                    ContentGrid.RowDefinitions.Add(new RowDefinition());
                    break;
            }

            BuildTabs();
        }

        /// <summary>
        /// Whenever the tabs have a visual update
        /// </summary>
        private void AdjustTabDisplay()
        {
            foreach (var tab in Tabs)
            {
                if (tab.Width > MaxTabWidth)
                {
                    tab.Width = MaxTabWidth;
                }
            }
        }

        private void BuildTabs()
        {
            Tabs.Clear();

            foreach (var child in Children)
            {
                // add tab
            }
        }

        protected override void OnResized(object sender, SizeChangedEventArgs e)
        {

        }

        protected override void OnChildAdded(MdiChild window)
        {

        }

        protected override void OnChildRemoved(MdiChild window)
        {

        }
    }
}
