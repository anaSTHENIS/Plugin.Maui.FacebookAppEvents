namespace Plugin.Maui.FacebookAppEvents;

/// <summary>
/// Static settings for Facebook tracking configuration.
/// Use this to manually set ATE (Advertiser Tracking Enabled) when handling ATT externally.
/// </summary>
public static class FacebookTrackingSettings
{
	private static bool? _manualAdvertiserTrackingEnabled;

	/// <summary>
	/// Gets whether a manual ATE value has been set.
	/// </summary>
	public static bool HasManualTrackingValue => _manualAdvertiserTrackingEnabled.HasValue;

	/// <summary>
	/// Gets the manually set ATE value, or null if not set.
	/// </summary>
	public static bool? ManualTrackingValue => _manualAdvertiserTrackingEnabled;

	/// <summary>
	/// Manually set the Advertiser Tracking Enabled (ATE) value.
	/// Use this when you handle ATT dialog externally (e.g., for Firebase consent coordination).
	/// </summary>
	/// <param name="enabled">True if user authorized tracking, false if denied.</param>
	/// <example>
	/// // In AppDelegate after ATT dialog response:
	/// ATTrackingManager.RequestTrackingAuthorization((status) =>
	/// {
	///     var isAuthorized = status == ATTrackingManagerAuthorizationStatus.Authorized;
	///     FacebookTrackingSettings.SetAdvertiserTrackingEnabled(isAuthorized);
	/// });
	/// </example>
	public static void SetAdvertiserTrackingEnabled(bool enabled)
	{
		_manualAdvertiserTrackingEnabled = enabled;
		System.Diagnostics.Debug.WriteLine($"[FacebookTrackingSettings] Manual ATE set to: {enabled}");
	}

	/// <summary>
	/// Clears the manual ATE value, reverting to automatic ATT detection.
	/// </summary>
	public static void ClearManualTracking()
	{
		_manualAdvertiserTrackingEnabled = null;
		System.Diagnostics.Debug.WriteLine("[FacebookTrackingSettings] Manual ATE cleared, using automatic detection");
	}
}
