using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DotnetSortAndSyncRefs.Test.Mocks
{
    internal class MockReporter : Common.IReporter
    {
        private void WriteLine(string message)
        {
           Debug.WriteLine(message);
        }

        public void Ok(string message)
        {
            WriteLine(message);
        }

        public void Do(string message)
        {
            WriteLine(message);
        }

        public void NotOk(string message)
        {
            WriteLine(message);
        }

        public void White(string message)
        {
            WriteLine(message);
        }

        public void Black(string message)
        {
            WriteLine(message);
        }

        public void Green(string message)
        {
            WriteLine(message);
        }

        public void DarkGreen(string message)
        {
            WriteLine(message);
        }

        public void Red(string message)
        {
            WriteLine(message);
        }

        public void DarkRed(string message)
        {
            WriteLine(message);
        }

        public void Cyan(string message)
        {
            WriteLine(message);
        }

        public void DarkCyan(string message)
        {
            WriteLine(message);
        }

        public void Magenta(string message)
        {
            WriteLine(message);
        }

        public void DarkMagenta(string message)
        {
            WriteLine(message);
        }

        public void Yellow(string message)
        {
            WriteLine(message);
        }

        public void DarkYellow(string message)
        {
            WriteLine(message);
        }

        public void Blue(string message)
        {
            WriteLine(message);
        }

        public void DarkBlue(string message)
        {
            WriteLine(message);
        }

        public void Gray(string message)
        {
            WriteLine(message);
        }

        public void DarkGray(string message)
        {
            WriteLine(message);
        }

        public void Error(string message)
        {
            WriteLine(message);
        }

        public void Warn(string message)
        {
            WriteLine(message);
        }

        public void Output(string message)
        {
            WriteLine(message);
        }

        public void Verbose(string message)
        {
            WriteLine(message);
        }

        public bool IsVerbose { get; set; }
        public bool IsQuiet { get; set; }
    }
}
