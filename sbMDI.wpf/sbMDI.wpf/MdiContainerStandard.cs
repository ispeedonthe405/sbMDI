using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace sbMDI.wpf
{
    public class MdiContainerStandard : MdiContainerBase
    {
        public MdiContainerStandard() : base()
        {

        }

        protected override void CreateContentGrid()
        {
            ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(MdiData.ButtonPanelHeight, GridUnitType.Pixel) });
            ContentGrid.RowDefinitions.Add(new RowDefinition());

            ContentGrid.Children.Add(ButtonPanel);
            Grid.SetRow(ButtonPanel, 0);

            ContentGrid.Children.Add(ClientArea);
            Grid.SetRow(ClientArea, 1);
        }

        protected override void OnChildAdded(MdiChild window)
        {

        }

        protected override void OnChildRemoved(MdiChild window)
        {

        }

        protected override void OnResized(object sender, SizeChangedEventArgs e)
        {

        }
    }
}
