using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace sbMDI.wpf
{
    public class MdiChild : UserControl
    {
        private static UInt32 _DebugCounter = 0;
        public static UInt32 DebugCounter
        {
            get => _DebugCounter;
            set => _DebugCounter = value;
        }
        public static string DebugID()
        {
            string id = DebugCounter.ToString();
            DebugCounter++;
            return id;
        }

        public MdiContainerBase Container { get; private set; }

        private string _Title = "untitled";
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

            window.Resize();
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
            // debug code: where the hell is my window?
            Title = DebugID();
            Random rand = new Random();
            int choice = rand.Next(1, 4);
            switch (choice)
            {
                case 1:
                    Background = new SolidColorBrush(Colors.Red);
                    break;
                case 2:
                    Background = new SolidColorBrush(Colors.Green);
                    break;
                case 3:
                    Background = new SolidColorBrush(Colors.Blue);
                    break;
            }
            ////////////////////////////

            Container = container;
            Resize();

            Loaded += MdiChild_Loaded;
            Unloaded += MdiChild_Unloaded;
            GotFocus += MdiChild_GotFocus;
            MouseLeftButtonUp += MdiChild_MouseLeftButtonUp;
        }

        private void MdiChild_GotFocus(object sender, RoutedEventArgs e)
        {
            Container.SetActiveWindow(this);
        }

        public void Close()
        {

        }

        public void Resize()
        {
            switch (WindowState)
            {
                case WindowState.Normal:
                    {
                        Position = new Point(0, 0);
                        Width = Container.ClientArea.ActualWidth / 3;
                        Height = Container.ClientArea.ActualHeight / 3;
                    }
                    break;

                case WindowState.Minimized:
                    {

                    }
                    break;

                case WindowState.Maximized:
                    {
                        Position = new Point(0, 0);
                        Width = Container.ClientArea.ActualWidth;
                        Height = Container.ClientArea.ActualHeight;
                    }
                    break;
            }
        }

        private void MdiChild_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Debug.WriteLine("Child window clicked");
        }

        private void MdiChild_Loaded(object sender, RoutedEventArgs e)
        {
            //Position = Position;
            //WindowState = WindowState;
        }

        private void MdiChild_Unloaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
