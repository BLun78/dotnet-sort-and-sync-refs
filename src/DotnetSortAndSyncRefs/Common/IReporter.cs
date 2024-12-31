namespace DotnetSortAndSyncRefs.Common;

public interface IReporter
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