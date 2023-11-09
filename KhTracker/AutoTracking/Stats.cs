using System.ComponentModel;
using System.Windows;

namespace KhTracker;

internal class Stats : INotifyPropertyChanged
{
    //next level check stuff
    // csharpier-ignore-start
    private readonly int[] levelChecks1 = { 1, 1 };
    private readonly int[] levelChecks50 = { 0, 2, 4, 7, 9, 10, 12, 14, 15, 17, 20, 23, 25, 28, 30, 32, 34, 36, 39, 41, 44, 46, 48, 50 };
    private readonly int[] levelChecks99 = { 0, 7, 9, 12, 15, 17, 20, 23, 25, 28, 31, 33, 36, 39, 41, 44, 47, 49, 53, 59, 65, 73, 85, 99 };
    private int[] currentCheckArray;
    private int nextLevelCheck;
    // csharpier-ignore-end

    public readonly int[] PreviousLevels = new int[3];
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
    private string weapon;
    public string Weapon
    {
        get => weapon;
        set
        {
            weapon = value;
            OnPropertyChanged(nameof(Weapon));
        }
    }
    private int strength;
    public int Strength
    {
        get => strength;
        set
        {
            strength = value;
            OnPropertyChanged(nameof(Strength));
        }
    }
    private int magic;
    public int Magic
    {
        get => magic;
        set
        {
            magic = value;
            OnPropertyChanged(nameof(Magic));
        }
    }
    private int defense;
    public int Defense
    {
        get => defense;
        set
        {
            defense = value;
            OnPropertyChanged(nameof(Defense));
        }
    }
    private int bonuslevel;
    public int BonusLevel
    {
        get => bonuslevel;
        set
        {
            bonuslevel = value;
            OnPropertyChanged(nameof(BonusLevel));
        }
    }

    //show next level check
    private readonly MainWindow window = (MainWindow)Application.Current.MainWindow;
    private int levelCheck;
    public int LevelCheck
    {
        get => levelCheck;
        set
        {
            levelCheck = value;
            window.NextLevelValue.Text = ">" + value;
            OnPropertyChanged(nameof(LevelCheck));
        }
    }

    public int Form;

    private readonly int levelAddress;
    private readonly int statsAddress;
    private readonly int formAddress;
    private readonly int bonusAddress;
    private readonly int nextSlotNum;

    private readonly int addressOffset;

    private readonly MemoryReader memory;

    public Stats(
        MemoryReader mem,
        int offset,
        int lvlAddress,
        int statsAddr,
        int formAddr,
        int bonusLvl,
        int nextSlot
    )
    {
        addressOffset = offset;
        memory = mem;
        levelAddress = lvlAddress;
        statsAddress = statsAddr;
        formAddress = formAddr;
        bonusAddress = bonusLvl;
        nextSlotNum = nextSlot;
    }

    // this is not working
    public event PropertyChangedEventHandler PropertyChanged = delegate { };

    public void OnPropertyChanged(string info)
    {
        var handler = PropertyChanged;
        handler?.Invoke(this, new PropertyChangedEventArgs(info));
    }

    public void UpdateMemory(int correctSlot)
    {
        var levelData = memory.ReadMemory(levelAddress + addressOffset, 2);

        Weapon = levelData[0] switch
        {
            0 when Weapon != "Sword" => "Sword",
            1 when Weapon != "Shield" => "Shield",
            2 when Weapon != "Staff" => "Staff",
            _ => Weapon,
        };

        PreviousLevels[0] = PreviousLevels[1];
        PreviousLevels[1] = PreviousLevels[2];
        PreviousLevels[2] = Level;

        if (Level != levelData[1])
            Level = levelData[1];

        var statsData = memory.ReadMemory(
            statsAddress - (nextSlotNum * correctSlot) + addressOffset,
            5
        );
        if (Strength != statsData[0])
            Strength = statsData[0];
        if (Magic != statsData[2])
            Magic = statsData[2];
        if (Defense != statsData[4])
            Defense = statsData[4];

        var modelData = memory.ReadMemory(formAddress + addressOffset, 1);
        Form = modelData[0];

        var bonusData = memory.ReadMemory(bonusAddress + addressOffset, 1);
        BonusLevel = bonusData[0];

        //change levelreward number
        if (level >= currentCheckArray[^1])
        {
            LevelCheck = currentCheckArray[^1];
            return;
        }

        if (Level >= currentCheckArray[nextLevelCheck])
        {
            nextLevelCheck++;
            LevelCheck = currentCheckArray[nextLevelCheck];
        }
    }

    public void SetMaxLevelCheck(int lvl)
    {
        currentCheckArray = lvl switch
        {
            50 => levelChecks50,
            99 => levelChecks99,
            _ => levelChecks1,
        };
    }

    public void SetNextLevelCheck(int lvl)
    {
        for (var i = 0; i < currentCheckArray.Length; i++)
        {
            if (lvl < currentCheckArray[i])
            {
                nextLevelCheck = i;
                LevelCheck = currentCheckArray[nextLevelCheck];
                break;
            }
        }
    }
}
