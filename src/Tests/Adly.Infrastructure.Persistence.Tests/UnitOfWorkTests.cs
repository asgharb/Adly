using Adly.Application.Features.Location.Commands;
using Adly.Application.Features.Location.Queries;
using Adly.Domain.Entities.Ad;
using Adly.Infrastructure.Persistence.Repositories.Common;
using FluentAssertions;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Adly.Infrastructure.Persistence.Tests;

public class UnitOfWorkTests:IClassFixture<PersistenceTestSetup>
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly UnitOfWork _unitOfWork;
    private readonly IServiceProvider _serviceProvider;
    public UnitOfWorkTests(PersistenceTestSetup setup,ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _unitOfWork = setup.UnitOfWork;
        _serviceProvider = setup.ServiceProvider;
    }

    [Fact]
    public async Task Adding_New_Location_Should_Save_To_Database()
    {
        var location = new LocationEntity("Test Location");

        await _unitOfWork.LocationRepository.CreateAsync(location);

        await _unitOfWork.CommitAsync();

        var locationInDb = await _unitOfWork.LocationRepository.GetLocationsByNameAsync("Test Location");

        locationInDb.Should().NotBeNull();
    }

    [Fact]
    public async Task Getting_Location_By_Id_Should_Success()
    {
        var location = new LocationEntity("Test Location 2");

        await _unitOfWork.LocationRepository.CreateAsync(location);

        await _unitOfWork.CommitAsync();

        var locationById = await _unitOfWork.LocationRepository.GetLocationByIdAsync(location.Id);

        locationById.Should().NotBeNull();
    }

    [Fact]
    public async Task Location_Added_Date_Should_Have_Value()
    {
        var location = new LocationEntity("Test Location 3");

        await _unitOfWork.LocationRepository.CreateAsync(location);

        await _unitOfWork.CommitAsync();

        var locationById = await _unitOfWork.LocationRepository.GetLocationByIdAsync(location.Id);

        locationById!.CreatedDate.Should().BeMoreThan(TimeSpan.MinValue);
        
        _outputHelper.WriteLine($"Current Added Location Date {locationById.CreatedDate}");
    }
    
    [Fact]
    public async Task Location_Modified_Date_Should_Not_Have_Value_When_Added()
    {
        var location = new LocationEntity("Test Location 4");

        await _unitOfWork.LocationRepository.CreateAsync(location);

        await _unitOfWork.CommitAsync();

        var locationById = await _unitOfWork.LocationRepository.GetLocationByIdAsync(location.Id);

        locationById!.ModifiedDate.Should().BeNull();
    }
    
    [Fact]
    public async Task Location_Modified_Date_Should_Have_Value_When_Updated()
    {
        var location = new LocationEntity("Test Location 5");

        await _unitOfWork.LocationRepository.CreateAsync(location);

        await _unitOfWork.CommitAsync();

        var locationById = await _unitOfWork.LocationRepository.GetLocationByIdForEditAsync(location.Id);

        locationById!.EditLocationName("Test Edited Location 5");

        await _unitOfWork.CommitAsync();
        
        var newLocationById = await _unitOfWork.LocationRepository.GetLocationByIdAsync(location.Id);
        
        newLocationById!.ModifiedDate.Should().BeMoreThan(TimeSpan.MinValue);
        
        _outputHelper.WriteLine($"Current Updated Location Date {newLocationById.ModifiedDate}");
    }

    [Fact]
    public async Task Adding_New_Location_By_Mediator_Should_Be_Success()
    {
        var sender = _serviceProvider.GetRequiredService<ISender>();

        var addLocationResult = await sender.Send(new CreateLocationCommand("Test_Location_By_Mediator"));

        addLocationResult.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public async Task Adding_New_Location_By_Mediator_Should_Persist_Data()
    {
        var sender = _serviceProvider.GetRequiredService<ISender>();

        await sender.Send(new CreateLocationCommand("Test_Location_By_Mediator"));

        var location = await sender.Send(new GetLocationByNameQuery("Test_Location_By_Mediator"));

        location.Result.Should().NotBeEmpty();
    }
}