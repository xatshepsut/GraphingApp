using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using QuickGraph;
using QuickGraph.Serialization;


namespace GraphingApp
{
    public partial class MainWindow : Window
    {
        private Dictionary<Node, List<Edge>> Nodes { get; set; }

        private Node SelectedNode { get; set; }
        private Node EdgeSourceNode { get; set; }
        private Node EdgeDestinationNode { get; set; }

        private double LargestX { get; set; }
        private double LargestY { get; set; }

        private int NodeIdCounter { get; set; }


        public MainWindow()
        {
            InitializeComponent();

            this.SizeChanged += window_Resize;

            Nodes = new Dictionary<Node, List<Edge>>();
            LargestX = LargestY = 0;
            NodeIdCounter = 0;
        }

        private void window_Resize(object sender, System.EventArgs e)
        {
            whiteboard.Height = (scroller.ActualHeight < LargestY) ? LargestY : scroller.ActualHeight - 10;
            whiteboard.Width = (scroller.ActualWidth < LargestX) ? LargestX : scroller.ActualWidth;
        }

        private Node addNode(Point position)
        {
            var node= new Node();
            node.Id = NodeIdCounter++;

            node.RemoveTriggered += this.node_RemoveTriggered;
            node.SelectTriggered += this.node_SelectTriggered;
            node.DiselectTriggered += this.node_DiselectTriggered;

            Canvas.SetTop(node, position.Y - node.Diameter / 2);
            Canvas.SetLeft(node, position.X - node.Diameter / 2);
            Canvas.SetZIndex(node, 20);
            whiteboard.Children.Add(node);

            if (position.X + node.Diameter > LargestX)
            {
                LargestX = position.X + node.Diameter;
            }
            if (position.Y + node.Diameter > LargestY)
            {
                LargestY = position.Y + node.Diameter;
            }

            Nodes.Add(node, new List<Edge>());

            return node;
        }

        private Edge addEdge(Node source, Node destination)
        {
            if (source == null || destination == null)
            {
                return null;
            }

            var edge = new Edge();
            edge.SourcePosition = new Point(Canvas.GetLeft(source) + source.Diameter / 2, Canvas.GetTop(source) + source.Diameter / 2);
            edge.DestinationPosition = new Point(Canvas.GetLeft(destination) + destination.Diameter / 2, Canvas.GetTop(destination) + destination.Diameter / 2);
            edge.SourceNode = source;
            edge.DestinationNode = destination;

            edge.RemoveTriggered += this.edge_RemoveTriggered;

            whiteboard.Children.Add(edge);

            List<Edge> edges = null;
            Nodes.TryGetValue(source, out edges);
            edges.Add(edge);
            Nodes.TryGetValue(destination, out edges);
            edges.Add(edge);

            return edge;
        }

        #region Node Events

        private void node_RemoveTriggered(object sender, EventArgs e)
        {
            var node = (Node)sender;
            node.RemoveTriggered -= this.node_RemoveTriggered;
            node.SelectTriggered -= this.node_SelectTriggered;
            node.DiselectTriggered -= this.node_DiselectTriggered;

            List<Edge> edges = null;
            Nodes.TryGetValue(node, out edges);
            foreach (var edge in edges)
            {
                var dest_node = (edge.SourceNode == node) ? edge.DestinationNode : edge.SourceNode;
                List<Edge> dest_edges = null;
                Nodes.TryGetValue(dest_node, out dest_edges);
                dest_edges.Remove(edge);

                whiteboard.Children.Remove((UIElement)edge);
            }

            Nodes.Remove(node);
            if (Nodes.Count == 0)
            {
                NodeIdCounter = 0;
                LargestX = LargestY = 0;
            }

            whiteboard.Children.Remove((UIElement)sender);
        }

        private void node_SelectTriggered(object sender, EventArgs e)
        {
            if (edgeToggleBtn.IsChecked == true)
            {
                if (EdgeSourceNode == null)
                {
                    EdgeSourceNode = (Node)sender;
                }
                else if (EdgeDestinationNode == null && EdgeSourceNode != SelectedNode)
                {
                    EdgeDestinationNode = (Node)sender;
                    addEdge(EdgeSourceNode, EdgeDestinationNode);
                    EdgeSourceNode = null;
                    EdgeDestinationNode = null;
                }
            }
            else
            {
                SelectedNode = (Node)sender;
            }
        }

        private void node_DiselectTriggered(object sender, EventArgs e)
        {
            SelectedNode = null;
        }

        #endregion

        #region Edge Events

        private void edge_RemoveTriggered(object sender, EventArgs e)
        {
            var edge = (Edge)sender;
            edge.RemoveTriggered -= this.edge_RemoveTriggered;

            List<Edge> edges = null;
            Nodes.TryGetValue(edge.SourceNode, out edges);
            edges.Remove(edge);
            Nodes.TryGetValue(edge.DestinationNode, out edges);
            edges.Remove(edge);

            whiteboard.Children.Remove((UIElement)sender);
        }

        #endregion


        private void ToggleBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == nodeToggleBtn)
            {
                edgeToggleBtn.IsChecked = false;

                EdgeSourceNode = null;
                EdgeDestinationNode = null;
            }
            else if (sender == edgeToggleBtn)
            {
                nodeToggleBtn.IsChecked = false;
            }
        }
        
        private void whiteboard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedNode == null && nodeToggleBtn.IsChecked == true)
            {
                addNode(e.GetPosition(whiteboard));
            }
        }

        private void whiteboard_MouseMove(object sender, MouseEventArgs e)
        {
            if (SelectedNode != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(whiteboard);
                Canvas.SetTop(SelectedNode, position.Y - SelectedNode.Diameter / 2);
                Canvas.SetLeft(SelectedNode, position.X - SelectedNode.Diameter / 2);

                List<Edge> edges = null;
                Nodes.TryGetValue(SelectedNode, out edges);
                foreach (var edge in edges)
                {
                    if (edge.SourceNode == SelectedNode)
                    {
                        edge.SourcePosition = position;
                    }
                    else
                    {
                        edge.DestinationPosition = position;
                    }
                }
            }
        }

        /// <summary>
        /// Tests graph generator module calling and result parsing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void test_generateToggleBtn_Click(object sender, RoutedEventArgs e)
        {
            String generatorDir = System.IO.Directory.GetCurrentDirectory() + "\\..\\..\\..\\graph_generator\\dist\\graph_generator";

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = generatorDir + "\\graph_generator.exe";
            start.Arguments = "classic --graph-type=complete --n=5";
            start.WorkingDirectory = generatorDir;
            start.CreateNoWindow = false;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;

            String filename = "";

            using (Process process = Process.Start(start))
            {
                using (System.IO.StreamReader reader = process.StandardOutput)
                {
                    string output = reader.ReadToEnd();
                    filename = System.Text.RegularExpressions.Regex.Match(output, "\"([^\"]*)\"").Groups[1].Value;
                }
            }

            var graph = new BidirectionalGraph<int, Edge<int>>();
            using (var xreader = System.Xml.XmlReader.Create(generatorDir + "\\" + filename))
            {
                graph.DeserializeFromGraphML(xreader,
                    id => int.Parse(id),
                    (source, target, id) => new Edge<int>(source, target)
                );
            }

        }

    }
}
