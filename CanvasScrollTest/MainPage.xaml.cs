using CanvasScrollTest.Components.ListComponent;
using ServioPOSCustomComponents;
using SkiaSharp;
using HorizontalAlignment = CanvasScrollTest.Components.ListComponent.HorizontalAlignment;
using VerticalAlignment = CanvasScrollTest.Components.ListComponent.VerticalAlignment;

namespace CanvasScrollTest
{
    public partial class MainPage : ContentPage
    {
        private readonly MainPageVM _vm;
        public MainPage(MainPageVM vm)
        {
            _vm = vm;
            InitializeComponent();
            lst.Items = _vm.Items;
            lst.ItemTemplate = new ItemTemplate()
            {
                Background = SKColors.White,
                Border = SKColors.LightGray,
                ShowHeaders = true,
                HeaderHeight = 50f,
                IsSelectedProperty = typeof(TestModel).GetProperty(nameof(TestModel.IsSelected)),
                GetBackGroundColor = (item) => ((TestModel)item).IsSelected ? SKColors.LightBlue : SKColors.White,
                RowDefinitions = new RowDefinitions()
                {
                    Rows = new List<Row>()
                    {
                        new Row()
                        {
                            Height = 75,
                            ColumnDefinitions =
                                new ColumnDefinitions()
                                {
                                    Columns = new List<ColumnBase>()
                                    {
                                        new CheckBoxColumn()
                                        {
                                            Name = "CheckBox",
                                            Width = 40f,
                                            UnitType = UnitType.Pixel,
                                            Size = 25f,
                                            CornerRadius = 5f,
                                            BorderPaint = new SKPaint()
                                            {
                                                Color = SKColors.LightBlue,
                                                Style = SKPaintStyle.Stroke,
                                                StrokeWidth = 2
                                            },
                                            FillPaint = new SKPaint()
                                            {
                                                Color = SKColors.LightPink,
                                                Style = SKPaintStyle.Fill
                                            },
                                            IsCheckedProperty = typeof(TestModel).GetProperty(nameof(TestModel.IsChecked)),
                                        },
                                        new PropertyColumn()
                                        {
                                            Name = "Id",
                                            Width = 0.1f,
                                            UnitType = UnitType.Star,
                                            HorizontalAlignment = HorizontalAlignment.Center,
                                            VerticalAlignment = VerticalAlignment.Center,
                                            MaxLinesCount = 1,
                                            Paint = new SKPaint()
                                            {
                                                Color = SKColors.Black,
                                                Style = SKPaintStyle.Fill,
                                                TextSize = 14,
                                                IsAntialias = true
                                            },
                                            ValueConverter = (value) => value.ToString(),
                                            PropertyInfo = typeof(TestModel).GetProperty(nameof(TestModel.Id)),
                                        },
                                        new PropertyColumn()
                                        {
                                            Name = "Name",
                                            Width = 0.8f,
                                            UnitType = UnitType.Star,
                                            ClickCommand = _vm.ChangeNameCommand,
                                            HorizontalAlignment = HorizontalAlignment.Left,
                                            VerticalAlignment = VerticalAlignment.Center,
                                            MaxLinesCount = 2,
                                            Paint = new SKPaint()
                                            {
                                                Color = SKColors.Black,
                                                Style = SKPaintStyle.Fill,
                                                TextSize = 14,
                                                IsAntialias = true
                                            },
                                            ValueConverter = (value) => value.ToString(),
                                            PropertyInfo = typeof(TestModel).GetProperty(nameof(TestModel.Name)),
                                        },
                                        new IconColumn()
                                        {
                                            Name = "Icon",
                                            Width = 30f,
                                            UnitType = UnitType.Pixel,
                                            _bitmap = BitmapExtensions.LoadBitmapResource(GetType(), "CanvasScrollTest.Resources.Images.dotnet_bot.png"),
                                        },
                                        new CustomColumn()
                                        {
                                            Name = "CustomIcon",
                                            Width = 0.1f,
                                            UnitType = UnitType.Star,
                                            DrawAction = (canvas, item, bounds) =>
                                            {
                                                var paint = new SKPaint() { Color = SKColors.Black, StrokeWidth = 2, };
                                                var width = bounds.Width / 2;
                                                //draw 3 horisontal lines at center of bounds
                                                canvas.DrawLine(bounds.Left + bounds.Width /2 - width/2, bounds.Top + bounds.Height / 4, bounds.Right - bounds.Width /2 + width/2, bounds.Top + bounds.Height / 4, paint);
                                                canvas.DrawLine(bounds.Left + bounds.Width /2 - width/3, bounds.Top + bounds.Height / 2, bounds.Right - bounds.Width /2 + width/2, bounds.Top + bounds.Height / 2, paint);
                                                canvas.DrawLine(bounds.Left + bounds.Width /2 - width/2, bounds.Top + bounds.Height * 3 / 4, bounds.Right - bounds.Width /2 + width/2, bounds.Top + bounds.Height * 3 / 4, paint);
                                            }
                                        }
                                    }
                                }
                        },
                        new Row()
                        {
                            Height = 25,
                            ShowRowDelegate = (item) => !string.IsNullOrEmpty(((TestModel)item).Comment),
                            ColumnDefinitions = new ColumnDefinitions()
                            {
                                Columns = new List<ColumnBase>()
                                {
                                    new PropertyColumn()
                                    {

                                        Name = "Comment",
                                        Width = 0.5f,
                                        UnitType = UnitType.Star,
                                        HorizontalAlignment = HorizontalAlignment.Left,
                                        VerticalAlignment = VerticalAlignment.Center,
                                        MaxLinesCount = 2,
                                        Paint = new SKPaint()
                                        {
                                            Color = SKColors.Black,
                                            Style = SKPaintStyle.Fill,
                                            TextSize = 12,
                                            IsAntialias = true,
                                        },
                                        ValueConverter = (value) => value.ToString(),
                                        PropertyInfo = typeof(TestModel).GetProperty(nameof(TestModel.Comment)),
                                    }
                                }
                            }
                        }
                    }
                }
            };




        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            _vm.AddItemCommand.Execute(null);
        }

        private void Button_Clicked_1(object sender, EventArgs e)
        {
            _vm.ChangeItemCommand.Execute(null);
        }
    }

}
