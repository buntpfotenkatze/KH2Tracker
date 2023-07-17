using System.Collections;

namespace KhTracker;

internal class Report : ImportantCheck
{
    private readonly int byteNum;

    public Report(MemoryReader mem, int address, int offset, int byteNumber, string name)
        : base(mem, address, offset, name)
    {
        byteNum = byteNumber;
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
        return null;
    }
}
