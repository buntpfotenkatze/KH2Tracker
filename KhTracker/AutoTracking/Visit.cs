using System.Collections;

namespace KhTracker;

internal class Visit : ImportantCheck
{
    private int quantity;
    public int Quantity
    {
        get => quantity;
        set
        {
            quantity = value;
            OnPropertyChanged(nameof(Quantity));
        }
    }

    public Visit(MemoryReader mem, int address, int offset, string name)
        : base(mem, address, offset, name) { }

    public override byte[] UpdateMemory()
    {
        var data = base.UpdateMemory()[0];
        if (Obtained == false && data > 0)
        {
            Obtained = true;
            //App.logger.Record(Name + " obtained");
        }

        if (Quantity < data)
        {
            Quantity = data;
        }

        return null;
    }
}
