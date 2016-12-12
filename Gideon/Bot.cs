using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Collections;
using System.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Discord;
using Discord.Audio;
using Discord.Commands;

using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using static Gideon.DictionaryData;


namespace Gideon

{
    class Bot
    {
        static DiscordClient client;
        static CommandService commands;
        static IAudioClient voiceClient;
        static bool playingAudio = false;
        static string lastOnline;

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
            client.UsingAudio(x => { x.Mode = AudioMode.Outgoing; });

            // Last online
            lastOnline = Directory.GetCurrentDirectory() + "\\last_online.txt";
            if (!File.Exists(lastOnline)) { using (File.Create(lastOnline)) { }; }

            // Load the commands
            sendMessage();
            purge();
            random();
            seen();
            calculate();
            lwiki();
            play();
            define();
            rank();
            tilt();
            
            // Join & Leave messages
            client.UserUpdated += async (s, e) =>
            {
                var channel = e.Server.DefaultChannel;

                if (!e.After.IsBot) // Don't notify if it's a bot joining/leaving
                {
                    // Voice channel
                    if (e.Before.VoiceChannel != null && e.After.VoiceChannel == null)
                    {
                        await channel.SendMessage("`" + name(e.After) + " has left " + e.Before.VoiceChannel + "`");
                    }
                    else if (e.Before.VoiceChannel != e.After.VoiceChannel)
                    {
                        await channel.SendMessage("`" + name(e.After) + " has joined " + e.After.VoiceChannel + "`");
                    }

                    // Text channel
                    if (e.Before.Status.Value.Equals("offline") && e.After.Status.Value.Equals("online"))
                    {
                        await channel.SendMessage("`" + name(e.After) + " is now online `");
                    }
                    else if (e.Before.Status.Value.Equals("online") && e.After.Status.Value.Equals("offline"))
                    {
                        await channel.SendMessage("`" + name(e.After) + " is now offline `");
                        updateSeen(e.After);
                    }
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
            // Displays help message
            commands.CreateCommand("help").Do(async (e) => {
                await e.Channel.SendMessage("https://github.com/adrianau/Gideon");
            });

        }

        // Deletes messages
        private void purge()
        {
            commands.CreateCommand("purge")
            .Parameter("param", ParameterType.Unparsed)
            .Do(async (e) => {
                if (e.User.ServerPermissions.Administrator)
                {
                    string parameter = e.GetArg("param");
                    string[] parameters = parameter.Split(' ');

                    // Download x number of messages
                    Message[] dl_msgs = await e.Channel.DownloadMessages(Int32.Parse(parameters[0]) + 1);
                    List<Message> messages = new List<Message>();

                    if (parameter.Length != 0)
                    { // If a parameter has been supplied

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

        //Solve a maths problem given as a string
        private float solve(String function)
        {
            String operations = "+-*/^";
            int total_ops = 0;
            char operation = ' ';

            //Replace factorial terms 
            function = get_factorial(function);

            //Find the least powerful operator
            for (int j = 0; j < operations.Length; j++)
            {
                for (int i = 0; i < function.Length; i++)
                {
                    if (function[i] == operations[j])
                    {
                        //Checks to do with a sign associated with a value i.e. -4
                        if (!((function[i] == '+' || function[i] == '-') && i == 0))
                        {
                            if ((int)function[i - 1] >= 48 && (int)function[i - 1] <= 57)
                            {
                                total_ops++;

                                if (total_ops == 1)
                                {
                                    operation = function[i];
                                }
                            }
                        }
                    }
                }
            }

            //Base case if function is just a single number
            if (total_ops == 0)
            {
                //Console.WriteLine(float.Parse(function));
                return float.Parse(function);
            }
            else
            {
                //Split the function into terms      
                String[] parameters = function.Split(new char[] { operation }, 2);

                //Console.WriteLine(parameters[0] + " " + parameters[1]);

                //Apply relevent operator and return value
                if (operation == '^')
                {
                    return (float)Math.Pow(solve(parameters[0]), solve(parameters[1]));
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

        //Return factorial of a number
        private int factorial(int value)
        {
            if (value < 2)
            {
                return 1;
            }

            return value * factorial(value - 1);
        }

        private string get_factorial(string function)
        {
            //Work out and replace any factorail terms
            for (int i = 0; i < function.Length; i++)
            {
                //Find factorial term
                if (function[i] == '!')
                {
                    int j = i - 1;
                    string value = "";

                    //Get the number to apply factorial to
                    while (j >= 0)
                    {
                        if ((int)function[j] >= 48 && (int)function[j] <= 57)
                        {
                            value = function[j] + value;
                            j--;
                        }
                        else
                        {
                            j = -1;
                        }
                    }

                    //Work out factorial and stick it back in the original function 
                    function = function.Replace(value + "!", factorial(Int32.Parse(value)).ToString());
                }
            }

            return function;
        }

        private string solve_brackets(string function)
        {
            int check = 0;
            bool write = false;
            string sub_function = "";
            string new_function = function;

            //Loop through input function
            for (int i = 0; i < function.Length; i++)
            {
                //Incorrect syntax check
                if (check < 0)
                {
                    i = function.Length;
                }
                else
                {
                    //Search for close brackets
                    if (function[i] == ')')
                    {
                        check--;

                        //If the bracket is the end of current pair
                        if (check == 0)
                        {
                            write = false;

                            //Replace the bracket pair with the solution to the pair
                            new_function = new_function.Replace('(' + sub_function + ')', solve_brackets(sub_function));

                            sub_function = "";
                        }
                    }

                    //Add characters to the sub function bracket pair
                    if (write)
                    {
                        sub_function += function[i];
                    }

                    //Check for open bracket
                    if (function[i] == '(')
                    {
                        check++;

                        //Start bracket pair
                        if (check == 1)
                        {
                            write = true;
                        }
                    }
                }
            }

            //Error in syntax
            if (check != 0)
            {
                Console.WriteLine("Incorrect Syntax");
                return "";
            }

            //Return solution
            return solve(new_function).ToString();
        }

        private void calculate()
        {
            commands.CreateCommand("cal")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string function = e.GetArg("param").Trim();

                    //Print the solved solution                  
                    await e.Channel.SendMessage(solve_brackets(function));
                });
        }

        //Function that returns how tilted a league player is
        private void tilt()
        {
            commands.CreateCommand("tilt")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string summoner_name = e.GetArg("param").Trim();

                    //Change summoner_name into a form to be used ion url
                    summoner_name = summoner_name.ToLower();
                    summoner_name = summoner_name.Replace(" ", "");

                    //Url to find players id
                    string URL = "https://euw.api.pvp.net/api/lol/euw/v1.4/summoner/by-name/" + summoner_name + "?api_key=" + ConfigurationManager.AppSettings["RiotKey"];

                    //Make request and turn into a json object
                    WebRequest request = WebRequest.Create(URL);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    string data = new StreamReader(response.GetResponseStream()).ReadToEnd();

                    JObject json = JObject.Parse(data);

                    //Find players id in the json
                    string summoner_id = (string)json[summoner_name]["id"];
                    string str;

                    try
                    {
                        //Url to look up players recent match histroy 
                        URL = "https://euw.api.pvp.net/api/lol/euw/v1.3/game/by-summoner/" + summoner_id + "/recent?api_key=" + ConfigurationManager.AppSettings["RiotKey"];

                        //Make request and turn into a json object
                        request = WebRequest.Create(URL);
                        response = (HttpWebResponse)request.GetResponse();
                        data = new StreamReader(response.GetResponseStream()).ReadToEnd();

                        json = JObject.Parse(data);

                        int deaths = 0;
                        int kills = 1;
                        int assists = 1;
                        int wins = 1;

                        //Loop through last three games
                        for (int i = 0; i < 3; i++)
                        {
                            //Add relevent information to variables
                            try
                            {
                                deaths += (int)json["games"][i]["stats"]["numDeaths"];
                            }
                            catch (Exception) { }

                            try
                            {
                                kills += (int)json["games"][i]["stats"]["championsKilled"];
                            }
                            catch (Exception) { }

                            try
                            {
                                assists += (int)json["games"][i]["stats"]["assists"];
                            }
                            catch (Exception) { }

                            try
                            {
                                wins += (int)json["games"][i]["stats"]["win"];
                            }
                            catch (Exception) { }
                        }

                        //Calculate tilt value
                        str = "Tilt Score: " + ((80 - 20 * wins) + (deaths * 50 / (assists + kills))).ToString();
                    }
                    catch (Exception)
                    {
                        //In case of http error
                        str = "Some error occured!";
                    }

                    //Return the tilt score
                    await e.Channel.SendMessage(str);
                });
        }

        //Function that returns a league players rank
        private void rank()
        {
            commands.CreateCommand("rank")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string summoner_name = e.GetArg("param").Trim();

                    //Change summoner_name into a form to be used ion url
                    summoner_name = summoner_name.ToLower();
                    summoner_name = summoner_name.Replace(" ", "");

                    //Url to find players id
                    string URL = "https://euw.api.pvp.net/api/lol/euw/v1.4/summoner/by-name/" + summoner_name + "?api_key=" + ConfigurationManager.AppSettings["RiotKey"];

                    //Make request and turn into a json object
                    WebRequest request = WebRequest.Create(URL);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    string data = new StreamReader(response.GetResponseStream()).ReadToEnd();

                    JObject json = JObject.Parse(data);

                    //Find players id in the json
                    string summoner_id = (string)json[summoner_name]["id"];
                    string rank;

                    try
                    {
                        //Url to look up players rank
                        URL = "https://euw.api.pvp.net/api/lol/euw/v2.5/league/by-summoner/" + summoner_id + "/entry?api_key=" + ConfigurationManager.AppSettings["RiotKey"];

                        //Make request and turn into a json object
                        request = WebRequest.Create(URL);
                        response = (HttpWebResponse)request.GetResponse();
                        data = new StreamReader(response.GetResponseStream()).ReadToEnd();

                        json = JObject.Parse(data);

                        //Find players rank in the json
                        rank = (string)json[summoner_id][0]["tier"] + " " + (string)json[summoner_id][0]["entries"][0]["division"];
                    }
                    catch (Exception)
                    {
                        //If no rank information is found 
                        rank = "UNRANKED";
                    }

                    //Return the rank                  
                    await e.Channel.SendMessage(rank);
                });
        }

        // Randoms a value from a list
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
                        if (Regex.IsMatch(p, mult_pattern))
                        {
                            string[] p_split = p.Split('*');
                            int max = Int32.Parse(p_split[1]);

                            // Warn and end function if out of range
                            if (max < 1000) { for (int i = 0; i < max; ++i) { new_param.Add(p_split[0]); } }
                            else
                            {
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
                    string percent = ((double)occurences / parameters.Length).ToString("#0.######%");
                    await e.Channel.SendMessage("`" + chosen + " [" + percent + "]`");
                }

                else { await e.Channel.SendMessage("```diff\n- No parameters given\n```"); }
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

        // Plays audio
        private void play()
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
                    if (playingAudio == true) return;

                    // Concats channel and its number and finds the Channel object for it
                    string channelName = "Channel " + channelNumber;
                    var chan = e.Server.FindChannels(channelName).FirstOrDefault();

                    // Concats the filepath and the specified songname
                    string fileName = songName.ToLower();
                    string fileURL = Directory.GetCurrentDirectory() + "\\" + fileName + ".mp3";

                    // Play the audio
                    playingAudio = true;
                    await SendAudio(fileURL, chan);
                }
                else { await e.Channel.SendMessage("`You don't have permissions`"); }
            });

            // Stop audio from being played
            commands.CreateCommand("stop")
                .Do(async (e) =>
                {
                    if (e.User.ServerPermissions.Administrator) { playingAudio = false; }
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

                    // Add in the "&& playingAudio" so that it only plays while true. For our cheesy skip command.
                    while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0 && playingAudio) // Read audio into our buffer, and keep a loop open while data is present
                    {
                        if (byteCount < blockSize)
                        {
                            // Incomplete Frame
                            for (int i = byteCount; i < blockSize; i++)
                                buffer[i] = 0;
                        }

                        voiceClient.Send(buffer, 0, blockSize); // Send the buffer to Discord
                    }
                    await PutTaskDelay(2000);
                    await voiceClient.Disconnect();
                    playingAudio = false;
                }
            }
            catch { Console.WriteLine("Something went wrong"); }
            await voiceClient.Disconnect();
        }

