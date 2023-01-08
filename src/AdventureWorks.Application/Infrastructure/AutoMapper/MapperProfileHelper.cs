using AdventureWorks.Application.Interfaces.Mapping;
using System.Reflection;

namespace AdventureWorks.Application.Infrastructure.AutoMapper;

public sealed class Map
{
    public Type Source { get; set; } = null!;

    public Type Destination { get; set; } = null!;
}

public static class MapperProfileHelper
{
    public static IList<Map> LoadStandardMappings(Assembly rootAssembly)
    {
        var types = rootAssembly.GetExportedTypes();

        var mapsFrom = (
            from type in types
            from instance in type.GetInterfaces()
            where
                instance.IsGenericType && instance.GetGenericTypeDefinition() == typeof(IMapFrom<>) &&
                !type.IsAbstract &&
                !type.IsInterface
            select new Map
            {
                Source = type.GetInterfaces().First().GetGenericArguments().First(),
                Destination = type
            }).ToList();

        return mapsFrom;
    }

    public static IList<IHaveCustomMapping> LoadCustomMappings(Assembly rootAssembly)
    {
        var types = rootAssembly.GetExportedTypes();

        var mapsFrom = (
            from type in types
            from instance in type.GetInterfaces()
            where
                typeof(IHaveCustomMapping).IsAssignableFrom(type) &&
                !type.IsAbstract &&
                !type.IsInterface
            select Activator.CreateInstance(type) as IHaveCustomMapping).ToList();

        return mapsFrom;
    }
}