using Adly.Application.Common;
using Adly.Application.Contracts.User;
using Adly.Application.Contracts.User.Models;
using Adly.Application.Extensions;
using Adly.Application.Features.Common;
using Adly.Application.Features.User.Commands.Register;
using Adly.Application.Features.User.Queries.PasswordLogin;
using Adly.Application.Tests.Extensions;
using Adly.Domain.Entities.User;
using Bogus;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit.Abstractions;

namespace Adly.Application.Tests;

public class UserFeaturesTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITestOutputHelper _testOutputHelper;

    public UserFeaturesTests(ITestOutputHelper testOutputHelper)
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.RegisterApplicationValidators();

        _serviceProvider = serviceCollection.BuildServiceProvider();
        _testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public async Task Creating_New_User_Should_Be_Success()
    {
        //Arrange
        var faker = new Faker();
        var password = faker.Random.String(10);
        var registerUserRequest = new RegisterUserCommand(faker.Person.FirstName
            , faker.Person.LastName
            , faker.Person.UserName
            , faker.Person.Email
            , faker.Person.Phone
            , password
            , password);

        var userManager = Substitute.For<IUserManager>();
        
        userManager
            .PasswordCreateAsync(Arg.Any<UserEntity>(), registerUserRequest.Password, CancellationToken.None)
            .Returns(Task.FromResult(IdentityResult.Success));
        
        //Act

        var userRegisterCommandHandler = new RegisterUserCommandHandler(userManager);

        var userRegisterResult = await userRegisterCommandHandler.Handle(registerUserRequest, CancellationToken.None);

        userRegisterResult.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public async Task User_Register_Email_Should_Be_Valid()
    {
        //Arrange
        var faker = new Faker();
        var password = Guid.NewGuid().ToString("N");
        var registerUserRequest = new RegisterUserCommand(faker.Person.FirstName
            , faker.Person.LastName
            , faker.Person.UserName
            , string.Empty
            , faker.Person.Phone
            , password
            , password);

        var userManager = Substitute.For<IUserManager>();
        
        userManager
            .PasswordCreateAsync(Arg.Any<UserEntity>(), registerUserRequest.Password, CancellationToken.None)
            .Returns(Task.FromResult(IdentityResult.Success));
        
        //Act

        var userRegisterCommandHandler = new RegisterUserCommandHandler(userManager);
        
        var validationBehavior =
            new ValidateRequestBehavior<RegisterUserCommand, OperationResult<bool>>(_serviceProvider.GetRequiredService<IValidator<RegisterUserCommand>>());
        


        var userRegisterResult = await validationBehavior.Handle(registerUserRequest, CancellationToken.None,userRegisterCommandHandler.Handle);

        userRegisterResult.IsSuccess.Should().BeFalse();
        
        _testOutputHelper.WritelineOperationResultErrors(userRegisterResult);
    }

    [Fact]
    public async Task Register_User_Password_And_Repeat_Password_Should_Be_Same()
    {
        //Arrange
        var faker = new Faker();
        
        var registerUserRequest = new RegisterUserCommand(faker.Person.FirstName
            , faker.Person.LastName
            , faker.Person.UserName
            , faker.Person.Email
            , faker.Person.Phone
            , Guid.NewGuid().ToString("N")
            , Guid.NewGuid().ToString("N"));

        var userManager = Substitute.For<IUserManager>();
        
        userManager
            .PasswordCreateAsync(Arg.Any<UserEntity>(), registerUserRequest.Password, CancellationToken.None)
            .Returns(Task.FromResult(IdentityResult.Success));
        
        //Act

        var userRegisterCommandHandler = new RegisterUserCommandHandler(userManager);
        var validationBehavior =
            new ValidateRequestBehavior<RegisterUserCommand, OperationResult<bool>>(_serviceProvider.GetRequiredService<IValidator<RegisterUserCommand>>());

        var userRegisterResult = await validationBehavior.Handle(registerUserRequest, CancellationToken.None,
            userRegisterCommandHandler.Handle);

        userRegisterResult.IsSuccess.Should().BeFalse();
        
        _testOutputHelper.WritelineOperationResultErrors(userRegisterResult);
    }

    [Fact]
    public async Task Password_Login_User_With_UserName_Should_Be_Success()
    {
        //Arrange
        var faker = new Faker();
        var loginQuery = new UserPasswordLoginQuery(faker.Person.UserName, Guid.NewGuid().ToString("N"));

        var userManager = Substitute.For<IUserManager>();

        var userEntity = new UserEntity(faker.Person.FirstName, faker.Person.LastName, faker.Person.UserName,
            faker.Person.Email);
        
        userManager.GetUserByUserNameAsync(loginQuery.UserNameOrEmail, CancellationToken.None)
            .Returns(Task.FromResult<UserEntity?>(userEntity));

        userManager.ValidatePasswordAsync(userEntity, loginQuery.Password, CancellationToken.None)
            .Returns(Task.FromResult<IdentityResult>(IdentityResult.Success));

        var jwtService = Substitute.For<IJwtService>();

        jwtService.GenerateTokenAsync(userEntity, CancellationToken.None)
            .Returns(Task.FromResult<JwtAccessTokenModel>(new JwtAccessTokenModel("AccessToken", 3000)));
        
        //Act
        var userLoginQueryHandler = new UserPasswordLoginQueryHandler(userManager, jwtService);

        var loginResult = await userLoginQueryHandler.Handle(loginQuery, CancellationToken.None);
        
        
        //Assert
        loginResult.Result.Should().NotBeNull();
        loginResult.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public async Task Password_Login_User_With_UserName_And_Wrong_Password_Should_Be_Failure()
    {
        //Arrange
        var faker = new Faker();
        var loginQuery = new UserPasswordLoginQuery(faker.Person.UserName, Guid.NewGuid().ToString("N"));

        var userManager = Substitute.For<IUserManager>();

        var userEntity = new UserEntity(faker.Person.FirstName, faker.Person.LastName, faker.Person.UserName,
            faker.Person.Email);
        
        userManager.GetUserByUserNameAsync(loginQuery.UserNameOrEmail, CancellationToken.None)
            .Returns(Task.FromResult<UserEntity?>(userEntity));

        userManager.ValidatePasswordAsync(userEntity, loginQuery.Password, CancellationToken.None)
            .Returns(Task.FromResult<IdentityResult>(IdentityResult.Failed()));

        var jwtService = Substitute.For<IJwtService>();

        jwtService.GenerateTokenAsync(userEntity, CancellationToken.None)
            .Returns(Task.FromResult<JwtAccessTokenModel>(new JwtAccessTokenModel("AccessToken", 3000)));
        
        //Act
        var userLoginQueryHandler = new UserPasswordLoginQueryHandler(userManager, jwtService);

        var loginResult = await userLoginQueryHandler.Handle(loginQuery, CancellationToken.None);
        
        
        //Assert
        loginResult.Result.Should().BeNull();
        loginResult.IsSuccess.Should().BeFalse();
        
        _testOutputHelper.WritelineOperationResultErrors(loginResult);
    }
    
    
    [Fact]
    public async Task Password_Login_User_With_Email_Should_Be_Success()
    {
        //Arrange
        var faker = new Faker();
        var loginQuery = new UserPasswordLoginQuery(faker.Person.Email, Guid.NewGuid().ToString("N"));

        var userManager = Substitute.For<IUserManager>();

        var userEntity = new UserEntity(faker.Person.FirstName, faker.Person.LastName, faker.Person.UserName,
            faker.Person.Email);
        
        userManager.GetUserByEmailAsync(loginQuery.UserNameOrEmail, CancellationToken.None)
            .Returns(Task.FromResult<UserEntity?>(userEntity));

        userManager.ValidatePasswordAsync(userEntity, loginQuery.Password, CancellationToken.None)
            .Returns(Task.FromResult<IdentityResult>(IdentityResult.Success));

        var jwtService = Substitute.For<IJwtService>();

        jwtService.GenerateTokenAsync(userEntity, CancellationToken.None)
            .Returns(Task.FromResult<JwtAccessTokenModel>(new JwtAccessTokenModel("AccessToken", 3000)));
        
        //Act
        var userLoginQueryHandler = new UserPasswordLoginQueryHandler(userManager, jwtService);

        var loginResult = await userLoginQueryHandler.Handle(loginQuery, CancellationToken.None);
        
        
        //Assert
        loginResult.Result.Should().NotBeNull();
        loginResult.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public async Task Password_Login_User_Not_Found_Should_Be_Success()
    {
        //Arrange
        var faker = new Faker();
        var loginQuery = new UserPasswordLoginQuery(faker.Person.Email, Guid.NewGuid().ToString("N"));

        var userManager = Substitute.For<IUserManager>();

        var userEntity = new UserEntity(faker.Person.FirstName, faker.Person.LastName, faker.Person.UserName,
            faker.Person.Email);
        
        userManager.GetUserByEmailAsync(loginQuery.UserNameOrEmail, CancellationToken.None)
            .Returns(Task.FromResult<UserEntity?>(null));

        userManager.ValidatePasswordAsync(userEntity, loginQuery.Password, CancellationToken.None)
            .Returns(Task.FromResult<IdentityResult>(IdentityResult.Success));

        var jwtService = Substitute.For<IJwtService>();

        jwtService.GenerateTokenAsync(userEntity, CancellationToken.None)
            .Returns(Task.FromResult<JwtAccessTokenModel>(new JwtAccessTokenModel("AccessToken", 3000)));
        
        //Act
        var userLoginQueryHandler = new UserPasswordLoginQueryHandler(userManager, jwtService);

        var loginResult = await userLoginQueryHandler.Handle(loginQuery, CancellationToken.None);
        
        
        //Assert
        loginResult.Result.Should().BeNull();
        loginResult.IsNotFound.Should().BeTrue();
        
        _testOutputHelper.WritelineOperationResultErrors(loginResult);
    }

    [Fact]
    public async Task Login_User_Inputs_Should_Be_Valid()
    {
        //Arrange
        var faker = new Faker();
        var loginQuery = new UserPasswordLoginQuery(faker.Person.Email, string.Empty);

        var userManager = Substitute.For<IUserManager>();

        var userEntity = new UserEntity(faker.Person.FirstName, faker.Person.LastName, faker.Person.UserName,
            faker.Person.Email);
        
        userManager.GetUserByEmailAsync(loginQuery.UserNameOrEmail, CancellationToken.None)
            .Returns(Task.FromResult<UserEntity?>(userEntity));

        userManager.ValidatePasswordAsync(userEntity, loginQuery.Password, CancellationToken.None)
            .Returns(Task.FromResult<IdentityResult>(IdentityResult.Success));

        var jwtService = Substitute.For<IJwtService>();

        jwtService.GenerateTokenAsync(userEntity, CancellationToken.None)
            .Returns(Task.FromResult<JwtAccessTokenModel>(new JwtAccessTokenModel("AccessToken", 3000)));
        
        //Act
        var userLoginQueryHandler = new UserPasswordLoginQueryHandler(userManager, jwtService);

        var validationBehavior =
            new ValidateRequestBehavior<UserPasswordLoginQuery, OperationResult<JwtAccessTokenModel>>(
               _serviceProvider.GetRequiredService<IValidator<UserPasswordLoginQuery>>());

        var loginResult = await validationBehavior.Handle(loginQuery, CancellationToken.None,userLoginQueryHandler.Handle);
        
        
        //Assert
        loginResult.Result.Should().BeNull();
        loginResult.IsSuccess.Should().BeFalse();
        
        _testOutputHelper.WritelineOperationResultErrors(loginResult);
    }
}