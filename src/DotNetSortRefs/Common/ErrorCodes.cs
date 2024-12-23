using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSortAndSyncRefs.Common
{
    internal static class ErrorCodes
    {
        public static int CentralPackageManagementCriticalError = -5;
        public static int ApplicationCriticalError = -4;
        public static int CriticalError = -3;
        public static int CentralPackageManagementFailed = -2;
        public static int RemoveFailed = -1;
        public static int Ok = 0;
        public static int DirectoryDoNotExists = 1;
        public static int FileDoNotExists = 2;
        public static int CreateCentralPackageManagementFailed = 3;
        public static int ProjectFileHasNotAValidXmlFormat = 4;
        public static int SortingIsFailed = 5;
    }
}
