using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CanvasScrollTest.Components.ListComponent;
using CanvasScrollTest.Components.ListComponent;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Random = System.Random;
using HorizontalAlignment = Microsoft.Maui.Graphics.HorizontalAlignment;
using VerticalAlignment = Microsoft.Maui.Graphics.VerticalAlignment;
using SkiaSharp;

namespace CanvasScrollTest
{
    public partial class MainPageVM : ObservableObject
    {
        
        [ObservableProperty]
        public ObservableCollection<TestModel> _items = new ObservableCollection<TestModel>();


        public MainPageVM()
        {
                  
        }

        int id = 0;

        [RelayCommand]
        public void AddItem()
        {
            for (int i = 0; i < 20; i++)
            {
                Items.Add(new TestModel() { Id = id, Name = "Item " + id, Comment = id % 3 == 0 ? "This is a long text that will be split into multiple lines based on the width and maximum line count." : "" });
                id++;
            }
        }

        [RelayCommand]
        private void ChangeItem()
        {
            if (Items.Count > 0)
            {
                Items[0].Name = "Changed " + Random.Shared.Next();
            }
        }

        [RelayCommand]
        private void ChangeName(object? obj)
        {
            var item = obj as TestModel;
            item.Name = "Changed " + Random.Shared.Next();
        }

    }


    public partial class TestModel : ObservableObject
    {
        [ObservableProperty] public int _id;
        [ObservableProperty] public string _name;
        [ObservableProperty] public string _comment;
        [ObservableProperty] public bool _isChecked;
        [ObservableProperty] public bool _isSelected;
    }
}
