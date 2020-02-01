using MhwOverlay.Log;
using System;
using System.Collections.Generic;
using System.IO;

namespace MhwOverlay.Config
{
    public static class AppConfig
    {
        private class ConfigKey
        {
            public const string LogType = "LogType";
            public const string MaxLogLines = "MaxLogLines";
            public const string LogFolder = "LogFolder";
        }
        private static Dictionary<string, string> ConfigDict = new Dictionary<string, string>();
        public static bool Load()
        {
            try
            {
                using (StreamReader file = new StreamReader("Config.txt"))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (!line.StartsWith("#"))
                        {
                            var pair = line.Split('=');
                            if (pair.Length == 2)
                                ConfigDict.Add(pair[0].Trim(), pair[1].Trim());
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static LogType LogType { get { try { return GetEnum<LogType>(ConfigKey.LogType); } catch { return LogType.Console; } } }
        public static int MaxLogLines { get { return GetInt(ConfigKey.MaxLogLines); } }
        public static string LogFolder { get { return GetString(ConfigKey.LogFolder); } }

        public static string ConfigToString()
        {
            var result = string.Empty;
            foreach (var config in ConfigDict)
            {
                result += $"{config.Key}={config.Value}\n";
            }
            return result;
        }

        public static void SetConfig(string configKey, string value)
        {
            if (ConfigDict.ContainsKey(configKey))
            {
                ConfigDict[configKey] = value;
            }
        }

        private static int GetInt(string configKey)
        {
            if (ConfigDict.ContainsKey(configKey))
                return Convert.ToInt32(ConfigDict[configKey]);
            else
                return 0;
        }
        private static string GetString(string configKey)
        {
            if (ConfigDict.ContainsKey(configKey))
                return ConfigDict[configKey];
            else
                return string.Empty;
        }

        private static T GetEnum<T>(string configKey)
        {
            return (T)Enum.Parse(typeof(T), ConfigDict[configKey]);
        }

    }
}
