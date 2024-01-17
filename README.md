# sbMDI
A simple .Net MDI library with multiple container types.

The goal here is to provide an easily-used approach to approximating the MDI paradigm into modern Xaml/C# projects.

For now WPF is the only target, since I need this for a project I'm involved with. UWP and WinUI3 will come later.

WPF Usage (XAML):
xmlns:mdi="clr-namespace:sbMDI.wpf;assembly=sbMDI.wpf"
...
<mdi:MdiContainerStandard> or <mdi:MdiContainerTabbed>

WPF Usage (C#):
MdiChild window = MyContainer.NewChildWindow();

See Test subproject for more details.

