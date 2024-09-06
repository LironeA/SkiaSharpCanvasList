using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CanvasScrollTest.Components.ListComponent;

public class CustomList : SKCanvasView
{

    #region canvas properties
    private SKCanvas _canvas;
    private SKRect _drawRect;
    private SKImageInfo _info;
    Animation animation = null;

    #endregion


    #region Scroll properties


    private float _startInteractionY;
    private bool _isScrolling;
    private float _currentYOffset;
    private float _targetYOffset;
    private float _maxHeight;
    private float _sumScroll;
    private Queue<(float, DateTime)> _scrollQueue = new Queue<(float, DateTime)>();
    #endregion

    private List<ListDrawable> ItemsDrawable { get; set; } = new List<ListDrawable>();
    private List<ListDrawable> VisibleItemsDrawable { get; set; } = new List<ListDrawable>();

    #region ItemsProperty

    public IList Items
    {
        get { return (IList)GetValue(ItemsProperty); }
        set { SetValue(ItemsProperty, value); }
    }

    public static readonly BindableProperty ItemsProperty =
        BindableProperty.Create(nameof(Items),
            typeof(IList),
            typeof(CustomList),
            null,
            propertyChanged: OnItemsPropertyChanged);

    private INotifyCollectionChanged _currentCollection;

