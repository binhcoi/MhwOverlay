using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MhwOverlay.Log;
using System.Text.RegularExpressions;

namespace MhwOverlay.Core.Helpers
{
    public class Monster
    {
        // Doubly linked list
        public static readonly ulong MonsterStartOfStructOffset = 0x40;
        public static readonly ulong NextMonsterOffset = 0x18;
        public static readonly ulong PreviousMonsterOffset = 0x10;
        public static readonly ulong MonsterHealthComponentOffset = 0x7670;
        public static readonly ulong MaxHealth = 0x60;
        public static readonly ulong CurrentHealth = 0x64;
        public static readonly int IdLength = 32;
        public static readonly ulong IdOffset = 0x179;

        public ulong Address { get; set; }
        public string Id { get; set; }
        public float MaxHP { get; set; }
        public float HP { get; set; }
    }


    public static class MhwHelper
    {
        public static void UpdateMonsters(Process process, ulong monsterBaseList)
        {
            List<ulong> monsterAddresses = new List<ulong>();

            ulong firstMonster = MemoryHelper.Read<ulong>(process, monsterBaseList + Monster.PreviousMonsterOffset);

            if (firstMonster == 0x0)
            {
                firstMonster = monsterBaseList;
            }

            firstMonster += Monster.MonsterStartOfStructOffset;

            ulong currentMonsterAddress = firstMonster;
            while (currentMonsterAddress != 0)
            {
                monsterAddresses.Insert(0, currentMonsterAddress);
                currentMonsterAddress = MemoryHelper.Read<ulong>(process, currentMonsterAddress + Monster.NextMonsterOffset);
            }

            foreach (var monsterAddress in monsterAddresses)
            {
                var monster = UpdateMonster(process, monsterAddress);
                if (monster != null)
                {
                    Logger.Log(LogLevel.Info, $"{monster.Id}: {monster.HP}/{monster.MaxHP} at {monster.Address}");
                }
            }
        }

        public static Monster UpdateMonster(Process process, ulong monsterAddress)
        {
            var tmp = monsterAddress + Monster.MonsterStartOfStructOffset + Monster.MonsterHealthComponentOffset;
            var health_component = MemoryHelper.Read<ulong>(process, tmp);
            var id = MemoryHelper.ReadString(process, tmp + Monster.IdOffset, (uint)Monster.IdLength);
            var maxHealth = MemoryHelper.Read<float>(process, health_component + Monster.MaxHealth);

            if (string.IsNullOrEmpty(id))
                return null;

            id = id.Split('\\').Last();
            if (!(new Regex("em[0-9]").IsMatch(id)))
                return null;
                
            if (maxHealth <= 0)
                return null;
            var currentHealth = MemoryHelper.Read<float>(process, health_component + Monster.CurrentHealth);
            return new Monster()
            {
                Address = monsterAddress,
                Id = id,
                MaxHP = maxHealth,
                HP = currentHealth
            };
        }
    }
}