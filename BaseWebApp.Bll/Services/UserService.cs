using BaseWebApp.Bll.Common; // Add this
using BaseWebApp.Bll.Interfaces;
using BaseWebApp.Dal.Entities;
using BaseWebApp.Dal.Repositories;

namespace BaseWebApp.Bll.Services;

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;

    public UserService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<User>> GetUserByEmailAsync(string email)
    {
        var users = await _userRepository.FindAsync(u => u.Email == email.ToLower());
        var user = users.FirstOrDefault();

        if (user is null)
        {
            // Use the Fail factory method
            return Result.Fail<User>("User with this email was not found.");
        }
        
        // Use the Ok factory method to wrap the user object
        return Result.Ok(user);
    }

    public async Task<Result> RegisterUserAsync(string email, string password)
    {
        var existingUserResult = await GetUserByEmailAsync(email);
        
        // This is a great example of the pattern. We check for success, not for null.
        if (existingUserResult.IsSucceed)
        {
            // Use the Fail factory method
            return Result.Fail("User with this email already exists.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLower(),
            PasswordHash = passwordHash
        };

        await _userRepository.AddAsync(newUser);
        await _userRepository.SaveChangesAsync();

        // Use the simple Ok factory method
        return Result.Ok();
    }
    
    public async Task<Result<User>> LoginAsync(string email, string password)
    {
        var userResult = await GetUserByEmailAsync(email);
        if (userResult.IsFailure)
        {
            return Result.Fail<User>("Invalid email or password.");
        }

        var user = userResult.Value!;
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return Result.Fail<User>("Invalid email or password.");
        }

        return Result.Ok(user);
    }
}