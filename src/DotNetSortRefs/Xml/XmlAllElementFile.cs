using System.IO.Abstractions;
using DotnetSortAndSyncRefs.Common;

namespace DotnetSortAndSyncRefs.Xml;

internal class XmlAllElementFile : XmlBaseFile
{
    public XmlAllElementFile(IFileSystem fileSystem, Reporter reporter)
        : base(fileSystem, reporter)
    {
    }

    protected override string GetItemGroupElements()
    {
        return ConstConfig.AllElementTypes;
    }
}