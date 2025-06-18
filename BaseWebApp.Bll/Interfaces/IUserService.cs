using BaseWebApp.Bll.Common; // Add this using statement
using BaseWebApp.Dal.Entities;

namespace BaseWebApp.Bll.Interfaces;

public interface IUserService
{
    // Now returns a non-generic Result
    Task<Result> RegisterUserAsync(string email, string password);
    
    // Now returns a generic Result<User>
    Task<Result<User>> GetUserByEmailAsync(string email);
    Task<Result<User>> LoginAsync(string email, string password);
}