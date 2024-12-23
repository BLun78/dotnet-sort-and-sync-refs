using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using DotnetSortAndSyncRefs.Common;

namespace DotnetSortAndSyncRefs.Processes
{
    internal class Processor : DryRun
    {
        private readonly Inspector _inspector;
        private readonly CentralPackageManagement _centralPackageManagement;
        private readonly SyncPackageVersions _syncPackageVersions;
        private readonly SortReferences _sortReferences;
        private readonly Reporter _reporter;
        private readonly IFileSystem _fileSystem;

        public Processor(
            Inspector inspector,
            CentralPackageManagement centralPackageManagement,
            SyncPackageVersions syncPackageVersions,
            SortReferences sortReferences,
            Reporter reporter,
            IFileSystem fileSystem)
        {
            _inspector = inspector;
            _centralPackageManagement = centralPackageManagement;
            _syncPackageVersions = syncPackageVersions;
            _sortReferences = sortReferences;
            _reporter = reporter;
            _fileSystem = fileSystem;
        }

        private HashSet<string> ProjectFilePostfix = new(StringComparer.OrdinalIgnoreCase) {
            ".csproj",
            ".vbproj",
            ".fsproj",
        };

        private HashSet<string> AdditionalFilePostfix = new(StringComparer.OrdinalIgnoreCase) {
            ".props"
        };

        public async Task<int> Process(Commands command)
        {
            var result = ErrorCodes.None;
            var allExtensions = new List<string>();
            allExtensions.AddRange(ProjectFilePostfix);
            allExtensions.AddRange(AdditionalFilePostfix);

            var fileProjects = LoadFilesFromExtension(ProjectFilePostfix);
            var fileProps = LoadFilesFromExtension(AdditionalFilePostfix);
            var allFiles = new List<string>();
            allFiles.AddRange(fileProjects);
            allFiles.AddRange(fileProps);

            if (allFiles.Count == 0)
            {
                _reporter.Error($"no '{string.Join(", ", allExtensions)}'' files found.");
                return ErrorCodes.FileDoNotExists;
            }

            _reporter.Output("Running analysis ...");
            var projFilesWithNonSortedReferences = await _inspector
                .Inspect(allFiles)
                .ConfigureAwait(false);

            if (projFilesWithNonSortedReferences == null)
            {
                _reporter.Do("Please solve the issue of the Project file(s).");
                return ErrorCodes.ProjectFileHasNotAValidXmlFormat;
            }

            switch (command)
            {
                case Commands.Inspect:
                    _reporter.Output("Running inspection ...");
                    PrintInspectionResults(allFiles, projFilesWithNonSortedReferences);
                    result = projFilesWithNonSortedReferences.Count > 0
                        ? 0
                        : 1;
                    break;
                case Commands.Remove:
                    _reporter.Output("Running remove not needed PackageVersion ...");
                    result = await _syncPackageVersions
                        .RemovePackageVersions(fileProjects, fileProps)
                        .ConfigureAwait(false);
                    if (result == ErrorCodes.Ok)
                    {
                        result = await _sortReferences
                            .SortIt(projFilesWithNonSortedReferences)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        result = ErrorCodes.RemoveFailed;
                    }
                    break;
                case Commands.Create:
                    _reporter.Output("Running create a Central Package Management file (\"Directory.Packages.props\") ...");
                    result = await _centralPackageManagement
                        .CreateCentralPackageManagementFile(fileProjects)
                        .ConfigureAwait(false);

                    if (result == ErrorCodes.Ok)
                    {
                        if (!string.IsNullOrWhiteSpace(_centralPackageManagement.FilePath))
                        {
                            projFilesWithNonSortedReferences.Add(_centralPackageManagement.FilePath);
                        }
                        result = await _sortReferences
                            .SortIt(projFilesWithNonSortedReferences)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        result = ErrorCodes.CentralPackageManagementFailed;
                    }
                    break;
                case Commands.Sort:
                    result = await _sortReferences
                        .SortIt(projFilesWithNonSortedReferences)
                        .ConfigureAwait(false);
                    break;
                case Commands.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(command), command, null);
            }

            _reporter.Output("Done.");
            return result;
        }


        private void PrintInspectionResults(
            ICollection<string> projFiles,
            ICollection<string> projFilesWithNonSortedReferences)
        {
            var max = projFiles
                .Max(x => x.Length);

            foreach (var proj in projFiles)
            {
                var paddedProjectFile = proj
                    .PadRight(max);

                if (projFilesWithNonSortedReferences.Contains(proj))
                {
                    _reporter.Error($"» {paddedProjectFile} - X");
                }
                else
                {
                    _reporter.Ok($"» {paddedProjectFile} - Ok");
                }
            }
        }

        private List<string> LoadFilesFromExtension(IEnumerable<string> extensions)
        {
            var projFiles = new List<string>();
            if (_fileSystem.File.Exists(Program.Path))
            {
                projFiles.Add(Program.Path);
            }
            else
            {
                projFiles = extensions
                    .SelectMany(ext => _fileSystem
                        .Directory
                        .GetFiles(Program.Path, $"*{ext}", SearchOption.AllDirectories))
                    .ToList();
            }

            return projFiles;
        }
    }
}
