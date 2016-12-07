using System;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Discord;
using Discord.Audio;
using Discord.Commands;

using NAudio.Wave;
using System.Threading.Tasks;

namespace Gideon

{
    class Bot
    {
        static DiscordClient client;
        static IAudioClient voiceClient;
        CommandService commands;
        static bool playingSong = false;

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

            // AudioService
            client.UsingAudio(x =>
            {
                x.Mode = AudioMode.Outgoing;
            });

            // Load the commands
            sendMessage();
            purge();
            random();
            seen();
			calculate();
            lwiki();
            playAudio();
            //voteMute();

            // EVENT LISTENERS

            /* BM Buster
            client.MessageDeleted += async (s, e) =>
            {
                if (!e.Message.IsAuthor) { await e.Channel.SendMessage(e.User.Name + " deleted: " + e.Message.Text); }
            }; */

            // Join & Leave messages
            client.UserUpdated += async (s, e) =>
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

            /* Message updated
            client.MessageUpdated += async (s, e) =>
            {
                await e.Channel.SendMessage(e.Before.Text + ", " + e.After.State.ToString());
                await e.Channel.SendMessage(e.After.Text + ", " + e.After.State.ToString());
            }; */

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
		
		// Solve a maths problem given as a string
        private float solve(String function)
        {
            String operations = "-+*/^";
            int total_ops = 0;
            char operation = ' ';

            // Find the least powerful operator
            for (int j = 0; j < operations.Length; j++) 
            {
                for (int i = 0; i < function.Length; i++)
                {
                    if (function[i] == operations[j])
                    {
                        total_ops++;

                        if (total_ops == 1)
                        {
                            operation = function[i];
                        }
                    }
                }
            }

            // Base case if function is just a single number
            if (total_ops == 0)
            {
                return float.Parse(function);
            }
            else
            {
                // Split the function into terms
                String[] parameters = function.Split(operation);

                // Apply relevant operator and return value
                if (operation == '^')
                {
                    return (float)Math.Pow(solve(parameters[0]),solve(parameters[1]));
                }
                else if (operation == '*')
                {
                    return solve(parameters[0]) * solve(parameters[1]);
                }
                else if (operation == '/')
                {
                    return solve(parameters[0]) / solve(parameters[1]);
                }
                else if (operation == '+')
                {
                    return solve(parameters[0]) + solve(parameters[1]);
                }
                else if (operation == '-')
                {
                    return solve(parameters[0]) - solve(parameters[1]);
                }

                return 0;
            }
        }

        // Return factorial of a number
        private int factorial(int value)
        {
            if (value==1)
            {
                return 1;
            }

            return value * factorial(value - 1);
        }

        private void calculate()
        {          
            commands.CreateCommand("calculate")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {              
                    string function = e.GetArg("param").Trim();

                    // Work out and replace any factorial terms
                    for (int i = 0; i < function.Length; i++)
                    {
                        // Find factorial term
                        if (function[i] == '!')
                        {
                            int j = i+1;
                            String value = "";

                            // Get the number to apply factorial to
                            while (j < function.Length)
                            {
                                if ((int)function[j] >= 48 && (int)function[j] <= 57)
                                {
                                    value += function[j];
                                    j++;
                                }
                                else
                                {
                                    j = function.Length;
                                }
                            }

                            // Work out factorial and stick it back in the original function 
                            function = function.Replace("!"+value, factorial(Int32.Parse(value)).ToString());
                        }
                    }

                    // Print the solution
                    await e.Channel.SendMessage(solve(function).ToString());
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

        // Returns a page in the League wiki
        private void lwiki()
        {
            commands.CreateCommand("lwiki")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string parameter = e.GetArg("param").ToLower().Replace(' ', '_');
                    string baseURL = "http://leagueoflegends.wikia.com/wiki/";

                    await e.Channel.SendMessage(baseURL + parameter);
                });
        }
        
        // Play audio
        private void playAudio()
        {

            // Play an audio file
            commands.CreateCommand("play")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
            {
                if (e.User.ServerPermissions.Administrator)
                {
                    string parameter = e.GetArg("param");
                    string[] parameters = parameter.Split(' ');

                    // parameters[] split into songname and channel number
                    string songName = parameters[0];
                    string channelNumber = "";

                    // If channel number specified, set it -- otherwise default to user one
                    if (parameters.Length == 2) { channelNumber = parameters[1]; }
                    else { channelNumber = e.User.VoiceChannel.ToString().Split(' ')[1]; }

                    // Don't run function if song is already playing
                    if (playingSong == true) return;

                    // Concats channel and its number and finds the Channel object for it
                    string channelName = "Channel " + channelNumber;
                    var chan = e.Server.FindChannels(channelName).FirstOrDefault();
                    
                    // Concats the filepath and the specified songname
                    string fileName = songName.ToLower();
                    string fileURL = @"C:\Users\Adrian\Dropbox\OP.GGT\gideon\" + fileName + ".mp3";

                    // Play the audio
                    playingSong = true;
                    await SendAudio(fileURL, chan);
                    playingSong = false;
                }
                else { await e.Channel.SendMessage("`You don't have permissions`"); }
            });

            // Stop audio from being played
            commands.CreateCommand("stop")
                .Do(async (e) =>
                {
                    if (e.User.ServerPermissions.Administrator) { playingSong = false; }
                    else { await e.Channel.SendMessage("`You don't have permissions`"); }
                });
        }

        // Sends an audio from filepath over a voice channel
        // ## Not my code ##
        public static async Task SendAudio(string filepath, Channel voiceChannel)
        {
            // Join the voice channel
            voiceClient = await client.GetService<AudioService>().Join(voiceChannel);

            try
            {
                var channelCount = client.GetService<AudioService>().Config.Channels; // Get the number of AudioChannels our AudioService has been configured to use.
                var OutFormat = new WaveFormat(48000, 16, channelCount); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.

                using (var MP3Reader = new Mp3FileReader(filepath)) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
                using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat)) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
                {
                    resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                    int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
                    byte[] buffer = new byte[blockSize];
                    int byteCount;

                    // Add in the "&& playingSong" so that it only plays while true. For our cheesy skip command.
                    while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0 && playingSong) // Read audio into our buffer, and keep a loop open while data is present
                    {
                        if (byteCount < blockSize)
                        {
                            // Incomplete Frame
                            for (int i = byteCount; i < blockSize; i++)
                                buffer[i] = 0;
                        }

                        voiceClient.Send(buffer, 0, blockSize); // Send the buffer to Discord
                    }
                    await voiceClient.Disconnect();
                }
            }
            catch { System.Console.WriteLine("Something went wrong"); }
            await voiceClient.Disconnect();
        }

        /* !votemute
        private void voteMute()
        {
            commands.CreateCommand("votemute")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    await e.Channel.SendMessage("wip");
                });
        }*/

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
