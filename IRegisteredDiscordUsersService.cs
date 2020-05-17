namespace TauManagerBot
{
    public interface IRegisteredDiscordUsersService
    {
         bool IsUserRegistered(string login);
         void AddUserToRegistered(string login);
    }
}