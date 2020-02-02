using MhwOverlay.UI;
using MhwOverlay.Config;
using MhwOverlay.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MhwOverlay.Console
{
    public class CommandCenter
    {
        private MainWindowModel mainModel;
        private List<string> commandList;
        private Dictionary<string, Func<string[], string>> commands = new Dictionary<string, Func<string[], string>>();

        public CommandCenter(MainWindowModel mainModel)
        {
            commandList = new List<string>();
            this.mainModel = mainModel;
            commands.Add("config", args => AppConfig.ConfigToString());
            commands.Add("set-config", args =>
            {
                AppConfig.SetConfig(args[1], args[2]);
                return $"New config sets";
            });
            commands.Add("log-error", args =>
            {
                Logger.Log(LogLevel.Error, args[1]);
                return $"log";
            });
            commands.Add("log-warn", args =>
            {
                Logger.Log(LogLevel.Warn, args[1]);
                return $"log";
            });
            commands.Add("log-info", args =>
            {
                Logger.Log(LogLevel.Info, args[1]);
                return $"log";
            });
            commands.Add("logboth", args =>
            {
                AppConfig.SetConfig("LogType", "Both");
                return "Log to both console and file";
            });
        }

        public void Execute(string command)
        {
            mainModel.AppendConsole(">" + command);
            commandList.Add(command);
            var args = Regex.Matches(command, @"[\""].+?[\""]|[^ ]+")
                .Cast<Match>()
                .Select(m => m.Value)
                .ToList();
            if (args.Count > 0 && commands.ContainsKey(args[0].ToLower()))
            {
                try
                {
                    mainModel.AppendConsole(commands[args[0]].Invoke(args.ToArray()));
                }
                catch
                {
                    mainModel.AppendConsole("Exception returns for command");
                }
            }
            else
            {
                mainModel.AppendConsole("Invalid command");
            }
        }

        public string LastCommand(ref int offset)
        {
            if (commandList.Count == 0 || offset <= 0)
            {
                offset = 0;
                return string.Empty;
            }
            if (offset > commandList.Count)
            {
                offset = commandList.Count;
                return commandList[0];
            }
            return commandList[commandList.Count - offset];
        }
    }
}
