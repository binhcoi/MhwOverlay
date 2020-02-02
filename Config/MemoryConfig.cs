using System.Text;
using MhwOverlay.Core;

namespace MhwOverlay.Config
{
    public class MemoryConfig
    {
        public string ProcessName = "MonsterHunterWorld";

        public int ThreadsPerScan = 1;

        public BytePatternConfig PlayerDamagePattern = new BytePatternConfig(
            "PlayerDamagePattern",
            "48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 75 04 33 C9",
            "140000000",
            "163B0A000",
            WindowsApi.RegionPageProtection.PAGE_READONLY
            );

        public BytePatternConfig PlayerNamePattern = new BytePatternConfig(
            "PlayerNamePattern",
            "48 8B 0D ?? ?? ?? ?? 48 8D 54 24 38 C6 44 24 20 00 E8 ?? ?? ?? ?? 48 8B 5C 24 70 48 8B 7C 24 60 48 83 C4 68 C3",
            "140000000",
            "163B0A000",
            WindowsApi.RegionPageProtection.PAGE_READONLY
            );

        public BytePatternConfig MonsterPattern = new BytePatternConfig(
            "MonsterPattern",
            /*"48 8B 05 ?? ?? ?? ?? 8B 48 ?? 89 8F ?? ?? ?? ??",*/
            "48 8b 0d ?? ?? ?? ?? B2 01 E8 ?? ?? ?? ?? C6 83 ?? ?? ?? ?? ?? 48 8b 0d",
            "140000000",
            "163B0A000",
            WindowsApi.RegionPageProtection.PAGE_READONLY
            );

        public BytePatternConfig MonsterOffsetPattern = new BytePatternConfig(
            "MonsterOffsetPattern",
            /*"48 8B 8B ?? ?? ?? ?? 48 8B 01 FF 50 ?? 48 8B 8B ?? ?? ?? ?? E8 ?? ?? ?? ??  48 8B 8B ?? ?? ?? ?? B2 01 E8 ?? ?? ?? ??",*/
            "48 8B 8B ?? ?? ?? ?? 48 89 4A ?? 48 89 42 ?? 4C 8D 9C 24",
            "140000000",
            "163B0A000",
            WindowsApi.RegionPageProtection.PAGE_READONLY
            );

        public BytePatternConfig PlayerBuffPattern = new BytePatternConfig(
            "PlayerBuffPattern",
            "48 8B 05 ?? ?? ?? ?? 41 8B 94 00 ?? ?? ?? ?? 89 51 ??",
            "140000000",
            "163B0A000",
            WindowsApi.RegionPageProtection.PAGE_READONLY
            );

        public override string ToString(){
            var strBuilder = new StringBuilder();
            strBuilder.AppendLine($"ProcessName = {ProcessName}");
            strBuilder.AppendLine($"ThreadsPerScan = {ThreadsPerScan}");
            strBuilder.Append($"PlayerDamagePattern:\n{PlayerDamagePattern}");
            strBuilder.Append($"MonsterPattern:\n{MonsterPattern}");
            strBuilder.Append($"MonsterOffsetPattern:\n{MonsterOffsetPattern}");
            strBuilder.Append($"PlayerBuffPattern:\n{PlayerBuffPattern}");
            return strBuilder.ToString();
        }
    }

    public class BytePatternConfig
    {
        public string Name;
        public string PatternString;
        public string AddressRangeStart = "140000000";
        public string AddressRangeEnd = "145000000";
        public WindowsApi.RegionPageProtection[] PageProtections;

        public BytePatternConfig(string name, string patternString, string addressRangeStart, string addressRangeEnd, params WindowsApi.RegionPageProtection[] pageProtections)
        {
            Name = name;
            PatternString = patternString;
            AddressRangeStart = addressRangeStart;
            AddressRangeEnd = addressRangeEnd;
            PageProtections = pageProtections;
        }

        public override string ToString(){
            var strBuilder = new StringBuilder();
            strBuilder.AppendLine($"Name = {Name}");
            strBuilder.AppendLine($"PatternString = {PatternString}");
            strBuilder.AppendLine($"AddressRangeStart = {AddressRangeStart}");
            strBuilder.AppendLine($"AddressRangeEnd = {AddressRangeEnd}");
            strBuilder.Append("PageProtections = ");
            foreach (var item in PageProtections){
                strBuilder.Append($"{item},");
            }
            strBuilder.AppendLine();
            return strBuilder.ToString();
        }
    }
}