namespace DotnetSortAndSyncRefs.Common
{
    internal static class ConstConfig
    {
        public const string AllElementTypes = @"PackageReference|Reference|PackageVersion|GlobalPackageReference";
        public const string ProjectElementTypes = @"PackageReference|Reference|GlobalPackageReference";
        public const string CentralPackageManagementElementTypes = @"PackageVersion";
        public const string Condition = @"Condition";
        public const string Version = @"Version";
        public const string WithOutCondition = "WithOutCondition";
        public const string ItemGroup = "ItemGroup";
    }
}
