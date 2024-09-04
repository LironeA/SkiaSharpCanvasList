using CanvasScrollTest.Components.ListComponent;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using SkiaSharp.Views.Maui.Handlers;

namespace CanvasScrollTest
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                      .UseMauiApp<App>()
                      .UseMauiCommunityToolkit()
                      .UseSkiaSharp()
                      .ConfigureFonts(fonts =>
                      {
                          fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                          fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                      });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddTransient<MainPage>();
            builder.Services.AddSingleton<MainPageVM>();

            builder.ConfigureMauiHandlers(h =>
            {
                h.AddHandler<CustomList, SKCanvasViewHandler>();
            });

            return builder.Build();
        }
    }
}
