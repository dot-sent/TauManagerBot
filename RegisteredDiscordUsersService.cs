using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TauManager;
using TauManager.BusinessLogic;

namespace TauManagerBot
{
    public class RegisteredDiscordUsersService: IRegisteredDiscordUsersService
    {
        private IServiceProvider _serviceProvider;
        private Dictionary<string, bool> _registeredUsers;
        private Dictionary<string, bool> _officers;

        public RegisteredDiscordUsersService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _registeredUsers = new Dictionary<string, bool>();
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TauDbContext>();
                _registeredUsers = dbContext.Player.AsQueryable()
                    .Where(p => p.DiscordLogin != null)
                    .ToDictionary(p => p.DiscordLogin, p => true);
            }
            ReloadOfficers();
        }

        public bool AddOfficer(string login)
        {
            if (string.IsNullOrWhiteSpace(login)) return false;
            using (var scope = _serviceProvider.CreateScope())
            {
                var integrationService = scope.ServiceProvider.GetRequiredService<IIntegrationLogic>();
                var result = integrationService.AddDiscordOfficer(login);
                if (!result) return false;
            }
            _officers[login] = true;
            return true;
        }

        public void AddUserToRegistered(string login)
        {
            if (string.IsNullOrWhiteSpace(login)) return;
            _registeredUsers[login] = true;
        }

        public List<string> GetOfficers()
        {
            return _officers.Keys.ToList();
        }

        public bool IsOfficer(string login)
        {
            return _officers.ContainsKey(login);
        }

        public bool IsUserRegistered(string login) => _registeredUsers.ContainsKey(login);

        public void ReloadOfficers()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var integrationService = scope.ServiceProvider.GetRequiredService<IIntegrationLogic>();
                var officers = integrationService.GetDiscordOfficerList();
                _officers = officers.ToDictionary(o => o, o => true);
            }
       }

        public bool RemoveOfficer(string login)
        {
            if (!IsOfficer(login) || string.IsNullOrEmpty(login)) return false;
            using (var scope = _serviceProvider.CreateScope())
            {
                var integrationService = scope.ServiceProvider.GetRequiredService<IIntegrationLogic>();
                var result = integrationService.RemoveDiscordOfficer(login);
                if (!result) return false;
            }
            _officers.Remove(login);
            return true;
        }
    }
}