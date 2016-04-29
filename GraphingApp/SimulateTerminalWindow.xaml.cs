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
using System.Windows.Shapes;

namespace GraphingApp
{
    public class ExecuteEventArgs : EventArgs
    {
        public List<Tuple<int, int>> Tuples { get; set; }
        public ExecuteEventArgs(List<Tuple<int, int>> tuples)
        {
            Tuples = tuples;
        }
    }

    public class MarkServerEventArgs : EventArgs
    {
        public int Id { get; set; }
        public MarkServerEventArgs(int id)
        {
            Id = id;
        }
    }

    public partial class SimulateTerminalWindow : Window
    {
        public event EventHandler<ExecuteEventArgs> Execute;
        public event EventHandler<MarkServerEventArgs> MarkServer;
        public event EventHandler ExecutionEnded;

        private Brush ClearHighlightBrush { get; set; } 
        private Brush GreenHighlightBrush { get; set; }
        private Brush RedHighlightBrush { get; set; }

        private Block CurrentBlock { get; set; }
        private int CurrentBlockIndex { get; set; }
        private int BlockCount { get; set; }
        
        public SimulateTerminalWindow()
        {
            InitializeComponent();

            this.Closing += window_Closing;

            ClearHighlightBrush = new SolidColorBrush(Colors.Transparent);
            GreenHighlightBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#80FF66"));
            RedHighlightBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6666"));
        }

        void window_Closing(object sender, EventArgs e)
        {
            ExecutionEnded(this, EventArgs.Empty);
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            richTextBox.IsEnabled = false;
            startBtn.IsEnabled = false;

            BlockCount = richTextBox.Document.Blocks.Count();
            CurrentBlockIndex = -1;

            {
                var enumerator = richTextBox.Document.Blocks.AsEnumerable().GetEnumerator();
                enumerator.MoveNext();
                CurrentBlock = enumerator.Current;
            }

            // parse
            bool success = ParseBlock();
            if (!success)
            {
                SetupErrorState();
                ShowErrorMessage(null);
                return;
            }

            HighlightBlock(CurrentBlock, GreenHighlightBrush);
            // inform main window

            List<Tuple<int, int>> tuples = new List<Tuple<int, int>>();
            tuples.Add(new Tuple<int, int>(1, 2));
            tuples.Add(new Tuple<int, int>(3, 4));
            tuples.Add(new Tuple<int, int>(5, 6));

            ExecuteEventArgs args = new ExecuteEventArgs(tuples);
            Execute(this, args);
        }

        private void next_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentBlockIndex < BlockCount)
            {
                // CurrentBlock is current from previous iteration
                HighlightBlock(CurrentBlock, ClearHighlightBrush);
               
                CurrentBlock = CurrentBlock.NextBlock;
                CurrentBlockIndex++;  // time slot

                bool success = ParseBlock();
                // parse
                // get tuples and time
                // compare times should be == CurrentBlockIndex
                // iterate through all tuples

                if (!success /*|| wrongtime || wrong id*/)
                {
                    SetupErrorState();
                    ShowErrorMessage(null);
                    return;
                }

                HighlightBlock(CurrentBlock, GreenHighlightBrush);
                // inform main window
            }
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            ResetSimulation();
            ExecutionEnded(this, EventArgs.Empty);
        }

        private void ResetSimulation()
        {
            HighlightBlock(CurrentBlock, ClearHighlightBrush);

            CurrentBlock = null;
            CurrentBlockIndex = -1;
            BlockCount = 0;

            richTextBox.IsEnabled = true;
            startBtn.IsEnabled = true;
            nextBtn.IsEnabled = true;
        }

        private void SetupErrorState()
        {
            HighlightBlock(CurrentBlock, RedHighlightBrush);
            nextBtn.IsEnabled = false;
        }

        private void ShowErrorMessage(String message)
        {
            if (message == null || message.Length == 0)
            {
                message = "Script contains an error on current line";            
            }

            MessageBox.Show(message, "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }

        private bool ParseBlock()
        {
            return true;
        }

        private void HighlightBlock(Block block, Brush brush)
        {
            var start = ((Paragraph)block).Inlines.FirstInline.ContentStart;
            var end = ((Paragraph)block).Inlines.FirstInline.ContentEnd;

            (new TextRange(start, end)).ApplyPropertyValue(TextElement.BackgroundProperty, brush);
        }

        private String GetBlockText(Block block)
        {
            string result = String.Join(String.Empty, ((Paragraph)block).Inlines.
                                        Select(line => line.ContentStart.GetTextInRun(LogicalDirection.Forward)));
            return result;
        }
    }
}
