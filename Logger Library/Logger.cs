using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;

namespace LoggerLib
{
    /// <summary>
    /// Message types
    /// </summary>
    public enum MessageType
    {
        General = 0,
        GeneralSub = 1,
        Alert = 2,
        AlertSub = 3,
        HighAlert = 4,
        HighAlertSub = 5,
    }
    public class Logger
    {
        /// <summary>
        /// Paths section from App.Config
        /// </summary>
        private static NameValueCollection filestreamPaths;
        /// <summary>
        /// Log folder
        /// </summary>
        private static string logPath;
        /// <summary>
        /// Log file
        /// </summary>
        private static string logFilePath;

        /// <summary>
        /// Message queue
        /// </summary>
        private static List<string> queue = new List<string>();

        private static bool withConsole = false;

        /// <summary>
        /// Initialization of the logger, creating new file
        /// </summary>
        public static void Initialize(string[] launch_args)
        {
            filestreamPaths = ConfigurationManager.GetSection("filestreamPaths") as NameValueCollection;
            if (filestreamPaths != null)
            {
                //log folder path
                logPath = filestreamPaths["DebugLog"];
                //log file path
                logFilePath = $"{logPath}\\log.txt";
                //Console.WriteLine($"Creating log folder: {logPath}");//DEBUG
                //Console.WriteLine($" |Log file: {logFilePath}");//DEBUG
                Directory.CreateDirectory(logPath);

                //get all filenames containing "log"
                string[] files = Directory.GetFiles(logPath).Where<string>(file => file.Contains("log")).ToArray();
                //file rotation controller
                switch (files.Length)
                {
                    case 0: break;
                    case 1:
                        {
                            if (File.Exists($"{logPath}\\log.txt"))
                            {
                                File.Move($"{logPath}\\log.txt", $"{logPath}\\log1.txt");
                            }
                            else
                            {
                                //clears the directory
                                foreach (string file in files)
                                {
                                    File.Delete(file);
                                }
                            }
                            break;
                        }
                    case 2:
                        {
                            if (File.Exists($"{logPath}\\log.txt") && File.Exists($"{logPath}\\log1.txt"))
                            {
                                File.Move($"{logPath}\\log1.txt", $"{logPath}\\log2.txt");
                                File.Move($"{logPath}\\log.txt", $"{logPath}\\log1.txt");
                            }
                            else
                            {
                                foreach (string file in files)
                                {
                                    File.Delete(file);
                                }
                            }
                            break;
                        }
                    case 3:
                        {
                            if (File.Exists($"{logPath}\\log.txt") && File.Exists($"{logPath}\\log1.txt") && File.Exists($"{logPath}\\log2.txt"))
                            {
                                File.Delete($"{logPath}\\log2.txt");
                                File.Move($"{logPath}\\log1.txt", $"{logPath}\\log2.txt");
                                File.Move($"{logPath}\\log.txt", $"{logPath}\\log1.txt");
                            }
                            else
                            {
                                foreach (string file in files)
                                {
                                    File.Delete(file);
                                }
                            }
                            break;
                        }
                }
            }


            if (launch_args.Length != 0)
            {
                if (launch_args.Contains("-console"))
                {
                    withConsole = true;
                    Log(MessageType.Alert, "Console logging enabled.");
                }
            }
        }

        /// <summary>
        /// Log a single General message
        /// </summary>
        /// <param name="message">Message</param>
        public static void Log(string message)
        {
            using (StreamWriter sw = File.AppendText(logFilePath))
            {
                sw.WriteLine(message);
                if (withConsole)
                {
                    Console.WriteLine(message);
                }
            }
        }
        /// <summary>
        /// Log multiple General messages
        /// </summary>
        /// <param name="messages">Messages</param>
        public static void Log(string[] messages)
        {
            File.AppendAllLines(logFilePath, messages);
            if (withConsole)
            {
                foreach (string message in messages)
                {
                    Console.WriteLine(message);
                }
            }
        }

