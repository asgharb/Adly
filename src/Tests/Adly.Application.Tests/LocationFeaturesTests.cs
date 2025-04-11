using Adly.Application.Common;
using Adly.Application.Extensions;
using Adly.Application.Features.Common;
using Adly.Application.Features.Location.Commands;
using Adly.Application.Features.Location.Queries;
using Adly.Application.Features.User.Commands.Register;
using Adly.Application.Repositories.Common;
using Adly.Application.Repositories.Location;
using Adly.Application.Tests.Extensions;
using Adly.Domain.Entities.Ad;
using Bogus;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit.Abstractions;

namespace Adly.Application.Tests;

public class LocationFeaturesTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITestOutputHelper _testOutputHelper;

    public LocationFeaturesTests(ITestOutputHelper testOutputHelper)
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.RegisterApplicationValidators();

        _serviceProvider = serviceCollection.BuildServiceProvider();
        _testOutputHelper = testOutputHelper;
    }


    [Fact]
    public async Task Add_Location_With_Valid_Parameters_Should_Success()
    {
        //Arrange
        var faker = new Faker();

        faker.Address.City();
        var location = new CreateLocationCommand(faker.Address.City());

        var locationRepositoryMock = Substitute.For<ILocationRepository>();

        locationRepositoryMock.IsLocationNameExistsAsync(location.LocationName)
            .Returns(Task.FromResult(false));

        var unitOfWorkMock = Substitute.For<IUnitOfWork>();
        unitOfWorkMock.LocationRepository.Returns(locationRepositoryMock);

        var validationBehavior =
            new ValidateRequestBehavior<CreateLocationCommand, OperationResult<bool>>(_serviceProvider
                .GetRequiredService<IValidator<CreateLocationCommand>>());

        var createLocationHandler = new CreateLocationCommandHandler(unitOfWorkMock);

        //Act

        var createLocationResult =
            await validationBehavior.Handle(location, CancellationToken.None, createLocationHandler.Handle);

        //Assert

        createLocationResult.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Existing_Location_Cannot_BeCreated()
    {
        //Arrange
        var faker = new Faker();

        faker.Address.City();
        var location = new CreateLocationCommand(faker.Address.City());

        var locationRepositoryMock = Substitute.For<ILocationRepository>();

        locationRepositoryMock.IsLocationNameExistsAsync(location.LocationName)
            .Returns(Task.FromResult(true));

        var unitOfWorkMock = Substitute.For<IUnitOfWork>();
        unitOfWorkMock.LocationRepository.Returns(locationRepositoryMock);

        var validationBehavior =
            new ValidateRequestBehavior<CreateLocationCommand, OperationResult<bool>>(_serviceProvider
                .GetRequiredService<IValidator<CreateLocationCommand>>());

        var createLocationHandler = new CreateLocationCommandHandler(unitOfWorkMock);

        //Act

        var createLocationResult =
            await validationBehavior.Handle(location, CancellationToken.None, createLocationHandler.Handle);

        //Assert

        createLocationResult.Result.Should().BeFalse();

        _testOutputHelper.WritelineOperationResultErrors(createLocationResult);
    }

    [Fact]
    public async Task Getting_List_Of_Locations_Should_Be_Success()
    {
        //Arrange
        var faker = new Faker();

        faker.Address.City();
        var location = new GetLocationByNameQuery(faker.Address.City());

        List<LocationEntity> fakeLocations =
            [new(faker.Address.City()), new(faker.Address.City()), new(faker.Address.City())];

        var locationRepositoryMock = Substitute.For<ILocationRepository>();

        locationRepositoryMock.GetLocationsByNameAsync(location.LocationNameSearchTerm)
            .Returns(Task.FromResult(fakeLocations));

        var unitOfWorkMock = Substitute.For<IUnitOfWork>();
        unitOfWorkMock.LocationRepository.Returns(locationRepositoryMock);

        var validationBehavior =
            new ValidateRequestBehavior<GetLocationByNameQuery, OperationResult<List<GetLocationByNameQueryResult>>>(
                _serviceProvider.GetRequiredService<IValidator<GetLocationByNameQuery>>());

        var getLocationByNameQueryHandler = new GetLocationByNameQueryHandler(unitOfWorkMock);

        //Act

        var getLocationResult =
            await validationBehavior.Handle(location, CancellationToken.None, getLocationByNameQueryHandler.Handle);

        //Assert

        getLocationResult.Result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Searching_For_Location_Should_Have_At_Least_Three_Characters()
    {
        //Arrange
        var faker = new Faker();

        faker.Address.City();
        var location = new GetLocationByNameQuery(faker.Address.City()[..2]);

        List<LocationEntity> fakeLocations =
            [new(faker.Address.City()), new(faker.Address.City()), new(faker.Address.City())];

        var locationRepositoryMock = Substitute.For<ILocationRepository>();

        locationRepositoryMock.GetLocationsByNameAsync(location.LocationNameSearchTerm)
            .Returns(Task.FromResult(fakeLocations));

        var unitOfWorkMock = Substitute.For<IUnitOfWork>();
        unitOfWorkMock.LocationRepository.Returns(locationRepositoryMock);

        var validationBehavior =
            new ValidateRequestBehavior<GetLocationByNameQuery, OperationResult<List<GetLocationByNameQueryResult>>>(
                _serviceProvider.GetRequiredService<IValidator<GetLocationByNameQuery>>());

        var getLocationByNameQueryHandler = new GetLocationByNameQueryHandler(unitOfWorkMock);

        //Act

        var getLocationResult =
            await validationBehavior.Handle(location, CancellationToken.None, getLocationByNameQueryHandler.Handle);

        //Assert

        getLocationResult.IsSuccess.Should().BeFalse();
        
        _testOutputHelper.WritelineOperationResultErrors(getLocationResult);
    }
}