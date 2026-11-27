using UnityEngine;

namespace JetpackJoe
{
    // display policy lives here so the bootstrap stays about wiring the game
    public static class PlatformConfig
    {
        const int MobileTargetFrameRate = 60;

        public static void ApplyDisplaySettings()
        {
            // android ignores vSyncCount and defaults to 30fps, so it needs an explicit target
            // desktop keeps -1 and relies on vSyncCount
#if UNITY_ANDROID && !UNITY_EDITOR
            Application.targetFrameRate = MobileTargetFrameRate;
#endif
        }
    }
}
