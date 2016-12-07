using System;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
            seen();

            // EVENT LISTENERS

            /* BM Buster
            client.MessageDeleted += async (s, e) =>
            {
                if (!e.Message.IsAuthor) { await e.Channel.SendMessage(e.User.Name + " deleted: " + e.Message.Text); }
            }; */

            // Join & Leave messages
            client.UserUpdated += async (cl, e) =>
            {
                var channel = e.Server.DefaultChannel;
                
                // Voice channel
                //var log_channel = e.Server.FindChannels("gideon").FirstOrDefault();
                if (e.Before.VoiceChannel != null && e.After.VoiceChannel == null)
                {
                    await channel.SendMessage("`" + name(e.After) + " has left " + e.Before.VoiceChannel + "`");
                }
                else if (e.Before.VoiceChannel != e.After.VoiceChannel)
                {
                    await channel.SendMessage("`" + name(e.After) + " has joined " + e.After.VoiceChannel + "`");
                }

                // Text channel
                
                if (e.Before.Status.Value.Equals("offline") && e.After.Status.Value.Equals("online") )
                {
                    await channel.SendMessage("`" + name(e.After) + " is now online `");
                }
                else if (e.Before.Status.Value.Equals("online") && e.After.Status.Value.Equals("offline"))
                {
                    await channel.SendMessage("`" + name(e.After) + " is now offline `");
                }

            };

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
            commands.CreateCommand("purge")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
            {
                if (e.User.ServerPermissions.Administrator)
                {
                    string parameter = e.GetArg("param");
                    string[] parameters = parameter.Split(' ');

                    // Download x number of messages
                    Message[] dl_msgs = await e.Channel.DownloadMessages(Int32.Parse(parameters[0]) + 1 );
                    List<Message> messages = new List<Message>();

                    // If a parameter has been supplied
                    if (parameter.Length != 0)
                    {
                        
                        // If a filter has been supplied
                        if (parameters.Length == 2)
                        {
                            foreach (var m in dl_msgs)
                            {
                                if (m.Text.Contains(parameters[1])) { messages.Add(m); }
                            }
                        }

                        // Otherwise just add all of them to delete
                        else { foreach (var m in dl_msgs) { messages.Add(m); } }

                    }
                    
                    // Delete the messages
                    await e.Channel.DeleteMessages(messages.ToArray());
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
                    string mult_pattern = "^.*\\*[0-9]*$";
                    string range_pattern = "^[0-9]*-[0-9]*$";
                    
                    // Check if a parameter has been supplied
                    if (parameter.Length != 0)
                    {
                        // Split parameter string by spaces
                        List<string> param_list = parameter.Split(' ').ToList<string>();

                        // Creates a new parameter list for the changes
                        List<string> new_param = new List<string>();

                        // Iterate through old parameters
                        foreach (string p in param_list)
                        {
                            // Check if current parameter is a multiplier
                            if (Regex.IsMatch(p,mult_pattern))
                            {
                                string[] p_split = p.Split('*');
                                int max = Int32.Parse(p_split[1]);

                                // Warn and end function if out of range
                                if (max < 1000) { for (int i = 0; i < max; ++i) { new_param.Add(p_split[0]); } }
                                else {
                                    await e.Channel.SendMessage("```diff\n- Select a multiplier less than 1000\n```");
                                    return;
                                }
                            }

                            // Else check if the parameter matches the pattern d*-d*
                            else if (Regex.IsMatch(p, range_pattern))
                            {
                                // Get the lower and upper bound in an array
                                string[] bounds = p.Split('-');
                                int min = Int32.Parse(bounds[0]);
                                int max = Int32.Parse(bounds[1]);

                                // Capping the range
                                if (min >= 0 && max <= 1000000) { for (int i = min; i < max + 1; ++i) { new_param.Add(i.ToString()); } }
                                else
                                {
                                    // Warn and end function if out of range
                                    await e.Channel.SendMessage("```diff\n- Range needs to be between [0 - 1,000,000]\n```");
                                    return;
                                }

                            }

                            // Otherwise add it normally
                            else { new_param.Add(p); } 
                        }
                        
                        // Converts the updated param list to array
                        string[] parameters = new_param.ToArray();

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

        // Last seen online -- wont work until bot is always online -- 
        private void seen()
        {
            commands.CreateCommand("seen")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string parameter = e.GetArg("param").ToLower();
                    var users = e.Server.Users;

                    // Search through all users on server
                    User found = null;
                    foreach (var u in users) { if (u.Name.ToLower().Equals(parameter)) { found = u; } }

                    // Send the resulting message
                    if (found != null) { await e.Channel.SendMessage( name(found) + " was last online at: " + found.LastOnlineAt.ToString()); }
                    else { await e.Channel.SendMessage("```diff\n- User not found\n```"); }

                });
        }

        // HELPER FUNCTIONS

        // Returns nickname if available
        private string name(User user)
        {
            if (user.Nickname != null) { return user.Nickname; }
            else { return user.Name; }
        }

        // Log to console
        private void Log(object sender, LogMessageEventArgs e) { Console.WriteLine(e.Message); }

    }
}
