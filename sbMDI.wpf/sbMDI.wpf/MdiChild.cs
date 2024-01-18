﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace sbMDI.wpf
{
    public class MdiChild : UserControl
    {
        public MdiContainerBase Container { get; private set; }


        ///////////////////////////////////////////////////////////
        #region Dependency Properties
        /////////////////////////////

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

            window.ApplyWindowState();
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

		public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(MdiChild));
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(MdiChild));
        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);        
        }

        public static readonly DependencyProperty ShowIconProperty =
            DependencyProperty.Register("ShowIcon", typeof(bool), typeof(MdiChild),
            new UIPropertyMetadata(true));
        public bool ShowIcon
        {
            get => (bool)GetValue(ShowIconProperty);
            set => SetValue(ShowIconProperty, value);
        }

        public static readonly DependencyProperty ResizableProperty =
            DependencyProperty.Register("Resizable", typeof(bool), typeof(MdiChild),
            new UIPropertyMetadata(true));
        public bool Resizable
        {
            get => (bool)GetValue(ResizableProperty);
            set => SetValue(ResizableProperty, value);
        }

        public static readonly DependencyProperty FocusedProperty =
            DependencyProperty.Register("Focused", typeof(bool), typeof(MdiChild),
            new UIPropertyMetadata(false, new PropertyChangedCallback(FocusedValueChanged)));
        public bool Focused
        {
            get => (bool)GetValue(FocusedProperty);
            set => SetValue(FocusedProperty, value);
        }

        private static void FocusedValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }

        /////////////////////////////
        #endregion Dependency Properties
        ///////////////////////////////////////////////////////////




        ///////////////////////////////////////////////////////////
        #region Construction & Init
        /////////////////////////////

        static MdiChild()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MdiChild), new FrameworkPropertyMetadata(typeof(MdiChild)));
        }

        public MdiChild(MdiContainerBase container)
        {
            Container = container;
            Focusable = true;
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(MdiChild), new FrameworkPropertyMetadata(typeof(MdiChild)));
            Application.Current.Resources.MergedDictionaries.Add(
                new ResourceDictionary 
                { 
                    Source = new Uri(@"/sbMDI.wpf;component/ThemeDefault.xaml", UriKind.Relative) 
                });

            ApplyWindowState();

            Loaded += MdiChild_Loaded;
            Unloaded += MdiChild_Unloaded;
            GotFocus += MdiChild_GotFocus;
        }

        /////////////////////////////
        #endregion Construction & Init
        ///////////////////////////////////////////////////////////
        
        private void MdiChild_GotFocus(object sender, RoutedEventArgs e)
        {
            Container.SetActiveWindow(this);
        }

        public void Close()
        {

        }

        public void ApplyWindowState()
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

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Thumb dragThumb = (Thumb)Template.FindName("DragThumb", this);

            if (dragThumb != null)
            {
                dragThumb.DragStarted += Thumb_DragStarted;
                dragThumb.DragDelta += dragThumb_DragDelta;

                dragThumb.MouseDoubleClick += (sender, e) =>
                {
                    if (WindowState == WindowState.Minimized)
                    {
                        //minimizeButton_Click(null, null);
                    }
                    else if (WindowState == WindowState.Normal)
                    {
                        //maximizeButton_Click(null, null);
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

        /// <summary>
        /// Handles the DragStarted event of the Thumb control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.Primitives.DragStartedEventArgs"/> instance containing the event data.</param>
        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            //if (!Focused)
            //    Focused = true;
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
            double rightBoundary = (Container.ClientAreaWidth() - Width);
            double bottomBoundary = (Container.ClientAreaHeight() - Height);

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
    }
}
