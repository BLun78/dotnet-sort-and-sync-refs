using DotnetSortAndSyncRefs.Common;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace DotnetSortAndSyncRefs.Xml
{
    internal class XmlProjectFile : XmlBaseFile
    {
        public XmlProjectFile(IFileSystem fileSystem, Reporter reporter) 
            : base(fileSystem, reporter)
        {
        }

        protected override string GetItemGroupElements()
        {
            return ConstConfig.ProjectElementTypes;
        }
    }
}
