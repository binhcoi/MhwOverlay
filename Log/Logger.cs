using MhwOverlay.UI;
using MhwOverlay.Config;
using System;
using System.Collections.Generic;
using System.IO;

namespace MhwOverlay.Log
{
    public enum LogLevel
    {
        Info,
        Warn,
        Error
    }

    public enum LogType
    {
        Console,
        File,
        Both
    }
    public class Logger
    {
        public static MainWindowModel MainModel;
        public static string SessionFileName;
        private static Queue<Tuple<LogLevel,string>> logsBuffer = new Queue<Tuple<LogLevel, string>>();
        public static void Log(LogLevel level, string message)
        {
            switch (AppConfig.LogType)
            {
                case LogType.Console:
                    WriteToConsole(level, message);
                    break;
                case LogType.File:
                    WriteToFile(level, message);
                    break;
                case LogType.Both:
                    WriteToConsole(level, message);
                    WriteToFile(level, message);
                    break;
                default:
                    WriteToConsole(LogLevel.Error, "Invalid log type. (\"Console\" or \"File\" only)");
                    break;
            }
        }

        public static void WriteToConsole(LogLevel level, string message)
        {
            if (MainModel == null){
                logsBuffer.Enqueue(new Tuple<LogLevel, string>(level,message));
                return;
            }
            while (logsBuffer.Count>0){
                var log = logsBuffer.Dequeue();
                WriteToConsole(log.Item1,log.Item2);
            }
            var dateTimeString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var logMessage = $"[{dateTimeString} | {level.ToString().ToUpper()}] {message}";
            switch (level)
            {
                case LogLevel.Info:
                    MainModel.AppendInfo(logMessage);
                    break;
                case LogLevel.Warn:
                    MainModel.AppendWarn(logMessage);
                    break;
                case LogLevel.Error:
                    MainModel.AppendError(logMessage);
                    break;
            }
        }

        public static void WriteToFile(LogLevel level, string message)
        {
            Directory.CreateDirectory(AppConfig.LogFolder);
            using (var file = File.AppendText(AppConfig.LogFolder + SessionFileName))
            {
                var dateTimeString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var logMessage = $"[{dateTimeString} | {level.ToString().ToUpper()}] {message}";
                file.WriteLine(logMessage);
            }
        }
    }
}
