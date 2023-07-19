using System;

namespace KhTracker;

internal class Ability : ImportantCheck
{
    private const int AddressStart = 0x2544;
    private const int AddressEnd = 0x25CC;

    private int level;
    public int Level
    {
        get => level;
        set
        {
            level = value;
            OnPropertyChanged(nameof(Level));
        }
    }

    private readonly int levelOffset;

    private readonly bool isGrowthAbility;

    public Ability(MemoryReader mem, int address, int offset, string name, int save)
        : base(mem, address, offset, name)
    {
        Bytes = 158;
        levelOffset = 0;
        Address = AddressStart + save;
        isGrowthAbility = false;
    }

    public Ability(MemoryReader mem, int address, int offset, int levOffset, string name)
        : base(mem, address, offset, name)
    {
        Bytes = 1;
        levelOffset = levOffset;
        isGrowthAbility = true;
    }

    public override byte[] UpdateMemory()
    {
        if (!isGrowthAbility)
        {
            var abilityData = base.UpdateMemory();
            for (var i = 0; i < abilityData.Length; i += 2)
            {
                if (abilityData[i + 1] == 1 && abilityData[i] == 159 && Name == "SecondChance")
                {
                    if (!Obtained)
                    {
                        Obtained = true;
                    }
                }
                if (abilityData[i + 1] == 1 && abilityData[i] == 160 && Name == "OnceMore")
                {
                    if (!Obtained)
                    {
                        Obtained = true;
                    }
                }
            }
            return null;
        }
        var data = base.UpdateMemory();
        int convertedData = data[0]; //BitConverter.To(data, 0);
        var equipped = 0;
        if (isGrowthAbility && convertedData > 0)
        {
            if (convertedData > 32768)
            {
                equipped = 32768;
            }

            var curLevel =
                convertedData
                - /*levelOffset - */
                equipped;
            //if (curLevel > Level)
            {
                Level = curLevel;
                if (curLevel > Level)
                    App.Logger?.Record(Name + " level " + Level + " obtained");
                else if (curLevel < Level)
                    App.Logger?.Record(Name + " level " + Level + " removed");
            }
        }
        else
        {
            Level = 0;
        }

        if (Obtained == false && Level >= 1)
        {
            Obtained = true;
            //App.logger.Record(Name + " obtained");
        }
        return null;
    }
}
