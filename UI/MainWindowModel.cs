using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        public ObservableCollection<MonsterData> MonstersList
        {
            get; private set;
        }
        public MainWindowModel(MainWindow window)
        {
            commandCenter = new CommandCenter(this);
            mainWindow = window;
            CommandInputText = string.Empty;
            MonstersList = new ObservableCollection<MonsterData>();
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

        public void AddOrUpdateMonster(MonsterData monster)
        {
            mainWindow.Dispatcher.Invoke(() =>
            {
                var existingMonster = MonstersList.FirstOrDefault(existing => existing.Address == monster.Address);
                if (existingMonster != null)
                {
                    existingMonster.Name = monster.Name;
                    existingMonster.HP = monster.HP;
                    existingMonster.MaxHP = monster.MaxHP;
                }
                else
                    MonstersList.Add(monster);
            });
        }

        public void CleanMonsterList(List<MonsterData> list)
        {
            mainWindow.Dispatcher.Invoke(() =>
            {
                var exclude = MonstersList.Except(list);
                foreach (var monster in exclude.Reverse())
                {
                    MonstersList.Remove(monster);
                }
            });
        }
    }
}
