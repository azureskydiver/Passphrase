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
        readonly long _maxRange;
        readonly byte[] _buffer = new byte[sizeof(uint)];

        public FairDice(IEnumerable<T> items)
        {
            _items = items.ToList();
            if (_items.Count <= 0)
                throw new ArgumentOutOfRangeException(nameof(items));

            var bucketSize = uint.MaxValue / _items.Count;
            _maxRange = bucketSize * _items.Count;
        }

        public int RollInt()
        {
            uint roll;
            do
            {
                _rngCsp.GetBytes(_buffer);
                roll = BitConverter.ToUInt32(_buffer);
            } while (roll >= _maxRange);
            return (int)(roll % _items.Count);
        }

        public T Roll() => _items[RollInt()];
    }

    class FairDice : FairDice<int>
    {
        public FairDice(int sides = 6)
            : base(Enumerable.Range(1, sides))
        {
        }
    }
}
