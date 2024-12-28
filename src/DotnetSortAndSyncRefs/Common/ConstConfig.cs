namespace DotnetSortAndSyncRefs.Common
{
    internal static class ConstConfig
    {
        public const string AllElementTypesQuery = @"PackageReference|Reference|PackageVersion|GlobalPackageReference";
        public const string ProjectElementTypesQuery = @"PackageReference|Reference|GlobalPackageReference";
        public const string CentralPackageManagementElementTypes = @"PackageVersion";
        public const string Condition = @"Condition";
        public const string Version = @"Version";
        public const string WithOutCondition = "WithOutCondition";
        public const string ItemGroup = "ItemGroup";
        public const string PropertyGroup = "PropertyGroup";
        public const string TargetFramework = "TargetFramework";
        public const string TargetFrameworks = "TargetFrameworks";
        public const string TargetFrameworksQuery = $"{TargetFramework}|{TargetFrameworks}";
    }
}
