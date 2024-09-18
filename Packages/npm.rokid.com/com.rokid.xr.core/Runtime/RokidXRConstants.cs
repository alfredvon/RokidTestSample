
namespace Rokid.XR.Core
{
	public static class Constants
	{
#if UNITY_ANDROID
		public const string UXR_GFX_PLUGIN = "GfxPluginRokidXRLoader";
#else
		public const string UXR_GFX_PLUGIN = "NOT_AVAILABLE";
#endif
		public const string SettingsKey = "rokid.xr.package.setting";

		public const string runtimeSettingsPath = "Assets/XR/Resources/";
	}
}
