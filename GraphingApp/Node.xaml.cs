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
    /// Interaction logic for Node.xaml
    /// </summary>
    public partial class Node : UserControl
    {
        public event EventHandler RemoveTriggered;
        public event EventHandler SelectTriggered;
        public event EventHandler DiselectTriggered;


        #region Id Property

        public int Id
        {
            get { return (int)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        public static readonly DependencyProperty IdProperty =
            DependencyProperty.Register("IdProperty", typeof(int), typeof(Node), new PropertyMetadata(25, OnIdPropertyChanged));

        private static void OnIdPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            int value = (int)e.NewValue;
            (d as Node).Label.Content = value.ToString();
        }

        #endregion

        #region Color Property

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("ColorProperty", typeof(Color), typeof(Node), new PropertyMetadata(Colors.Black, OnColorPropertyChanged));

        private static void OnColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as Node).Circle.Fill = new SolidColorBrush((Color)e.NewValue);
            (d as Node).Circle.Stroke = new SolidColorBrush((Color)e.NewValue);
        }

        #endregion

        #region HighlightColor Property

        public Color HighlightColor
        {
            get { return (Color)GetValue(HighlightColorProperty); }
            set { SetValue(HighlightColorProperty, value); }
        }

        public static readonly DependencyProperty HighlightColorProperty =
            DependencyProperty.Register("HighlightColorProperty", typeof(Color), typeof(Node), new PropertyMetadata(Colors.Black, OnHighlightColorPropertyChanged));

        private static void OnHighlightColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as Node).OuterCicrle.Fill = new SolidColorBrush((Color)e.NewValue);
            (d as Node).OuterCicrle.Stroke = new SolidColorBrush((Color)e.NewValue);
        }

        #endregion

        #region Highlight Property

        public bool Highlight
        {
            get { return (bool)GetValue(HighlightProperty); }
            set { SetValue(HighlightProperty, value); }
        }

        public static readonly DependencyProperty HighlightProperty =
            DependencyProperty.Register("HighlightProperty", typeof(bool), typeof(Node), new PropertyMetadata(false, OnHighlightPropertyChanged));

        private static void OnHighlightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as Node).OuterCicrleContainer.Visibility = ((bool)e.NewValue == true) ? Visibility.Visible : Visibility.Hidden;
        }

        #endregion

        #region Diameter Property

        public int Diameter
        {
            get { return (int)GetValue(DiameterProperty); }
            set { SetValue(DiameterProperty, value); }
        }

        public static readonly DependencyProperty DiameterProperty = 
            DependencyProperty.Register("DiameterProperty", typeof(int), typeof(Node), new PropertyMetadata(30, OnDiameterPropertyChanged));

        private static void OnDiameterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as Node).Circle.Height = (int)e.NewValue;
            (d as Node).Circle.Width = (int)e.NewValue;
        }

        #endregion


        public Node()
        {            
            InitializeComponent();
        }

        private void Control_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            RemoveTriggered(this, EventArgs.Empty);
        }

        private void Control_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectTriggered(this, EventArgs.Empty);
        }

        private void Control_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DiselectTriggered(this, EventArgs.Empty);
        }
    }
}
