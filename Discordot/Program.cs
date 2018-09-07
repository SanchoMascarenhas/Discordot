using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Discordot
{
    public class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            var client = new DiscordSocketClient();

            client.Log += Log;

            string token = "NDg3MjIxNTk2ODY4ODM3Mzg2.DnKgSw.5r2DkXA5cDL7oqmzzieAkq4IDVs"; // Remember to keep this private!
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            client.MessageReceived += MessageReceived;

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private async Task MessageReceived(SocketMessage message)
        {
            if (message.Content == "!ping")
            {
                Console.WriteLine(message.ToString());
                await message.Channel.SendMessageAsync("Pong!");
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