        /// <summary>
        /// Log message with a specified type
        /// </summary>
        /// <param name="messageType">Message type</param>
        /// <param name="message">Message</param>
        public static void Log(MessageType messageType, string message)
        {
            switch (messageType)
            {
                case MessageType.GeneralSub:
                    {
                        message = $" |{message}";
                        break;
                    }
                case MessageType.Alert:
                    {
                        message = $"!{message}";
                        break;
                    }
                case MessageType.AlertSub:
                    {
                        message = $"!-{message}";
                        break;
                    }
                case MessageType.HighAlert:
                    {
                        message = $"!!{message}";
                        break;
                    }
                case MessageType.HighAlertSub:
                    {
                        message = $"!!-{message}";
                        break;
                    }
                default: break;
            }

            using (StreamWriter sw = File.AppendText(logFilePath))
            {
                sw.WriteLine(message);
                if (withConsole)
                {
                    Console.WriteLine(message);
                }
            }
        }
        /// <summary>
        /// Log multiple messages with the same type
        /// </summary>
        /// <param name="messageType">Message type</param>
        /// <param name="messages">Messages</param>
        public static void Log(MessageType messageType, string[] messages)
        {
            switch (messageType)
            {
                case MessageType.GeneralSub:
                    {
                        {
                            for (int i = 0; i < messages.Length; i++)
                            {
                                messages[i] = $" |{messages[i]}";
                            }
                        }
                        break;
                    }
                case MessageType.Alert:
                    {
                        {
                            for (int i = 0; i < messages.Length; i++)
                            {
                                messages[i] = $"!{messages[i]}";
                            }
                        }
                        break;
                    }
                case MessageType.AlertSub:
                    {
                        {
                            for (int i = 0; i < messages.Length; i++)
                            {
                                messages[i] = $"!-{messages[i]}";
                            }
                        }
                        break;
                    }
                case MessageType.HighAlert:
                    {
                        for (int i = 0; i < messages.Length; i++)
                        {
                            messages[i] = $"!!{messages[i]}";
                        }
                        break;
                    }
                case MessageType.HighAlertSub:
                    {
                        for (int i = 0; i < messages.Length; i++)
                        {
                            messages[i] = $"!!-{messages[i]}";
                        }
                        break;
                    }
                default: break;
            }
            File.AppendAllLines(logFilePath, messages);

            if (withConsole)
            {
                foreach (string message in messages)
                {
                    Console.WriteLine(message);
                }
            }
        }
        /// <summary>
        /// Log multiple messages with multiple types (type[i] -> message[i])
        /// </summary>
        /// <param name="messageTypes">Message types</param>
        /// <param name="messages">Messages</param>
        public static void Log(MessageType[] messageTypes, string[] messages)
        {
            for (int i = 0; i < messageTypes.Length; i++)
            {
                switch (messageTypes[i])
                {
                    case MessageType.GeneralSub:
                        {
                            messages[i] = $" |{messages[i]}";
                            break;
                        }
                    case MessageType.Alert:
                        {
                            messages[i] = $"!{messages[i]}";
                            break;
                        }
                    case MessageType.AlertSub:
                        {
                            messages[i] = $"!-{messages[i]}";
                            break;
                        }
                    case MessageType.HighAlert:
                        {
                            messages[i] = $"!!{messages[i]}";
                            break;
                        }
                    case MessageType.HighAlertSub:
                        {
                            messages[i] = $"!!-{messages[i]}";
                            break;
                        }
                    default: break;
                }
            }
            File.AppendAllLines(logFilePath, messages);

            if (withConsole)
            {
                foreach (string message in messages)
                {
                    Console.WriteLine(message);
                }
            }
        }

        /// <summary>
        /// Add General message to the log queue
        /// </summary>
        /// <param name="message">Message</param>
        public static void LogQueueAdd(string message)
        {
            queue.Add(message);
        }
        /// <summary>
        /// Add message with a specified type to the log queue
        /// </summary>
        /// <param name="messageType">Message type</param>
        /// <param name="message">Message</param>
        public static void LogQueueAdd(MessageType messageType, string message)
        {
            switch (messageType)
            {
                case MessageType.GeneralSub:
                    {
                        message = $" |{message}";
                        break;
                    }
                case MessageType.Alert:
                    {
                        message = $"!{message}";
                        break;
                    }
                case MessageType.AlertSub:
                    {
                        message = $"!-{message}";
                        break;
                    }
                case MessageType.HighAlert:
                    {
                        message = $"!!{message}";
                        break;
                    }
                case MessageType.HighAlertSub:
                    {
                        message = $"!!-{message}";
                        break;
                    }
                default: break;
            }

            queue.Add(message);
        }
        /// <summary>
        /// Logs queued messages and clears queue
        /// </summary>
        public static void LogQueueExecute()
        {
            File.AppendAllLines(logFilePath, queue);
            if (withConsole)
            {
                foreach (string message in queue)
                {
                    Console.WriteLine(message);
                }
            }
            queue.Clear();
        }
        /// <summary>
        /// Adds an empty line to the log
        /// </summary>
        public static void LogDivide()
        {
            using (StreamWriter sw = File.AppendText(logFilePath))
            {
                sw.WriteLine();
                if (withConsole)
                {
                    Console.WriteLine();
                }
            }
        }
        /// <summary>
        /// Adds a division line to the log
        /// </summary>
        public static void LogDivideLine()
        {
            using (StreamWriter sw = File.AppendText(logFilePath))
            {
                sw.WriteLine("----------------------------------------------");
                if (withConsole)
                {
                    Console.WriteLine("----------------------------------------------");
                }
            }
        }
    }
}
