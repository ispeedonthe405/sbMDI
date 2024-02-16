using sbMDI.wpf;
using System.Windows;
using System.Windows.Controls;

namespace Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        static UInt32 id = 0;
        private void btn_NewChild_Click(object sender, RoutedEventArgs e)
        {
            var tab = (TabItem)MainTabs.SelectedItem;
            if(tab is not null)
            {
                var container = tab.Content as MdiContainerBase;
                string title = string.Format("Window {0} with a really long string for a title", id++.ToString());
                container?.NewChildWindow(title);
            }            
        }
    }
}