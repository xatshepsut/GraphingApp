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
using System.Text.RegularExpressions;

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

        public List<int> NodeList { get; set; }

        private Brush ClearHighlightBrush { get; set; } 
        private Brush GreenHighlightBrush { get; set; }
        private Brush RedHighlightBrush { get; set; }

        private Block CurrentBlock { get; set; }
        private int CurrentBlockIndex { get; set; }
        private int BlockCount { get; set; }
        
        public SimulateTerminalWindow()
        {
            InitializeComponent();

            ClearHighlightBrush = new SolidColorBrush(Colors.Transparent);
            GreenHighlightBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#80FF66"));
            RedHighlightBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6666"));

            nextBtn.IsEnabled = false;
            stopBtn.IsEnabled = false;
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            richTextBox.IsEnabled = false;
            startBtn.IsEnabled = false;
            nextBtn.IsEnabled = true;
            stopBtn.IsEnabled = true;

            BlockCount = richTextBox.Document.Blocks.Count();
            if (BlockCount <= 1)
            {
                nextBtn.IsEnabled = false;
                return;
            } 
            else if (BlockCount == 2)
            {
                nextBtn.IsEnabled = false;
            }

            CurrentBlockIndex = 0;

            {
                var enumerator = richTextBox.Document.Blocks.AsEnumerable().GetEnumerator();
                enumerator.MoveNext();
                CurrentBlock = enumerator.Current;
            }

            String blockText = GetBlockText(CurrentBlock);
            String regex = "^0: server <- ([0-9]+)$";

            int serverId = -1;
            bool error = false;
            String errorMessage = "";

            Match match = Regex.Match(blockText, regex);
            if (match.Success && match.Groups.Count > 1)
            {
                Int32.TryParse(match.Groups[1].Value, out serverId);
                if (!NodeList.Contains(serverId))
                {
                    errorMessage = String.Format("Node with id \"{0}\" does not exist", serverId);
                    error = true;
                }
            }
            else
            {
                error = true;
            }

            if (error)
            {
                SetupErrorState();
                ShowErrorMessage(errorMessage);
                return;
            }

            HighlightBlock(CurrentBlock, GreenHighlightBrush);

            MarkServerEventArgs args = new MarkServerEventArgs(serverId);
            MarkServer(this, args);
        }

        private void next_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentBlockIndex < BlockCount - 2)
            {
                // CurrentBlock is current from previous iteration
                HighlightBlock(CurrentBlock, ClearHighlightBrush);
               
                CurrentBlock = CurrentBlock.NextBlock;
                CurrentBlockIndex++;  // time slot


                List<Tuple<int, int>> tuples = new List<Tuple<int, int>>();

                String blockText = GetBlockText(CurrentBlock);
                String regex = "^([0-9]+):( [0-9]+ -> [0-9]+,)* [0-9]+ -> [0-9]+$";
                String tupleRegexString = " ?([0-9]+) -> ([0-9]+),?";

                bool success = true;
                String errorMessage = "";

                Match match = Regex.Match(blockText, regex);
                if (match.Success && match.Groups.Count > 1)
                {
                    bool timeMatched = false;
                    int time;
                    Int32.TryParse(match.Groups[1].Value, out time);
                    if (time == CurrentBlockIndex)
                    {
                        timeMatched = true;
                    }

                    if (timeMatched == true)
                    {
                        Regex tupleRegex = new Regex(tupleRegexString, RegexOptions.Compiled);
                        foreach (Match tupleMatch in tupleRegex.Matches(blockText))
                        {
                            if (tupleMatch.Success && tupleMatch.Groups.Count >= 2)
                            {
                                int value1, value2;
                                Int32.TryParse(tupleMatch.Groups[1].Value, out value1);
                                Int32.TryParse(tupleMatch.Groups[2].Value, out value2);

                                int missingValue = -1;
                                if (!NodeList.Contains(value1))
                                {
                                    missingValue = value1;
                                }
                                else if (!NodeList.Contains(value2))
                                {
                                    missingValue = value2;
                                }

                                if (missingValue == -1)
                                {
                                    tuples.Add(new Tuple<int, int>(value1, value2));
                                }
                                else
                                {
                                    success = false;
                                    errorMessage = String.Format("Node with id \"{0}\" does not exist", missingValue);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        success = false;
                        errorMessage = String.Format("Failed to parse time id \"{0}\".\nTime slots should be numbered sequentialy starting from 0.", time);   
                    }
                }
                else
                {
                    success = false;
                }

                if (!success)
                {
                    SetupErrorState();
                    ShowErrorMessage(errorMessage);
                    return;
                }

                HighlightBlock(CurrentBlock, GreenHighlightBrush);

                ExecuteEventArgs args = new ExecuteEventArgs(tuples);
                Execute(this, args);
            }
            
            if (CurrentBlockIndex == BlockCount - 2)
            {
                nextBtn.IsEnabled = false;
            }
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            ResetSimulation();
            ExecutionEnded(this, EventArgs.Empty);
        }

        private void ResetSimulation()
        {
            if (CurrentBlock != null)
            {
                HighlightBlock(CurrentBlock, ClearHighlightBrush);
            }

            CurrentBlock = null;
            CurrentBlockIndex = 0;
            BlockCount = 0;

            richTextBox.IsEnabled = true;
            startBtn.IsEnabled = true;
            nextBtn.IsEnabled = false;
            stopBtn.IsEnabled = false;
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
