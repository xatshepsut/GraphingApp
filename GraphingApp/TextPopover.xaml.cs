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
    /// Interaction logic for TextPopover.xaml
    /// </summary>
    public partial class TextPopover : UserControl
    {
        public TextPopover()
        {
            InitializeComponent();
        }

        #region Text Property

        public String Text
        {
            get { return (String)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("TextProperty", typeof(String), typeof(TextPopover), new PropertyMetadata("", OnTextPropertyChanged));

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TextPopover).popoverText.Text = (String)e.NewValue;
        }

        #endregion
    }
}
