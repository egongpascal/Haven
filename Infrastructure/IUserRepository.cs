using Haven.Domain;
using Haven.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Haven.Infrastructure
{
    public interface IUserRepository
    {
        Task<User> CreateAsync(User user);
        Task<User> GetByIdAsync(Guid id);
        Task<User> GetByUsernameAsync(string username);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> UpdateAsync(User user);
    }
}
