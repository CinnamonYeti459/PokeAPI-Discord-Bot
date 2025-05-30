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

        [Command("funfact")]
        public async Task FunFact(CommandContext ctx)
        {
            var funFacts = new List<string>
            {
                "💡 Pikachu was not the original mascot of Pokémon — it was Clefairy!",
                "🔥 Charizard is not a Dragon-type Pokémon unless it's in Mega Evolution X form.",
                "🐉 Gyarados was originally planned to be a Water/Dragon type, but was changed to Water/Flying.",
                "👻 The Pokémon Kadabra was banned from the TCG for years due to a lawsuit from a magician.",
                "🥚 Every Pokémon hatches from an egg — even Legendaries in non-canon games.",
                "👀 Wobbuffet's actual body might be its tail — not the big blue part!",
                "💪 Machamp can throw 500 punches in one second.",
                "🌕 Lunatone was first discovered at the site of a meteor crash.",
                "🔁 Ditto and Mew share almost identical stats and both can learn Transform.",
                "🧠 Alakazam’s brain constantly grows, giving it an IQ of 5000.",
                "🦇 Zubat has no eyes, but it navigates using echolocation.",
                "🌿 Bulbasaur is the only starter Pokémon that is part Poison-type.",
                "🐢 Blastoise’s water cannons can blast holes through steel.",
                "🔮 Espeon’s fur acts as a sensor to predict its opponent’s moves.",
                "🧊 Regice’s body is made of Antarctic ice that never melts.",
                "🦊 Ninetales is said to live for 1,000 years and curses anyone who touches its tails.",
                "🌩️ Jolteon generates electricity using the negative ions in the atmosphere.",
                "🌌 Deoxys’ form changes depending on the game version it's in.",
                "🕳️ Diglett’s true body is still a mystery — it might be huge underground.",
                "🐲 Dragonite can circle the globe in just 16 hours.",
                "🍥 Swirlix's fur is made of cotton candy — and it's sticky to the touch.",
                "🥶 Froslass freezes its prey and displays them like trophies.",
                "💀 Cubone wears the skull of its deceased mother.",
                "🎭 Zoroark can create illusions so realistic they affect all five senses.",
                "🌿 Chikorita waves its leaf to check the temperature and humidity.",
                "🔧 Magnemite is often seen at power plants absorbing electricity.",
                "🌊 Kyogre has the power to expand the oceans.",
                "🔥 Entei is said to erupt a volcano whenever it roars.",
                "🕒 Celebi can travel through time and appears during times of peace.",
                "🧬 Porygon was the first Pokémon to be made entirely of programming code.",
                "🗿 Nosepass always points north, like a living compass.",
                "🌕 Munna eats dreams and emits dream mist from its forehead.",
                "🎃 Gourgeist sings eerily on moonless nights.",
                "🐉 Salamence only achieved flight through intense desire over many generations.",
                "🪙 Meowth is one of the few Pokémon that can speak human language.",
                "👁️ Shedinja has a hole in its back — looking into it supposedly steals your soul.",
                "🧛‍♂️ Gliscor uses sound waves to track prey at night like a vampire bat.",
                "🧵 Banette is a doll that came to life due to a grudge.",
                "🛡️ Aegislash can control people with its spectral powers.",
                "🕷️ Galvantula uses electrically charged threads to trap prey.",
                "⚙️ Klink starts rotating when born and never stops.",
                "🌫️ Drifloon tries to steal children — it’s said to be formed from lost spirits.",
                "🌪️ Rayquaza lives in the ozone layer and calms the weather duels of Kyogre and Groudon.",
                "🪨 Sudowoodo pretends to be a tree, but it's actually a Rock-type.",
                "🌙 Darkrai can cause nightmares and becomes active during the new moon.",
                "🍽️ Snorlax’s diet consists of nearly 900 pounds of food per day.",
                "🐌 Magcargo's body temperature is hotter than the surface of the sun.",
                "🪦 Yamask carries a mask of its former human face and cries when it looks at it.",
                "🛸 Beheeyem can manipulate memories and is believed to come from space.",
                "🌊 Milotic is considered the most beautiful Pokémon and is known to calm hostility.",
                "🧟‍♂️ Phantump is created when a spirit possesses a tree stump.",
                "🏋️‍♂️ Conkeldurr teaches humans how to build using concrete.",
                "🦖 Tyrantrum was considered the king of the ancient world.",
                "👂 Exploud uses its voice to attack, communicate, and even drill through rock.",
                "🧼 Sinistea may be fake — some are said to be possessed cups instead of real antiques.",
                "🌱 Oddish plants itself during the day and moves at night to find better soil.",
                "⚡ Electrode is known as the 'Pokémon Bomb' because it can explode spontaneously.",
                "🦜 Pidgeot can fly at speeds up to 150 mph.",
                "💨 Ninjask is one of the fastest Pokémon, reaching speeds of 60 mph in flight.",
                "🛡️ Shieldon could survive a Tyrantrum’s attack due to its strong shield-like head.",
                "🕸️ Spinarak uses silk threads as traps to capture prey.",
                "🦀 Kingler’s claw can crush anything, even hard metal.",
                "🦄 Rapidash can run faster than a car at full speed.",
                "💥 Exploud’s sound waves are so powerful they can shatter boulders.",
                "❄️ Glaceon’s fur is like diamond-hard ice crystals.",
                "💎 Diancie can create diamonds by compressing carbon in its body.",
                "🐉 Haxorus has tusks that can cut through steel beams.",
                "🌟 Jirachi awakens every thousand years to grant wishes.",
                "🔥 Magmar’s body temperature is about 18,000°F, hotter than lava.",
                "🦇 Noibat uses ultrasonic waves to communicate and navigate in dark caves.",
                "💨 Talonflame is known for its incredible speed and agility in the air.",
                "🛶 Corsola can regenerate lost branches and heal itself.",
                "🧙‍♂️ Gardevoir can sense its Trainer’s feelings and will protect them at all costs.",
                "⚙️ Klefki collects keys and uses them to defend itself.",
                "🦢 Swanna is graceful and uses its wings to create powerful gusts.",
                "🌞 Solgaleo’s body is said to be made of the sun’s rays.",
                "🕷️ Ariados uses its venom to immobilize prey and defend itself.",
                "🎇 Volcanion can shoot steam hot enough to melt rock from its arms.",
                "🌬️ Tornadus is a legendary Pokémon known for causing violent storms.",
                "🧊 Vanilluxe can freeze moisture in the air to form ice crystals.",
                "🦅 Braviary is known for its courage and can carry a human in its talons.",
                "🌪️ Tornadus can create powerful tornadoes and gusts of wind.",
                "🌋 Heatran lives inside volcanoes and controls magma flow.",
                "🦉 Noctowl can see in the dark and rotate its head nearly 180 degrees.",
                "🕸️ Ariados spins strong webs that can trap even large Pokémon.",
                "🐬 Dewgong can swim backwards at high speeds.",
                "🌜 Lunala can absorb moonlight and open portals to other dimensions.",
                "🌿 Sawsbuck changes its fur and antlers based on the seasons.",
                "🐉 Dragapult can launch its young from its horns as projectiles.",
                "🐢 Torkoal’s shell can heat up to 1,100 degrees Fahrenheit.",
                "🐉 Hydreigon has three heads and can fly despite its heavy body.",
                "🐛 Caterpie uses its antenna to sense danger.",
                "🦀 Crabrawler fights with its powerful claws and can punch through rock.",
                "🌊 Lapras is known for ferrying people across water bodies.",
                "⚡ Pachirisu stores electricity in its cheeks like a squirrel.",
                "🧙‍♀️ Mismagius uses spells to curse its enemies.",
                "🌑 Darkrai hides in shadows and can put people to sleep with nightmares.",
                "🌟 Magearna was created by scientists and contains a soul inside its body.",
                "🦋 Beautifly has colorful wings and feeds on flower nectar.",
                "🦀 Kingler’s claws are so strong they can lift over 2200 lbs.",
                "🌿 Tropius has bananas growing from its neck and uses them as food.",
                "🦅 Talonflame is the fastest Pokémon to evolve mid-flight.",
                "🔥 Blaziken’s legs can burn up to 3200 degrees Fahrenheit.",
                "🦀 Crustle carries large rocks on its back as weapons and protection.",
                "⚡ Zapdos controls electricity and is one of the legendary birds.",
                "🌪️ Swellow can dive at over 200 mph to attack.",
                "🐍 Seviper uses its poisonous tail in battles with Zangoose.",
                "🦊 Vulpix’s six tails grow longer and stronger with age.",
                "🐦 Pidgeotto’s keen eyesight can spot prey from miles away.",
                "🕸️ Sableye lives in caves and can see in complete darkness.",
                "🐳 Wailord is the largest known Pokémon and can weigh over 4000 lbs.",
                "🦔 Sandslash curls into a ball and rolls to attack.",
                "🐇 Bunnelby uses its strong ears to dig underground.",
                "🦀 Kingler’s claw snaps so loudly it can stun opponents.",
                "🐦 Murkrow is considered a bad omen in some cultures.",
                "🌿 Leavanny weaves leaves into clothing for other Pokémon.",
                "🐉 Garchomp is a fast and powerful dragon with sharp fins.",
                "🦋 Butterfree releases toxic scales to defend itself.",
                "🐢 Turtwig uses the leaf on its head for photosynthesis.",
                "🦝 Zigzagoon’s zigzagging movement confuses predators.",
                "🌊 Starmie glows with a mysterious inner light.",
                "🦅 Fearow is capable of flying great distances at high speed.",
                "🔥 Talonflame uses its fiery feathers to intimidate foes.",
                "🧟‍♂️ Phantump is said to be the spirit of a child trapped in a tree stump."
            };

            var randomFact = funFacts[random.Next(funFacts.Count)];

            var embed = new DiscordEmbedBuilder()
                .WithTitle("🎓 Pokémon Fun Fact")
                .WithDescription(randomFact)
                .WithColor(DiscordColor.Goldenrod);

            await ctx.RespondAsync(embed.Build());
        }

        [Command("location")]
        public async Task GetLocation(CommandContext ctx, [RemainingText] string name)
        {
            Console.WriteLine($"Command received: location {name}");

            if (string.IsNullOrWhiteSpace(name))
            {
                await ctx.RespondAsync("Please specify a Pokémon name.");
                return;
            }

            var pokemon = await PokeApiWrapper.GetPokemonAsync(name);
            if (pokemon == null)
            {
                await ctx.RespondAsync("Pokémon not found!");
                return;
            }

            var embed = new DiscordEmbedBuilder()
                .WithTitle("🎓 Pokémon Fun Fact")
                .WithDescription($"Location: {pokemon.LocationAreaEncounters}")
                .WithColor(DiscordColor.SpringGreen);

            await ctx.RespondAsync(embed.Build());
        }

        [Command("scramble")]
        public async Task ScramblePokemon(CommandContext ctx)
        {
            // Get a random Pokémon
            int randomId = random.Next(1, 1026);
            var pokemon = await PokeApiWrapper.GetPokemonAsync(randomId.ToString());

            if (pokemon == null)
            {
                await ctx.RespondAsync("Failed to fetch a Pokémon. Try again!");
                return;
            }

            string originalName = pokemon.Name.ToLower();

            // Scrambled name
            string scrambled = ScrambleWord(originalName);

            // If it's the same scramble again
            if (scrambled == originalName)
                scrambled = ScrambleWord(originalName);

            var embed = new DiscordEmbedBuilder()
                .WithTitle("🧩 Scrambled Pokémon Name!")
                .WithDescription($"Unscramble this: **{scrambled}**\n_You have 15 seconds to guess!_")
                .WithColor(DiscordColor.Violet);

            await ctx.RespondAsync(embed.Build());

            // Wait for response
            var interactivity = ctx.Client.GetInteractivity();

            var guessResult = await interactivity.WaitForMessageAsync(m => m.ChannelId == ctx.Channel.Id && m.Author.Id == ctx.User.Id, TimeSpan.FromSeconds(15));

            if (!guessResult.TimedOut)
            {
                var guess = guessResult.Result.Content.Trim().ToLower();

                if (guess == originalName)
                {
                    await ctx.RespondAsync($"🎉 Correct! It was **{Utility.CapitalizeFirstLetter(originalName)}**!");
                }
                else
                {
                    await ctx.RespondAsync($"❌ Nope! The correct answer was **{Utility.CapitalizeFirstLetter(originalName)}**.");
                }
            }
            else
            {
                await ctx.RespondAsync($"⏰ Time's up! The answer was **{Utility.CapitalizeFirstLetter(originalName)}**.");
            }
        }

        // Scramble helper
        private static string ScrambleWord(string word)
        {
            var chars = word.ToCharArray(); // Turns word to an array
            var rng = new Random(); // Creates a random number generator var
            int n = chars.Length; //
            while (n > 1) // If there's more than 1 character to swap 
            {
                int k = rng.Next(n--); // Selects a random character to replace and removes a char from n to ensure the same char cannot be chosen
                (chars[n], chars[k]) = (chars[k], chars[n]); // Swaps the characters at n with k
            }
            return new string(chars);
        }

        [Command("togglecry")]
        [RequirePermissions(Permissions.ManageGuild)] // Manage Guild is server management permissions
        public async Task ToggleCryCommand(CommandContext ctx)
        {
            Globals.canSendCry = !Globals.canSendCry;
            string status = Globals.canSendCry ? "enabled" : "disabled";
            await ctx.RespondAsync($"Cry sounds are now **{status}**.");
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

    }
}