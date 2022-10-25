using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.JsonWebTokens;
using NUnit.Framework;
using zedbank.Controllers;
using zedbank.Database;
using zedbank.Models;
using zedbank.Services;

namespace zedbank.Test;

public class WalletControllerTests
{
    private Context _context = null!;
    private WalletController _controller = null!;

    [SetUp]
    public void Setup()
    {
        var opts = new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _context = new Context(opts);
        WalletService.TestMode = true;
    }

    [Test]
    public async Task TestWalletFundingSuccess()
    {
        var userData = new UserRegistrationDto
        {
            Email = Faker.Internet.Email(),
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            Password = "StrongPassword1",
        };
        var existingUser = await UserService.RegisterUser(userData, _context);
        var wallet = existingUser.Wallets[0];
        var currentBalance = wallet.Balance;
        
        var claim = new Claim(JwtRegisteredClaimNames.Sub, existingUser.Id.ToString());
        var context = new ControllerContext { HttpContext = new DefaultHttpContext { User = new TestPrincipal(claim) } };
        var controller = new WalletController(_context, new NullLogger<WalletController>()) { ControllerContext = context };
        
        decimal amount = 1000.0m;
        var data = new WalletTransactionDto(amount.ToString("0.00"));
        var result = await controller.FundWallet(wallet.Id, data);
        var objResult = result.Result as ObjectResult;
        Assert.AreEqual(200, objResult!.StatusCode);

        var refreshedWallet = await _context.Wallets.FindAsync(wallet.Id);
        Assert.AreEqual(currentBalance + amount, refreshedWallet!.Balance);
    }
    
    [Test]
    public async Task TestWalletFundingInvalidAmount()
    {
        var userData = new UserRegistrationDto
        {
            Email = Faker.Internet.Email(),
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            Password = "StrongPassword1",
        };
        var existingUser = await UserService.RegisterUser(userData, _context);
        var wallet = existingUser.Wallets[0];
        
        var claim = new Claim(JwtRegisteredClaimNames.Sub, existingUser.Id.ToString());
        var context = new ControllerContext { HttpContext = new DefaultHttpContext { User = new TestPrincipal(claim) } };
        var controller = new WalletController(_context, new NullLogger<WalletController>()) { ControllerContext = context };
        
        decimal amount = -1000.0m;
        var data = new WalletTransactionDto(amount.ToString("0.00"));
        var result = await controller.FundWallet(wallet.Id, data);
        var objResult = result.Result as ObjectResult;
        Assert.AreEqual(400, objResult!.StatusCode);
    }
    
    [Test]
    public async Task TestWalletFundingWalletNotFound()
    {
        var userData = new UserRegistrationDto
        {
            Email = Faker.Internet.Email(),
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            Password = "StrongPassword1",
        };
        var existingUser = await UserService.RegisterUser(userData, _context);
        
        var claim = new Claim(JwtRegisteredClaimNames.Sub, existingUser.Id.ToString());
        var context = new ControllerContext { HttpContext = new DefaultHttpContext { User = new TestPrincipal(claim) } };
        var controller = new WalletController(_context, new NullLogger<WalletController>()) { ControllerContext = context };
        
        var data = new WalletTransactionDto(1000m.ToString("0.00"));
        var result = await controller.FundWallet(5000, data);
        var objResult = result.Result as ObjectResult;
        Assert.AreEqual(404, objResult!.StatusCode);
    }
    
    [Test]
    public async Task TestWalletFundingWalletNotOwner()
    {
        var userData = new UserRegistrationDto
        {
            Email = Faker.Internet.Email(),
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            Password = "StrongPassword1",
        };
        var existingUser = await UserService.RegisterUser(userData, _context);
        var wallet = existingUser.Wallets[0];
        
        var otherUserData = new UserRegistrationDto
        {
            Email = Faker.Internet.Email(),
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            Password = "StrongPassword1",
        };
        var otherExistingUser = await UserService.RegisterUser(otherUserData, _context);

        var claim = new Claim(JwtRegisteredClaimNames.Sub, otherExistingUser.Id.ToString());
        var context = new ControllerContext { HttpContext = new DefaultHttpContext { User = new TestPrincipal(claim) } };
        var controller = new WalletController(_context, new NullLogger<WalletController>()) { ControllerContext = context };
        
        decimal amount = 1000.0m;
        var data = new WalletTransactionDto(amount.ToString("0.00"));
        var result = await controller.FundWallet(wallet.Id, data);
        var objResult = result.Result as ObjectResult;
        Assert.AreEqual(403, objResult!.StatusCode);
    }
    
