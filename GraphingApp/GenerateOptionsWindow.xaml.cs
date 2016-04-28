using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Diagnostics;
using QuickGraph;
using QuickGraph.Serialization;


namespace GraphingApp
{
    /// <summary>
    /// Interaction logic for GenerateOptionsWindow.xaml
    /// </summary>
    public partial class GenerateOptionsWindow : Window
    {
        public AdjacencyGraph<int, Edge<int>> GeneratedGraph { get; set; }
        public bool IsGenerated { get; set; }

        private String generatorDirectory = System.IO.Directory.GetCurrentDirectory() + "\\..\\..\\..\\graph_generator\\dist\\graph_generator";
        private String generatorFilename = "graph_generator.exe";

        public GenerateOptionsWindow()
        {
            InitializeComponent();

            randomRadioBtn.IsChecked = true;
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Proper field validation
            int nodeNumber = 0;
            Int32.TryParse(nodeNumberTextBox.Text, out nodeNumber);
            if (nodeNumber == 0)
            {
                MessageBox.Show("Please enter valid node number", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }
            
            String filename = "";

            if (classicRadioBtn.IsChecked == true)
            {
                String type = ((ComboBoxItem)comboOptions.SelectedItem).Tag.ToString();
                filename = GenerateClassicGraph(type, nodeNumber);
            }
            else
            {
                filename = GenerateRandomGraph(nodeNumber);
            }

            GeneratedGraph = ParseGraphML(generatorDirectory + "\\" + filename);
            IsGenerated = true;

            this.Close();
        }

        private String GenerateClassicGraph(String type, int nodeNumber)
        {
            String args = "classic --graph-type=" + type + " --n=" + nodeNumber;
            return CallGenerator(args);
        }

        private String GenerateRandomGraph(int nodeNumber)
        {
            String args = "random --n=" + nodeNumber;
            return CallGenerator(args);
        }

        private String CallGenerator(String args)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = generatorDirectory + "\\" + generatorFilename;
            start.Arguments = args;
            start.WorkingDirectory = generatorDirectory;
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

            return filename;
        }

        private AdjacencyGraph<int, Edge<int>> ParseGraphML(String filepath)
        {
            if (filepath == "" || !System.IO.File.Exists(filepath))
            {
                return null;
            }

            var graph = new AdjacencyGraph<int, Edge<int>>();
            using (var xreader = System.Xml.XmlReader.Create(filepath))
            {
                graph.DeserializeFromGraphML(xreader,
                    id => int.Parse(id),
                    (source, target, id) => new Edge<int>(source, target)
                );
            }

            return graph;
        }

        private void radioBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == classicRadioBtn)
            {
                randomRadioBtn.IsChecked = false;
                comboOptions.IsEnabled = true;
            }
            else
            {
                classicRadioBtn.IsChecked = false;
                comboOptions.IsEnabled = false;
            }
        }

    }
}
