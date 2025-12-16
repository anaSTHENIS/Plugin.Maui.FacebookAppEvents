# Plugin.Maui.FacebookAppEvents


[![NuGet](https://img.shields.io/nuget/v/Plugin.Maui.FacebookAppEvents.svg)](https://www.nuget.org/packages/Plugin.Maui.FacebookAppEvents/)
[![Downloads](https://img.shields.io/nuget/dt/Plugin.Maui.FacebookAppEvents)](https://www.nuget.org/packages/Plugin.Maui.FacebookAppEvents/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0+-blue.svg)](https://dotnet.microsoft.com/download)


*Because implementing Facebook App Events shouldn't be this hard*

[Get Started](#installation) • [Examples](#examples) • [Troubleshooting](#troubleshooting)

---

## The Problem

You want to track app events in Facebook. You install their SDK. It's 50MB, breaks your build, and requires 30 lines of setup code just to track a simple purchase. There's got to be a better way.

## The Solution

This library does exactly one thing: sends Facebook App Events from your .NET MAUI app. No bloat, no complexity, no headaches. It handles the annoying parts (getting IDFA/GAID, privacy permissions) so you can focus on what matters.

## What You Get

- Works with iOS and Android MAUI apps
- Handles advertising IDs automatically (IDFA on iOS, GAID on Android)
- Respects user privacy (ATT on iOS 14+, LAT on Android)
- Simple methods for common events (purchase, add to cart, login, etc.)
- Fully customizable when you need it
- Actually documented

## Installation

```
dotnet add package Plugin.Maui.FacebookAppEvents
```

## Setup

First, get your Facebook App ID and Client Token from [developers.facebook.com](https://developers.facebook.com/).

### Android
Add this to `AndroidManifest.xml`:
```
<uses-permission android:name="com.google.android.gms.permission.AD_ID" />
```

### iOS
Add this to `Info.plist`:
```
<key>NSUserTrackingUsageDescription</key>
<string>This helps us show you relevant ads and improve the app.</string>
```

## Quick Setup (Recommended)

The easiest way to get started - just one line in your `MauiProgram.cs`:

```
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    builder
        .UseMauiApp<App>()
        .ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
        })
        .UseFacebookEvents("YOUR_APP_ID", "YOUR_CLIENT_TOKEN");

    //version 1.1.4 +
    var app = builder.Build();

    var facebookSender = app.Services.GetRequiredService<FacebookAppEventSender>();
    FacebookAppEventSender.InitializeInstance(facebookSender);

    return app;
}
```

### Advanced Setup Options

Need more control? Here are additional setup options:

```
// With HttpClient configuration
builder.UseFacebookEvents("YOUR_APP_ID", "YOUR_CLIENT_TOKEN", httpClient =>
{
    httpClient.Timeout = TimeSpan.FromSeconds(30);
    httpClient.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0");
});

// With configuration options
builder.UseFacebookEvents(options =>
{
    options.AppId = "YOUR_APP_ID";
    options.ClientToken = "YOUR_CLIENT_TOKEN";
    options.ConfigureHttpClient = httpClient =>
    {
        httpClient.Timeout = TimeSpan.FromSeconds(15);
    };
});
```

<details>
<summary><strong>Manual Setup (if you prefer DIY)</strong></summary>

```
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    
#if ANDROID
    builder.Services.AddSingleton<IAdvertiserIdService, 
        Plugin.Maui.FacebookAppEvents.Platforms.Android.AdvertiserIdService>();
#elif IOS
    builder.Services.AddSingleton<IAdvertiserIdService, 
        Plugin.Maui.FacebookAppEvents.Platforms.iOS.AdvertiserIdService>();
#endif

    builder.Services.AddSingleton<FacebookAppEventSender>(provider =>
    {
        var httpClient = new HttpClient();
        var advertiserIdService = provider.GetRequiredService<IAdvertiserIdService>();
        return new FacebookAppEventSender(httpClient, "YOUR_APP_ID", "YOUR_CLIENT_TOKEN", advertiserIdService);
    });

    //version 1.1.4 +
    var app = builder.Build();

    var facebookSender = app.Services.GetRequiredService<FacebookAppEventSender>();
    FacebookAppEventSender.InitializeInstance(facebookSender);

    return app;
}
```

</details>

## Examples

### Track a Purchase
```
public async Task OnOrderCompleted(Order order)
{
    var items = order.Items.Select(item => new FacebookContentItem
    {
        Id = item.ProductId,
        Quantity = item.Quantity
    }).ToList();

    var purchaseEvent = FacebookAppEventFactory.CreatePurchaseEvent(
        items, 
        order.Total, 
        "USD"
    );

    FacebookAppEventSender.SendEvents(purchaseEvent);
}
```

### Track Add to Cart
```
var items = new List<FacebookContentItem>
{
    new() { Id = "product-123", Quantity = 1 }
};

var event = FacebookAppEventFactory.CreateAddToCartEvent(items);
FacebookAppEventSender.SendEvents(event);
```

### Track Screen Views
```
// In your page's OnAppearing or constructor
var screenEvent = FacebookAppEventFactory.CreateScreenViewEvent("ProductDetails");
FacebookAppEventSender.SendEvents(screenEvent);
```

### Track User Registration
```
var loginEvent = FacebookAppEventFactory.CreateLoginEvent();
FacebookAppEventSender.SendEvents(loginEvent);
```

### Custom Events
```
var customEvent = FacebookAppEventFactory.CreateCustomEvent(
    eventName: "video_completed",
    contentType: "media",
    contents: new List<FacebookContentItem>
    {
        new() { Id = "intro_video", Quantity = 1 }
    }
);

FacebookAppEventSender.SendEvents(customEvent);
```

### Send Multiple Events
```
// More efficient than sending one by one
FacebookAppEventSender.SendEvents(screenEvent, addToCartEvent, purchaseEvent);
```

### Static API Fire-and-Forget
```
FacebookAppEventSender.SendEvents(FacebookAppEventFactory.CreateScreenViewEvent(nameof(AppSettingsPage)));
```

## Don't Like Dependency Injection?

Fair enough. You can create everything manually:

```
IAdvertiserIdService advertiserService;
#if ANDROID
advertiserService = new Plugin.Maui.FacebookAppEvents.Platforms.Android.AdvertiserIdService();
#elif IOS
advertiserService = new Plugin.Maui.FacebookAppEvents.Platforms.iOS.AdvertiserIdService();
#endif

var sender = new FacebookAppEventSender(
    new HttpClient(), 
    "YOUR_APP_ID", 
    "YOUR_CLIENT_TOKEN", 
    advertiserService
);

var event = FacebookAppEventFactory.CreatePurchaseEvent(items, 99.99, "USD");
FacebookAppEventSender.SendEvents(event);
```

## Privacy

This library respects user privacy:

- **iOS 14+**: Checks App Tracking Transparency status automatically
- **Android**: Checks if user has limited ad tracking
- **Both**: If users opt out, sends empty advertiser IDs and marks tracking as disabled

No sketchy stuff, no personal data without consent.

### Manual ATE Control (External ATT Handling)

If your app handles the ATT dialog externally (e.g., to coordinate consent with Firebase Analytics or other SDKs), you can manually set the Advertiser Tracking Enabled (ATE) value:

```csharp
// In your AppDelegate.cs (iOS) after showing ATT dialog:
using Plugin.Maui.FacebookAppEvents;

ATTrackingManager.RequestTrackingAuthorization((status) =>
{
    var isAuthorized = status == ATTrackingManagerAuthorizationStatus.Authorized;
    FacebookTrackingSettings.SetAdvertiserTrackingEnabled(isAuthorized);

    // Now all Facebook events will use this ATE value
});
```

**Why use this?**
- iOS only allows showing the ATT dialog once per app install
- If you show the dialog in AppDelegate (for Firebase, etc.), the SDK can't determine the result automatically
- Manual ATE ensures the correct tracking status is sent with all events

**Available methods:**
| Method | Purpose |
|--------|---------|
| `FacebookTrackingSettings.SetAdvertiserTrackingEnabled(bool)` | Set ATE value after external ATT handling |
| `FacebookTrackingSettings.ClearManualTracking()` | Revert to automatic ATT detection |
| `FacebookTrackingSettings.HasManualTrackingValue` | Check if manual value is set |
| `FacebookTrackingSettings.ManualTrackingValue` | Get current manual value (nullable) |

**Note:** If `SetAdvertiserTrackingEnabled()` is never called, the SDK falls back to automatic ATT status detection (backward compatible).

## Troubleshooting

**Events not showing up in Facebook?**
- Check your App ID and Client Token (classic mistake)
- Events take 15-20 minutes to appear in Facebook's dashboard
- Test on a real device, not simulator
- Ensure Privacy permissions in iOS `Info.plist` and Android `AndroidManifest.xml`

**iOS permission issues?**
- Make sure the ATT description is in Info.plist
- Permission dialog only shows once per app install
- Need iOS 14+ for ATT

**Android advertising ID not working?**
- Verify AD_ID permission is in AndroidManifest.xml
- Update Google Play Services on your test device
- Library handles threading automatically (you're welcome)

## API Reference
| Method | Purpose |
|--------|---------|
| `SendEvents(params FacebookAppEvent[] events)` | Static fire-and-forget method |

### FacebookAppEventFactory Methods

| Method | Purpose |
|--------|---------|
| `CreatePurchaseEvent()` | Track completed purchases |
| `CreateAddToCartEvent()` | Track items added to cart |
| `CreateRemoveFromCartEvent()` | Track items removed from cart |
| `CreateScreenViewEvent()` | Track page/screen views |
| `CreateLoginEvent()` | Track user registration/login |
| `CreateSearchEvent()` | Track search queries |
| `CreateCustomEvent()` | Create any custom event |

### FacebookAppEvent Properties

| Property | Type | Description |
|----------|------|-------------|
| `EventName` | `string` | Facebook event name (required) |
| `EventId` | `string` | Unique event identifier |
| `FbContent` | `List<FacebookContentItem>` | Items involved in event |
| `FbContentType` | `string` | Content type ("product", "screen", etc.) |
| `ValueToSum` | `decimal?` | Monetary value |
| `FbCurrency` | `string` | Currency code ("USD", "EUR", etc.) |

## Contributing

Found a bug? Want to add something? Cool, but let's keep it simple. This library intentionally does one thing well rather than trying to be everything to everyone.

1. Fork it
2. Create your feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

MIT License. Use it however you want.

## Support

- **Bug Reports**: [Create an issue](../../issues)
- **Feature Requests**: [Start a discussion](../../discussions)
- **Documentation**: Check the XML docs in your IDE

---

*Made with coffee and mild annoyance at Facebook's unnecessarily complex SDKs.*
