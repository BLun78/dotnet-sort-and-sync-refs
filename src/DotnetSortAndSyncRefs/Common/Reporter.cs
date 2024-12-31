using System;
using McMaster.Extensions.CommandLineUtils;

namespace DotnetSortAndSyncRefs.Common
{
    internal interface IReporter
    {
        void Ok(string message);
        void Do(string message);
        void NotOk(string message);
        void White(string message);
        void Black(string message);
        void Green(string message);
        void DarkGreen(string message);
        void Red(string message);
        void DarkRed(string message);
        void Cyan(string message);
        void DarkCyan(string message);
        void Magenta(string message);
        void DarkMagenta(string message);
        void Yellow(string message);
        void DarkYellow(string message);
        void Blue(string message);
        void DarkBlue(string message);
        void Gray(string message);
        void DarkGray(string message);
        void Error(string message);
        void Warn(string message);
        void Output(string message);
        void Verbose(string message);
        bool IsVerbose { get; set; }
        bool IsQuiet { get; set; }
    }

    internal class Reporter : ConsoleReporter,
        global::DotnetSortAndSyncRefs.Common.IReporter,
        global::McMaster.Extensions.CommandLineUtils.IReporter
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

        // Colors
        public virtual void White(string message)
            => WriteLine(Console.Out, message, ConsoleColor.White);

        public virtual void Black(string message)
            => WriteLine(Console.Out, message, ConsoleColor.Black);

        public virtual void Green(string message)
            => WriteLine(Console.Out, message, ConsoleColor.Green);

        public virtual void DarkGreen(string message)
            => WriteLine(Console.Out, message, ConsoleColor.DarkGreen);

        public virtual void Red(string message)
            => WriteLine(Console.Out, message, ConsoleColor.Red);

        public virtual void DarkRed(string message)
            => WriteLine(Console.Out, message, ConsoleColor.DarkRed);

        public virtual void Cyan(string message)
            => WriteLine(Console.Out, message, ConsoleColor.Cyan);

        public virtual void DarkCyan(string message)
            => WriteLine(Console.Out, message, ConsoleColor.DarkCyan);

        public virtual void Magenta(string message)
            => WriteLine(Console.Out, message, ConsoleColor.Magenta);

        public virtual void DarkMagenta(string message)
            => WriteLine(Console.Out, message, ConsoleColor.DarkMagenta);

        public virtual void Yellow(string message)
            => WriteLine(Console.Out, message, ConsoleColor.Yellow);

        public virtual void DarkYellow(string message)
            => WriteLine(Console.Out, message, ConsoleColor.DarkYellow);

        public virtual void Blue(string message)
            => WriteLine(Console.Out, message, ConsoleColor.Blue);

        public virtual void DarkBlue(string message)
            => WriteLine(Console.Out, message, ConsoleColor.DarkBlue);

        public virtual void Gray(string message)
            => WriteLine(Console.Out, message, ConsoleColor.Gray);

        public virtual void DarkGray(string message)
            => WriteLine(Console.Out, message, ConsoleColor.DarkGray);
    }
}
