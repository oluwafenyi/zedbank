using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zedbank.Models;

[Table("users"), Index(nameof(Email), IsUnique = true)]
public class User
{
    [Column("password"), Required]
    private byte[]? _password;
    
    public long Id { get; set; }
    public string Email { get; set; }
    
    [Column(TypeName = "varchar(100)")]
    public string FirstName { get; set; }
    
    [Column(TypeName = "varchar(100)")]
    public string LastName { get; set; }

    public List<Wallet> Wallets { get; } = new();

    public User(string email, string firstName, string lastName)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }
    
    public void SetPassword(string password)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        var hashBytes = System.Text.Encoding.Unicode.GetBytes(hash);
        this._password = hashBytes;
    }

    public bool CheckPassword(string password)
    {
        if (_password == null)
        {
            return false;
        }
        var text = System.Text.Encoding.Unicode.GetString(_password);
        return BCrypt.Net.BCrypt.Verify(password, text);
    }
}

public class UserRegistrationDto
{
    public string Email { get; }
    public string FirstName { get; }
    public string LastName { get;  }
    public string Password { get; }
    public string ConfirmPassword { get; }
    
    public UserRegistrationDto(string email, string firstName, string lastName, string password, string confirmPassword)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        Password = password;
        ConfirmPassword = confirmPassword;
    }
}

public class UserDisplayDto
{
    public long Id { get; }
    public string Email { get; }
    public string FirstName { get; }
    public string LastName { get; }
    
    public List<UserWalletDisplayDto> Wallets { get; }

    public UserDisplayDto(User user)
    {
        Id = user.Id;
        Email = user.Email;
        FirstName = user.FirstName;
        LastName = user.LastName;
        Wallets = user.Wallets.Select(w => new UserWalletDisplayDto(w)).ToList();
    }
}

public class UserAuthDto
{
    public string? Email { get; set; }
    public string? Password { get; set;  }
}