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
                case "host":
                case "h":
                    break;
                case "client":
                case "c":
                    break;
                default:
                    ErrorExit("Usage:" + Environment.NewLine +
                        "Client mode: client <ip> <port> <window name> <width> <height> <fps>" + Environment.NewLine +
                        "Host mode: host <port>");
                    break;
            }
        }

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
