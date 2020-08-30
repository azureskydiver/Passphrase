using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Passphrase
{
    class Program
    {
        const string DefaultWordListKey = "large";
        const string Symbols = @"~!#$%^&*()-=+[]\{}:;""'<>?/";

        class Config
        {
            public string Resource { get; set; }
            public int DiceCount { get; set; }
            public int WordCount { get; set; }
        }

        static Dictionary<string, Config> _wordListConfig = new Dictionary<string, Config>()
        {
            ["large"] = new Config() 
            {
                Resource = "Passphrase.EFFLargeWordlist.txt",
                DiceCount = 5,
                WordCount = 6,
            },
            ["short"] = new Config()
            {
                Resource = "Passphrase.EFFShortWordlist1.txt",
                DiceCount = 4,
                WordCount = 4,
            },
            ["memorable"] = new Config()
            {
                Resource = "Passphrase.EFFShortWordlist2.txt",
                DiceCount = 4,
                WordCount = 4,
            },
        };

        static Config GetConfig(string key)
        {
            key = key.ToLower();
            if (!_wordListConfig.ContainsKey(key))
                key = DefaultWordListKey;
            return _wordListConfig[key];
        }

        static IEnumerable<(string diceRoll, string word)> LoadWordList(string resourceName)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var parts = line.Split('\t');
                if (parts.Length == 2)
                    yield return (parts[0].Trim(), parts[1].Trim());
            }
        }

        static string GetDiceRoll(int diceCount, byte sides = 6)
            => string.Join("", Enumerable.Repeat(0, diceCount).Select(n => FairDice.Roll(sides)));

        static IEnumerable<string> GetWords(Config config)
        {
            var wordList = LoadWordList(config.Resource).ToDictionary(kvp => kvp.diceRoll, kvp => kvp.word);
            return Enumerable.Repeat(0, config.WordCount)
                             .Select(i => GetDiceRoll(config.DiceCount))
                             .Select(diceRoll => wordList[diceRoll]);
        }

        static void ChangeRandomWord(IList<string> words, Func<string, string> transform)
        {
            int index = FairDice.Roll((byte)words.Count) - 1;
            words[index] = transform(words[index]);
        }

        static string MakeCapital(string word)
            => new CultureInfo("en-us", false).TextInfo.ToTitleCase(word);

        static string AddSymbol(string word)
            => word + Symbols[FairDice.Roll((byte)Symbols.Length) - 1];

        static int[] _digits = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        static string AddNumber(string word)
            => word + string.Join("", Enumerable.Repeat(0, 3).Select(n => FairDice.Roll(_digits)));

        static void Main(string wordlist = "large",
                         int count = -1,
                         bool capitalize = false,
                         bool symbol = false,
                         bool number = false)
        {
            var config = GetConfig(wordlist);

            if (count > 0)
                config.WordCount = count;

            var words = GetWords(config).ToList();

            if (capitalize)
                ChangeRandomWord(words, MakeCapital);

            if (symbol)
                ChangeRandomWord(words, AddSymbol);

            if (number)
                ChangeRandomWord(words, AddNumber);

            Console.WriteLine(string.Join(" ", words));

        }
    }
}
