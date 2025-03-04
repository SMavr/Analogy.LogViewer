﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Analogy.Common.DataTypes;
using Analogy.Interfaces;
using Analogy.Interfaces.Factories;

namespace Analogy.Common.Interfaces
{
    public interface IFactoriesManager
    {
        List<string> ProbingPaths { get; set; }
        List<FactoryContainer> BuiltInFactories { get; }
        List<FactoryContainer> Factories { get; }
        Task InitializeBuiltInFactories();
        Task AddExternalDataSources();

        IEnumerable<(IAnalogyOfflineDataProvider DataProvider, Guid FactoryID)> GetSupportedOfflineDataSources(
            string[] fileNames);

        IEnumerable<IAnalogyOfflineDataProvider> GetSupportedOfflineDataSourcesFromFactory(Guid factoryId,
            string[] fileNames);

        IEnumerable<(string Name, Guid ID, Image Image, string Description)> GetRealTimeDataSourcesNamesAndIds();
        Assembly GetAssemblyOfFactory(IAnalogyFactory factory);
        FactoryContainer GetBuiltInFactoryContainer(Guid id);
        bool IsBuiltInFactory(IAnalogyFactory factory);
        bool IsBuiltInFactory(Guid factoryId);
        List<IAnalogyDataProviderSettings> GetProvidersSettings();
        Image GetLargeImage(Guid componentId);
        FactoryContainer GetFactoryContainer(Guid componentId);
        Image? GetSmallImage(Guid componentId);
        IEnumerable<IAnalogyExtension> GetExtensions(IAnalogyDataProvider dataProvider);
        IEnumerable<IAnalogyExtension> GetAllExtensions();
        FactoryContainer FactoryContainer(Guid componentId);
    }
}
