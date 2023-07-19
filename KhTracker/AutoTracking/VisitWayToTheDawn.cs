namespace KhTracker;

internal class VisitWayToTheDawn : Visit
{
    public VisitWayToTheDawn(MemoryReader mem, int save, int addressOffset)
        : base(mem, save, addressOffset, "WayToTheDawn") { }

    public override byte[] UpdateMemory()
    {
        if (ReadByte(0x1EDF) != 3)
            return null;

        if (ReadByte(0x3607) > 0)
        {
            if (ReadByte(0x35C1) <= 1 || ReadByte(0x1B62) != 0)
                return null;

            if (Obtained == false)
            {
                Obtained = true;
            }

            if (Quantity < 1)
            {
                Quantity = 1;
            }
        }
        else
        {
            if (ReadByte(0x35C1) <= 0 || ReadByte(0x1B62) != 0)
                return null;

            if (Obtained == false)
            {
                Obtained = true;
            }

            if (Quantity < 1)
            {
                Quantity = 1;
            }
        }

        return null;
    }

    private byte ReadByte(int offset) =>
        Memory.ReadMemory(Address + AddressOffset + offset, Bytes)[0];
}