    private static void OnItemsPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        var control = (CustomList)bindable;
        control.OnItemsChanged((IList)oldvalue, (IList)newvalue);
    }
    private void OnItemsChanged(IList oldItems, IList newItems)
    {
        if (_currentCollection != null)
        {
            _currentCollection.CollectionChanged -= OnCollectionChanged;
        }

        if (newItems is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged += OnCollectionChanged;
            _currentCollection = newCollection;
        }
        else
        {
            _currentCollection = null;
        }
    }
    #endregion

    #region ItemTemplateProperty
    public ItemTemplate ItemTemplate
    {
        get { return (ItemTemplate)GetValue(ItemTemplateProperty); }
        set { SetValue(ItemTemplateProperty, value); }
    }

    public static readonly BindableProperty ItemTemplateProperty =
        BindableProperty.Create(nameof(ItemTemplate),
            typeof(ItemTemplate),
            typeof(CustomList),
            null,
            propertyChanged: OnItemTemplatePropertyChanged);

    private static void OnItemTemplatePropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        var control = (CustomList)bindable;
        control.OnItemTemplateChanged((ItemTemplate)oldvalue, (ItemTemplate)newvalue);
    }

    private void OnItemTemplateChanged(ItemTemplate oldvalue, ItemTemplate newvalue)
    {

    }

    #endregion

    private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        InvalidateSurface();
        Console.WriteLine($"Item changed: {sender}, Property: {e.PropertyName}");
    }


    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        e.OldItems?.Cast<INotifyPropertyChanged>().ToList().ForEach(item => item.PropertyChanged -= OnItemPropertyChanged);
        e.NewItems?.Cast<INotifyPropertyChanged>().ToList().ForEach(item => item.PropertyChanged += OnItemPropertyChanged);

        CreateItemDrawables();
        GetVisibleItems(_currentYOffset, _info.Height);
        InvalidateSurface();
    }

    private void CreateItemDrawables()
    {
        ItemsDrawable = new List<ListDrawable>();
        float height = 0;
        for (var index = 0; index < Items.Count; index++)
        {
            var item = Items[index] as ObservableObject;
            var itemHeight = ItemTemplate.GetTotalHeight(item);
            var drawable = new ListDrawable()
            {
                Item = item,
                Rect = new SKRect(0, height, _info.Width, height + itemHeight)
            };
            ItemsDrawable.Add(drawable);
            height += itemHeight;
        }
        _maxHeight = height;
    }


    public CustomList()
    {
        EnableTouchEvents = true;
        InputTransparent = false;
        Touch += CustomList_Touch;
        animation = new Animation((value) =>
        {
            if (MathF.Abs(_currentYOffset - _targetYOffset) < 10)
            {
                return;
            }
            _currentYOffset = Lerp(_currentYOffset, _targetYOffset, (float)value);
            GetVisibleItems(_currentYOffset, _info.Height);
            InvalidateSurface();
        });
        animation.Commit(this, "yRotationAnimation", length: 1000, repeat: () => true, rate: (int)(1f / 60f * 1000f));
    }

    //lerp function
    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    private void CustomList_Touch(object? sender, SKTouchEventArgs e)
    {
        var y = e.Location.Y;
        var x = e.Location.X;
        //Debug.WriteLine($"Event: {e.ActionType.ToString()} Y:{e.Location.Y} dY:{dy}");
        if (e.ActionType == SKTouchAction.Pressed)
        {
            _startInteractionY = y;
            _sumScroll = 0;
            if (_maxHeight > _info.Height - ItemTemplate.GetHeaderHeight())
            {
                _isScrolling = true;
            }
        }

        if (e.ActionType == SKTouchAction.Moved)
        {
            var dy = y - _startInteractionY;
            EnqueueTouchEvent(dy);
            if (_isScrolling)
            {
                SetTargetOffset(dy);
                _sumScroll += dy;
            }
            _startInteractionY = y;
        }

        if (e.ActionType == SKTouchAction.Released)
        {
            //CalculateInertia();
            _scrollQueue.Clear();
            _startInteractionY = y;
            _isScrolling = false;
            if (MathF.Abs(_sumScroll) < 20)
            {
                ItemClicked(x, y - _currentYOffset - ItemTemplate.GetHeaderHeight());
            }

        }

        e.Handled = true;
    }

    private void EnqueueTouchEvent(float dy)
    {
        _scrollQueue.Enqueue((dy, DateTime.Now));
        if (_scrollQueue.Count > 10)
        {
            _scrollQueue.Dequeue();
        }
    }

    private void CalculateInertia()
    {
        if(_scrollQueue.Count < 5)
        {
            return;
        }
        
        var sum = 0f;
        var totalTime = 0f;
        var previousTime = _scrollQueue.Peek().Item2;

        foreach (var (dy, currentTime) in _scrollQueue)
        {
            var deltaTime = (float)(currentTime - previousTime).TotalMilliseconds;
            sum += dy;
            totalTime += deltaTime;
            previousTime = currentTime;
        }

        var velocity = sum / totalTime;
        Debug.WriteLine($"Velocity: {velocity}");

        if (MathF.Abs(velocity) < 3f)
        {
            return;
        }

        var timeToStop = MathF.Abs(velocity) / 0.1f;
        var distance = velocity * timeToStop;
        Debug.WriteLine($"Distance: {distance}");
        SetTargetOffset(distance);
    }

    private void SetTargetOffset(float dy)
    {
        if (_targetYOffset + dy > 0)
        {
            _targetYOffset = 0;
        }
        else if (_targetYOffset + dy < -(_maxHeight - _info.Height + ItemTemplate.GetHeaderHeight()))
        {
            _targetYOffset = -(_maxHeight - _info.Height + ItemTemplate.GetHeaderHeight());
        }
        else
        {
            _targetYOffset += dy;

        }
    }

    private void ItemClicked(float x, float y)
    {
        try
        {
            var item = VisibleItemsDrawable.FirstOrDefault(x => x.Rect.Top < y && x.Rect.Bottom > y);
            if (item != null)
            {
                ItemTemplate.ProccessItemClick(item, x, y);
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }


    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        base.OnPaintSurface(e);

        _canvas = e.Surface.Canvas;
        _canvas.Clear();
        _info = e.Info;
        if(_drawRect.Right != _info.Width || _drawRect.Bottom != _info.Height)
        {
            _drawRect = new SKRect(0, 0, _info.Width, _info.Height);
            CreateItemDrawables();
            GetVisibleItems(_currentYOffset, _info.Height);
        }
       
        _canvas.DrawRect(_drawRect, ItemTemplate.BackgroundPaint);
        for (int i = 0; i < VisibleItemsDrawable.Count; i++)
        {
            var drawable = VisibleItemsDrawable[i];
            var newRect = new SKRect(drawable.Rect.Left, drawable.Rect.Top + _currentYOffset + ItemTemplate.GetHeaderHeight(), drawable.Rect.Right, drawable.Rect.Bottom + _currentYOffset + ItemTemplate.GetHeaderHeight());
            ItemTemplate.Draw(_canvas, drawable.Item, newRect);
        }

        ItemTemplate.DrawHeaders(_canvas, _drawRect);

        DrawScrollLine(_canvas, _drawRect);
    }

    private void DrawScrollLine(SKCanvas canvas, SKRect drawRect)
    {
        SKColor thumbColor = new SKColor(0, 0, 0, 80);
        SKColor trackColor = new SKColor(0, 0, 0, 30);
        var paint = new SKPaint()
        {
            Style = SKPaintStyle.Fill,
            StrokeWidth = 2,
            IsAntialias = true
        };
        var width = 8;
        var cornerRadius = 10;
        var height = drawRect.Height - ItemTemplate.GetHeaderHeight();
        var scrollHeight = height * drawRect.Height / _maxHeight;
        var scrollY = (-_currentYOffset) * height / _maxHeight;

        paint.Color = trackColor;
        var trackRect = new SKRect(drawRect.Right - width + 2, ItemTemplate.GetHeaderHeight(), drawRect.Right - 2, ItemTemplate.GetHeaderHeight() + height);
        canvas.DrawRoundRect(trackRect, cornerRadius, cornerRadius, paint);

        paint.Color = thumbColor;
        var rect = new SKRect(drawRect.Right - width + 2, ItemTemplate.GetHeaderHeight() + scrollY, drawRect.Right - 2, ItemTemplate.GetHeaderHeight() + scrollY + scrollHeight);
        canvas.DrawRoundRect(rect,cornerRadius, cornerRadius, paint);
       
    }

    private void GetVisibleItems(float currentYOffset, float canvasHeight)
    {
        VisibleItemsDrawable = new List<ListDrawable>();
        for (int i = 0; i < ItemsDrawable.Count; i++)
        {
            var rect = ItemsDrawable[i].Rect;
            if (rect.Bottom + currentYOffset > 0 && rect.Top + currentYOffset < canvasHeight)
            {
                VisibleItemsDrawable.Add(ItemsDrawable[i]);
            }
        }
    }
}


