using SkiaSharp;
using System.Reflection;

namespace CanvasScrollTest.Components.ListComponent;

public class ItemTemplate
{
    private SKColor _background;
    private SKColor _border;
    private SKColor _headerBackground;

    public SKColor Background
    {
        get => _background;
        set
        {
            _background = value;
            BackgroundPaint = new SKPaint { Color = value, Style = SKPaintStyle.Fill };
        }
    }

    public SKColor Border
    {
        get => _border;
        set
        {
            _borderPaint = new SKPaint { Color = value, Style = SKPaintStyle.Stroke, StrokeWidth = 2 };
            _border = value;
        }
    }

    public bool ShowHeaders { get; set; }
    public float HeaderHeight { get; set; }

    public SKColor HeaderBackground
    {
        get => _headerBackground;
        set
        {
            _headerPaint = new SKPaint { Color = value, Style = SKPaintStyle.Fill };
            _headerBackground = value;
        }
    }

    public RowDefinitions RowDefinitions { get; set; }

    public SKPaint BackgroundPaint { get; set; }
    private SKPaint _borderPaint { get; set; }
    private SKPaint _headerPaint { get; set; }

    public ItemTemplate()
    {
        Background = SKColors.White;
        Border = SKColors.LightGray;
        ShowHeaders = true;
        HeaderHeight = 50f;
        HeaderBackground = SKColors.LightGray;
    }

    public float GetHeaderHeight()
    {
        return ShowHeaders ? HeaderHeight : 0;
    }
    public float GetTotalHeight(object? item)
    {
        return RowDefinitions.Rows.Sum(r => r.GetHeight(item));
    }

    public void Draw(SKCanvas canvas, object item, SKRect bounds)
    {

        //canvas.DrawRect(bounds, _backgroundPaint);
        canvas.DrawRect(bounds, _borderPaint);

        RowDefinitions.DrawRows(canvas, item, bounds);
    }


    public void DrawHeaders(SKCanvas canvas, SKRect bounds)
    {
        if (ShowHeaders)
        {
            var headerBounds = new SKRect(bounds.Left, bounds.Top, bounds.Right, bounds.Top + HeaderHeight);
            canvas.DrawRect(headerBounds, _headerPaint);
            RowDefinitions.Rows[0].ColumnDefinitions.DrawHeaders(canvas, headerBounds);
        }
    }
}

public class RowDefinitions
{

    public List<Row> Rows { get; set; }
    public RowDefinitions()
    {
    }

    public void DrawRows(SKCanvas canvas, object item, SKRect bounds)
    {
        for (int i = 0; i < Rows.Count; i++)
        {
            var row = Rows[i];
            if (row.ShowRowDelegate is null || row.ShowRowDelegate(item))
            {
                var rowBounds = GetRowBounds(item, i, bounds.Top, bounds.Width, bounds.Height);
                row.Draw(canvas, item, rowBounds);
            }
        }
    }
    public float GetRowStartY(object item, int row, float height)
    {
        float y = 0;
        for (int i = 0; i < row; i++)
        {
            y += Rows[i].GetHeight(item);
        }
        return y;
    }

    public SKRect GetRowBounds(object item, int row, float y, float width, float height)
    {
        return new SKRect(0, y + GetRowStartY(item, row, height), width, y + GetRowStartY(item, row + 1, height));
    }

}

public class Row
{
    public float Height { get; set; }
    //public UnitType UnitType { get; set; }
    public ColumnDefinitions ColumnDefinitions { get; set; }
    public Func<object?, bool> ShowRowDelegate { get; set; }

    public Row()
    {
    }

    public float GetHeight(object? item)
    {
        if (ShowRowDelegate is null)
        {
            return Height;
        }
        else
        {
            return ShowRowDelegate(item) ? Height : 0f;
        }
        //return UnitType switch
        //{
        //    UnitType.Star => Height * height,
        //    UnitType.Pixel => Height,
        //    _ => 0
        //};
    }


    public void Draw(SKCanvas canvas, object item, SKRect bounds)
    {
        ColumnDefinitions.DrawColumns(canvas, item, bounds);
    }

}

public class ColumnDefinitions
{
    public List<ColumnBase> Columns { get; set; }

    public ColumnDefinitions()
    {
    }

    public SKRect GetColumnBounds(int column, float y, float height, float width)
    {
        return new SKRect(GetColumnStartX(column, width), y, GetColumnStartX(column + 1, width), y + height);
    }

    public float GetColumnStartX(int column, float canvasWidth)
    {
        float x = 0;
        for (int i = 0; i < column; i++)
        {
            x += Columns[i].GetWidth(canvasWidth);
        }
        return x;
    }

    public void DrawColumns(SKCanvas canvas, object item, SKRect bounds)
    {
        for (int i = 0; i < Columns.Count; i++)
        {
            var column = Columns[i];
            var columnBounds = GetColumnBounds(i, bounds.Top, bounds.Height, bounds.Width);
            column.Draw(canvas, item, columnBounds);
        }
    }

    public void DrawHeaders(SKCanvas canvas, SKRect bounds)
    {
        for (int i = 0; i < Columns.Count; i++)
        {
            var column = Columns[i];
            var columnBounds = GetColumnBounds(i, bounds.Top, bounds.Height, bounds.Width);
            column.DrawHeader(canvas, columnBounds);
        }
    }
}


public abstract class ColumnBase
{
    public string Name { get; set; }
    public float Width { get; set; }
    public UnitType UnitType { get; set; }
    public virtual void Draw(SKCanvas canvas, object item, SKRect bounds) { }
    public virtual void DrawHeader(SKCanvas canvas, SKRect bounds) { }

    public float GetWidth(float canvasWidth)
    {
        return UnitType switch
        {
            UnitType.Star => Width * canvasWidth,
            UnitType.Pixel => Width,
            _ => 0
        };
    }
}


public class PropertyColumn : ColumnBase
{
    private int _maxLinesCount;
    public PropertyInfo PropertyInfo { get; set; }
    public Func<object?, string> ValueConverter { get; set; }

    public SKPaint Paint { get; set; }

    public int MaxLinesCount
    {
        get { return _maxLinesCount; }
        set
        {
            if (value < 1)
            {
                throw new ArgumentException("MaxLinesCount must be greater than 1", nameof(MaxLinesCount));
            }
            _maxLinesCount = value;
        }
    }

    public HorizontalAlignment HorizontalAlignment { get; set; }
    public VerticalAlignment VerticalAlignment { get; set; }

    public PropertyColumn()
    {
        MaxLinesCount = 1;
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Top;
    }

    public override void Draw(SKCanvas canvas, object item, SKRect bounds)
    {
        var value = PropertyInfo.GetValue(item);
        var stringValue = ValueConverter(value);
        canvas.DrawString(stringValue, bounds, Paint, MaxLinesCount, HorizontalAlignment, VerticalAlignment);
    }

    public override void DrawHeader(SKCanvas canvas, SKRect bounds)
    {
        canvas.DrawString(Name, bounds, Paint, 1, HorizontalAlignment, VerticalAlignment);
    }
}

public enum UnitType
{
    Star,
    Pixel
}