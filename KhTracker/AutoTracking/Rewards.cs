using System;
using System.Collections.Generic;

namespace KhTracker;

internal class Rewards
{
    public readonly List<Tuple<int, string>> SwordChecks;
    public readonly List<Tuple<int, string>> ShieldChecks;
    public readonly List<Tuple<int, string>> StaffChecks;
    public readonly List<Tuple<int, string>> ValorChecks;
    public readonly List<Tuple<int, string>> WisdomChecks;
    public readonly List<Tuple<int, string>> LimitChecks;
    public readonly List<Tuple<int, string>> MasterChecks;
    public readonly List<Tuple<int, string>> FinalChecks;

    public readonly int AddressOffset;
    private readonly int bt10;

    private readonly MemoryReader memory;

    public Rewards(MemoryReader mem, int offset, int bt10)
    {
        AddressOffset = offset;
        memory = mem;
        this.bt10 = bt10;
        SwordChecks = new List<Tuple<int, string>>();
        ShieldChecks = new List<Tuple<int, string>>();
        StaffChecks = new List<Tuple<int, string>>();
        ValorChecks = new List<Tuple<int, string>>();
        WisdomChecks = new List<Tuple<int, string>>();
        LimitChecks = new List<Tuple<int, string>>();
        MasterChecks = new List<Tuple<int, string>>();
        FinalChecks = new List<Tuple<int, string>>();
        ReadRewards();
    }

