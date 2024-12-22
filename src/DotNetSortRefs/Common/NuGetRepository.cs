using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace DotnetSortAndSyncRefs.Common
{
    /// <summary>
    /// https://learn.microsoft.com/en-us/nuget/reference/nuget-client-sdk
    /// </summary>
    internal class NuGetRepository
    {
        private readonly SourceCacheContext _sourceCacheContext;
        private readonly SourceRepository _sourceRepository;
        private readonly ILogger _logger;

        public NuGetRepository(SourceCacheContext sourceCacheContext,
            SourceRepository sourceRepository,
            ILogger logger)
        {
            _sourceCacheContext = sourceCacheContext;
            _sourceRepository = sourceRepository;
            _logger = logger;

            
            // only for info in dev process
            string frameworkName = Assembly.GetExecutingAssembly().GetCustomAttributes(true)
                .OfType<System.Runtime.Versioning.TargetFrameworkAttribute>()
                .Select(x => x.FrameworkName)
                .FirstOrDefault();

        }

        public async Task<IEnumerable<NuGetVersion>> GetAllVersionsAsync(string packageId, CancellationToken cancellationToken = default)
        {
            FindPackageByIdResource resource = await _sourceRepository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);

            return await resource.GetAllVersionsAsync(
                packageId,
                _sourceCacheContext,
                _logger,
                cancellationToken);
        }

        public async Task<IEnumerable<IPackageSearchMetadata>> GetMetadataAsync(string packageId, CancellationToken cancellationToken = default)
        {
            PackageMetadataResource resource = await _sourceRepository.GetResourceAsync<PackageMetadataResource>(cancellationToken);

            return await resource.GetMetadataAsync(
                packageId,
                includePrerelease: true,
                includeUnlisted: false,
                _sourceCacheContext,
                _logger,
                cancellationToken);
        }
    }
}
