using System;
using System.Collections.Generic;
using System.Text;
using McMaster.Extensions.CommandLineUtils;

namespace DotNetSortRefs.Common
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

    }
}
