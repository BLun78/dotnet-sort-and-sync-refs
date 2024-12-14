using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text;
using System.Threading.Tasks;

namespace DotNetSortRefs.Xml
{
    internal static class CreatePackageVersion
    {
        public static Task<int> CreatePackageVersions(this IFileSystem fileSystem, 
            List<string> fileProjects,
            string path)
        {


            return Task.FromResult(0);
        }
    }
}
