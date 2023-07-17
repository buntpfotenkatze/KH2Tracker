using System.Collections;

namespace KhTracker;

internal class Proof : ImportantCheck
{
    public Proof(MemoryReader mem, int address, int offset, string name)
        : base(mem, address, offset, name) { }

    public override byte[] UpdateMemory()
    {
        var data = base.UpdateMemory();
        var flag = new BitArray(data)[0];
        if (Obtained == false && flag)
        {
            Obtained = true;
            //App.logger.Record(Name + " obtained");
        }
        return null;
    }
}
