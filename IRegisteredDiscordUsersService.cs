using System.Collections.Generic;

namespace TauManagerBot
{
    public interface IRegisteredDiscordUsersService
    {
        bool IsUserRegistered(string login);
        void AddUserToRegistered(string login);
        bool AddOfficer(string login);
        bool RemoveOfficer(string login);
        List<string> GetOfficers();
        void ReloadOfficers();
        bool IsOfficer(string login);
    }
}