using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Passphrase
{
    class Program
    {
        const string DefaultWordListKey = "large";

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

        static void Main(string wordlist = "large", int count = -1)
        {
            var config = GetConfig(wordlist);
            if (count > 0)
                config.WordCount = count;
            var wordList = LoadWordList(config.Resource).ToDictionary(kvp => kvp.diceRoll, kvp => kvp.word);
            var words = Enumerable.Repeat(0, config.WordCount)
                                  .Select(i => GetDiceRoll(config.DiceCount))
                                  .Select(diceRoll => wordList[diceRoll]);
            Console.WriteLine(string.Join(" ", words));

            string GetDiceRoll(int diceCount)
                => string.Join("", Enumerable.Repeat(0, diceCount).Select(n => FairDice.Roll()));
        }
    }
}
