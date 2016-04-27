using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GraphingApp
{
    /// <summary>
    /// Interaction logic for Edge.xaml
    /// </summary>
    public partial class Edge : UserControl
    {
        public event EventHandler RemoveTriggered;

        #region SourcePosition property

        public Point SourcePosition
        {
            get { return (Point)GetValue(SourcePositionProperty); }
            set { SetValue(SourcePositionProperty, value); }
        }

        public static readonly DependencyProperty SourcePositionProperty =
            DependencyProperty.Register("SourcePositionProperty", typeof(Point), typeof(Edge), new PropertyMetadata(new Point(0, 0), OnSourcePositionPropertyChanged));

        private static void OnSourcePositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Point point = (Point)e.NewValue;
            (d as Edge).Line.X1 = point.X;
            (d as Edge).Line.Y1 = point.Y;
        }

        #endregion

        #region DestinationPosition property

        public Point DestinationPosition
        {
            get { return (Point)GetValue(DestinationPositionProperty); }
            set { SetValue(DestinationPositionProperty, value); }
        }

        public static readonly DependencyProperty DestinationPositionProperty =
            DependencyProperty.Register("DestinationPositionProperty", typeof(Point), typeof(Edge), new PropertyMetadata(new Point(0, 0), OnDestinationPositionPropertyChanged));

        private static void OnDestinationPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Point point = (Point)e.NewValue;
            (d as Edge).Line.X2 = point.X;
            (d as Edge).Line.Y2 = point.Y;
        }

        #endregion

        
        public Edge()
        {
            InitializeComponent();
        }

        private void Control_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            RemoveTriggered(this, EventArgs.Empty);
        }
    }
}

