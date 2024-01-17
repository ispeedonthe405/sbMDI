using System.Windows;
using System.Windows.Controls;

namespace sbMDI.wpf
{
    public class MdiChild : UserControl
    {
        public MdiContainerBase Container { get; private set; }

        private static UInt32 windowCounter = 0;
        private string _Title = "untitled" + windowCounter++.ToString();
        public string Title
        {
            get => _Title;
            set => _Title = value;
        }

        public static readonly DependencyProperty WindowStateProperty =
            DependencyProperty.Register("WindowState", typeof(WindowState), typeof(MdiChild),
            new UIPropertyMetadata(WindowState.Maximized, new PropertyChangedCallback(WindowStateValueChanged)));

        public WindowState WindowState
        {
            get => (WindowState)GetValue(WindowStateProperty);
            set => SetValue(WindowStateProperty, value);
        }

        private static void WindowStateValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not MdiChild window)
            {
                return;
            }

            window.ReapplyWindowState();
        }

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(Point), typeof(MdiChild),
            new UIPropertyMetadata(new Point(-1, -1), new PropertyChangedCallback(PositionValueChanged)));

        public Point Position
        {
            get => (Point)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        private static void PositionValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            //if ((Point)e.NewValue == (Point)e.OldValue)
            //    return;

            MdiChild mdiChild = (MdiChild)sender;
            Point newPosition = (Point)e.NewValue;

            Canvas.SetTop(mdiChild, newPosition.Y < 0 ? 0 : newPosition.Y);
            Canvas.SetLeft(mdiChild, newPosition.X < 0 ? 0 : newPosition.X);
        }

        public MdiChild(MdiContainerBase container)
        {
            Container = container;
            Focusable = true;
            ReapplyWindowState();

            Loaded += MdiChild_Loaded;
            Unloaded += MdiChild_Unloaded;
            GotFocus += MdiChild_GotFocus;
        }

        private void MdiChild_GotFocus(object sender, RoutedEventArgs e)
        {
            Container.SetActiveWindow(this);
        }

        public void Close()
        {

        }

        public void ReapplyWindowState()
        {
            switch (WindowState)
            {
                case WindowState.Normal:
                    {
                        Position = new Point(0, 0);
                        Width = Container.ClientAreaWidth() / 3;
                        Height = Container.ClientAreaHeight() / 3;
                    }
                    break;

                case WindowState.Maximized:
                    {
                        Position = new Point(0, 0);
                        Width = Container.ClientAreaWidth();
                        Height = Container.ClientAreaHeight();
                    }
                    break;
            }
        }

        private void MdiChild_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void MdiChild_Unloaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
