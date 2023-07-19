using System.Windows;

namespace KhTracker;

internal class TornPage : ImportantCheck
{
    private int current;
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

    public TornPage(MemoryReader mem, int address, int offset, string name)
        : base(mem, address, offset, name) { }

    public override byte[] UpdateMemory()
    {
        var data = base.UpdateMemory();
        var used = (Application.Current.MainWindow as MainWindow)!.GetUsedPages();
        var test = WorldGrid.RealPages;

        //use world grid real pages since that doesn't reset on disconnect
        if (test < (data[0] + used))
        {
            //safty i guess
            if (current == 0)
                current = WorldGrid.RealPages - used;

            // add the difference incase of getting multiple at the same time
            Quantity += data[0] - current;
            if (App.Logger != null)
                App.Logger.Record(Quantity + " torn pages obtained");
        }
        else if (test > (data[0] + used))
        {
            // reduce quantity so when you regrab a torn page after dying the quantity goes back to where it should be
            if ((Application.Current.MainWindow as MainWindow)!.GetWorld() != "HundredAcreWood")
                Quantity -= current - data[0];
            else
                (Application.Current.MainWindow as MainWindow)!.UpdateUsedPages();
        }
        current = data[0];
        return null;

        //if (current < data[0])
        //{
        //    // add the difference incase of getting multiple at the same time
        //    Quantity += data[0] - current;
        //    if (App.logger != null)
        //        App.logger.Record(Quantity.ToString() + " torn pages obtained");
        //}
        //else if (current > data[0])
        //{
        //    // reduce quantity so when you regrab a torn page after dying the quantity goes back to where it should be
        //    if ((App.Current.MainWindow as MainWindow).GetWorld() != "HundredAcreWood")
        //        Quantity -= current - data[0];
        //    else
        //        (App.Current.MainWindow as MainWindow).UpdateUsedPages();
        //}
    }
}
