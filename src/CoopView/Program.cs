using System;

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
        public static void Main(string[] args)
        {
            switch (GetArgument(args, 0))
            {
                case "server":
                case "s":
                    break;
                case "client":
                case "c":
                    try
                    {
                        Client.Create(
                            GetArgument(args, 1),
                            GetArgument(args, 2),
                            GetArgument(args, 3),
                            GetArgument(args, 4),
                            GetArgument(args, 5),
                            GetArgument(args, 6)).Start();
                    }
                    catch
                    {
                        UsageErrorExit();
                    }

                    break;
                default:
                    UsageErrorExit();
                    break;
            }

            Console.WriteLine("Press any key to exit...");
            Console.Read();
        }

        private static void UsageErrorExit()
                => ErrorExit("Usage:" + Environment.NewLine +
                        "Client mode: client <ip> <port> <window name> <width> <height> <fps>" + Environment.NewLine +
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
