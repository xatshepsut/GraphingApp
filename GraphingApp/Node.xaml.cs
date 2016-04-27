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
            (d as Node).Label.Content = (value < 10) ? (" " + value) : value.ToString();
        }

        #endregion

        #region Diameter Property

        public int Diameter
        {
            get { return (int)GetValue(DiameterProperty); }
            set { SetValue(DiameterProperty, value); }
        }

        public static readonly DependencyProperty DiameterProperty = 
            DependencyProperty.Register("DiameterProperty", typeof(int), typeof(Node), new PropertyMetadata(25, OnDiameterPropertyChanged));

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
