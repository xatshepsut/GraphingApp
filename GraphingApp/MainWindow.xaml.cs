using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using QuickGraph;
using QuickGraph.Serialization;
using System.Windows.Media;


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

        private Node addNode(Point position, int id)
        {
            var node= new Node();
            node.Id = id;

            node.RemoveTriggered += this.node_RemoveTriggered;
            node.SelectTriggered += this.node_SelectTriggered;
            node.DiselectTriggered += this.node_DiselectTriggered;

            Canvas.SetTop(node, position.Y - node.Diameter / 2);
            Canvas.SetLeft(node, position.X - node.Diameter / 2);
            Canvas.SetZIndex(node, 20);
            whiteboard.Children.Add(node);

            Point new_point = new Point(position.X + 2 * node.Diameter, position.Y + 2 * node.Diameter);
            if (new_point.X > LargestX)
            {
                LargestX = new_point.X;
            }
            if (new_point.Y > LargestY)
            {
                LargestY = new_point.Y;
            }

            Nodes.Add(node, new List<Edge>());

            return node;
        }

        private Edge addEdge(Node source, Node destination)
        {
            if (source == null || destination == null || source == destination)
            {
                return null;
            }

            var edge = new Edge();
            edge.RemoveTriggered += this.edge_RemoveTriggered;

            edge.SourceNode = source;
            edge.DestinationNode = destination;
            edge.SourcePosition = GetNodeCenter(source);
            edge.DestinationPosition = GetNodeCenter(destination);
            
           whiteboard.Children.Add(edge);

            List<Edge> edges = null;
            Nodes.TryGetValue(source, out edges);
            edges.Add(edge);
            Nodes.TryGetValue(destination, out edges);
            edges.Add(edge);

            return edge;
        }

        private void removeAllNodes()
        {
            while (Nodes.Keys.Count > 0)
            {
                var enumerator = Nodes.Keys.GetEnumerator();
                enumerator.MoveNext();
                removeNode(enumerator.Current);
            }
        }

        private void removeNode(Node node)
        {
            if (node == null || !Nodes.ContainsKey(node))
            {
                return;
            }

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

            whiteboard.Children.Remove((UIElement)node);
        }

        private void removeEdge(Edge edge)
        {
            edge.RemoveTriggered -= this.edge_RemoveTriggered;

            List<Edge> edges = null;
            Nodes.TryGetValue(edge.SourceNode, out edges);
            edges.Remove(edge);
            Nodes.TryGetValue(edge.DestinationNode, out edges);
            edges.Remove(edge);

            whiteboard.Children.Remove((UIElement)edge);
        }

        #region Node Events

        private void node_RemoveTriggered(object sender, EventArgs e)
        {
            removeNode((Node)sender);
        }

        private void node_SelectTriggered(object sender, EventArgs e)
        {
            if (edgeToggleBtn.IsChecked == true)
            {
                if (EdgeSourceNode == null)
                {
                    EdgeSourceNode = (Node)sender;
                    EdgeSourceNode.Color = Colors.Red;
                }
                else if (EdgeDestinationNode == null)
                {
                    EdgeDestinationNode = (Node)sender;
                    addEdge(EdgeSourceNode, EdgeDestinationNode);

                    EdgeSourceNode.Color = Colors.Black;
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
            removeEdge((Edge)sender);
        }

        #endregion


        private void toggle_Checked(object sender, RoutedEventArgs e)
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
                addNode(e.GetPosition(whiteboard), NodeIdCounter++);
            }
        }

        private void whiteboard_MouseMove(object sender, MouseEventArgs e)
        {
            if (SelectedNode != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(whiteboard);
                Canvas.SetTop(SelectedNode, position.Y - SelectedNode.Diameter / 2);
                Canvas.SetLeft(SelectedNode, position.X - SelectedNode.Diameter / 2);

                Point new_point = new Point(position.X + 2 * SelectedNode.Diameter, position.Y + 2 * SelectedNode.Diameter);
                if (new_point.X > LargestX)
                {
                    LargestX = new_point.X;
                }
                if (new_point.Y > LargestY)
                {
                    LargestY = new_point.Y;
                }

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

        private void generate_Click(object sender, RoutedEventArgs e)
        {
            var generateWindow = new GenerateOptionsWindow();
            generateWindow.ShowDialog();

            var graph = generateWindow.GeneratedGraph;
            if (graph == null || !generateWindow.IsGenerated)
            {
                return;
            }

            removeAllNodes();

            Dictionary<int, Node> nodeMap = new Dictionary<int, Node>();
            double offsetX = whiteboard.ActualWidth / 2;
            double offsetY = whiteboard.ActualHeight / 2;
            double r = Math.Min(whiteboard.ActualWidth, whiteboard.ActualHeight) / 3;
            int i = 0, n = graph.VertexCount;

            foreach (var vertex in graph.Vertices)
            {
                Point position = new Point(offsetX + r * Math.Cos(2 * Math.PI * i / n), offsetY + r * Math.Sin(2 * Math.PI * i / n));
                Node node = addNode(position, vertex);
                nodeMap.Add(vertex, node);
                i++;
            }

            foreach (var edge in graph.Edges)
            {
                Node sourceNode, destNode;
                nodeMap.TryGetValue(edge.Source, out sourceNode);
                nodeMap.TryGetValue(edge.Target, out destNode);
                addEdge(sourceNode, destNode);
            }
        }

        private void clear_Click(object sender, RoutedEventArgs e)
        {
            removeAllNodes();
        }

        public Point GetNodeCenter(Node node)
        {
            Point result = new Point();
            result.X = Canvas.GetLeft(node) + node.Diameter / 2;
            result.Y = Canvas.GetTop(node) + node.Diameter / 2;
            return result;
        }



    }
}
