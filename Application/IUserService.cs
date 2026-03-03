using Haven.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Haven.Application
{
    public interface IUserService
    {
        Task<User> RegisterAsync(string username, string email, string password, string firstName = "", string lastName = "");
        Task<User> AuthenticateAsync(string username, string password);
        Task<User> GetByIdAsync(Guid id);
        Task<User> UpdateAsync(User user);
        Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    }
}
