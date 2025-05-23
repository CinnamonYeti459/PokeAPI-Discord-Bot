namespace PokeAPI_Discord_Bot
{
    public static class Utility
    {
        public static string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            return char.ToUpper(input[0]) + input.Substring(1);
        }

    }
}