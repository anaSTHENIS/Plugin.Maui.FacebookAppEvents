using Plugin.Maui.FacebookAppEvents.Services;
using Google.Ads.Identifier;

namespace Plugin.Maui.FacebookAppEvents.Platforms.Android;

/// <summary>
/// Android implementation of IAdvertiserIdService using Google Advertising ID (GAID).
/// Supports manual ATE override via FacebookTrackingSettings.
/// </summary>
public class AdvertiserIdService : IAdvertiserIdService
{
	/// <summary>
	/// Gets the Google Advertising ID (GAID) for Android devices.
	/// </summary>
	/// <returns>The GAID if available and tracking is enabled, null otherwise.</returns>
	public async Task<string?> GetAdvertiserIdAsync()
	{
		var isTracking = await IsTrackingEnabledAsync();
		if(!isTracking)
		{
			return null;
		}

		return await Task.Run(() =>
		{
			try
			{
				var info = AdvertisingIdClient.GetAdvertisingIdInfo(global::Android.App.Application.Context);

				if(info != null &&
					!string.IsNullOrEmpty(info.Id) &&
					info.Id != "00000000-0000-0000-0000-000000000000")
				{
					return info.Id;
				}
			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Android Advertising ID error: {ex.Message}");
			}

			return null;
		}).ConfigureAwait(false);
	}

	/// <summary>
	/// Checks if the user has enabled advertising tracking on Android.
	/// If FacebookTrackingSettings.SetAdvertiserTrackingEnabled() was called, uses that value.
	/// </summary>
	/// <returns>True if tracking is enabled, false if limited or unavailable.</returns>
	public async Task<bool> IsTrackingEnabledAsync()
	{
		// Check for manual override first
		if(FacebookTrackingSettings.HasManualTrackingValue)
		{
			System.Diagnostics.Debug.WriteLine($"[Android AdvertiserIdService] Using manual ATE: {FacebookTrackingSettings.ManualTrackingValue}");
			return FacebookTrackingSettings.ManualTrackingValue!.Value;
		}

		return await Task.Run(() =>
		{
			try
			{
				var info = AdvertisingIdClient.GetAdvertisingIdInfo(global::Android.App.Application.Context);
				var isEnabled = info != null && !info.IsLimitAdTrackingEnabled;
				System.Diagnostics.Debug.WriteLine($"[Android AdvertiserIdService] LAT: {info?.IsLimitAdTrackingEnabled}, Enabled: {isEnabled}");
				return isEnabled;
			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Android tracking status error: {ex.Message}");
				return false;
			}
		}).ConfigureAwait(false);
	}
}