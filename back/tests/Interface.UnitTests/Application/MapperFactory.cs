using AutoMapper;
using Interface.Application;
using Microsoft.Extensions.Logging.Abstractions;

namespace Interface.UnitTests.Application;

internal static class MapperFactory
{
    public static MapperConfiguration CreateConfiguration() =>
        new(configuration => configuration.AddMaps(typeof(ApplicationServiceRegistration).Assembly),
            NullLoggerFactory.Instance);

    public static IMapper Create() => CreateConfiguration().CreateMapper();
}

public sealed class MappingConfigurationTests
{
    [Fact]
    public void All_AutoMapper_profiles_are_valid()
    {
        MapperFactory.CreateConfiguration().AssertConfigurationIsValid();
    }
}
