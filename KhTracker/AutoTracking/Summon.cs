using System.Collections;

namespace KhTracker;

internal class Summon : ImportantCheck
{
    private readonly int byteNum;

    public Summon(MemoryReader mem, int address, int offset, int byteNumber, string name)
        : base(mem, address, offset, name)
    {
        Bytes = 2;
        byteNum = byteNumber;
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
        return null;
    }
}
