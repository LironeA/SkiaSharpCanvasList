using CanvasScrollTest.Components.ListComponent;
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
                RowDefinitions = new RowDefinitions()
                {
                    Rows = new List<Row>()
                    {
                        new Row()
                        {
                            Height = 100,
                            ColumnDefinitions =
                                new ColumnDefinitions()
                                {
                                    Columns = new List<ColumnBase>()
                                    {
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
                                                TextSize = 14
                                            },
                                            ValueConverter = (value) => value.ToString(),
                                            PropertyInfo = typeof(TestModel).GetProperty(nameof(TestModel.Id)),
                                        },
                                        new PropertyColumn()
                                        {
                                            Name = "Name",
                                            Width = 0.9f,
                                            UnitType = UnitType.Star,
                                            HorizontalAlignment = HorizontalAlignment.Left,
                                            VerticalAlignment = VerticalAlignment.Center,
                                            MaxLinesCount = 2,
                                            Paint = new SKPaint()
                                            {
                                                Color = SKColors.Black,
                                                Style = SKPaintStyle.Fill,
                                                TextSize = 14
                                            },
                                            ValueConverter = (value) => value.ToString(),
                                            PropertyInfo = typeof(TestModel).GetProperty(nameof(TestModel.Name)),
                                        }
                                    }
                                }
                        },
                        new Row()
                        {
                            Height = 50,
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
                                            TextSize = 12
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
