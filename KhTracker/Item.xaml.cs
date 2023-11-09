using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace KhTracker;

/// <summary>
/// Interaction logic for Draggable.xaml
/// </summary>
public partial class Item
{
    private bool selected;
    private readonly MainWindow mainW = (MainWindow)Application.Current.MainWindow;

    public Item()
    {
        InitializeComponent();
    }

    //Adorner subclass specific to this control
    private class ItemAdorner : Adorner
    {
        private readonly Rect renderRect;
        private readonly ImageSource imageSource;
        public readonly Point CenterOffset;

        public ItemAdorner(Item adornedElement)
            : base(adornedElement)
        {
            renderRect = new Rect(adornedElement.DesiredSize);
            IsHitTestVisible = false;
            imageSource = (adornedElement.Content as Image)?.Source;
            CenterOffset = new Point(-renderRect.Width / 2, -renderRect.Height / 2);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawImage(imageSource, renderRect);
        }
    }

    //Struct to use in the GetCursorPos function
    private struct PInPoint
    {
        public readonly int X;
        public readonly int Y;

        public PInPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public PInPoint(double x, double y)
        {
            X = (int)x;
            Y = (int)y;
        }

        public Point GetPoint(double xOffset = 0, double yOffet = 0)
        {
            return new Point(X + xOffset, Y + yOffet);
        }

        public Point GetPoint(Point offset)
        {
            return new Point(X + offset.X, Y + offset.Y);
        }
    }

    [LibraryImport("user32.dll", EntryPoint = "GetCursorPos")]
    private static partial void GetCursorPos(ref PInPoint p);

    private ItemAdorner myAdornment;
    private PInPoint pointRef;

    public void Item_Click(object sender, RoutedEventArgs e)
    {
        var data = MainWindow.Data;
        if (data.Selected != null)
        {
            data.WorldsData[data.Selected.Name].WorldGrid.Add_Item(this);
        }
    }

    public void Item_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (selected)
            Item_Click(sender, e);
    }

    public void Item_MouseDown(object sender, MouseButtonEventArgs e)
    {
        selected = true;
    }

    public void Item_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            var adLayer = AdornerLayer.GetAdornerLayer(this);
            myAdornment = new ItemAdorner(this);
            adLayer!.Add(myAdornment);
            DragDrop.DoDragDrop(this, this, DragDropEffects.Copy);
            adLayer.Remove(myAdornment);
        }
    }

    private void Item_PreviewGiveFeedback(object sender, GiveFeedbackEventArgs e)
    {
        GetCursorPos(ref pointRef);
        var relPos = PointFromScreen(pointRef.GetPoint(myAdornment.CenterOffset));
        myAdornment.Arrange(new Rect(relPos, myAdornment.DesiredSize));

        Mouse.SetCursor(Cursors.None);
        e.Handled = true;
    }

    public void Item_Return(object sender, RoutedEventArgs e)
    {
        HandleItemReturn();
    }

    public void HandleItemReturn()
    {
        var data = MainWindow.Data;

        //int index = data.Items.IndexOf(this);
        //Grid ItemRow = data.ItemsGrid[index];
        var itemRow = data.Items[Name].Item2;

        if (Parent != itemRow)
        {
            ((WorldGrid)Parent).Handle_WorldGrid(this, false);

            itemRow.Children.Add(this);

            mainW.SetCollected(false);

            MouseDown -= Item_Return;

            if (data.DragDrop)
            {
                MouseDoubleClick -= Item_Click;
                MouseDoubleClick += Item_Click;
                MouseMove -= Item_MouseMove;
                MouseMove += Item_MouseMove;
            }
            else
            {
                MouseDown -= Item_MouseDown;
                MouseDown += Item_MouseDown;
                MouseUp -= Item_MouseUp;
                MouseUp += Item_MouseUp;
            }
        }
    }
}
