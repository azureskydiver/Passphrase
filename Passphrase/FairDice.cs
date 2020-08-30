using System;
using System.Collections.Generic;
using System.CommandLine.Rendering.Views;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Passphrase
{
    class FairDice<T>
    {
        static RNGCryptoServiceProvider _rngCsp = new RNGCryptoServiceProvider();

        readonly List<T> _items;

        public FairDice(IEnumerable<T> items)
        {
            _items = items.ToList();
            if (_items.Count <= 0 || _items.Count >= 256)
                throw new ArgumentOutOfRangeException(nameof(items));
        }

        public T Roll()
        {
            var itemCount = (byte)_items.Count;
            int fullSetsOfValues = Byte.MaxValue / itemCount;
            var randomNumber = new byte[1];
            do
            {
                _rngCsp.GetBytes(randomNumber);
            } while (!IsFairRoll(randomNumber[0]));
            return _items[randomNumber[0] % itemCount];

            bool IsFairRoll(byte roll)
                => roll < itemCount * fullSetsOfValues;
        }
    }

    class FairDice : FairDice<int>
    {
        public FairDice(int sides = 6)
            : base(Enumerable.Range(1, sides))
        {
        }
    }
}
