using Plugin.Maui.FacebookAppEvents.Events;
using Plugin.Maui.FacebookAppEvents.Services;

namespace Plugin.Maui.FacebookAppEvents.Extensions
{
    /// <summary>
    /// Extension methods for MauiAppBuilder to easily register FacebookAppEvents services.
    /// </summary>
    public static class MauiAppBuilderExtensions
    {
        /// <summary>
        /// Registers FacebookAppEvents services with dependency injection.
        /// This extension method makes it super easy to add Facebook App Events to your MAUI app.
        /// </summary>
        /// <param name="builder">The MauiAppBuilder instance.</param>
        /// <param name="appId">Your Facebook App ID from developers.facebook.com</param>
        /// <param name="clientToken">Your Facebook Client Token from developers.facebook.com</param>
        /// <returns>The updated MauiAppBuilder for method chaining.</returns>
        public static MauiAppBuilder UseFacebookEvents(this MauiAppBuilder builder, string appId, string clientToken)
        {
            if (string.IsNullOrWhiteSpace(appId))
                throw new ArgumentException("Facebook App ID cannot be null or empty", nameof(appId));

            if (string.IsNullOrWhiteSpace(clientToken))
                throw new ArgumentException("Facebook Client Token cannot be null or empty", nameof(clientToken));

            // Register platform-specific advertiser ID services
#if ANDROID
            builder.Services.AddSingleton<IAdvertiserIdService, Platforms.Android.AdvertiserIdService>();
#elif IOS
            builder.Services.AddSingleton<IAdvertiserIdService, Platforms.iOS.AdvertiserIdService>();
#endif

            // Register the Facebook event sender
            builder.Services.AddSingleton<FacebookAppEventSender>(provider =>
            {
                var httpClient = new HttpClient();
                var advertiserIdService = provider.GetRequiredService<IAdvertiserIdService>();
                return new FacebookAppEventSender(httpClient, appId, clientToken, advertiserIdService);
            });

            return builder;
        }

        /// <summary>
        /// Registers FacebookAppEvents services with a custom HttpClient configuration.
        /// Use this if you need to configure the HttpClient (timeout, headers, etc.).
        /// </summary>
        /// <param name="builder">The MauiAppBuilder instance.</param>
        /// <param name="appId">Your Facebook App ID.</param>
        /// <param name="clientToken">Your Facebook Client Token.</param>
        /// <param name="configureHttpClient">Action to configure the HttpClient.</param>
        /// <returns>The updated MauiAppBuilder for method chaining.</returns>
        public static MauiAppBuilder UseFacebookEvents(this MauiAppBuilder builder, string appId, string clientToken, Action<HttpClient> configureHttpClient)
        {
            if (string.IsNullOrWhiteSpace(appId))
                throw new ArgumentException("Facebook App ID cannot be null or empty", nameof(appId));

            if (string.IsNullOrWhiteSpace(clientToken))
                throw new ArgumentException("Facebook Client Token cannot be null or empty", nameof(clientToken));

#if ANDROID
            builder.Services.AddSingleton<IAdvertiserIdService, Platforms.Android.AdvertiserIdService>();
#elif IOS
            builder.Services.AddSingleton<IAdvertiserIdService, Platforms.iOS.AdvertiserIdService>();
#endif

            builder.Services.AddSingleton<FacebookAppEventSender>(provider =>
            {
                var httpClient = new HttpClient();
                configureHttpClient?.Invoke(httpClient);

                var advertiserIdService = provider.GetRequiredService<IAdvertiserIdService>();
                return new FacebookAppEventSender(httpClient, appId, clientToken, advertiserIdService);
            });

            return builder;
        }

        /// <summary>
        /// Registers FacebookAppEvents services with configuration options.
        /// Use this for advanced scenarios where you need more control.
        /// </summary>
        /// <param name="builder">The MauiAppBuilder instance.</param>
        /// <param name="configure">Action to configure FacebookAppEvents options.</param>
        /// <returns>The updated MauiAppBuilder for method chaining.</returns>
        public static MauiAppBuilder UseFacebookEvents(this MauiAppBuilder builder, Action<FacebookEventsOptions> configure)
        {
            var options = new FacebookEventsOptions();
            configure(options);

            if (string.IsNullOrWhiteSpace(options.AppId))
                throw new ArgumentException("Facebook App ID must be provided in options");

            if (string.IsNullOrWhiteSpace(options.ClientToken))
                throw new ArgumentException("Facebook Client Token must be provided in options");

#if ANDROID
            builder.Services.AddSingleton<IAdvertiserIdService, Platforms.Android.AdvertiserIdService>();
#elif IOS
            builder.Services.AddSingleton<IAdvertiserIdService, Platforms.iOS.AdvertiserIdService>();
#endif

            builder.Services.AddSingleton<FacebookAppEventSender>(provider =>
            {
                var httpClient = new HttpClient();
                options.ConfigureHttpClient?.Invoke(httpClient);

                var advertiserIdService = provider.GetRequiredService<IAdvertiserIdService>();
                var sender = new FacebookAppEventSender(httpClient, options.AppId, options.ClientToken, advertiserIdService);

                if (options.AutoLogAppLaunch)
                {
                    var activateEvent = FacebookAppEventFactory.CreateCustomEvent(
                        eventName: "fb_mobile_activate_app"
                    );

                    _ = sender.SendEventsAsync(activateEvent);
                }

                return sender;
            });

            return builder;
        }
    }

    /// <summary>
    /// Configuration options for FacebookAppEvents.
    /// </summary>
    public class FacebookEventsOptions
    {
        /// <summary>
        /// Your Facebook App ID from developers.facebook.com
        /// </summary>
        public string AppId { get; set; } = string.Empty;

        /// <summary>
        /// Your Facebook Client Token from developers.facebook.com
        /// </summary>
        public string ClientToken { get; set; } = string.Empty;

        /// <summary>
        /// Optional action to configure the HttpClient used for sending events.
        /// </summary>
        public Action<HttpClient>? ConfigureHttpClient { get; set; }

        /// <summary>
        /// When true, the plugin will automatically send a fb_mobile_activate_app
        /// event once when the FacebookAppEventSender is created.
        /// </summary>
        public bool AutoLogAppLaunch { get; set; } = true;
    }
}
