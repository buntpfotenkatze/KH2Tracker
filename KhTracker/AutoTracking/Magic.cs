namespace KhTracker;

internal class Magic : ImportantCheck
{
    private int level;
    public int Level
    {
        get => level;
        set
        {
            level = value;
            OnPropertyChanged("Level");
        }
    }

    public Magic(MemoryReader mem, int address, int offset, string name)
        : base(mem, address, offset, name) { }

    public override byte[] UpdateMemory()
    {
        var data = base.UpdateMemory();

        //data = base.UpdateMemory();
        if (Obtained == false && data[0] > 0)
        {
            Obtained = true;
            //App.logger.Record(Name + " obtained");
        }

        if (Level < data[0])
        {
            Level = data[0];
            if (App.Logger != null)
                App.Logger.Record(Name + " level " + Level + " obtained");
        }

        return null;
    }
}