    [Test]
    public async Task TestWalletWithdrawal()
    {
        var userData = new UserRegistrationDto
        {
            Email = Faker.Internet.Email(),
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            Password = "StrongPassword1",
        };
        var existingUser = await UserService.RegisterUser(userData, _context);
        var wallet = existingUser.Wallets[0];
        wallet.Balance = 90000m;
        await _context.SaveChangesAsync();
        
        var currentBalance = wallet.Balance;
        var claim = new Claim(JwtRegisteredClaimNames.Sub, existingUser.Id.ToString());
        var context = new ControllerContext { HttpContext = new DefaultHttpContext { User = new TestPrincipal(claim) } };
        var controller = new WalletController(_context, new NullLogger<WalletController>()) { ControllerContext = context };
        
        decimal amount = 1000.0m;
        var data = new WalletTransactionDto(amount.ToString("0.00"));
        var result = await controller.WithdrawFromWallet(wallet.Id, data);
        var objResult = result.Result as ObjectResult;
        Assert.AreEqual(200, objResult!.StatusCode);

        var refreshedWallet = await _context.Wallets.FindAsync(wallet.Id);
        Assert.AreEqual(currentBalance - amount, refreshedWallet!.Balance);
    }
    
    [Test]
    public async Task TestWalletWithdrawalOverdraft()
    {
        var userData = new UserRegistrationDto
        {
            Email = Faker.Internet.Email(),
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            Password = "StrongPassword1",
        };
        var existingUser = await UserService.RegisterUser(userData, _context);
        var wallet = existingUser.Wallets[0];
        var currentBalance = wallet.Balance;
        
        var claim = new Claim(JwtRegisteredClaimNames.Sub, existingUser.Id.ToString());
        var context = new ControllerContext { HttpContext = new DefaultHttpContext { User = new TestPrincipal(claim) } };
        var controller = new WalletController(_context, new NullLogger<WalletController>()) { ControllerContext = context };
        
        decimal amount = 1000.0m;
        var data = new WalletTransactionDto(amount.ToString("0.00"));
        var result = await controller.WithdrawFromWallet(wallet.Id, data);
        var objResult = result.Result as ObjectResult;
        Assert.AreEqual(400, objResult!.StatusCode);
    }
    
    [Test]
    public async Task TestWalletWithdrawalInvalidAmount()
    {
        var userData = new UserRegistrationDto
        {
            Email = Faker.Internet.Email(),
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            Password = "StrongPassword1",
        };
        var existingUser = await UserService.RegisterUser(userData, _context);
        var wallet = existingUser.Wallets[0];
        
        var claim = new Claim(JwtRegisteredClaimNames.Sub, existingUser.Id.ToString());
        var context = new ControllerContext { HttpContext = new DefaultHttpContext { User = new TestPrincipal(claim) } };
        var controller = new WalletController(_context, new NullLogger<WalletController>()) { ControllerContext = context };
        
        decimal amount = -1000.0m;
        var data = new WalletTransactionDto(amount.ToString("0.00"));
        var result = await controller.WithdrawFromWallet(wallet.Id, data);
        var objResult = result.Result as ObjectResult;
        Assert.AreEqual(400, objResult!.StatusCode);
    }
    
    [Test]
    public async Task TestWalletWithdrawalWalletNotFound()
    {
        var userData = new UserRegistrationDto
        {
            Email = Faker.Internet.Email(),
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            Password = "StrongPassword1",
        };
        var existingUser = await UserService.RegisterUser(userData, _context);
        
        var claim = new Claim(JwtRegisteredClaimNames.Sub, existingUser.Id.ToString());
        var context = new ControllerContext { HttpContext = new DefaultHttpContext { User = new TestPrincipal(claim) } };
        var controller = new WalletController(_context, new NullLogger<WalletController>()) { ControllerContext = context };
        
        var data = new WalletTransactionDto(1000m.ToString("0.00"));
        var result = await controller.WithdrawFromWallet(5000, data);
        var objResult = result.Result as ObjectResult;
        Assert.AreEqual(404, objResult!.StatusCode);
    }
    
