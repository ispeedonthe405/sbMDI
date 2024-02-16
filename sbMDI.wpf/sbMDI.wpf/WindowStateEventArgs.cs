using System.Windows;

namespace sbMDI.wpf
{
    public delegate void WindowStateEventHandler(object sender, WindowStateEventArgs e);

    public class WindowStateEventArgs : EventArgs
    {
        public WindowState State { get; set; } = default;

        public WindowStateEventArgs(WindowState state)
        {
            State = state;
        }
    }
}
