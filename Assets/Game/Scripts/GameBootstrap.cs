using UnityEngine;

namespace JetpackJoe
{
    // builds the whole game at runtime from one GameObject in the scene
    public class GameBootstrap : MonoBehaviour
    {
        void Awake()
        {
            PlatformConfig.ApplyDisplaySettings();
            Time.timeScale = 1f;
            GameManager.ApplySavedDisplaySettings();

            var camera = Camera.main;
            if (camera == null)
            {
                var cameraGo = new GameObject("Main Camera");
                cameraGo.tag = "MainCamera";
                camera = cameraGo.AddComponent<Camera>();
                cameraGo.AddComponent<AudioListener>();
            }

            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Palette.Base;
            // no post-processing and only unlit sprites, so HDR would just cost mobile bandwidth
            camera.allowHDR = false;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;

            var audioGo = new GameObject("Audio");
            var audio = audioGo.AddComponent<GameAudio>();
            audio.Build();

            var managerGo = new GameObject("GameManager");
            var manager = managerGo.AddComponent<GameManager>();

            var backdropGo = new GameObject("City");
            var backdrop = backdropGo.AddComponent<CityBackdrop>();
            backdrop.Init(manager);

            var playerGo = new GameObject("Player");
            var player = playerGo.AddComponent<PlayerController>();
            player.Init(manager);

            var orbsGo = new GameObject("HeartOrbs");
            orbsGo.transform.SetParent(playerGo.transform, false);
            var heartOrbs = orbsGo.AddComponent<HeartOrbs>();
            heartOrbs.Init(manager);

            var spawnerGo = new GameObject("World");
            var spawner = spawnerGo.AddComponent<WorldSpawner>();
            spawner.Init(manager);

            var hudGo = new GameObject("HUD");
            var hud = hudGo.AddComponent<GameHud>();
            hud.Build();

            manager.Init(player, spawner, backdrop, hud, audio, camera);
        }
    }
}
