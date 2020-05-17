using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TauManager;

namespace TauManagerBot
{
    public class RegisteredDiscordUsersService: IRegisteredDiscordUsersService
    {
        private Dictionary<string, bool> _registeredUsers;

        public RegisteredDiscordUsersService(IServiceProvider serviceProvider)
        {
            _registeredUsers = new Dictionary<string, bool>();
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TauDbContext>();
                _registeredUsers = dbContext.Player.AsQueryable()
                    .Where(p => p.DiscordLogin != null)
                    .ToDictionary(p => p.DiscordLogin, p => true);
            }
        }

        public void AddUserToRegistered(string login)
        {
            if (string.IsNullOrWhiteSpace(login)) return;
            _registeredUsers[login] = true;
        }

        public bool IsUserRegistered(string login) => _registeredUsers.ContainsKey(login);
    }
}