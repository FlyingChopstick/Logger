using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

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
        /// Maintenance message: "~ " default prefix
        /// </summary>
        Maintenance = 6,
        /// <summary>
        /// Maintenance submessage: "~ |" default prefix
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
            ReadArguments(launch_args);
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
            ReadArguments(launch_args);
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
            if (isPathSet)
            {
                try
                {
                    message = $"{Timestamp()} {Prefixes[MessageType.General]}{message}";
                    using (StreamWriter sw = File.AppendText(LogFilePath))
                    {
                        sw.WriteLine(message);
                        if (withConsole)
                        {
                            Console.ForegroundColor = Highlights[MessageType.General];
                            Console.WriteLine(message);
                        }
                    }
                }
                catch (AccessViolationException)
                {
                    DispatchError(ErrorType.PathNotAccessible);
                }
            }
            else
            {
                DispatchError(ErrorType.PathNotSet);
            }
        }
        /// <summary>
        /// Log multiple General messages
        /// </summary>
        /// <param name="messages">Messages</param>
        public static void Log(string[] messages)
        {
            if (isPathSet)
            {
                try
                {
                    int count = messages.Count();
                    for (int i = 0; i < count; i++)
                    {
                        messages[i] = $"{Timestamp()} {Prefixes[MessageType.General]}{messages[i]}";
                    }

                    File.AppendAllLines(LogFilePath, messages);
                    if (withConsole)
                    {
                        Console.ForegroundColor = Highlights[MessageType.General];
                        foreach (string message in messages)
                        {
                            Console.WriteLine(message);
                        }
                    }
                }
                catch (AccessViolationException)
                {
                    DispatchError(ErrorType.PathNotAccessible);
                }
            }
            else
            {
                DispatchError(ErrorType.PathNotSet);
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
            if (isPathSet)
            {
                try
                {
                    message = $"{Timestamp()} {Prefixes[messageType]}{message}";
                    using (StreamWriter sw = File.AppendText(LogFilePath))
                    {
                        sw.WriteLine(message);
                        if (withConsole)
                        {
                            Console.ForegroundColor = Highlights[messageType];
                            Console.WriteLine(message);
                        }
                    }
                }
                catch (AccessViolationException)
                {
                    DispatchError(ErrorType.PathNotAccessible);
                }
            }
            else
            {
                DispatchError(ErrorType.PathNotSet);
            }
        }
        /// <summary>
        /// Log multiple messages with the same type
        /// </summary>
        /// <param name="messageType">Message type</param>
        /// <param name="messages">Messages</param>
        public static void Log(MessageType messageType, string[] messages)
        {
            if (isPathSet)
            {
                try
                {
                    for (int i = 0; i < messages.Length; i++)
                    {
                        messages[i] = $"{Timestamp()} {Prefixes[messageType]}{messages[i]}";
                    }
                    File.AppendAllLines(LogFilePath, messages);

                    if (withConsole)
                    {
                        Console.ForegroundColor = Highlights[messageType];
                        foreach (string message in messages)
                        {
                            Console.WriteLine(message);
                        }
                    }
                }
                catch (AccessViolationException)
                {
                    DispatchError(ErrorType.PathNotAccessible);
                }
            }
            else
            {
                DispatchError(ErrorType.PathNotSet);
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
            if (isPathSet)
            {
                try
                {
                    int i;

                    if (withConsole)
                    {
                        for (i = 0; i < messageTypes.Length; i++)
                        {
                            messages[i] = $"{Timestamp()} {Prefixes[messageTypes[i]]}{messages[i]}";
                            Console.ForegroundColor = Highlights[messageTypes[i]];
                            Console.WriteLine(messages[i]);
                        }
                        if (messageTypes.Count() < messages.Count())
                        {
                            int last = i;
                            for (i = last; i < messages.Count(); i++)
                            {
                                messages[i] = $"{Timestamp()} {Prefixes[messageTypes[last - 1]]}{messages[i]}";

                                Console.WriteLine(messages[i]);
                            }
                        }
                    }
                    else
                    {
                        for (i = 0; i < messageTypes.Length; i++)
                        {
                            messages[i] = $"{Timestamp()} {Prefixes[messageTypes[i]]}{messages[i]}";
                        }
                        if (messageTypes.Count() < messages.Count())
                        {
                            int last = i;
                            for (i = last; i < messages.Count(); i++)
                            {
                                messages[i] = $"{Timestamp()} {Prefixes[messageTypes[last - 1]]}{messages[i]}";
                            }
                        }
                    }

                    File.AppendAllLines(LogFilePath, messages);
                }
                catch (AccessViolationException)
                {
                    DispatchError(ErrorType.PathNotAccessible);
                }
            }
            else
            {
                DispatchError(ErrorType.PathNotSet);
            }
        }

        //QUEUED LOGGING=========================================================================================
        /// <summary>
        /// Add General message to the log queue
        /// </summary>
        /// <param name="message">Message</param>
        public static void QueueAdd(string message)
        {
            if (isPathSet)
            {
                message = $"{Timestamp()} {Prefixes[MessageType.General]}{message}";
                queue.Add(message);
                if (withConsole)
                {
                    Console.ForegroundColor = Highlights[MessageType.General];
                    Console.WriteLine(message);
                }
            }
            else
            {
                DispatchError(ErrorType.PathNotSet);
            }

        }
        /// <summary>
        /// Add message with a specified type to the log queue
        /// </summary>
        /// <param name="messageType">Message type</param>
        /// <param name="message">Message</param>
        public static void QueueAdd(MessageType messageType, string message)
        {
            if (isPathSet)
            {
                message = $"{Timestamp()} {Prefixes[messageType]}{message}";
                queue.Add(message);

                if (withConsole)
                {
                    Console.ForegroundColor = Highlights[messageType];
                    Console.WriteLine(message);
                }
            }
            else
            {
                DispatchError(ErrorType.PathNotSet);
            }
        }
        /// <summary>
        /// Logs queued messages and clears queue
        /// </summary>
        public static void QueueExecute()
        {
            if (isPathSet)
            {
                try
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
                catch (AccessViolationException)
                {
                    DispatchError(ErrorType.PathNotAccessible);
                }
            }
            else
            {
                DispatchError(ErrorType.PathNotSet);
            }
        }

        /// <summary>
        /// Message queue
        /// </summary>
        private static List<string> queue = new List<string>();

        //ERROR LOGGING==========================================================================================
        /// <summary>
        /// Runs dangerous piece of code inside a try/catch and logs exception.
        /// </summary>
        /// <typeparam name="T">Dangerous function Return type</typeparam>
        /// <param name="dangerousCode">Code that can throw an exception</param>
        /// <param name="logTrace">Whether to log stack</param>
        /// <returns></returns>
        public static T Catch<T>(Func<T> dangerousCode, bool logTrace)
        {
            try
            {
                return dangerousCode.Invoke();
            }
            catch (Exception ex)
            {
                ErrorCount++;
                DividerDashedLine(ConsoleColor.Red);
                Log(MessageType.HighAlert, $"{ex.GetType().Name} occured in {ex.TargetSite.DeclaringType.FullName}");
                if (logTrace)
                {
                    Log(MessageType.HighAlertSub, "Call stack:");
                    Console.ForegroundColor = ConsoleColor.White;
                    Log(ex.StackTrace);
                }
                DividerDashedLine(ConsoleColor.Red);
                throw ex;
            }
        }
        /// <summary>
        /// Runs dangerous piece of code inside a try/catch and logs exception.
        /// </summary>
        /// <param name="dangerousCode">Code that can throw an exception</param>
        /// <param name="logTrace">Whether to log stack</param>
        /// <returns></returns>
        public static void Catch(Action dangerousCode, bool logTrace)
        {
            try
            {
                dangerousCode.Invoke();
            }
            catch (Exception ex)
            {
                ErrorCount++;
                DividerDashedLine(ConsoleColor.Red);
                Log(MessageType.HighAlert, $"{ex.GetType().Name} occured in {ex.TargetSite.DeclaringType.FullName}");
                if (logTrace)
                {
                    Log(MessageType.HighAlertSub, "Call stack:");
                    Console.ForegroundColor = ConsoleColor.White;
                    Log(ex.StackTrace);
                }
                DividerDashedLine(ConsoleColor.Red);
                throw ex;
            }
        }


        //DIVIDERS===============================================================================================
        /// <summary>
        /// Adds an empty line to the log
        /// </summary>
        public static void Divider()
        {
            if (isPathSet)
            {
                try
                {
                    using (StreamWriter sw = File.AppendText(LogFilePath))
                    {
                        sw.WriteLine();
                        if (withConsole)
                        {
                            Console.ResetColor();
                            Console.WriteLine();
                        }
                    }
                }
                catch (AccessViolationException)
                {
                    DispatchError(ErrorType.PathNotAccessible);
                }
            }
            else
            {
                DispatchError(ErrorType.PathNotSet);
            }
        }
        /// <summary>
        /// Adds a division line to the log
        /// </summary>
        public static void DividerDashedLine()
        {
            if (isPathSet)
            {
                try
                {
                    using (StreamWriter sw = File.AppendText(LogFilePath))
                    {
                        sw.WriteLine("----------------------------------------------");
                        if (withConsole)
                        {
                            Console.ResetColor();
                            Console.WriteLine("----------------------------------------------");
                        }
                    }
                }
                catch (AccessViolationException)
                {
                    DispatchError(ErrorType.PathNotAccessible);
                }
            }
        }
        /// <summary>
        /// Adds a division line to the log
        /// </summary>
        /// <param name="color">Line color in console</param>
        public static void DividerDashedLine(ConsoleColor color)
        {
            Console.ForegroundColor = color;
            if (isPathSet)
            {
                try
                {
                    using (StreamWriter sw = File.AppendText(LogFilePath))
                    {
                        sw.WriteLine("----------------------------------------------");
                        if (withConsole)
                        {
                            Console.WriteLine("----------------------------------------------");
                            Console.ResetColor();
                        }
                    }
                }
                catch (AccessViolationException)
                {
                    DispatchError(ErrorType.PathNotAccessible);
                }
            }
        }

        //STATES AND PATHS=======================================================================================
        /// <summary>
        /// Default log folder
        /// </summary>
        private const string defaultLogFolder = @".\\Logs";
        /// <summary>
        /// Log folder
        /// </summary>
        public static string LogPath { get; private set; } = "NOT SET";
        /// <summary>
        /// Log file
        /// </summary>
        public static string LogFilePath { get; private set; } = "NOT SET";

        /// <summary>
        /// Is log path set or not
        /// </summary>
        private static bool isPathSet = false;

        /// <summary>
        /// Is console logging enabled or not
        /// </summary>
        private static bool withConsole = false;
        /// <summary>
        /// Should library throw error or relay it to the event
        /// </summary>
        private static bool throwError = false;


        //EVENTS=================================================================================================
        /// <summary>
        /// Delegate for logger errors
        /// </summary>
        /// <param name="errorType">Type of the error</param>
        /// <param name="errorMessage">Error message</param>
        public delegate void LogError(ErrorType errorType, string errorMessage);
        /// <summary>
        /// Invoked on logger error, use to relay error information to your error handler
        /// </summary>
        public static event LogError OnLogError;


        //ENUMS==================================================================================================
        /// <summary>
        /// Types of console arguments
        /// </summary>
        internal enum ArgumentType
        {
            /// <summary>
            /// Console logging
            /// </summary>
            ConsoleLogging = 0,
            /// <summary>
            /// Should library throw error or relay it
            /// </summary>
            ThrowError = 1,
        }
        /// <summary>
        /// Logger error types
        /// </summary>
        public enum ErrorType
        {
            /// <summary>
            /// Log directory path is not set
            /// </summary>
            PathNotSet = 0,
            /// <summary>
            /// Log directory is not accessible
            /// </summary>
            PathNotAccessible = 1,
        }


        //DICTIONARIES===========================================================================================
        /// <summary>
        /// Message default prefix dictionary
        /// <para>
        /// Can be changed to customize prefixes
        /// </para>
        /// </summary>
        internal static readonly Dictionary<MessageType, string> Prefixes
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
        /// <summary>
        /// Console output text highlight
        /// </summary>
        internal static readonly Dictionary<MessageType, ConsoleColor> Highlights
            = new Dictionary<MessageType, ConsoleColor>()
            {
                { MessageType.General, ConsoleColor.White },
                { MessageType.GeneralSub, ConsoleColor.White },
                { MessageType.Alert, ConsoleColor.Yellow },
                { MessageType.AlertSub, ConsoleColor.Yellow},
                { MessageType.HighAlert, ConsoleColor.Red },
                { MessageType.HighAlertSub, ConsoleColor.Red },
                { MessageType.Maintenance, ConsoleColor.DarkGray },
                { MessageType.MaintenanceSub, ConsoleColor.DarkGray },
            };

        /// <summary>
        /// Console arguments
        /// </summary>
        internal static readonly Dictionary<ArgumentType, string> Arguments
            = new Dictionary<ArgumentType, string>()
            {
                { ArgumentType.ConsoleLogging, "-ToHconsole" },
                { ArgumentType.ThrowError, "-ToHerror" },
            };

        /// <summary>
        /// Error messages
        /// </summary>
        internal static readonly Dictionary<ErrorType, string> ErrorMessages
            = new Dictionary<ErrorType, string>()
            {
                { ErrorType.PathNotSet, "Log path is not set. Have you used Initialize?" },
                { ErrorType.PathNotAccessible, "Cannot access log directory." },
            };


        //MISCELLANEOUS==========================================================================================
        /// <summary>
        /// Reads launch arguments and enables required parametres
        /// </summary>
        /// <param name="launch_args"></param>
        private static void ReadArguments(string[] launch_args)
        { 
            if (launch_args.Length != 0)
            {
                //if launch argument for console logging is provided, enable console logging
                if (launch_args.Contains(Arguments[ArgumentType.ConsoleLogging]))
                {
                    withConsole = true;
                }
                //if launch argument for throwing error is provided, library will throw errors instead of relaying
                if (launch_args.Contains(Arguments[ArgumentType.ThrowError]))
                {
                    throwError = true;
                }
            }
        }

        /// <summary>
        /// Rename old logs, create new log file
        /// </summary>
        private static void LogSetup()
        {
            if (LogPath == "NOT SET")
            {
                DispatchError(ErrorType.PathNotSet);
            }

            try
            {
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
            catch (AccessViolationException)
            {
                DispatchError(ErrorType.PathNotAccessible);
            }
        }
        /// <summary>
        /// Resets the Logger. You need to Initialize() to log again
        /// </summary>
        private static void Reset()
        {
            Console.ResetColor();
            queue.Clear();
            LogPath = "NOT SET";
            LogFilePath = "NOT SET";
            isPathSet = false;
            withConsole = false;
            ErrorCount = 0;
        }
        /// <summary>
        /// Creates a log header
        /// </summary>
        private static void Begin()
        {
            DividerDashedLine();

            //header messages
            string[] header = new string[]
            {
                $"{Prefixes[MessageType.Maintenance]}Logger initialization complete",
                $"{Prefixes[MessageType.MaintenanceSub]}Console logging enabled: {withConsole}",
                $"{Prefixes[MessageType.MaintenanceSub]}Log path: {LogFilePath}",
                $"{Prefixes[MessageType.MaintenanceSub]}{DateTime.UtcNow.ToString(new CultureInfo("en-GB"))}"
            };
            File.AppendAllLines(LogFilePath, header);

            //print to console with the proper highlight
            if (withConsole)
            {
                int i = 1;
                int count = header.Count();

                Console.ForegroundColor = Highlights[MessageType.Maintenance];
                Console.WriteLine(header[0]);
                Console.ForegroundColor = Highlights[MessageType.MaintenanceSub];
                while (i < count)
                {
                    Console.WriteLine(header[i]);
                    i++;
                }
            }

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
            string[] footer;

            //footer messages
            if (ErrorCount > 0)
            {
                footer = new string[]
                {
                $"{Prefixes[MessageType.Maintenance]}Log write complete",
                $"{Prefixes[MessageType.MaintenanceSub]}Errors encountered: {ErrorCount}",
                $"{Prefixes[MessageType.MaintenanceSub]}Log path: {LogFilePath}",
                $"{Prefixes[MessageType.MaintenanceSub]}{DateTime.UtcNow.ToString(new CultureInfo("en-GB"))}"
                };
            }
            else
            {
                footer = new string[]
                {
                $"{Prefixes[MessageType.Maintenance]}Log write complete",
                $"{Prefixes[MessageType.MaintenanceSub]}Log path: {LogFilePath}",
                $"{Prefixes[MessageType.MaintenanceSub]}{DateTime.UtcNow.ToString(new CultureInfo("en-GB"))}"
                };
            }
            File.AppendAllLines(LogFilePath, footer);

            //print to console with the proper highlight
            if (withConsole)
            {
                int i = 1;
                int count = footer.Count();

                Console.ForegroundColor = Highlights[MessageType.Maintenance];
                Console.WriteLine(footer[0]);
                Console.ForegroundColor = Highlights[MessageType.MaintenanceSub];
                while (i < count)
                {
                    Console.WriteLine(footer[i]);
                    i++;
                }
            }

            DividerDashedLine();

            Reset();
        }
        /// <summary>
        /// Provides current time
        /// </summary>
        /// <returns>Current time in HH:mm:ss</returns>
        private static string Timestamp()
        {
            return $"[{DateTime.UtcNow.ToString("HH:mm:ss")}]";
        }

        /// <summary>
        /// Invokes onLogError or throws exception if throwError is enabled
        /// </summary>
        /// <param name="errorType"></param>
        private static void DispatchError(ErrorType errorType)
        {
            if (throwError)
            {
                switch (errorType)
                {
                    case ErrorType.PathNotSet:
                        {
                            throw new ArgumentException(ErrorMessages[ErrorType.PathNotSet]);
                        }
                    case ErrorType.PathNotAccessible:
                        {
                            throw new AccessViolationException(ErrorMessages[ErrorType.PathNotAccessible]);
                        }
                    default: break;
                }
            }
            OnLogError?.Invoke(errorType, ErrorMessages[errorType]);
        }


        //STATS==================================================================================================
        /// <summary>
        /// Number of logged errors 
        /// </summary>
        public static int ErrorCount { get; private set; } = 0
    }
}