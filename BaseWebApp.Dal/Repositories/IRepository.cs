using System.Linq.Expressions;

namespace BaseWebApp.Dal.Repositories;

// The "where T : class" constraint means this interface can only be used with reference types (classes).
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    
    // Example: FindAsync(user => user.Email == "test@test.com")
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    
    Task AddAsync(T entity);
    void Remove(T entity);

    // A single method to save all changes made in a transaction.
    Task<int> SaveChangesAsync();
}