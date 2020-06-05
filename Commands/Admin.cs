using System;
using System.Text;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace TauManagerBot.Commands
{
    public class Admin : MessageHandlerBase
    {
        public Admin(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override MessageResponse HandleMessage(string[] args, SocketUserMessage message)
        {
            if (args.Length < 2) return MessageResponse.NotHandled();
            if (args[0] != "!admin") return MessageResponse.NotHandled();
            if (message.Author.Username != "Dotsent" || message.Author.DiscriminatorValue != 5616) return MessageResponse.NotHandled();
            if (args[1] == "officer")
            {
                if (args.Length < 3) return MessageResponse.NotHandled();
                var officerListService = _serviceProvider.GetRequiredService<IRegisteredDiscordUsersService>();
                if (args[2] == "list")
                {
                    var officerList = new StringBuilder();
                    foreach(var officer in officerListService.GetOfficers())
                    {
                        officerList.AppendLine(officer);
                    }
                    return MessageResponse.HandledFormat("Current officers in Discord: \n{0}", officerList);
                } else if (args[2] == "add" && args.Length == 4) {
                    var result = officerListService.AddOfficer(args[3]);
                    if (result) return MessageResponse.Handled("Officer added OK.");
                    return MessageResponse.Handled("Officer addition failed.");
                } else if (args[2] == "remove" && args.Length == 4) {
                    var result = officerListService.RemoveOfficer(args[3]);
                    if (result) return MessageResponse.Handled("Officer removed OK.");
                    return MessageResponse.Handled("Officer removal failed.");
                } else if (args[2] == "reload") {
                    officerListService.ReloadOfficers();
                    return MessageResponse.Handled("Officer list reloaded from DB.");
                }
            } else if (args[1] == "say") {
                if (args.Length < 4) return MessageResponse.NotHandled();
                
            }
            return MessageResponse.Handled("Sorry, boss, I don't get it!");
        }
    }
}