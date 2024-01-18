# sbMDI

--------------------------------------------------------------------

A simple .Net MDI library with multiple container types.

The goal here is to provide an easily-used approach to approximating the old MDI paradigm in modern Xaml/C# projects.

For now WPF is the only target, since I need this for a project I'm involved with. UWP and WinUI3 will come later.

Current status: Functional, usable, but not yet finished.

Notes:

For the programmer interested in using this, there are essentially only two entities to be concerned with: Container and ChildWindow.

The Container is not your main window. Instead, think of it as a panel that you would place somewhere in your window. Its purpose is
to function as the client area which contains and controls the multiple document interface. Presently, the MdiContainer comes in two forms: Standard and Tabbed.

Standard is essentially just an MDI client area panel with no other features. Tabbed adds a TabControl, with tabs that come and go
automatically with each child window. Please note that the child windows are not TabItems; they are still the children of the
MDI client area. Nevertheless, selecting a tab will activate the corresponding MDI child, and selecting an MDI child
will activate the corresponding tab. Essentially, you can think of this as a tab control with a shared item space
that happens to be an MDI region. This UX is therefore a simple but usable blend of the two paradigms.

As for the MdiChild, for your purposes this is a blank window. It's actually a UserControl that's been made to
look and work like the old MDI child windows: window state includes minimized, maximized, and normal.
A child window is automatically bounded by its Container's space.

Integration and usage are simple. See Test subproject for more details.



