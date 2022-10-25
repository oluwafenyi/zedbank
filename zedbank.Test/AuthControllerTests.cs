using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.JsonWebTokens;
using NUnit.Framework;
using zedbank.Controllers;
using zedbank.Core;
using zedbank.Database;
using zedbank.Models;
using zedbank.Services;

namespace zedbank.Test;

public class AuthControllerTests
{
    private Context _context = null!;
    private AuthController _controller = null!;
    
    [SetUp]
    public void Setup()
    {
        var opts = new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _context = new Context(opts);
        _controller = new AuthController(_context);
        ConfigurationHelper.InitializeTestConfig();
    }

    [Test]
    public async Task TestUserGenerateAuthTokenSuccess()
    {
        var email = Faker.Internet.Email();
        var password = "StrongPassword1";
        var existingUser = new User(email, Faker.Name.First(), Faker.Name.Last());
        existingUser.SetPassword(password);
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var data = new UserAuthDto
        {
            Email = email,
            Password = password,
        };
        var result = await _controller.GenerateAuthToken(data);
        var objResult = result.Result as ObjectResult;
        Assert.AreEqual(200, objResult!.StatusCode!);
        var resObj = objResult.Value as Token;
        Assert.IsNotEmpty(resObj!.AccessToken);
    }
    
    [Test]
    public async Task TestUserGenerateAuthNoAccountExists()
    {
        var data = new UserAuthDto
        {
            Email = Faker.Internet.Email(),
            Password = "password",
        };
        var result = await _controller.GenerateAuthToken(data);
        var objResult = result.Result as ObjectResult;
        Assert.AreEqual(401, objResult!.StatusCode!);
    }

    [Test]
    public async Task TestGetAuthUser()
    {
        var email = Faker.Internet.Email();
        var data = new UserRegistrationDto
        {
            Email = email,
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            Password = "StrongPassword1",
        };
        var existingUser = await UserService.RegisterUser(data, _context);

        var claim = new Claim(JwtRegisteredClaimNames.Sub, existingUser.Id.ToString());
        var context = new ControllerContext { HttpContext = new DefaultHttpContext { User = new TestPrincipal(claim) } };
        var controller = new AuthController(_context) { ControllerContext = context };
        
        var result = await controller.GetAuthUser();
        var objResult = result.Result as ObjectResult;
        Assert.AreEqual(200, objResult!.StatusCode!);
        var resObj = objResult.Value as UserDisplayDto;
        Assert.AreEqual(email, resObj!.Email);
    }
}