    // populate reward lists
    public void ReadRewards()
    {
        // if sword
        ReadReward(bt10 + 0x25940, 2, SwordChecks, 2);
        ReadReward(bt10 + 0x25960, 2, SwordChecks, 4);
        ReadReward(bt10 + 0x25990, 2, SwordChecks, 7);
        ReadReward(bt10 + 0x259B0, 2, SwordChecks, 9);
        ReadReward(bt10 + 0x259C0, 2, SwordChecks, 10);
        ReadReward(bt10 + 0x259E0, 2, SwordChecks, 12);
        ReadReward(bt10 + 0x25A00, 2, SwordChecks, 14);
        ReadReward(bt10 + 0x25A10, 2, SwordChecks, 15);
        ReadReward(bt10 + 0x25A30, 2, SwordChecks, 17);
        ReadReward(bt10 + 0x25A60, 2, SwordChecks, 20);
        ReadReward(bt10 + 0x25A90, 2, SwordChecks, 23);
        ReadReward(bt10 + 0x25AB0, 2, SwordChecks, 25);
        ReadReward(bt10 + 0x25AE0, 2, SwordChecks, 28);
        ReadReward(bt10 + 0x25B00, 2, SwordChecks, 30);
        ReadReward(bt10 + 0x25B10, 2, SwordChecks, 31); // 99
        ReadReward(bt10 + 0x25B20, 2, SwordChecks, 32);
        ReadReward(bt10 + 0x25B30, 2, SwordChecks, 33); // 99
        ReadReward(bt10 + 0x25B40, 2, SwordChecks, 34);
        ReadReward(bt10 + 0x25B60, 2, SwordChecks, 36);
        ReadReward(bt10 + 0x25B90, 2, SwordChecks, 39);
        ReadReward(bt10 + 0x25BB0, 2, SwordChecks, 41);
        ReadReward(bt10 + 0x25BE0, 2, SwordChecks, 44);
        ReadReward(bt10 + 0x25C00, 2, SwordChecks, 46);
        ReadReward(bt10 + 0x25C10, 2, SwordChecks, 47); // 99
        ReadReward(bt10 + 0x25C20, 2, SwordChecks, 48);
        ReadReward(bt10 + 0x25C30, 2, SwordChecks, 49); // 99
        ReadReward(bt10 + 0x25C40, 2, SwordChecks, 50);
        ReadReward(bt10 + 0x25C70, 2, SwordChecks, 53); // 99
        ReadReward(bt10 + 0x25CD0, 2, SwordChecks, 59); // 99
        ReadReward(bt10 + 0x25D30, 2, SwordChecks, 65); // 99
        ReadReward(bt10 + 0x25DB0, 2, SwordChecks, 73); // 99
        ReadReward(bt10 + 0x25E70, 2, SwordChecks, 85); // 99
        ReadReward(bt10 + 0x25F50, 2, SwordChecks, 99); // 99

        // if shield
        ReadReward(bt10 + 0x25942, 2, ShieldChecks, 2);
        ReadReward(bt10 + 0x25962, 2, ShieldChecks, 4);
        ReadReward(bt10 + 0x25992, 2, ShieldChecks, 7);
        ReadReward(bt10 + 0x259B2, 2, ShieldChecks, 9);
        ReadReward(bt10 + 0x259C2, 2, ShieldChecks, 10);
        ReadReward(bt10 + 0x259E2, 2, ShieldChecks, 12);
        ReadReward(bt10 + 0x25A02, 2, ShieldChecks, 14);
        ReadReward(bt10 + 0x25A12, 2, ShieldChecks, 15);
        ReadReward(bt10 + 0x25A32, 2, ShieldChecks, 17);
        ReadReward(bt10 + 0x25A62, 2, ShieldChecks, 20);
        ReadReward(bt10 + 0x25A92, 2, ShieldChecks, 23);
        ReadReward(bt10 + 0x25AB2, 2, ShieldChecks, 25);
        ReadReward(bt10 + 0x25AE2, 2, ShieldChecks, 28);
        ReadReward(bt10 + 0x25B02, 2, ShieldChecks, 30);
        ReadReward(bt10 + 0x25B12, 2, ShieldChecks, 31); // 99
        ReadReward(bt10 + 0x25B22, 2, ShieldChecks, 32);
        ReadReward(bt10 + 0x25B32, 2, ShieldChecks, 33); // 99
        ReadReward(bt10 + 0x25B42, 2, ShieldChecks, 34);
        ReadReward(bt10 + 0x25B62, 2, ShieldChecks, 36);
        ReadReward(bt10 + 0x25B92, 2, ShieldChecks, 39);
        ReadReward(bt10 + 0x25BB2, 2, ShieldChecks, 41);
        ReadReward(bt10 + 0x25BE2, 2, ShieldChecks, 44);
        ReadReward(bt10 + 0x25C02, 2, ShieldChecks, 46);
        ReadReward(bt10 + 0x25C12, 2, ShieldChecks, 47); // 99
        ReadReward(bt10 + 0x25C22, 2, ShieldChecks, 48);
        ReadReward(bt10 + 0x25C32, 2, ShieldChecks, 49); // 99
        ReadReward(bt10 + 0x25C42, 2, ShieldChecks, 50);
        ReadReward(bt10 + 0x25C72, 2, ShieldChecks, 53); // 99
        ReadReward(bt10 + 0x25CD2, 2, ShieldChecks, 59); // 99
        ReadReward(bt10 + 0x25D32, 2, ShieldChecks, 65); // 99
        ReadReward(bt10 + 0x25DB2, 2, ShieldChecks, 73); // 99
        ReadReward(bt10 + 0x25E72, 2, ShieldChecks, 85); // 99
        ReadReward(bt10 + 0x25F52, 2, ShieldChecks, 99); // 99

        // if staff
        ReadReward(bt10 + 0x25944, 2, StaffChecks, 2);
        ReadReward(bt10 + 0x25964, 2, StaffChecks, 4);
        ReadReward(bt10 + 0x25994, 2, StaffChecks, 7);
        ReadReward(bt10 + 0x259B4, 2, StaffChecks, 9);
        ReadReward(bt10 + 0x259C4, 2, StaffChecks, 10);
        ReadReward(bt10 + 0x259E4, 2, StaffChecks, 12);
        ReadReward(bt10 + 0x25A04, 2, StaffChecks, 14);
        ReadReward(bt10 + 0x25A14, 2, StaffChecks, 15);
        ReadReward(bt10 + 0x25A34, 2, StaffChecks, 17);
        ReadReward(bt10 + 0x25A64, 2, StaffChecks, 20);
        ReadReward(bt10 + 0x25A94, 2, StaffChecks, 23);
        ReadReward(bt10 + 0x25AB4, 2, StaffChecks, 25);
        ReadReward(bt10 + 0x25AE4, 2, StaffChecks, 28);
        ReadReward(bt10 + 0x25B04, 2, StaffChecks, 30);
        ReadReward(bt10 + 0x25B14, 2, StaffChecks, 31); // 99
        ReadReward(bt10 + 0x25B24, 2, StaffChecks, 32);
        ReadReward(bt10 + 0x25B34, 2, StaffChecks, 33); // 99
        ReadReward(bt10 + 0x25B44, 2, StaffChecks, 34);
        ReadReward(bt10 + 0x25B64, 2, StaffChecks, 36);
        ReadReward(bt10 + 0x25B94, 2, StaffChecks, 39);
        ReadReward(bt10 + 0x25BB4, 2, StaffChecks, 41);
        ReadReward(bt10 + 0x25BE4, 2, StaffChecks, 44);
        ReadReward(bt10 + 0x25C04, 2, StaffChecks, 46);
        ReadReward(bt10 + 0x25C14, 2, StaffChecks, 47); // 99
        ReadReward(bt10 + 0x25C24, 2, StaffChecks, 48);
        ReadReward(bt10 + 0x25C34, 2, StaffChecks, 49); // 99
        ReadReward(bt10 + 0x25C44, 2, StaffChecks, 50);
        ReadReward(bt10 + 0x25C74, 2, StaffChecks, 53); // 99
        ReadReward(bt10 + 0x25CD4, 2, StaffChecks, 59); // 99
        ReadReward(bt10 + 0x25D34, 2, StaffChecks, 65); // 99
        ReadReward(bt10 + 0x25DB4, 2, StaffChecks, 73); // 99
        ReadReward(bt10 + 0x25E74, 2, StaffChecks, 85); // 99
        ReadReward(bt10 + 0x25F54, 2, StaffChecks, 99); // 99

        // valor
        ReadReward(bt10 + 0x344AE, 2, ValorChecks, 2);
        ReadReward(bt10 + 0x344B6, 2, ValorChecks, 3);
        ReadReward(bt10 + 0x344BE, 2, ValorChecks, 4);
        ReadReward(bt10 + 0x344C6, 2, ValorChecks, 5);
        ReadReward(bt10 + 0x344CE, 2, ValorChecks, 6);
        ReadReward(bt10 + 0x344D6, 2, ValorChecks, 7);

        // wisdom
        ReadReward(bt10 + 0x344E6, 2, WisdomChecks, 2);
        ReadReward(bt10 + 0x344EE, 2, WisdomChecks, 3);
        ReadReward(bt10 + 0x344F6, 2, WisdomChecks, 4);
        ReadReward(bt10 + 0x344FE, 2, WisdomChecks, 5);
        ReadReward(bt10 + 0x34506, 2, WisdomChecks, 6);
        ReadReward(bt10 + 0x3450E, 2, WisdomChecks, 7);

        // limit
        ReadReward(bt10 + 0x3451E, 2, LimitChecks, 2);
        ReadReward(bt10 + 0x34526, 2, LimitChecks, 3);
        ReadReward(bt10 + 0x3452E, 2, LimitChecks, 4);
        ReadReward(bt10 + 0x34536, 2, LimitChecks, 5);
        ReadReward(bt10 + 0x3453E, 2, LimitChecks, 6);
        ReadReward(bt10 + 0x34546, 2, LimitChecks, 7);

        // master
        ReadReward(bt10 + 0x34556, 2, MasterChecks, 2);
        ReadReward(bt10 + 0x3455E, 2, MasterChecks, 3);
        ReadReward(bt10 + 0x34566, 2, MasterChecks, 4);
        ReadReward(bt10 + 0x3456E, 2, MasterChecks, 5);
        ReadReward(bt10 + 0x34576, 2, MasterChecks, 6);
        ReadReward(bt10 + 0x3457E, 2, MasterChecks, 7);

        // final
        ReadReward(bt10 + 0x3458E, 2, FinalChecks, 2);
        ReadReward(bt10 + 0x34596, 2, FinalChecks, 3);
        ReadReward(bt10 + 0x3459E, 2, FinalChecks, 4);
        ReadReward(bt10 + 0x345A6, 2, FinalChecks, 5);
        ReadReward(bt10 + 0x345AE, 2, FinalChecks, 6);
        ReadReward(bt10 + 0x345B6, 2, FinalChecks, 7);
    }

    private void ReadReward(int address, int byteCount, List<Tuple<int, string>> rewards, int level)
    {
        var num = address + AddressOffset;
        var reward = memory.ReadMemory(num, byteCount);
        int i = BitConverter.ToInt16(reward, 0);
        if (IsImportant(i, out var name))
        {
            rewards.Add(new Tuple<int, string>(level, name));
        }
    }

    private bool IsImportant(int num, out string name)
    {
        if (MainWindow.Data.Codes.ItemCodes.TryGetValue(num, out var itemCode))
        {
            name = itemCode;
            return true;
        }
        name = "";
        return false;
    }

    public List<Tuple<int, string>> GetLevelRewards(string weapon)
    {
        return weapon switch
        {
            "Sword" => SwordChecks,
            "Shield" => ShieldChecks,
            _ => StaffChecks
        };
    }
}
