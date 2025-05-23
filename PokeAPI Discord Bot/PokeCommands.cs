using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using PokeAPI_Wrapper;

namespace PokeAPI_Discord_Bot
{
    public class PokeCommands : BaseCommandModule
    {
        private static readonly Random random = new();

        [Command("commandlist")]
        public async Task GetCommandList(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
                .WithTitle("📘 Bot Commands")
                .WithDescription("Here are the available commands you can use:")
                .AddField("🔍 !pokemon [name]", "Shows information about a specific Pokémon. You can add `shiny` at the end. Examples:\n" +
                                                  "`!pokemon bulbasaur`\n`!pokemon bulbasaur shiny`", false)
                .AddField("🔀 !random", "Fetches a random Pokémon, with a 50% chance to be shiny. Example:\n" +
                                             "`!random` or `!random shiny` (optional, but shiny is automatic anyway)", false)
                .AddField("❓ !guess", "Starts a guessing game where you must identify a hidden Pokémon within 15 seconds.", false)
                .AddField("😭 !togglecry", "Toggles whether the bot sends Pokémon cry audio files in chat. Only server managers can use this command.", false)
                .AddField("💻 GitHub", "[PokeBot Source Code](https://github.com/CinnamonYeti459/PokeAPI-Discord-Bot)", false)
                .WithColor(DiscordColor.Gold)
                .WithFooter("PokeBot • Powered by DSharpPlus & PokeAPI Library Wrapper");

            await ctx.RespondAsync(embed.Build());
        }

        [Command("pokemon")]
        public async Task GetPokemon(CommandContext ctx, [RemainingText] string name)
        {
            Console.WriteLine($"Command received: pokemon {name}");

            if (string.IsNullOrWhiteSpace(name))
            {
                await ctx.RespondAsync("Please specify a Pokémon name.");
                return;
            }

            bool isShiny = false;

            if (name.EndsWith(" shiny", StringComparison.OrdinalIgnoreCase)) // Check if "shiny" is specified at the end
            {
                isShiny = true;
                name = name[..^6].TrimEnd(); // Removes the last 6 characters, which will be " shiny" if the statement is true
            }

            var pokemon = await PokeApiWrapper.GetPokemonAsync(name);
            if (pokemon == null)
            {
                await ctx.RespondAsync("Pokémon not found!");
                return;
            }

            await SendPokemonEmbed(ctx, pokemon, isShiny);
        }

        [Command("random")]
        public async Task GetRandomPokemon(CommandContext ctx)
        {
            Console.WriteLine($"Command received: random");

            int randomId = random.Next(1, 1026);
            var pokemon = await PokeApiWrapper.GetPokemonAsync(randomId.ToString());

            if (pokemon == null)
            {
                await ctx.RespondAsync("Failed to fetch a random Pokémon.");
                return;
            }

            bool isShiny = random.Next(2) == 1; // 50/50

            await SendPokemonEmbed(ctx, pokemon, isShiny);
        }

        [Command("guess")]
        public async Task GuessPokemon(CommandContext ctx)
        {
            int randomId = random.Next(1, 1026); // List of registered pokemon in PokeAPI

            var pokemon = await PokeApiWrapper.GetPokemonAsync(randomId.ToString());
            if (pokemon == null)
            {
                await ctx.RespondAsync("Failed to get a Pokémon. Try again!");
                return;
            }

            var embed = new DiscordEmbedBuilder()
                .WithTitle("Guess the Pokémon!")
                .WithImageUrl(pokemon.Sprites?.FrontDefault)
                .WithColor(DiscordColor.Gold);

            await ctx.RespondAsync(embed: embed); // Send the Pokémon image only

            var interactivity = ctx.Client.GetInteractivity();

            var guessResult = await interactivity.WaitForMessageAsync(
                m => m.ChannelId == ctx.Channel.Id && m.Author.Id == ctx.User.Id,
                TimeSpan.FromSeconds(15));

            if (!guessResult.TimedOut)
            {
                var guess = guessResult.Result.Content.Trim().ToLower();

                if (guess == pokemon.Name.ToLower())
                {
                    await ctx.RespondAsync($"Correct! It's {Utility.CapitalizeFirstLetter(pokemon.Name)}! 🎉");
                }
                else
                {
                    await ctx.RespondAsync($"Nope, the answer was {Utility.CapitalizeFirstLetter(pokemon.Name)}.");
                }
            }
            else
            {
                await ctx.RespondAsync($"Time's up! The answer was {Utility.CapitalizeFirstLetter(pokemon.Name)}.");
            }
        }

        private static async Task SendPokemonEmbed(CommandContext ctx, Pokemon pokemon, bool isShiny)
        {
            string title = Utility.CapitalizeFirstLetter(pokemon.Name);
            if (isShiny)
                title += " (Shiny)";
            title += $" (ID: {pokemon.Id})";

            var embed = new DiscordEmbedBuilder()
                .WithTitle(title)
                .WithDescription($"Height: {pokemon.Height}\nWeight: {pokemon.Weight}\nBase Experience: {pokemon.BaseExperience}");

            if (isShiny)
            {
                bool shinyType = random.Next(2) == 1;
                if (shinyType)
                    embed.WithThumbnail(pokemon.Sprites?.FrontShiny)
                         .WithColor(DiscordColor.Goldenrod);
                else
                    embed.WithThumbnail(pokemon.Sprites?.FrontShinyFemale ?? pokemon.Sprites?.FrontShiny)
                         .WithColor(DiscordColor.Goldenrod);
            }
            else
            {
                embed.WithThumbnail(pokemon.Sprites?.FrontDefault)
                     .WithColor(DiscordColor.Gold);
            }

            // Send the embed first
            await ctx.RespondAsync(embed.Build());

            // Then send the audio file
            if (!string.IsNullOrEmpty(pokemon.Cries?.Latest.ToString()) && Globals.canSendCry)
            {
                using var httpClient = new HttpClient();
                var audioStream = await httpClient.GetStreamAsync(pokemon.Cries.Latest);
                var messageBuilder = new DiscordMessageBuilder()
                    .WithContent($"Here's the cry for {title}")
                    .AddFile($"{title} cry.ogg", audioStream);

                await ctx.Channel.SendMessageAsync(messageBuilder);
            }
        }

        [Command("togglecry")]
        [RequirePermissions(Permissions.ManageGuild)] // Manage Guild is server management permissions
        public async Task ToggleCryCommand(CommandContext ctx)
        {
            Globals.canSendCry = !Globals.canSendCry;
            string status = Globals.canSendCry ? "enabled" : "disabled";
            await ctx.RespondAsync($"Cry sounds are now **{status}**.");
        }

    }
}