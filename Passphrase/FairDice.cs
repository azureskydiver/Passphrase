using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Passphrase
{
    class FairDice
    {
        static RNGCryptoServiceProvider _rngCsp = new RNGCryptoServiceProvider();

        public static byte Roll(byte sideCount = 6)
        {
            if (sideCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(sideCount));

            int fullSetsOfValues = Byte.MaxValue / sideCount;
            var randomNumber = new byte[1];
            do
            {
                _rngCsp.GetBytes(randomNumber);
            } while (!IsFairRoll(randomNumber[0]));
            return (byte)((randomNumber[0] % sideCount) + 1);

            bool IsFairRoll(byte roll)
                => roll < sideCount * fullSetsOfValues;
        }
    }
}
