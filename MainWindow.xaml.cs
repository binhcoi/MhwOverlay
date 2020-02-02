using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using MhwOverlay.UI;
using MhwOverlay.Config;

namespace MhwOverlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowModel model;
        public MainWindow()
        {
            InitializeComponent();
        }

        public void SetDataContext(MainWindowModel model)
        {
            this.model = model;
            DataContext = model;
        }

        public void AppendLog(string message, SolidColorBrush color)
        {
            this.Dispatcher.Invoke(() =>
            {
                var run = new Run(message);
                run.Foreground = color;
                LogsTextBox.Document.Blocks.Add(new Paragraph(run));
                TrimLogs();
            });
        }

        private void TrimLogs()
        {
            var list = LogsTextBox.Document.Blocks;
            while (list.Count > AppConfig.MaxLogLines)
            {
                list.Remove(list.First());
            }
        }

        private void CommandInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                model.Execute();
            }
            else
            {
                model.HandleCommandInputKeys(e.Key);
            }
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            model.Execute();
        }

        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LogsTextBox.ScrollToEnd();

        }

        private void ConsoleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConsoleTextBox.ScrollToEnd();
        }
    }
}
