using System;
using System.Configuration;
using System.Linq;

using Discord;
using Discord.Commands;


namespace Gideon

{
    class Bot
    {
        DiscordClient client;
        CommandService commands;

        // Constructor
        public Bot()
        {
            // New Client
            client = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Info;
                x.LogHandler = Log;
            });

            // Commands prefix
            client.UsingCommands(x =>
            {
                x.PrefixChar = '!';
                x.AllowMentionPrefix = true;
            });

            // Sets the commands service
            commands = client.GetService<CommandService>();

            // Load the commands
            sendMessage();
            purge();
            random();

            // Bot connection
            client.ExecuteAndWait(async () =>
            {
                string botToken = ConfigurationManager.AppSettings["BotToken"];
                await client.Connect(botToken, TokenType.Bot);
            });
        }

        // Commands that only needs a reply message
        private void sendMessage()
        {
            // !help
            commands.CreateCommand("help").Do(async (e) =>
            {
                await e.Channel.SendMessage("https://github.com/adrianau/Gideon");
            });
        }

        // !purge
        private void purge()
        {
            commands.CreateCommand("purge").Do(async (e) =>
            {
                if (e.User.ServerPermissions.Administrator)
                {
                    Message[] messages;
                    messages = await e.Channel.DownloadMessages(6);

                    await e.Channel.DeleteMessages(messages);
                }
                else { await e.Channel.SendMessage("You don't have permissions"); }
            });
        }

        // !random
        private void random()
        {
            commands.CreateCommand("random")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    if (e.GetArg("param").Length != 0)
                    {
                        // Split parameter string by spaces
                        string[] parameters = e.GetArg("param").Split(' ');

                        // Randoms an index in the string array
                        Random rnd = new Random();
                        string chosen = parameters[rnd.Next(parameters.Length)];
                        int occurences = parameters.Count(str => str.Equals(chosen));

                        // Calculates probability and converts to % format
                        string percent = ((double)occurences / parameters.Length).ToString("#0.##%");
                        await e.Channel.SendMessage(chosen + " [" + percent + "]");
                    }
                    else { await e.Channel.SendMessage("No parameters given"); }
                });
        }

        // Log to console
        private void Log(object sender, LogMessageEventArgs e) { Console.WriteLine(e.Message); }

    }
}
