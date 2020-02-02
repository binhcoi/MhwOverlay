using System;
using System.Collections.Generic;
using System.Globalization;
using MhwOverlay.Config;
using MhwOverlay.Log;

namespace MhwOverlay.Core
{
    public class BytePattern
    {
        public BytePatternConfig Config { get; }
        public byte?[] Bytes { get; private set; }
        public List<ulong> MatchedAddresses { get; private set; }
        public AddressRange AddressRange { get; private set; }

        public BytePattern(BytePatternConfig config)
        {
            Config = config;
            Bytes = BytesFromString(config.PatternString);
            MatchedAddresses = new List<ulong>();

            if (ulong.TryParse(config.AddressRangeStart, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong start)
                && ulong.TryParse(config.AddressRangeEnd, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong end))
            {
                AddressRange = new AddressRange(start, end);
            }
            else
            {
                AddressRange = new AddressRange(0, 0);
                Logger.Log(LogLevel.Error, $"Failed to parse address range ({config.AddressRangeStart} - {config.AddressRangeEnd}) for pattern: {config.PatternString}");
            }
        }

        public static byte?[] BytesFromString(string byteString)
        {
            List<byte?> byteList = new List<byte?>();

            var singleByteStrings = byteString.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var singleByteString in singleByteStrings)
            {
                byte parsedByte = 0;
                if (byte.TryParse(singleByteString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out parsedByte))
                {
                    byteList.Add(parsedByte);
                }
                else
                {
                    byteList.Add(null);
                }
            }

            return byteList.ToArray();
        }
    }
}