        // Last seen online
        private void seen()
        {
            commands.CreateCommand("seen")
            .Parameter("param", ParameterType.Unparsed)
            .Do(async (e) =>
            {
                string parameter = e.GetArg("param").ToLower();
                User found = null;

                // Check if we can find the user by name or nickname
                foreach (var u in e.Server.Users)
                {
                    if (u.Name.ToLower().Equals(parameter)) { found = u; }

                    // Else check if they have a nickname & if it matches
                    else if (u.Nickname != null)
                    {
                        if (u.Nickname.ToLower().Equals(parameter)) { found = u; }
                    }
                }

                // If we found the user...
                if (found != null)
                {

                    // Check if they're already online
                    if (found.Status.Value.Equals("online")) { await e.Channel.SendMessage("`" + name(found) + " is online right now `"); }
                    else
                    {

                        bool inFile = false;

                        // Read every line in file
                        foreach (string line in File.ReadLines(lastOnline))
                        {

                            // If id is in file
                            if (line.Contains(found.Id.ToString()))
                            {
                                inFile = true;
                                await e.Channel.SendMessage("`" + name(found) + " was last seen at " + line.Split(',')[1] + "`");
                            }
                        }

                        if (!inFile) { await e.Channel.SendMessage("`" + name(found) + " not seen yet`"); }
                    }
                }
                else { await e.Channel.SendMessage("`User not found `"); }
            });
        }

