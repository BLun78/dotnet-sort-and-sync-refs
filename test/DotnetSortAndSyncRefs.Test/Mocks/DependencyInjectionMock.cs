using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;
using DotnetSortAndSyncRefs.Commands;
using DotnetSortAndSyncRefs.Common;
using DotnetSortAndSyncRefs.Xml;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DotnetSortAndSyncRefs.Test.Mocks
{
    internal class DependencyInjectionMock
    {
        public IServiceCollection ServiceCollection { get; private set; }

        public DependencyInjectionMock(IFileSystem fileSystem, IReporter reporter)
        {
            ServiceCollection = new ServiceCollection();

            ServiceCollection.AddSingleton(reporter);
            ServiceCollection.AddSingleton(fileSystem);
            ServiceCollection.AddXmlFiles();
            ServiceCollection.AddCommands();
        }

        public IServiceProvider CreateServiceProvider()
        {
            return ServiceCollection.BuildServiceProvider();
        }
    }
}
