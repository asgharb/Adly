using Adly.Application.Contracts.FileService.Interfaces;
using Adly.Application.Contracts.FileService.Models;
using Adly.Application.Contracts.User;
using Adly.Application.Extensions;
using Adly.Application.Features.Ad.Commands;
using Adly.Application.Features.Ad.Queries;
using Adly.Application.Repositories.Ad;
using Adly.Application.Repositories.Category;
using Adly.Application.Repositories.Common;
using Adly.Application.Repositories.Location;
using Adly.Application.Tests.Extensions;
using Adly.Domain.Common.ValueObjects;
using Adly.Domain.Entities.Ad;
using Adly.Domain.Entities.User;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit.Abstractions;

namespace Adly.Application.Tests;

public class AdFeaturesTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITestOutputHelper _testOutputHelper;

    public AdFeaturesTests(ITestOutputHelper testOutputHelper)
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .RegisterApplicationValidators()
            .AddApplicationAutomapper();

        _serviceProvider = serviceCollection.BuildServiceProvider();
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task Create_Ad_With_Valid_Parameters_Should_Success()
    {
        //Arrange
        var createAdCommand = new CreateAdCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Test Title",
            "Test Description",
            new[] { new CreateAdCommand.CreateAdImagesModel("Test", "image/png") }
        );

        var unitOfWorkMock = Substitute.For<IUnitOfWork>();
        var adRepositoryMock = Substitute.For<IAdRepository>();
        var userManagerMock = Substitute.For<IUserManager>();
        var fileServiceMock = Substitute.For<IFileService>();
        var categoryRepositoryMock = Substitute.For<ICategoryRepository>();
        var locationRepositoryMock = Substitute.For<ILocationRepository>();

        categoryRepositoryMock.GetCategoryByIdAsync(Arg.Any<Guid>())
            .Returns(Task.FromResult<CategoryEntity?>(new CategoryEntity("Test Category")));

        locationRepositoryMock.GetLocationByIdAsync(Arg.Any<Guid>())
            .Returns(Task.FromResult<LocationEntity?>(new LocationEntity("Test Location")));


        adRepositoryMock.CreateAdAsync(Arg.Any<AdEntity>()).Returns(Task.CompletedTask);

        userManagerMock.GetUserByIdAsync(Arg.Any<Guid>())!
            .Returns(Task.FromResult(new UserEntity("test", "test", "test", "test@test.com")));


        fileServiceMock.SaveFilesAsync(Arg.Any<List<SaveFileModel>>()).Returns(Task.FromResult(
            new List<SaveFileModelResult>()
            {
                new("Test.png", "image/png")
            }));


        unitOfWorkMock.AdRepository.Returns(adRepositoryMock);
        unitOfWorkMock.LocationRepository.Returns(locationRepositoryMock);
        unitOfWorkMock.CategoryRepository.Returns(categoryRepositoryMock);

        var createAdHandler = new CreateAdCommandHandler(unitOfWorkMock, fileServiceMock, userManagerMock);

        var createAdResult = await Helpers.ValidateAndExecuteAsync(createAdCommand, createAdHandler, _serviceProvider);


        createAdResult.Result.Should().BeTrue();
    }


    [Fact]
    public async Task Editing_An_Ad_With_Valid_Parameters_Should_Be_Success()
    {
        var mockId = Guid.NewGuid();
        var adEntityMock = AdEntity.Create(mockId, "Test", "Test Description", Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid());


        var mockAdImages = new List<ImageValueObject>()
        {
            new("TestFile1.png", "Image/png"),
            new("TestFile2.png", "Image/png"),
            new("TestFile3.png", "Image/png"),
        };

        mockAdImages.ForEach(c => adEntityMock.AddImage(c));

        var unitOfWorkMock = Substitute.For<IUnitOfWork>();
        var adRepositoryMock = Substitute.For<IAdRepository>();
        var fileServiceMock = Substitute.For<IFileService>();
        var categoryRepositoryMock = Substitute.For<ICategoryRepository>();
        var locationRepositoryMock = Substitute.For<ILocationRepository>();

        categoryRepositoryMock.GetCategoryByIdAsync(Arg.Any<Guid>())
            .Returns(Task.FromResult<CategoryEntity?>(new CategoryEntity("Test Category")));

        locationRepositoryMock.GetLocationByIdAsync(Arg.Any<Guid>())
            .Returns(Task.FromResult<LocationEntity?>(new LocationEntity("Test Location")));


        adRepositoryMock.GetAdByIdForUpdateAsync(mockId)!.Returns(Task.FromResult(adEntityMock));


        fileServiceMock.SaveFilesAsync(Arg.Any<List<SaveFileModel>>()).Returns(Task.FromResult(
            new List<SaveFileModelResult>()
            {
                new("TestFile4.png", "image/png")
            }));

        fileServiceMock.RemoveFilesAsync(Arg.Any<string[]>())
            .Returns(Task.CompletedTask);


        unitOfWorkMock.AdRepository.Returns(adRepositoryMock);
        unitOfWorkMock.LocationRepository.Returns(locationRepositoryMock);
        unitOfWorkMock.CategoryRepository.Returns(categoryRepositoryMock);


        var editAdCommand = new EditAdCommand(mockId, Guid.NewGuid(), Guid.NewGuid(), "Edited Title",
            "Edited Description", ["TestFile1.png"],
            [new EditAdCommand.AddNewImagesModel("Test Image Content", "image.png")]);

        var editAdCommandHandler = new EditAdCommandHandler(unitOfWorkMock, fileServiceMock);

        var editAdCommandResult =
            await Helpers.ValidateAndExecuteAsync(editAdCommand, editAdCommandHandler, _serviceProvider);


        editAdCommandResult.Result.Should().BeTrue();

        adEntityMock.Title.Should().BeEquivalentTo("Edited Title");

        adEntityMock.Description.Should().BeEquivalentTo("Edited Description");

        adEntityMock.Images.Should().NotContain(c => c.FileName.Equals("TestFile1.png"));

        adEntityMock.Images.Should().HaveCount(3);

        adEntityMock.Images.Should().Contain(c => c.FileName.Equals("TestFile4.png"));

        adEntityMock.CurrentState.Should().Be(AdEntity.AdStates.Pending);
    }


    [Fact]
    public async Task Getting_Add_Details_With_Valid_Parameters_Should_Success()
    {
        var userMock = new UserEntity("Test", "Test LastName", "TestUser", "Test@test.com")
        {
            PhoneNumber = "123456"
        };

        var locationMock = new LocationEntity("Test Location");

        var categoryMock = new CategoryEntity("Test Category");


        var adMock = AdEntity.Create("Test Title", "Test description", userMock, categoryMock, locationMock);


        var mockAdImages = new List<ImageValueObject>()
        {
            new("TestFile1.png", "Image/png"),
            new("TestFile2.png", "Image/png"),
            new("TestFile3.png", "Image/png"),
        };

        mockAdImages.ForEach(c => adMock.AddImage(c));


        var unitOfWorkMock = Substitute.For<IUnitOfWork>();
        var adRepositoryMock = Substitute.For<IAdRepository>();
        var fileServiceMock = Substitute.For<IFileService>();


        adRepositoryMock.GetAdDetailByIdAsync(adMock.Id)
            .Returns(Task.FromResult(adMock));

        fileServiceMock.GetFilesByNameAsync(Arg.Any<List<string>>())
            .Returns(Task.FromResult<List<GetFileModel>>([
                new("TestFileUrl1", "Image/png", "TestFile1.png"),
                new("TestFileUrl2", "Image/png", "TestFile2.png"),
                new("TestFileUrl3", "Image/png", "TestFile3.png")
            ]));


        unitOfWorkMock.AdRepository.Returns(adRepositoryMock);


        var queryModel = new GetAdDetailByIdQuery(adMock.Id);

        var mapper = _serviceProvider.GetRequiredService<IMapper>();
        var queryHandler = new GetAdDetailByIdQueryHandler(unitOfWorkMock, fileServiceMock, mapper);


        var result = await Helpers.ValidateAndExecuteAsync(queryModel, queryHandler, _serviceProvider);


        result.IsSuccess.Should().BeTrue();


        result.Result!.Title.Should().BeEquivalentTo(adMock.Title);
        result.Result!.CategoryName.Should().BeEquivalentTo(categoryMock.Name);
        result.Result!.OwnerUserName.Should().BeEquivalentTo(userMock.UserName);
        result.Result!.OwnerPhoneNumber.Should().BeEquivalentTo(userMock.PhoneNumber);

    }
}