﻿using System;
using System.Linq;
using System.Configuration;
using System.Text.RegularExpressions;

using Discord;
using Discord.Commands;
using System.Collections.Generic;

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

            // !test
            commands.CreateCommand("test").Do(async (e) =>
            {
                await e.Channel.SendMessage("`This sends a test message`");
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
                else { await e.Channel.SendMessage("`You don't have permissions`"); }
            });
        }

        // !random
        private void random()
        {
            commands.CreateCommand("random")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string parameter = e.GetArg("param");
                    string range_pattern = "^[0-9]*-[0-9]*$";

                    if (parameter.Length != 0)
                    {
                        // Split parameter string by spaces
                        string[] parameters = parameter.Split(' ');

                        // If the parameter matches the pattern d*-d*
                        if (Regex.IsMatch(parameter, range_pattern))
                        {
                            // Get the lower and upper bound in an array
                            string[] bounds = parameter.Split('-');
                            int min = Int32.Parse(bounds[0]);
                            int max = Int32.Parse(bounds[1]);

                            // Capping the range
                            if (min >= 0 && max <= 1000000)
                            {
                                // Add all the numbers to a list
                                List<string> nums = new List<string>();
                                for (int i = min; i < max + 1; ++i)
                                {
                                    nums.Add(i.ToString());
                                }

                                parameters = nums.ToArray();

                            } else
                            {
                                // Warn and end function if out of range
                                await e.Channel.SendMessage("```diff\n- Range needs to be between [0 - 1,000,000]\n```");
                                return;
                            }

                        }

                        // Randoms an index in the string array
                        Random rnd = new Random();
                        string chosen = parameters[rnd.Next(parameters.Length)];
                        int occurences = parameters.Count(str => str.Equals(chosen));

                        // Calculates probability and converts to % format
                        string percent = ( (double) occurences / parameters.Length ).ToString("#0.######%");
                        await e.Channel.SendMessage("`" + chosen + " [" + percent + "]`");
                    }
                    else { await e.Channel.SendMessage("```diff\n- No parameters given\n```"); }
                });
        }

        // Log to console
        private void Log(object sender, LogMessageEventArgs e) { Console.WriteLine(e.Message); }

    }
}
