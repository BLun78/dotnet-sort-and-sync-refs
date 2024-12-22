namespace DotnetSortAndSyncRefs.Common
{
    internal static class ConstConfig
    {
        public const string AllElementTypes = @"PackageReference|Reference|PackageVersion|GlobalPackageReference";
        public const string ProjectElementTypes = @"PackageReference|Reference|GlobalPackageReference";
        public const string PropsElementTypes = @"PackageVersion";
        public const string Condition = @"Condition";
        public const string Version = @"Version";
        public const string WithOutCondition = "WithOutCondition";
    }
}
