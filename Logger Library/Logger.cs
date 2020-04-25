using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace LoggerLib
{
    /// <summary>
    /// Message types
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Simple message - no default prefix
        /// </summary>
        General = 0,
        /// <summary>
        /// Submessage: " |" default prefix
        /// </summary>
        GeneralSub = 1,
        /// <summary>
        /// Alert: "!" default prefix
        /// </summary>
        Alert = 2,
        /// <summary>
        /// Alert submessage: "! |" default prefix
        /// </summary>
        AlertSub = 3,
        /// <summary>
        /// High Alert: "!!" default prefix
        /// </summary>
        HighAlert = 4,
        /// <summary>
        /// High Alert submessage: "!! |" default prefix
        /// </summary>
        HighAlertSub = 5,
        /// <summary>
        /// For Maintenance messages: "~ " default prefix
        /// </summary>
        Maintenance = 6,
        /// <summary>
        /// For Maintenance submessages: "~ |" default prefix
        /// </summary>
        MaintenanceSub = 7,
    }

    /// <summary>
    /// Custom logger, allows adding Prefixes to messages according to message type
    /// <para>
    /// Use Initialize() it in the beginning of Main()
    /// </para>
    /// <para>
    /// Supports up to three total log files - latest and from two previous launches.
    /// Rotation is handled automatically.
    /// </para>
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Is console logging enabled or not
        /// </summary>
        private static bool withConsole = false;
        /// <summary>
        /// Is log path set or not
        /// </summary>
        private static bool isPathSet = false;

        /// <summary>
        /// Message queue
        /// </summary>
        private static List<string> queue = new List<string>();
        /// <summary>
        /// Message default prefix dictionary
        /// <para>
        /// Can be changed to customize prefixes
        /// </para>
        /// </summary>
        public static Dictionary<MessageType, string> Prefixes { get; private set; }
            = new Dictionary<MessageType, string>() 
            {
                { MessageType.General, "" },
                { MessageType.GeneralSub, " |" },
                { MessageType.Alert, "!" },
                { MessageType.AlertSub, "! |" },
                { MessageType.HighAlert, "!!" },
                { MessageType.HighAlertSub, "!! |" },
                { MessageType.Maintenance, "~" },
                { MessageType.MaintenanceSub, "~ |" },
            };

        //PATH AND NAME==========================================================================================
        /// <summary>
        /// Default log folder
        /// </summary>
        public const string defaultLogFolder = @".\\Logs";
        /// <summary>
        /// Log folder
        /// </summary>
        public static string LogPath { get; private set; } = "NOT SET";
        /// <summary>
        /// Log file
        /// </summary>
        public static string LogFilePath { get; private set; }


        //MAINTENANCE============================================================================================
        /// <summary>
        /// Rename old logs, create new log file
        /// </summary>
        private static void LogSetup()
        {
            if (LogPath == "NOT SET")
            {
                throw new ArgumentException("Log path is not set. Have you Initialized Logger?");
            }

            //get all filenames containing "log"
            string[] files = Directory.GetFiles(LogPath).Where<string>(file => file.Contains("log")).ToArray();
            //file rotation controller
            switch (files.Length)
            {
                case 0: break;
                case 1:
                    {
                        if (File.Exists($"{LogPath}\\log.txt"))
                        {
                            //rename log.txt to log1.txt
                            File.Move($"{LogPath}\\log.txt", $"{LogPath}\\log1.txt");
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
                        if (File.Exists($"{LogPath}\\log.txt") && File.Exists($"{LogPath}\\log1.txt"))
                        {
                            File.Move($"{LogPath}\\log1.txt", $"{LogPath}\\log2.txt");
                            File.Move($"{LogPath}\\log.txt", $"{LogPath}\\log1.txt");
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
                        if (File.Exists($"{LogPath}\\log.txt") && File.Exists($"{LogPath}\\log1.txt") && File.Exists($"{LogPath}\\log2.txt"))
                        {
                            //delete the oldest log
                            File.Delete($"{LogPath}\\log2.txt");
                            File.Move($"{LogPath}\\log1.txt", $"{LogPath}\\log2.txt");
                            File.Move($"{LogPath}\\log.txt", $"{LogPath}\\log1.txt");
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
        /// <summary>
        /// Resets the Logger. You need to Initialize() to log again
        /// </summary>
        private static void Reset()
        {
            queue.Clear();
            LogPath = "NOT SET";
            LogFilePath = null;
            withConsole = false;
        }
        /// <summary>
        /// Creates a log header
        /// </summary>
        private static void Begin()
        {
            DividerDashedLine();
            Log(new MessageType[] {
                MessageType.Maintenance,
                MessageType.MaintenanceSub,
            },
            new string[] {
                "Logger initialization complete",
                $"Console logging enabled: {withConsole}",
                $"Log path: {LogPath}",
                $"{DateTime.UtcNow.ToString(new CultureInfo("en-GB"))}"
            });
            DividerDashedLine();
        }
        /// <summary>
        /// Creates a log footer and resets Logger data
        /// <para>
        /// Intended use - at the end of Main(), to record the time of process completion. 
        /// </para>
        /// <para>
        /// Initialization() is required if logging is invoked further.
        /// </para>
        /// </summary>
        public static void End()
        {
            DividerDashedLine();
            Log(new MessageType[] {
                MessageType.Maintenance,
                MessageType.MaintenanceSub,
            },
            new string[] {
                "Log write complete",
                $"Log path: {LogPath}",
                $"{DateTime.UtcNow.ToString(new CultureInfo("en-GB"))}"
            });
            DividerDashedLine();

            Reset();
        }


        //INITIALIZATION=========================================================================================
        /// <summary>
        /// Initialization of the logger using default log folder
        /// <para>
        /// Console logging cannot be enabled
        /// </para>
        /// </summary>
        public static void Initialize()
        {
            //set log output folder
            LogPath = defaultLogFolder;
            isPathSet = true;
            //set log file name
            LogFilePath = $"{LogPath}\\log.txt";

            //Console.WriteLine($"Creating log folder: {logPath}");//DEBUG
            //Console.WriteLine($" |Log file: {logFilePath}");//DEBUG

            //create log folder
            Directory.CreateDirectory(LogPath);

            LogSetup();
            Begin();
        }
        /// <summary>
        /// Initialization of the logger using default log folder
        /// <para>
        /// Console logging can be enabled using enableConsoleLogging
        /// </para>
        /// </summary>
        /// <param name="enableConsoleLogging">Enable console logging</param>
        public static void Initialize(bool enableConsoleLogging)
        {
            //set log output folder
            LogPath = defaultLogFolder;
            isPathSet = true;
            //set log file name
            LogFilePath = $"{LogPath}\\log.txt";

            //Console.WriteLine($"Creating log folder: {logPath}");//DEBUG
            //Console.WriteLine($" |Log file: {logFilePath}");//DEBUG

            //create log folder
            Directory.CreateDirectory(LogPath);

            LogSetup();
            Begin();
        }
        /// <summary>
        /// Initialization of the logger using provided log folder
        /// <para>
        /// Console logging cannot be enabled
        /// </para>
        /// </summary>
        /// <param name="logFolder">Folder to store logs. WARNING: it will be emptied in certain conditions</param>
        public static void Initialize(string logFolder)
        {
            //set log output folder
            LogPath = logFolder;
            isPathSet = true;
            //set log file name
            LogFilePath = $"{LogPath}\\log.txt";

            //Console.WriteLine($"Creating log folder: {logPath}");//DEBUG
            //Console.WriteLine($" |Log file: {logFilePath}");//DEBUG

            //create log folder
            Directory.CreateDirectory(LogPath);

            LogSetup();
            Begin();
        }
        /// <summary>
        /// Initialization of the logger using provided log folder
        /// <para>
        /// Console logging can be enabled using enableConsoleLogging
        /// </para>
        /// </summary>
        /// <param name="logFolder">Folder to store logs. WARNING: it will be emptied in certain conditions</param>
        /// <param name="enableConsoleLogging">Enable console logging</param>
        public static void Initialize(string logFolder, bool enableConsoleLogging)
        {
            //if console logging is enabled
            if (enableConsoleLogging)
            {
                withConsole = true;
            }

            //set log output folder
            LogPath = logFolder;
            isPathSet = true;
            //set log file name
            LogFilePath = $"{LogPath}\\log.txt";

            //Console.WriteLine($"Creating log folder: {logPath}");//DEBUG
            //Console.WriteLine($" |Log file: {logFilePath}");//DEBUG

            //create log folder
            Directory.CreateDirectory(LogPath);

            LogSetup();
            Begin();
        }
        /// <summary>
        /// Initialization of the logger using default log folder and launch arguments
        /// <para>
        /// Console logging may be enabled if "-console" argument is provided
        /// </para>
        /// </summary>
        /// <param name="launch_args">Program launch arguments</param>
        public static void Initialize(string[] launch_args)
        {
            //if launch argument "-console" is provided, enable console logging
            if (launch_args.Length != 0)
            {
                if (launch_args.Contains("-console"))
                {
                    withConsole = true;
                }
            }

            //set log output folder
            LogPath = defaultLogFolder;
            isPathSet = true;
            //set log file name
            LogFilePath = $"{LogPath}\\log.txt";

            //Console.WriteLine($"Creating log folder: {logPath}");//DEBUG
            //Console.WriteLine($" |Log file: {logFilePath}");//DEBUG

            //create log folder
            Directory.CreateDirectory(LogPath);

            LogSetup();
            Begin();
        }
        /// <summary>
        /// Initialization of the logger using provided log folder and launch arguments
        /// <para>
        /// Console logging may be enabled if "-console" argument is provided
        /// </para>
        /// </summary>
        /// <param name="logFolder">Folder to store logs. WARNING: it will be emptied in certain conditions</param>
        /// <param name="launch_args">Program launch arguments</param>
        public static void Initialize(string logFolder, string[] launch_args)
        {
            //if launch argument "-console" is provided, enable console logging
            if (launch_args.Length != 0)
            {
                if (launch_args.Contains("-console"))
                {
                    withConsole = true;
                }
            }

            //set log output folder
            LogPath = logFolder;
            isPathSet = true;
            //set log file name
            LogFilePath = $"{LogPath}\\log.txt";

            //Console.WriteLine($"Creating log folder: {logPath}");//DEBUG
            //Console.WriteLine($" |Log file: {logFilePath}");//DEBUG

            //create log folder
            Directory.CreateDirectory(LogPath);

            LogSetup();
            Begin();
        }

        //TYPELESS LOGGING=======================================================================================
        /// <summary>
        /// Log a single General message
        /// </summary>
        /// <param name="message">Message</param>
        public static void Log(string message)
        {
            if (isPathSet == false)
            {
                throw new ArgumentException("Log path is not set. Have you used Initialize?");
            }

            using (StreamWriter sw = File.AppendText(LogFilePath))
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
            if (isPathSet == false)
            {
                throw new ArgumentException("Log path is not set. Have you used Initialize?");
            }

            File.AppendAllLines(LogFilePath, messages);
            if (withConsole)
            {
                foreach (string message in messages)
                {
                    Console.WriteLine(message);
                }
            }
        }


        //TYPED LOGGING==========================================================================================
        /// <summary>
        /// Log message with a specified type
        /// </summary>
        /// <param name="messageType">Message type</param>
        /// <param name="message">Message</param>
        public static void Log(MessageType messageType, string message)
        {
            if (isPathSet == false)
            {
                throw new ArgumentException("Log path is not set. Have you used Initialize?");
            }

            message = $"{Prefixes[messageType]}{message}";

            using (StreamWriter sw = File.AppendText(LogFilePath))
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
            if (isPathSet == false)
            {
                throw new ArgumentException("Log path is not set. Have you used Initialize?");
            }

            for (int i = 0; i < messages.Length; i++)
            {
                messages[i] = $"{Prefixes[messageType]}{messages[i]}";
            }
            File.AppendAllLines(LogFilePath, messages);

            if (withConsole)
            {
                foreach (string message in messages)
                {
                    Console.WriteLine(message);
                }
            }
        }
        /// <summary>
        /// Log multiple messages with multiple types
        /// <para>
        /// If messageTypes Count is less then messages Count, 
        /// the last MessageType will be used for the excessive messages
        /// </para>
        /// </summary>
        /// <param name="messageTypes">Message types</param>
        /// <param name="messages">Messages</param>
        public static void Log(MessageType[] messageTypes, string[] messages)
        {
            if (isPathSet == false)
            {
                throw new ArgumentException("Log path is not set. Have you used Initialize?");
            }

            int i;
            for (i = 0; i < messageTypes.Length; i++)
            {
                messages[i] = $"{Prefixes[messageTypes[i]]}{messages[i]}";
            }
            if (messageTypes.Count() < messages.Count())
            {
                int last = i;
                for (i = last; i < messages.Count(); i++)
                {
                    messages[i] = $"{Prefixes[messageTypes[last - 1]]}{messages[i]}";
                }
            }
            File.AppendAllLines(LogFilePath, messages);

            if (withConsole)
            {
                foreach (string message in messages)
                {
                    Console.WriteLine(message);
                }
            }
        }


        //QUEUED LOGGING=========================================================================================
        /// <summary>
        /// Add General message to the log queue
        /// </summary>
        /// <param name="message">Message</param>
        public static void QueueAdd(string message)
        {
            if (isPathSet == false)
            {
                throw new ArgumentException("Log path is not set. Have you used Initialize?");
            }

            queue.Add(message);
        }
        /// <summary>
        /// Add message with a specified type to the log queue
        /// </summary>
        /// <param name="messageType">Message type</param>
        /// <param name="message">Message</param>
        public static void QueueAdd(MessageType messageType, string message)
        {
            if (isPathSet == false)
            {
                throw new ArgumentException("Log path is not set. Have you used Initialize?");
            }

            message = $"{Prefixes[messageType]}{message}";
            queue.Add(message);
        }
        /// <summary>
        /// Logs queued messages and clears queue
        /// </summary>
        public static void QueueExecute()
        {
            File.AppendAllLines(LogFilePath, queue);
            if (withConsole)
            {
                foreach (string message in queue)
                {
                    Console.WriteLine(message);
                }
            }
            queue.Clear();
        }


        //DIVIDERS===============================================================================================
        /// <summary>
        /// Adds an empty line to the log
        /// </summary>
        public static void Divider()
        {
            using (StreamWriter sw = File.AppendText(LogFilePath))
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
        public static void DividerDashedLine()
        {
            using (StreamWriter sw = File.AppendText(LogFilePath))
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
