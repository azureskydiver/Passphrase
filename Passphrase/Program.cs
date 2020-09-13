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
        class Config
        {
            public string Resource { get; set; }
            public int WordCount { get; set; }
        }

        static Dictionary<string, Config> _wordListConfig = new Dictionary<string, Config>()
        {
            ["large"] = new Config() 
            {
                Resource = "Passphrase.EFFLargeWordlist.txt",
                WordCount = 6,
            },
            ["short"] = new Config()
            {
                Resource = "Passphrase.EFFShortWordlist1.txt",
                WordCount = 4,
            },
            ["memorable"] = new Config()
            {
                Resource = "Passphrase.EFFShortWordlist2.txt",
                WordCount = 4,
            },
        };

        static Config GetConfig(string key)
        {
            return _wordListConfig.TryGetValue(key.ToLower(), out Config config)
                    ? config
                    : _wordListConfig.First().Value;
        }

        static IEnumerable<string> LoadWordList(string resourceName)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var wordIndex = line.IndexOf('\t');
                if (wordIndex >= 0)
                {
                    yield return line.AsSpan()
                                     .Slice(wordIndex + 1)
                                     .Trim()
                                     .ToString();
                }
            }
        }

        static IEnumerable<string> GetWords(Config config)
        {
            var wordList = LoadWordList(config.Resource);
            var dice = new FairDie<string>(wordList);
            return Enumerable.Repeat(0, config.WordCount)
                             .Select(i => dice.Roll());
        }

        static void ChangeRandomWord(IList<string> words, Func<string, string> transform)
        {
            var dice = new FairIntDie(words.Count);
            int index = dice.Roll() - 1;
            words[index] = transform(words[index]);
        }

        static string MakeCapital(string word)
            => new CultureInfo("en-us", false).TextInfo.ToTitleCase(word);

        static string AddSymbol(string word)
        {
            var dice = new FairDie<char>(@"~!#$%^&*()-=+[]\{}:;""'<>?/");
            return word + dice.Roll();
        }

        static string AddNumber(string word)
        {
            var dice = new FairDie<int>(Enumerable.Range(0, 10));
            return word + string.Join("", Enumerable.Repeat(0, 3).Select(n => dice.Roll()));
        }

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
