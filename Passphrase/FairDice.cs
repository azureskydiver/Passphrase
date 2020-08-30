using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Passphrase
{
    class FairDice
    {
        static RNGCryptoServiceProvider _rngCsp = new RNGCryptoServiceProvider();

        public static byte Roll(byte sideCount = 6)
            => (byte) Roll(Enumerable.Range(1, sideCount).ToList());

        public static T Roll<T>(IList<T> items)
        {
            if (items.Count <= 0 || items.Count >= 256)
                throw new ArgumentOutOfRangeException(nameof(items));

            var itemCount = (byte)items.Count;
            int fullSetsOfValues = Byte.MaxValue / itemCount;
            var randomNumber = new byte[1];
            do
            {
                _rngCsp.GetBytes(randomNumber);
            } while (!IsFairRoll(randomNumber[0]));
            return items[randomNumber[0] % itemCount];

            bool IsFairRoll(byte roll)
                => roll < itemCount * fullSetsOfValues;
        }
    }
}
