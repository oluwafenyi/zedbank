using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using zedbank.Controllers;
using zedbank.Database;
using zedbank.Models;

namespace zedbank.Test;

public class UserControllerTests
{
    private Context _context = null!;
    private UserController _controller = null!;
    
    [SetUp]
    public void Setup()
    {
        var opts = new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _context = new Context(opts);
        _controller = new UserController(_context, new NullLogger<UserController>());
    }

    [Test]
    public async Task TestPostUserSuccess()
    {
        var password = "StrongPassword1";
        var email = Faker.Internet.Email();
        var data = new UserRegistrationDto
        {
            Email = email,
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            Password = password,
            ConfirmPassword = password,
        };

        var result = await _controller.PostUser(data);
        var objResult = result.Result as ObjectResult;
        Assert.AreEqual(201, objResult!.StatusCode!);
        var resObj = objResult.Value as UserDisplayDto;
        Assert.AreEqual(email, resObj!.Email);
    }

    [Test]
    public async Task TestPostUserShortPassword()
    {
        var password = "Strong";
        var email = Faker.Internet.Email();
        var data = new UserRegistrationDto
        {
            Email = email,
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            Password = password,
            ConfirmPassword = password,
        };
        
        var result = await _controller.PostUser(data);
        var objResult = result.Result as ObjectResult;
        Assert.AreEqual(400, objResult!.StatusCode!);
    }

    [Test]
    public async Task TestPostUserExistingEmail()
    {
        var email = Faker.Internet.Email();
        var existingUser = new User(email, Faker.Name.First(), Faker.Name.Last());
        existingUser.SetPassword("password");
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();
        
        var password = "StrongPassword1";
        var data = new UserRegistrationDto
        {
            Email = email,
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            Password = password,
            ConfirmPassword = password,
        };
        
        var result = await _controller.PostUser(data);
        var objResult = result.Result as ObjectResult;
        Assert.AreEqual(400, objResult!.StatusCode!);
    }
    
    [Test]
    public async Task TestPostUserNonMatchingPasswords()
    {
        var email = Faker.Internet.Email();
        var password = "StrongPassword1";
        var data = new UserRegistrationDto
        {
            Email = email,
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            Password = password,
            ConfirmPassword = password + "123",
        };
        
        var result = await _controller.PostUser(data);
        var objResult = result.Result as ObjectResult;
        Assert.AreEqual(400, objResult!.StatusCode!);
    }
}