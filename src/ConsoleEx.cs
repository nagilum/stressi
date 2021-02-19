using System;
using System.Collections.Generic;
using System.Linq;

namespace Stressi
{
    public class ConsoleEx
    {
        #region Properties

        /// <summary>
        /// Stored command-line arguments.
        /// </summary>
        public static string[] CmdArgs { get; set; }

        /// <summary>
        /// Prefix, if any, for each cmd-line arg option, if short-hand.
        /// </summary>
        public static string ShortHandOptionPrefix { get; set; }

        /// <summary>
        /// Prefix, if any, for each cmd-line arg option, if long-hand.
        /// </summary>
        public static string LongHandOptionPrefix { get; set; }

        #endregion

        #region Initiation

        /// <summary>
        /// Store the cmd-args for later use.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        /// <param name="shortHandOptionPrefix">Prefix, if any, for each cmd-line arg option, if short-hand. Defaults to '-'.</param>
        /// <param name="longHandOptionPrefix">Prefix, if any, for each cmd-line arg option, if long-hand. Defaults to '--'.</param>
        public static void Init(string[] args, string shortHandOptionPrefix = "-", string longHandOptionPrefix = "--")
        {
            CmdArgs = args;
            ShortHandOptionPrefix = shortHandOptionPrefix;
            LongHandOptionPrefix = longHandOptionPrefix;
        }

        #endregion

        #region Get functions

        /// <summary>
        /// Get value from cmd-line arguments, if present.
        /// </summary>
        /// <param name="keys">Keys to find.</param>
        /// <returns>Value, if present.</returns>
        public static string GetArgValue(params string[] keys)
        {
            if (CmdArgs == null ||
                CmdArgs.Length < 2)
            {
                return null;
            }

            for (var i = 0; i < CmdArgs.Length; i++)
            {
                if (keys.Any(key => CmdArgs[i] == $"{ShortHandOptionPrefix}{key}" ||
                                    CmdArgs[i] == $"{LongHandOptionPrefix}{key}"))
                {
                    return CmdArgs[i + 1];
                }
            }

            return null;
        }

        /// <summary>
        /// Get value from cmd-line arguments, case as dictionary, if present.
        /// </summary>
        /// <param name="keys">Keys to find.</param>
        /// <returns>Value, as dictionary.</returns>
        public static Dictionary<string, string> GetArgValueAsDictionary(params string[] keys)
        {
            var value = GetArgValue(keys);
            var items = value?.Split(',');

            return items?.Select(item => item.Split(':'))
                .Where(kv => kv.Length == 2)
                .ToDictionary(kv => kv[0], kv => kv[1]);
        }

        /// <summary>
        /// Get value from cmd-line arguments, cast as int, if present.
        /// </summary>
        /// <param name="keys">Keys to find.</param>
        /// <returns>Value, as int.</returns>
        public static int? GetArgValueAsInt32(params string[] keys)
        {
            var value = GetArgValue(keys);

            if (value != null &&
                int.TryParse(value, out var temp))
            {
                return temp;
            }

            return null;
        }

        /// <summary>
        /// Check if a switch is present in the cmd-line args given.
        /// </summary>
        /// <param name="keys">Keys to find.</param>
        /// <returns>Success.</returns>
        public static bool IsSwitchPresent(params string[] keys)
        {
            if (CmdArgs == null)
            {
                return false;
            }

            return keys.Any(key => CmdArgs.Any(n => n == $"{ShortHandOptionPrefix}{key}") ||
                                   CmdArgs.Any(n => n == $"{LongHandOptionPrefix}{key}"));
        }

        /// <summary>
        /// Checks if there are no command-line options.
        /// </summary>
        /// <returns>Success.</returns>
        public static bool NoOptions()
        {
            return CmdArgs?.Length == 0;
        }

        #endregion

        #region Write functions

        /// <summary>
        /// Write an error to console.
        /// </summary>
        /// <param name="message">Message to write.</param>
        public static void WriteError(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("[ERROR] ");

            Console.ResetColor();
            Console.WriteLine(message);
        }

        /// <summary>
        /// Write a line to console word-wrapped instead of letter-wrapped.
        /// </summary>
        /// <param name="message">Message to write.</param>
        public static void WriteLineWordWrapped(string message)
        {
            var words = message.Split(' ');
            var charsLeft = Console.WindowWidth;

            foreach (var word in words)
            {
                if (word.Length > charsLeft)
                {
                    charsLeft = Console.WindowWidth;
                    Console.WriteLine();
                }

                charsLeft -= word.Length + 1;

                Console.Write($"{word} ");
            }

            Console.WriteLine();
        }

        #endregion
    }
}