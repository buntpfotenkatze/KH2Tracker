using System;
using System.Collections;

namespace KhTracker;

internal class DriveForm : ImportantCheck
{
    public readonly int[] PreviousLevels = new int[3];
    private int level;
    private int visualLevel;
    public int Level
    {
        get => level;
        set
        {
            level = value;
            OnPropertyChanged(nameof(Level));
        }
    }
    public int VisualLevel
    {
        get => visualLevel;
        set
        {
            visualLevel = value;
            OnPropertyChanged(nameof(VisualLevel));
        }
    }
    private readonly int byteNum;
    private readonly int levelAddr;
    private readonly int genieFixAddr;
    public bool GenieFix;

    public DriveForm(
        MemoryReader mem,
        int address,
        int offset,
        int byteNumber,
        int levelAddress,
        string name
    )
        : base(mem, address, offset, name)
    {
        byteNum = byteNumber;
        levelAddr = levelAddress;
        Bytes = 2;
    }

    public DriveForm(
        MemoryReader mem,
        int address,
        int offset,
        int byteNumber,
        int levelAddress,
        int genieFixAddress,
        string name
    )
        : base(mem, address, offset, name)
    {
        byteNum = byteNumber;
        levelAddr = levelAddress;
        genieFixAddr = genieFixAddress;
        GenieFix = false;
        Bytes = 2;
    }

    public override byte[] UpdateMemory()
    {
        var data = base.UpdateMemory();
        var flag = new BitArray(data)[byteNum];
        if (Obtained == false && flag)
        {
            Obtained = true;
            //App.logger.Record(Name + " obtained");
        }

        var levelData = Memory.ReadMemory(levelAddr + AddressOffset, 1);
        PreviousLevels[0] = PreviousLevels[1];
        PreviousLevels[1] = PreviousLevels[2];
        PreviousLevels[2] = Level;

        VisualLevel = levelData[0];

        if (Level < levelData[0])
        {
            Level = levelData[0];
            if (App.Logger != null)
                App.Logger.Record(Name + " level " + Level + " obtained");
        }

        if (genieFixAddr != 0)
        {
            var genieData = Memory.ReadMemory(genieFixAddr + AddressOffset, 1);
            GenieFix = Convert.ToBoolean(genieData[0]);
        }

        return null;
    }
}
