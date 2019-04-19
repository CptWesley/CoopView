using System;
using System.Windows.Forms;

namespace CoopView
{
    /// <summary>
    /// Entry point of the program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">Program arguments.</param>
        [STAThread]
        public static void Main(string[] args)
        {
            switch (GetArgument(args, 0))
            {
                case "server":
                case "s":
                    Application.EnableVisualStyles();
                    Application.Run(Server.Create(GetArgument(args, 1)));
                    break;
                case "client":
                case "c":
                    Client.Create(
                            GetArgument(args, 1),
                            GetArgument(args, 2),
                            GetArgument(args, 3),
                            GetArgument(args, 4),
                            GetArgument(args, 5)).Start();
                    break;
                default:
                    UsageErrorExit();
                    break;
            }
        }

        private static void UsageErrorExit()
                => ErrorExit("Usage:" + Environment.NewLine +
                        "Client mode: client <process> <ip> <port> <scale> <fps>" + Environment.NewLine +
                        "Server mode: host <port>");

        /// <summary>
        /// Exits the program with the given error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        private static void ErrorExit(string message)
        {
            Console.Error.WriteLine(message);
            int hash = message.GetHashCode();
            if (hash == 0)
            {
                hash++;
            }

            Environment.Exit(hash);
        }

        /// <summary>
        /// Gets the argument at the index.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="index">The index.</param>
        /// <returns>The argument at the index, null otherwise.</returns>
        private static string GetArgument(string[] args, int index)
            => index < args.Length ? args[index] : null;
    }
}
