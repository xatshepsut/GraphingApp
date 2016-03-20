using System;
using System.Windows;
using System.Windows.Controls;


namespace GraphingApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void addNode(double top, double left)
        {
            var node= new Node();
            node.RemoveTriggered += this.node_RemoveTriggered;

            Canvas.SetTop(node, top - node.Diameter / 2);
            Canvas.SetLeft(node, left - node.Diameter / 2);
            whiteboard.Children.Add(node);
        }

        private void whiteboard_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (nodeToggleBtn.IsChecked == true)
            {
                Point position = e.GetPosition(whiteboard);
                addNode(position.Y, position.X);
            }
        }

        private void node_RemoveTriggered(object sender, EventArgs e)
        {
            whiteboard.Children.Remove((UIElement)sender);
        }
    }
}
