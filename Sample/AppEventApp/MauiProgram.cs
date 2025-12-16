using Microsoft.Extensions.Logging;
using Plugin.Maui.FacebookAppEvents.Events;
using Plugin.Maui.FacebookAppEvents.Extensions;

namespace AppEventApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .UseFacebookEvents(options =>
                {
                    options.AutoLogAppLaunch = true;
                    options.AppId = "";
                    options.ClientToken = "";
                });

            var app = builder.Build();

            var facebookSender = app.Services.GetRequiredService<FacebookAppEventSender>();
            FacebookAppEventSender.InitializeInstance(facebookSender);
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return app;
        }
    }
}
