using Discord;
using Discord.WebSocket;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Discordot
{
    public class Program
    {
        HttpClient httpClient = new HttpClient();
        
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
            if (!message.Content.StartsWith("!"))
                return;

            string[] command = message.Content.Split(" ");
            foreach(String s in command){
                Console.WriteLine(s);
            }

            switch (command[0]){
                case "!achievements":
                    Console.WriteLine("achievements:");
                    CallAchievementsApi(command, message);
                    break;

            }

            if (message.Content == "!ping")
            {
                Console.WriteLine(message.ToString());
                await message.Channel.SendMessageAsync("Pong!");
            }
        }

        private async void CallAchievementsApi(String[] command, SocketMessage message)
        {
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (command.Length <= 1)
            {
                using (httpClient = new HttpClient())
                {
                    HttpResponseMessage response = httpClient.GetAsync("https://api.guildwars2.com/v2/achievements").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var dataObjects = response.Content.ReadAsStringAsync().Result;

                        Console.WriteLine(dataObjects);
                        await message.Channel.SendMessageAsync(dataObjects.Substring(0, 1500) + "\n" +
                            "...\n" +
                            "Usage: !achievements [-option]\n" +
                            "where option include:\n" +
                            "    -id=[id1, id2, ...]    for specific achievemtns information\n" +
                            "    -daily[-mode]          for daily achievements\n" +
                            "    -tomorrowDaily[-mode]  for tomorrow daily achievements\n" +
                            "where mode include:\n" +
                            "    -pve                   for pve dailies\n" +
                            "    -pvp                   for pvp dailies\n" +
                            "    -wvw                   for wvw dailies\n" +
                            "    -fractals              for fractals dailies\n");

                    }
                }
                return;
            }

            string dailies = "";
            if (command[1].ToLower().StartsWith("-daily"))
            {
                dailies = DailyAchievements(command[1]);
            }
            else if (command[1].ToLower().StartsWith("-tomorrowdailies"))
            {
                dailies = TomorrowDailyAchievements(command[1]);
            }

            if(dailies =="")
                await message.Channel.SendMessageAsync("Could not find daily achievements...");
            else if (2000 <= dailies.Length)
                await message.Channel.SendMessageAsync(dailies.Substring(0, 1980) + "\n+ Others...");
            else
                await message.Channel.SendMessageAsync(dailies.Substring(0, dailies.Length));

        }

        private string TomorrowDailyAchievements(string args)
        {
            JObject jsonDailies;

            using (httpClient = new HttpClient())
            {
                HttpResponseMessage allDailiesRS = httpClient.GetAsync("https://api.guildwars2.com/v2/achievements/daily/tomorrow").Result;
                if (!allDailiesRS.IsSuccessStatusCode)
                    return "Request failed: " + allDailiesRS.ToString();

                string responseDailies = allDailiesRS.Content.ReadAsStringAsync().Result;
                jsonDailies = JObject.Parse(responseDailies);
            }

            return GetAchievementsByID(args, jsonDailies);

        }

        private string DailyAchievements(string args)
        {
            JObject jsonDailies;

            using (httpClient = new HttpClient())
            {
                HttpResponseMessage allDailiesRS = httpClient.GetAsync("https://api.guildwars2.com/v2/achievements/daily").Result;
                if (!allDailiesRS.IsSuccessStatusCode)
                    return "Request failed: " + allDailiesRS.ToString();

                string responseDailies = allDailiesRS.Content.ReadAsStringAsync().Result;
                jsonDailies = JObject.Parse(responseDailies);
            }

            return GetAchievementsByID(args, jsonDailies);
                        
        }

        private string GetAchievementsByID(string args, JObject jsonDailies)
        {
            //get dailies' IDs (all or by mode)
            List<string> dailyIDs = new List<string>();
            string[] tokens = args.Split("-");
            if (tokens.Length > 2)
            {
                string mode = tokens[2];
                foreach (var daily in jsonDailies[mode])
                    dailyIDs.Add((string)daily["id"]);
            }
            else
            {
                foreach (var mode in jsonDailies)
                {
                    foreach (var daily in jsonDailies[mode.Key])
                        dailyIDs.Add((string)daily["id"]);
                }

            }

            //concat daily IDs
            string IDarguments = "?ids=";
            foreach (string id in dailyIDs)
            {
                IDarguments += id + ",";
            }
            using (httpClient = new HttpClient())
            {
                HttpResponseMessage dailiesRS = httpClient.GetAsync("https://api.guildwars2.com/v2/achievements" + IDarguments).Result;

                if (!dailiesRS.IsSuccessStatusCode)
                    return "Request failed: " + dailiesRS.ToString();

                string responseDailiesInfo = dailiesRS.Content.ReadAsStringAsync().Result;
                JArray jsonDailiesInfo = JArray.Parse(responseDailiesInfo);
                string outputDailies = "";
                foreach (var dailyInfo in jsonDailiesInfo)
                {
                    outputDailies += "-" + (string)dailyInfo["name"] + ": " + (string)dailyInfo["requirement"] + "\n";
                }
                return outputDailies;
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