    [Test]
    public async Task TestWalletWithdrawalWalletNotOwner()
    {
        var userData = new UserRegistrationDto
        {
            Email = Faker.Internet.Email(),
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            Password = "StrongPassword1",
        };
        var existingUser = await UserService.RegisterUser(userData, _context);
        var wallet = existingUser.Wallets[0];
        
        var otherUserData = new UserRegistrationDto
        {
            Email = Faker.Internet.Email(),
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            Password = "StrongPassword1",
        };
        var otherExistingUser = await UserService.RegisterUser(otherUserData, _context);

        var claim = new Claim(JwtRegisteredClaimNames.Sub, otherExistingUser.Id.ToString());
        var context = new ControllerContext { HttpContext = new DefaultHttpContext { User = new TestPrincipal(claim) } };
        var controller = new WalletController(_context, new NullLogger<WalletController>()) { ControllerContext = context };
        
        decimal amount = 1000.0m;
        var data = new WalletTransactionDto(amount.ToString("0.00"));
        var result = await controller.WithdrawFromWallet(wallet.Id, data);
        var objResult = result.Result as ObjectResult;
        Assert.AreEqual(403, objResult!.StatusCode);
    }

    [Test]
    public async Task TestGetWalletTransactionHistorySuccess()
    {
        var userData = new UserRegistrationDto
        {
            Email = Faker.Internet.Email(),
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            Password = "StrongPassword1",
        };
        var existingUser = await UserService.RegisterUser(userData, _context);
        var wallet = existingUser.Wallets[0];
        await WalletService.FundWallet(wallet, new WalletTransactionDto("10000.00"), _context);
        await WalletService.FundWallet(wallet, new WalletTransactionDto("10000.00"), _context);
        await WalletService.FundWallet(wallet, new WalletTransactionDto("10000.00"), _context);
        await WalletService.WithdrawFromWallet(wallet, new WalletTransactionDto("1000.00"), _context);
        
        var claim = new Claim(JwtRegisteredClaimNames.Sub, existingUser.Id.ToString());
        var context = new ControllerContext { HttpContext = new DefaultHttpContext { User = new TestPrincipal(claim) } };
        var controller = new WalletController(_context, new NullLogger<WalletController>()) { ControllerContext = context };
        
        var result = await controller.GetWalletTransactionHistory(wallet.Id);
        var objResult = result.Result as ObjectResult;
        Assert.AreEqual(200, objResult!.StatusCode);
        var resObj = objResult.Value as List<TransactionDto>;
        Assert.AreEqual(4, resObj!.Count);
    }
    
    [Test]
    public async Task TestGetWalletTransactionHistoryNotOwner()
    {
        var userData = new UserRegistrationDto
        {
            Email = Faker.Internet.Email(),
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            Password = "StrongPassword1",
        };
        var existingUser = await UserService.RegisterUser(userData, _context);
        var wallet = existingUser.Wallets[0];
        
        var otherUserData = new UserRegistrationDto
        {
            Email = Faker.Internet.Email(),
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            Password = "StrongPassword1",
        };
        var otherExistingUser = await UserService.RegisterUser(otherUserData, _context);

        var claim = new Claim(JwtRegisteredClaimNames.Sub, otherExistingUser.Id.ToString());
        var context = new ControllerContext { HttpContext = new DefaultHttpContext { User = new TestPrincipal(claim) } };
        var controller = new WalletController(_context, new NullLogger<WalletController>()) { ControllerContext = context };
        
        var result = await controller.GetWalletTransactionHistory(wallet.Id);
        var objResult = result.Result as ObjectResult;
        Assert.AreEqual(403, objResult!.StatusCode);
    }
    
    [Test]
    public async Task TestGetWalletTransactionHistoryWalletNotFound()
    {
        var userData = new UserRegistrationDto
        {
            Email = Faker.Internet.Email(),
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            Password = "StrongPassword1",
        };
        var existingUser = await UserService.RegisterUser(userData, _context);

        var claim = new Claim(JwtRegisteredClaimNames.Sub, existingUser.Id.ToString());
        var context = new ControllerContext { HttpContext = new DefaultHttpContext { User = new TestPrincipal(claim) } };
        var controller = new WalletController(_context, new NullLogger<WalletController>()) { ControllerContext = context };
        
        var result = await controller.GetWalletTransactionHistory(50000);
        var objResult = result.Result as ObjectResult;
        Assert.AreEqual(404, objResult!.StatusCode);
    }
}