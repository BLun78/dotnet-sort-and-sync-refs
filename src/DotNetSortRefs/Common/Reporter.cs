using System;
using McMaster.Extensions.CommandLineUtils;

namespace DotnetSortAndSyncRefs.Common
{
    internal class Reporter : ConsoleReporter, IReporter
    {
        public Reporter(IConsole console) : base(console)
        {
        }

        public Reporter(IConsole console, bool verbose, bool quiet) : base(console, verbose, quiet)
        {
        }

        public virtual void Ok(string message)
            => WriteLine(Console.Out, message, ConsoleColor.DarkGreen);

        public virtual void Do(string message)
            => WriteLine(Console.Out, message, ConsoleColor.DarkCyan);

        public virtual void NotOk(string message)
            => WriteLine(Console.Out, message, ConsoleColor.DarkYellow);
    }
}
