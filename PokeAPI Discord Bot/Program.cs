using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Newtonsoft.Json.Linq;
using PokeAPI_Discord_Bot;

class Program
{
    static async Task Main(string[] args)
    {
        var configJson = JObject.Parse(File.ReadAllText("config.json")); // Reads config.json
        var token = configJson["Token"].ToString(); // Fetches the token
        var prefix = configJson["Prefix"].ToString(); // Fetches the prefix

        var discord = new DiscordClient(new DiscordConfiguration
        {
            Token = token,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents // ALlows bot to read messages
        });

        discord.UseInteractivity(new InteractivityConfiguration // Enables interactivity for the guess game and more
        {
            Timeout = TimeSpan.FromSeconds(15) // Doesn't make the bot wait forever for user input
        });

        var commands = discord.UseCommandsNext(new CommandsNextConfiguration // Allows the bot to use !pokemon commands
        {
            StringPrefixes = new[] { prefix } // Sets the prefix
        });

        commands.RegisterCommands<PokeCommands>(); // Register the commands class

        commands.CommandExecuted += async (s, e) => // Log command execution
        {
            Console.WriteLine($"Command '{e.Command.Name}' executed successfully by {e.Context.User.Id}.");
        };

        commands.CommandErrored += async (s, e) =>         // Log command errors with exception details
        {
            if (e.Exception is DSharpPlus.CommandsNext.Exceptions.CommandNotFoundException)
                return;

            Console.WriteLine($"Command '{e.Command?.Name ?? "Unknown"}' failed: {e.Exception.GetType()}: {e.Exception.Message}");
        };

        await discord.ConnectAsync(); // Connects the bot with the chosen token and configuration
        await Task.Delay(-1); // Keeps the bot running indefinitely until the application ends
    }
}