        // Updates the last seen online
        void updateSeen(User u)
        {

            var id = u.Id.ToString();
            var time = String.Format("{0:H:mmtt on ddd d MMM yyyy}", u.LastOnlineAt);
            string ln = null;

            // Read each line and check if user id is already on there
            foreach (string line in File.ReadLines(lastOnline))
            {
                if (line.Contains(id)) { ln = line; }
            }

            if (ln != null)
            { // If user id is in file, update the details 
                string text = File.ReadAllText(lastOnline);
                text = text.Replace(ln, id + "," + time);
                File.WriteAllText(lastOnline, text);

            }
            else
            { // If it isn't, append the details to new line
                using (StreamWriter file = new StreamWriter(lastOnline, true)) { file.WriteLine(id + "," + time + ""); }
            }

        }

        // Define
        void define()
        {
            commands.CreateCommand("define")
            .Parameter("param", ParameterType.Unparsed)
            .Do(async (e) =>
            {

                // Prepares the API url
                string parameter = e.GetArg("param").ToLower().Replace(' ', '_').Trim();
                string url = "https://od-api.oxforddictionaries.com:443/api/v1/entries/en/" + parameter;

                // Add authentication headers
                Dictionary<string, string> headers = new Dictionary<string, string>();
                string oxId = ConfigurationManager.AppSettings["OxfordId"];
                string oxKey = ConfigurationManager.AppSettings["OxfordKey"];
                headers.Add("app_id", oxId); headers.Add("app_key", oxKey);

                // Attempts the GET request
                string response = GET(url, headers);
                if (response == null) { await e.Channel.SendMessage("`Can't find the word`"); return; }  // No response

                // Data
                DictionaryResults data = JsonConvert.DeserializeObject<DictionaryResults>(response);
                ArrayList definitions = new ArrayList(); ArrayList examples = new ArrayList();

                // Iterate through the data
                foreach (Lexicalentry lex in data.results[0].lexicalEntries)
                {
                    foreach (Entry ent in lex.entries)
                    {
                        foreach (Sense sen in ent.senses)
                        {

                            // If there is a definition
                            if (sen.definitions != null)
                            {
                                if (definitions.Count != 2)
                                {
                                    definitions.Add("(" + lex.lexicalCategory + ") " + sen.definitions[0]);
                                }
                            }

                            // If there is an example
                            if (sen.examples != null)
                            {
                                if (examples.Count != 2)
                                {
                                    examples.Add("Example: " + '"' + sen.examples[0].text + '"');
                                }
                            }
                            else { examples.Add(""); }

                        }
                    }
                }

                // Fill the ArrayLists
                while (definitions.Count < 2) definitions.Add("");
                while (examples.Count < 2) examples.Add("");

                // Build the message
                var n = Environment.NewLine;
                string output = definitions[0] + n + examples[0] + n + n + definitions[1] + n + examples[1];
                await e.Channel.SendMessage("```diff" + n + parameter + n + "-" + n + output + n + "```");

                /* Audio playback disabled on Pi due to memory issues
                // Download pronunciation audio file
                string audio = data.results[0].lexicalEntries[0].pronunciations[0].audioFile;
                string download = Directory.GetCurrentDirectory() + "\\" + parameter + ".mp3";
                using (var wc = new WebClient()) { wc.DownloadFile(new Uri(audio), download); }

                // Don't play audio if something is already playing
                if (playingAudio == true) return;

                // Play the audio
                playingAudio = true;
                await SendAudio(download, e.User.VoiceChannel);
                */
            });
        }

        // GET request 
        string GET(string url, Dictionary<string, string> headers)
        {
            try
            {
                // New web request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";

                // Add headers if specified
                if (headers != null)
                {
                    foreach (var pair in headers)
                    {
                        request.Headers.Add(pair.Key, pair.Value);
                    }
                }

                // Get the response, read and return it
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream answer = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(answer)) { return reader.ReadToEnd(); }

            }
            catch { return null; }

        }

        // Returns nickname if available
        private string name(User user)
        {
            if (user.Nickname != null) { return user.Nickname; }
            else { return user.Name; }
        }

        // Delay (milliseconds)
        static async Task PutTaskDelay(int ms) { await Task.Delay(ms); }

        // Log to console
        private void Log(object sender, LogMessageEventArgs e) { Console.WriteLine(e.Message); }
    }
}

