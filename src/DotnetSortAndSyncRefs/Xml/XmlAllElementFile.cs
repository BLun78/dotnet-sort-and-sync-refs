using System.IO.Abstractions;
using DotnetSortAndSyncRefs.Common;

namespace DotnetSortAndSyncRefs.Xml;

internal class XmlAllElementFile : XmlBaseFile
{
    public XmlAllElementFile(IFileSystem fileSystem, IReporter reporter)
        : base(fileSystem, reporter)
    {
    }

    protected override string GetItemGroupElements()
    {
        return ConstConfig.AllElementTypesQuery;
    }
}