using AppTrackingTransparency;
using AdSupport;
using Foundation;
using Plugin.Maui.FacebookAppEvents.Services;

namespace Plugin.Maui.FacebookAppEvents.Platforms.iOS;

/// <summary>
/// iOS implementation of IAdvertiserIdService using IDFA with App Tracking Transparency.
/// Supports manual ATE override via FacebookTrackingSettings for external ATT handling.
/// </summary>
public class AdvertiserIdService : IAdvertiserIdService
{
	/// <summary>
	/// Gets the iOS Identifier for Advertisers (IDFA).
	/// </summary>
	/// <returns>The IDFA if available and authorized, null otherwise.</returns>
	public async Task<string?> GetAdvertiserIdAsync()
	{
		try
		{
			var isTracking = await IsTrackingEnabledAsync();
			if(isTracking)
			{
				return GetIDFA();
			}
		}
		catch(Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"iOS IDFA error: {ex.Message}");
		}

		return null;
	}

	/// <summary>
	/// Checks if App Tracking Transparency is authorized on iOS.
	/// If FacebookTrackingSettings.SetAdvertiserTrackingEnabled() was called, uses that value.
	/// Otherwise checks ATTrackingManager status directly (without showing dialog).
	/// </summary>
	/// <returns>True if authorized/enabled, false otherwise.</returns>
	public Task<bool> IsTrackingEnabledAsync()
	{
		try
		{
			// Check for manual override first (for external ATT handling)
			if(FacebookTrackingSettings.HasManualTrackingValue)
			{
				System.Diagnostics.Debug.WriteLine($"[iOS AdvertiserIdService] Using manual ATE: {FacebookTrackingSettings.ManualTrackingValue}");
				return Task.FromResult(FacebookTrackingSettings.ManualTrackingValue!.Value);
			}

			// iOS 14+ - Check ATT status directly (don't request/show dialog)
			if(NSProcessInfo.ProcessInfo.IsOperatingSystemAtLeastVersion(new NSOperatingSystemVersion(14, 0, 0)))
			{
				var status = ATTrackingManager.TrackingAuthorizationStatus;
				var isAuthorized = status == ATTrackingManagerAuthorizationStatus.Authorized;
				System.Diagnostics.Debug.WriteLine($"[iOS AdvertiserIdService] ATT Status: {status}, Authorized: {isAuthorized}");
				return Task.FromResult(isAuthorized);
			}
			else
			{
				// Pre-iOS 14 - use legacy API
				return Task.FromResult(ASIdentifierManager.SharedManager.IsAdvertisingTrackingEnabled);
			}
		}
		catch(Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"iOS tracking status error: {ex.Message}");
			return Task.FromResult(false);
		}
	}

	private string? GetIDFA()
	{
		try
		{
			var idfa = ASIdentifierManager.SharedManager.AdvertisingIdentifier.AsString();

			if(!string.IsNullOrEmpty(idfa) && idfa != "00000000-0000-0000-0000-000000000000")
			{
				return idfa;
			}
		}
		catch(Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"IDFA extraction error: {ex.Message}");
		}

		return null;
	}
}