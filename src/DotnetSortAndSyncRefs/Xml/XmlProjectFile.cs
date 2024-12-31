using DotnetSortAndSyncRefs.Common;
using System;
using System.IO.Abstractions;
using System.Linq;
using System.Text;

namespace DotnetSortAndSyncRefs.Xml
{
    internal class XmlProjectFile : XmlBaseFile
    {
        public XmlProjectFile(IFileSystem fileSystem, IReporter reporter) 
            : base(fileSystem, reporter)
        {
        }

        protected override string GetItemGroupElements()
        {
            return ConstConfig.ProjectElementTypesQuery;
        }
    }
}
