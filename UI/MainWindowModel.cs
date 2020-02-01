using System;
using System.Windows.Input;
using System.Windows.Media;
using MhwOverlay;
using MhwOverlay.Console;

namespace MhwOverlay.UI
{
    public class MainWindowModel : Bindable
    {
        private CommandCenter commandCenter;
        private MainWindow mainWindow;
        private int commandOffset = 0;

        public MainWindowModel(MainWindow window)
        {
            commandCenter = new CommandCenter(this);
            mainWindow = window;
            CommandInputText=string.Empty;
        }

        private string consoleText;
        public string ConsoleText
        {
            get { return consoleText; }
            set
            {
                SetProperty(ref consoleText, value);
            }
        }

        private string commandInputText;
        public string CommandInputText
        {
            get { return commandInputText; }
            set
            {
                SetProperty(ref commandInputText, value);
            }
        }

        public void Execute()
        {
            commandCenter.Execute(CommandInputText);
            commandOffset = 0;
            CommandInputText = "";
        }

        public void HandleCommandInputKeys(Key key)
        {
            if (key == Key.Up)
            {
                commandOffset++;
                CommandInputText = commandCenter.LastCommand(ref commandOffset);
            }
            else if (key == Key.Down)
            {
                commandOffset--;
                CommandInputText = commandCenter.LastCommand(ref commandOffset);
            }
        }

        public void AppendConsole(string message)
        {
            ConsoleText += message;
            ConsoleText += Environment.NewLine;
        }

        public void AppendInfo(string message)
        {
            mainWindow.AppendLog(message, Brushes.Black);
        }
        public void AppendWarn(string message)
        {
            mainWindow.AppendLog(message, Brushes.DarkOrange);
        }
        public void AppendError(string message)
        {
            mainWindow.AppendLog(message, Brushes.Red);
        }
    }
}
