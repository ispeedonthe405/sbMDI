using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace sbMDI.wpf
{
    [ContentProperty("Content")]
    public partial class MdiChild : UserControl
    {
        public MdiContainerBase Container { get; set; }

        ///////////////////////////////////////////////////////////
        #region Dependency Properties
        /////////////////////////////

        public static readonly new DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(UserControl), typeof(MdiChild),
                new UIPropertyMetadata(new PropertyChangedCallback(ContentValueChanged)));

        public static readonly DependencyProperty WindowStateProperty =
            DependencyProperty.Register("WindowState", typeof(WindowState), typeof(MdiChild),
                new UIPropertyMetadata(WindowState.Maximized, new PropertyChangedCallback(WindowStateValueChanged)));

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(Point), typeof(MdiChild),
                new UIPropertyMetadata(new Point(-1, -1), new PropertyChangedCallback(PositionValueChanged)));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(MdiChild));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(MdiChild));

        public static readonly DependencyProperty ShowIconProperty =
            DependencyProperty.Register("ShowIcon", typeof(bool), typeof(MdiChild),
            new UIPropertyMetadata(true));

        public static readonly DependencyProperty ResizableProperty =
            DependencyProperty.Register("Resizable", typeof(bool), typeof(MdiChild),
            new UIPropertyMetadata(true));

        public static readonly DependencyProperty FocusedProperty =
            DependencyProperty.Register("Focused", typeof(bool), typeof(MdiChild),
            new UIPropertyMetadata(false, new PropertyChangedCallback(FocusedValueChanged)));

        /////////////////////////////
        #endregion Dependency Properties
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Dependency Property Callbacks
        /////////////////////////////

        private static void PositionValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Point)e.NewValue == (Point)e.OldValue)
            {
                return;
            }

            MdiChild mdiChild = (MdiChild)sender;
            Point newPosition = (Point)e.NewValue;

            Canvas.SetTop(mdiChild, newPosition.Y < 0 ? 0 : newPosition.Y);
            Canvas.SetLeft(mdiChild, newPosition.X < 0 ? 0 : newPosition.X);
        }

        private static void ContentValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is MdiChild window)
            {
                
            }
        }

        private static void WindowStateValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is MdiChild window)
            {
                window.ApplyWindowState();
                window.WindowStateChanged?.Invoke(sender, new(window.WindowState));
            }            
        }

        private static void FocusedValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == (bool)e.OldValue)
                return;

            MdiChild mdiChild = (MdiChild)sender;
            if ((bool)e.NewValue)
            {
                mdiChild.Dispatcher.BeginInvoke(new Func<IInputElement, IInputElement>(Keyboard.Focus), System.Windows.Threading.DispatcherPriority.ApplicationIdle, mdiChild.Content);
                mdiChild.RaiseEvent(new RoutedEventArgs(GotFocusEvent, mdiChild));
            }
            else
            {
                if (mdiChild.WindowState == WindowState.Maximized)
                {
                    //mdiChild.Unmaximize();
                }
                mdiChild.RaiseEvent(new RoutedEventArgs(LostFocusEvent, mdiChild));
            }
        }

        /////////////////////////////
        #endregion Dependency Property Callbacks
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        private Button? ButtonMinimize;
        private Button? ButtonMaximize;
        private Button? ButtonClose;
        private StackPanel? ButtonsPanel;

        public new UserControl Content
        {
            get => (UserControl)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public WindowState WindowState
        {
            get => (WindowState)GetValue(WindowStateProperty);
            set => SetValue(WindowStateProperty, value);
        }

        public bool Focused
        {
            get => (bool)GetValue(FocusedProperty);
            set => SetValue(FocusedProperty, value);
        }

        public bool Resizable
        {
            get => (bool)GetValue(ResizableProperty);
            set => SetValue(ResizableProperty, value);
        }

        public bool ShowIcon
        {
            get => (bool)GetValue(ShowIconProperty);
            set => SetValue(ShowIconProperty, value);
        }

        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public Point Position
        {
            get => (Point)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        internal const int MinimizedWidth = 160;
        internal const int MinimizedHeight = 29;

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Events
        /////////////////////////////

        public static readonly RoutedEvent ClosingEvent =
            EventManager.RegisterRoutedEvent("Closing", RoutingStrategy.Bubble, typeof(ClosingEventArgs), typeof(MdiChild));

        public static readonly RoutedEvent ClosedEvent =
            EventManager.RegisterRoutedEvent("Closed", RoutingStrategy.Bubble, typeof(RoutedEventArgs), typeof(MdiChild));

        public event WindowStateEventHandler? WindowStateChanged;

        /////////////////////////////
        #endregion Events
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Construction & Init
        /////////////////////////////

        static MdiChild()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MdiChild), new FrameworkPropertyMetadata(typeof(MdiChild)));
        }

        // NOTE: This ctor is only a crutch for design-time
        public MdiChild()
        {
            Container = new MdiContainerStandard();
            CommonCtor();
        }

        public MdiChild(MdiContainerBase container)
        {
            Container = container;
            CommonCtor();
        }

        protected void CommonCtor()
        {
            Focusable = false;

            ApplyWindowState();

            Loaded += MdiChild_Loaded;
            Unloaded += MdiChild_Unloaded;
            GotFocus += MdiChild_GotFocus;
            KeyDown += MdiChild_KeyDown;
            MouseDown += MdiChild_MouseDown;
            DataContextChanged += MdiChild_DataContextChanged;
        }

        private void MdiChild_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(Content is not null)
            {
                Content.DataContext = DataContext;
            }
        }


        /////////////////////////////
        #endregion Construction & Init
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Events
        /////////////////////////////

        private void MdiChild_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Focused = true;
        }

        private void MdiChild_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not MdiChild window)
            {
                return;
            }
            switch (e.Key)
            {
                case Key.F4:
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                    {
                        window.Close();
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void MdiChild_GotFocus(object sender, RoutedEventArgs e)
        {
            Focus();
            Content?.Focus();
            Container?.SetActiveWindow(this);
        }

        private void MdiChild_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void MdiChild_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        protected virtual void OnActivated() { }
        protected virtual void OnDeactivated() { }

        /////////////////////////////
        #endregion Events
        ///////////////////////////////////////////////////////////
        


        ///////////////////////////////////////////////////////////
        #region Window Interface
        /////////////////////////////

        public void Close()
        {
            Container?.CloseChildWindow(this);
        }

        /////////////////////////////
        #endregion Window Interface
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Window / Framework Internal
        /////////////////////////////

        public void ApplyWindowState()
        {
            switch (WindowState)
            {
                case WindowState.Normal:
                    {
                        Position = new Point(0, 0);
                        if (Container is not null)
                        {
                            Width = Container.ClientAreaWidth / 3;
                            Height = Container.ClientAreaHeight / 3;
                        }
                    }
                    break;

                case WindowState.Maximized:
                    {
                        Position = new Point(0, 0);
                        if (Container is not null)
                        {
                            Width = Container.ClientAreaWidth;
                            Height = Container.ClientAreaHeight;
                        }
                    }
                    break;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ButtonMinimize = (Button)Template.FindName("MinimizeButton", this);
            ButtonMaximize = (Button)Template.FindName("MaximizeButton", this);
            ButtonClose = (Button)Template.FindName("CloseButton", this);
            ButtonsPanel = (StackPanel)Template.FindName("ButtonsPanel", this);

            if (ButtonMinimize is not null)
            {
                ButtonMinimize.Click += ButtonMinimize_Click;
            }

            if (ButtonMaximize is not null)
            {
                ButtonMaximize.Click += ButtonMaximize_Click;
            }

            if (ButtonClose != null)
            {
                ButtonClose.Click += ButtonClose_Click;
            }

            Thumb dragThumb = (Thumb)Template.FindName("DragThumb", this);

            if (dragThumb != null)
            {
                dragThumb.DragStarted += Thumb_DragStarted;
                dragThumb.DragDelta += dragThumb_DragDelta;

                dragThumb.MouseDoubleClick += (sender, e) =>
                {
                    if (WindowState == WindowState.Minimized)
                    {
                        ButtonMinimize_Click(this, new RoutedEventArgs());
                    }
                    else if (WindowState == WindowState.Normal)
                    {
                        ButtonMaximize_Click(this, new RoutedEventArgs());
                    }
                };
            }

            Thumb resizeLeft = (Thumb)Template.FindName("ResizeLeft", this);
            Thumb resizeTopLeft = (Thumb)Template.FindName("ResizeTopLeft", this);
            Thumb resizeTop = (Thumb)Template.FindName("ResizeTop", this);
            Thumb resizeTopRight = (Thumb)Template.FindName("ResizeTopRight", this);
            Thumb resizeRight = (Thumb)Template.FindName("ResizeRight", this);
            Thumb resizeBottomRight = (Thumb)Template.FindName("ResizeBottomRight", this);
            Thumb resizeBottom = (Thumb)Template.FindName("ResizeBottom", this);
            Thumb resizeBottomLeft = (Thumb)Template.FindName("ResizeBottomLeft", this);

            if (resizeLeft != null)
            {
                resizeLeft.DragStarted += Thumb_DragStarted;
                resizeLeft.DragDelta += ResizeLeft_DragDelta;
            }

            if (resizeTop != null)
            {
                resizeTop.DragStarted += Thumb_DragStarted;
                resizeTop.DragDelta += ResizeTop_DragDelta;
            }

            if (resizeRight != null)
            {
                resizeRight.DragStarted += Thumb_DragStarted;
                resizeRight.DragDelta += ResizeRight_DragDelta;
            }

            if (resizeBottom != null)
            {
                resizeBottom.DragStarted += Thumb_DragStarted;
                resizeBottom.DragDelta += ResizeBottom_DragDelta;
            }

            if (resizeTopLeft != null)
            {
                resizeTopLeft.DragStarted += Thumb_DragStarted;

                resizeTopLeft.DragDelta += (sender, e) =>
                {
                    ResizeTop_DragDelta(sender, e);
                    ResizeLeft_DragDelta(sender, e);

                    //Container.InvalidateSize();
                };
            }

            if (resizeTopRight != null)
            {
                resizeTopRight.DragStarted += Thumb_DragStarted;

                resizeTopRight.DragDelta += (sender, e) =>
                {
                    ResizeTop_DragDelta(sender, e);
                    ResizeRight_DragDelta(sender, e);

                    //Container.InvalidateSize();
                };
            }

            if (resizeBottomRight != null)
            {
                resizeBottomRight.DragStarted += Thumb_DragStarted;

                resizeBottomRight.DragDelta += (sender, e) =>
                {
                    ResizeBottom_DragDelta(sender, e);
                    ResizeRight_DragDelta(sender, e);

                    //Container.InvalidateSize();
                };
            }

            if (resizeBottomLeft != null)
            {
                resizeBottomLeft.DragStarted += Thumb_DragStarted;

                resizeBottomLeft.DragDelta += (sender, e) =>
                {
                    ResizeBottom_DragDelta(sender, e);
                    ResizeLeft_DragDelta(sender, e);

                    //Container.InvalidateSize();
                };
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            ClosingEventArgs eventArgs = new ClosingEventArgs(ClosingEvent);
            RaiseEvent(eventArgs);

            if (eventArgs.Cancel)
                return;

            Close();

            RaiseEvent(new RoutedEventArgs(ClosedEvent));
        }

        private void ButtonMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Minimized;
            }
        }

        /// <summary>
        /// Handles the DragStarted event of the Thumb control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.Primitives.DragStartedEventArgs"/> instance containing the event data.</param>
        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            if(!Focused)
            {
                Container?.SetActiveWindow(this);
            }
        }

        /// <summary>
        /// Handles the DragDelta event of the ResizeLeft control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.Primitives.DragDeltaEventArgs"/> instance containing the event data.</param>
        private void ResizeLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (Width - e.HorizontalChange < MinWidth)
            {
                return;
            }

            double newLeft = e.HorizontalChange;

            if (Position.X + newLeft < 0)
            {
                newLeft = 0 - Position.X;
            }

            Width -= newLeft;
            Position = new Point(Position.X + newLeft, Position.Y);

            if (sender != null)
            {
                //Container.InvalidateSize();
            }
        }

        /// <summary>
        /// Handles the DragDelta event of the ResizeTop control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.Primitives.DragDeltaEventArgs"/> instance containing the event data.</param>
        private void ResizeTop_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (Height - e.VerticalChange < MinHeight)
            {
                return;
            }

            double newTop = e.VerticalChange;

            if (Position.Y + newTop < 0)
            {
                newTop = 0 - Position.Y;
            }

            Height -= newTop;
            Position = new Point(Position.X, Position.Y + newTop);

            if (sender != null)
            {
                //Container.InvalidateSize();
            }
        }

        /// <summary>
        /// Handles the DragDelta event of the ResizeRight control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.Primitives.DragDeltaEventArgs"/> instance containing the event data.</param>
        private void ResizeRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (Width + e.HorizontalChange < MinWidth)
            {
                return;
            }
            
            Width += e.HorizontalChange;

            if (sender != null)
            {
                //Container.InvalidateSize();
            }
        }

        /// <summary>
        /// Handles the DragDelta event of the ResizeBottom control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.Primitives.DragDeltaEventArgs"/> instance containing the event data.</param>
        private void ResizeBottom_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (Height + e.VerticalChange < MinHeight)
            {
                return;
            }

            Height += e.VerticalChange;

            if (sender != null)
            {
                //Container.InvalidateSize();
            }
        }

        /// <summary>
        /// Handles the DragDelta event of the dragThumb control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.Primitives.DragDeltaEventArgs"/> instance containing the event data.</param>
        private void dragThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                return;
            }

            double newLeft = Position.X + e.HorizontalChange;
            double newTop = Position.Y + e.VerticalChange;
            double rightBoundary = (Container.ClientAreaWidth - Width);
            double bottomBoundary = (Container.ClientAreaHeight - Height);

            if (newLeft < 0)
            {
                newLeft = 0;
            }
            if(newLeft > rightBoundary)
            {
                newLeft = rightBoundary;
            }

            if (newTop < 0)
            {
                newTop = 0;
            }
            if(newTop > bottomBoundary)
            {
                newTop = bottomBoundary;
            }

            Position = new Point(newLeft, newTop);

            //Container.InvalidateSize();
        }

        /////////////////////////////
        #endregion Window / Framework Internal
        ///////////////////////////////////////////////////////////
    }
}
