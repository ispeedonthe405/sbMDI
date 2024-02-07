using System.Windows;
using System.Windows.Controls;

namespace sbMDI.wpf
{
    /// <summary>
    /// A very basic MDI container; essentially just a client area for child
    /// windows to live in.
    /// </summary>
    public class MdiContainerStandard : MdiContainerBase
    {
        public MdiContainerStandard() : base()
        {

        }

        protected override void CreateContainerGrid()
        {
            ContainerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(MdiData.ButtonPanelHeight, GridUnitType.Pixel) });
            ContainerGrid.RowDefinitions.Add(new RowDefinition());

            ContainerGrid.Children.Add(ButtonPanel);
            Grid.SetRow(ButtonPanel, 0);

            ContainerGrid.Children.Add(ClientArea);
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

        protected override void OnChildActivated(MdiChild window)
        {

        }
    }
}
