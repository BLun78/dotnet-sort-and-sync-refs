using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using DotnetSortAndSyncRefs.Common;

namespace DotnetSortAndSyncRefs.Extensions
{
    internal static partial class XElementExtensions
    {
        public static List<XElement> GetPackageReferenceElements(this List<XElement> elementsOfProjectFiles)
        {
            return elementsOfProjectFiles.Elements(ConstConfig.PackageReference).ToList();
        }

        public static List<XElement> GetPackageReferenceElementsWithVersionSorted(this List<XElement> itemGroups)
        {
            return itemGroups
                .Elements(ConstConfig.PackageReference)
                .Where(x => x.Attribute(ConstConfig.Version) != null)
                .OrderByDescending(x => (string)x.Attribute(ConstConfig.Include))
                .ThenBy(x => (string)x.Attribute(ConstConfig.Version))
                .ToList(); ;
        }

        public static List<XElement> GetPackageReferenceElementsWithVersionSorted(this XElement itemGroups)
        {
            return itemGroups
                .Elements(ConstConfig.PackageReference)
                .Where(x => x.Attribute(ConstConfig.Version) != null)
                .OrderByDescending(x => (string)x.Attribute(ConstConfig.Include))
                .ThenBy(x => (string)x.Attribute(ConstConfig.Version))
                .ToList(); ;
        }
    }
}