public class ListDrawable
{
    public ObservableObject Item { get; set; }
    public SKRect Rect { get; set; }
}


public enum HorizontalAlignment
{
    Left,
    Center,
    Right
}

public enum VerticalAlignment
{
    Top,
    Center,
    Bottom
}
public static class CanvasExtentions
{
    public static void DrawString(
        this SKCanvas canvas,
        string text,
        SKRect bounds,
        SKPaint paint,
        int maxLinesCount = 1,
        HorizontalAlignment hAlign = HorizontalAlignment.Left,
        VerticalAlignment vAlign = VerticalAlignment.Top)
    {
        // Split the text into multiple lines that fit within the width of the bounds
        List<string> lines = WrapText(text, paint, bounds.Width);

        // Measure line height
        float lineHeight = paint.FontMetrics.Descent - paint.FontMetrics.Ascent;

        // Calculate the maximum number of lines that can fit within the bounds
        int maxLines = (int)(bounds.Height / lineHeight);

        // Handle truncation if lines exceed the bounds
        if (lines.Count > maxLinesCount)
        {
            lines = lines.GetRange(0, maxLinesCount);
            lines[^1] = TruncateText(lines[^1], paint, bounds.Width); // Truncate the last line
        }

        // Calculate vertical offset based on vertical alignment
        float totalHeight = lines.Count * lineHeight;
        float yOffset = vAlign switch
        {
            VerticalAlignment.Center => bounds.MidY - totalHeight / 2,
            VerticalAlignment.Bottom => bounds.Bottom - totalHeight,
            _ => bounds.Top
        };

        // Draw each line within the bounds
        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];

            // Calculate horizontal offset based on horizontal alignment
            float lineWidth = paint.MeasureText(line);
            float xOffset = hAlign switch
            {
                HorizontalAlignment.Center => bounds.MidX - lineWidth / 2,
                HorizontalAlignment.Right => bounds.Right - lineWidth,
                _ => bounds.Left
            };

            float y = yOffset + lineHeight * (i + 1) - paint.FontMetrics.Descent; // Adjust to align baseline
            canvas.DrawText(line, xOffset, y, paint);
        }
    }

    private static List<string> WrapText(string text, SKPaint paint, float maxWidth)
    {
        List<string> lines = new List<string>();
        string[] words = text.Split(' ');

        string currentLine = "";
        foreach (var word in words)
        {
            string testLine = string.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";
            float lineWidth = paint.MeasureText(testLine);

            if (lineWidth > maxWidth && !string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine);
                currentLine = word; // Start new line with the current word
            }
            else
            {
                currentLine = testLine;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
            lines.Add(currentLine); // Add the remaining line
        }

        return lines;
    }

    private static string TruncateText(string text, SKPaint paint, float maxWidth)
    {
        string ellipsis = "...";
        float ellipsisWidth = paint.MeasureText(ellipsis);

        while (paint.MeasureText(text) > maxWidth - ellipsisWidth && text.Length > 0)
        {
            text = text.Substring(0, text.Length - 1); // Remove the last character
        }

        return text + ellipsis;
    }

}


