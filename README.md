# PokeAPI-Discord-Bot

A C# Discord bot that fetches Pokémon info and plays a guess-the-Pokémon game using PokeAPI.

## Pokémon Bot Commands

🔍 `!pokemon [name] [shiny (optional)]`  
Shows information about a specific Pokémon by name. You can add **shiny** at the end to get the shiny version.  
Examples:  
!pokemon bulbasaur  
!pokemon bulbasaur shiny  
![Showcase of how you request pokemon information](Images/Pokemon.png)

🔀 `!random [shiny (optional)]`  
Fetches a random Pokémon with a 50% chance to be shiny automatically. You can also force shiny with **shiny** (optional).  
Examples:  
!random  
!random shiny

❓ `!guess`  
Starts a guessing game where a hidden Pokémon’s image is displayed and you have 15 seconds to guess which Pokémon it is!  
![Showcase of how the guess game works](Images/Guess.png)

😭 `!togglecry`  
Toggles whether the bot sends Pokémon cry audio files in chat.  
*Only server managers can use this command.*


## How it works

This bot uses the [PokemonApiWrapper](https://github.com/CinnamonYeti459/PokemonApiWrapper) library to interact with the official PokeAPI, retrieving Pokémon data such as stats, sprites, and more.


## Getting Started

- Download the repo
- Download DSharpPlus, DSharpPlus.CommandsNext, DSharpPlus.Interactivity, Newtonsoft.Json and my API wrapper for PokeAPI on my GitHub
- Add a `config.json` file to your VS project to include your own bot token and prefix.  
  Use the following format:
`{
  "Token": "your-bot-token-here",
  "Prefix": "!"
}`

- Invit the bot to your server and have fun


## Coming Soon

- More detailed Pokémon information, including abilities, ~~various sprites (like shiny versions)~~, and additional features.


## License

This project is open source. Feel free to contribute or report issues!
