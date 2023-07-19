using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace KhTracker;

internal class ImportantCheck : INotifyPropertyChanged
{
    public string Name;
    protected int Address;
    protected int Bytes = 1;
    private bool obtained;
    public bool Obtained
    {
        get => obtained;
        set
        {
            obtained = value;
            if (App.Logger != null)
                App.Logger.Record(Name + " obtained");
            OnPropertyChanged(nameof(Obtained));
        }
    }

    protected readonly int AddressOffset;

    protected readonly MemoryReader Memory;

    public ImportantCheck(MemoryReader mem, int address, int offset, string name)
    {
        AddressOffset = offset;
        Address = address;
        Memory = mem;
        Name = name;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged(string info)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
    }

    public virtual byte[] UpdateMemory()
    {
        return Memory.ReadMemory(Address + AddressOffset, Bytes);
    }

    public void BindLabel(ContentControl cc, string property, bool hideZero = true)
    {
        var binding = new Binding(property) { Source = this };
        if (hideZero)
        {
            binding.Converter = new HideZeroConverter();
        }
        cc.SetBinding(ContentControl.ContentProperty, binding);
    }

    public void BindImage(Image cc, string property)
    {
        var binding = new Binding(property) { Source = this, Converter = new ObtainedConverter() };
        cc.SetBinding(UIElement.OpacityProperty, binding);
    }
}
