using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace JetpackJoe
{
    public enum GameState { Title, Playing, Paused, Dead }

    public class GameManager : MonoBehaviour
    {
        public const int MaxHearts = 3;
        const float BaseSpeed = 6.5f;
        const float MaxSpeedMultiplier = 2f;
        const float RampSeconds = 100f;
        const string BestKey = "JetpackJoe.BestKm";
        const string FullscreenKey = "JetpackJoe.Fullscreen";

        public GameState State { get; private set; } = GameState.Title;
        public float DistanceMeters { get; private set; }
        public float Elapsed { get; private set; }
        public float CurrentSpeed { get; private set; } = BaseSpeed;
        public int Hearts { get; private set; } = 1;
        public float BestKm { get; private set; }
        public float DistanceKm => DistanceMeters / 1000f;
        public bool IsNewRecord { get; private set; }
        public PlayerController Player { get; private set; }
        public WorldSpawner Spawner { get; private set; }
        public CityBackdrop Backdrop { get; private set; }
        public GameHud Hud { get; private set; }
        public GameAudio Audio { get; private set; }

        Camera gameCamera;
        Vector3 cameraHome;
        Coroutine shakeRoutine;
        bool restarting;
        float diedAt;

        public void Init(PlayerController player, WorldSpawner spawner, CityBackdrop backdrop, GameHud hud,
            GameAudio audio, Camera camera)
        {
            Player = player;
            Spawner = spawner;
            Backdrop = backdrop;
            Hud = hud;
            Audio = audio;
            gameCamera = camera;
            cameraHome = camera.transform.position;
            BestKm = PlayerPrefs.GetFloat(BestKey, 0f);
            State = GameState.Title;
            CurrentSpeed = BaseSpeed;
            Hearts = 1;
            Hud.ShowTitle(BestKm);
            Hud.SetHearts(Hearts, false);
            Hud.FadeFromBlack(0.7f);
            Hud.ToggleFullscreenRequested = ToggleFullscreen;
            Hud.ToggleVolumeRequested = () => ToggleMute(false);
            Hud.ExitRequested = QuitGame;
            Hud.SetSettings(Screen.fullScreen, !Audio.Muted);
        }

        public static void ApplySavedDisplaySettings()
        {
            if (PlayerPrefs.HasKey(FullscreenKey))
                Screen.fullScreen = PlayerPrefs.GetInt(FullscreenKey) == 1;
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.mKey.wasPressedThisFrame) ToggleMute();

            switch (State)
            {
                case GameState.Title:
                    if (JumpPressed()) StartRun();
                    break;

                case GameState.Playing:
                    Elapsed += Time.deltaTime;
                    CurrentSpeed = BaseSpeed * Mathf.Lerp(1f, MaxSpeedMultiplier, Mathf.Clamp01(Elapsed / RampSeconds));
                    DistanceMeters += CurrentSpeed * Time.deltaTime;
                    Hud.UpdateStats(DistanceKm, CurrentSpeed / BaseSpeed);
                    if (EscapePressed()) Pause();
                    break;

                case GameState.Paused:
                    if (EscapePressed()) Resume();
                    else if (RestartPressed()) Restart();
                    break;

                case GameState.Dead:
                    if (!restarting && Time.unscaledTime - diedAt > 0.65f && RestartPressed()) Restart();
                    break;
            }
        }

        void StartRun()
        {
            State = GameState.Playing;
            Elapsed = 0f;
            DistanceMeters = 0f;
            CurrentSpeed = BaseSpeed;
            Hearts = 1;
            IsNewRecord = false;
            Player.ResetForRun(false);
            Spawner.ResetWorld();
            Hud.BeginRun();
            Hud.SetHearts(Hearts, false);
            Hud.UpdateStats(0f, 1f);
            Audio.PlayStart();
            Time.timeScale = 1f;
        }

        public void AddHeart()
        {
            if (State != GameState.Playing) return;
            Audio.PlayHeart();
            if (Hearts < MaxHearts) Hearts++;
            Hud.SetHearts(Hearts, true);
            if (Hearts == MaxHearts)
            {
                // a full set is worth a small celebration, not another stacked heart
                GameArt.Burst(Player.transform.position + Vector3.up * 1.2f, Palette.Pink, 8, 3f, 0.12f, 0.45f, 6f);
            }
        }

        public bool TryDamage()
        {
            if (State != GameState.Playing || Player.IsInvulnerable) return false;

            Hearts = Mathf.Max(0, Hearts - 1);
            Audio.PlayHit();
            Player.TakeHit();
            Hud.SetHearts(Hearts, true);
            Shake(0.32f, 0.22f);
            StartCoroutine(HitStop());
            if (Hearts <= 0) Die();
            return true;
        }

        void Die()
        {
            State = GameState.Dead;
            diedAt = Time.unscaledTime;
            float km = DistanceKm;
            IsNewRecord = km > BestKm + 0.001f;
            if (km > BestKm)
            {
                BestKm = km;
                PlayerPrefs.SetFloat(BestKey, BestKm);
                PlayerPrefs.Save();
            }

            Audio.PlayDeath();
            Player.Die();
            GameArt.Burst(Player.transform.position + Vector3.up * 0.8f, Palette.Red, 26, 7f, 0.2f, 0.75f, 7f);
            Shake(0.42f, 0.3f);
            Hud.ShowGameOver(km, BestKm, IsNewRecord);
        }

        void Pause()
        {
            if (State != GameState.Playing) return;
            State = GameState.Paused;
            Time.timeScale = 0f;
            Audio.SetPaused(true);
            Hud.SetSettings(Screen.fullScreen, !Audio.Muted);
            Hud.ShowPaused(true);
        }

        void Resume()
        {
            if (State != GameState.Paused) return;
            State = GameState.Playing;
            Time.timeScale = 1f;
            Audio.SetPaused(false);
            Hud.ShowPaused(false);
        }

        void Restart()
        {
            if (restarting) return;
            restarting = true;
            Time.timeScale = 1f;
            Audio.SetPaused(false);
            StartCoroutine(RestartRoutine());
        }

        IEnumerator RestartRoutine()
        {
            Hud.FadeToBlack(0.3f);
            yield return new WaitForSecondsRealtime(0.32f);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        void ToggleMute(bool showToast = true)
        {
            bool muted = Audio.ToggleMute();
            Hud.SetSettings(Screen.fullScreen, !muted);
            if (showToast) Hud.Toast(muted ? "ხმა გამორთულია" : "ხმა ჩართულია");
        }

        void ToggleFullscreen()
        {
            Screen.fullScreen = !Screen.fullScreen;
            PlayerPrefs.SetInt(FullscreenKey, Screen.fullScreen ? 1 : 0);
            PlayerPrefs.Save();
            Hud.SetSettings(Screen.fullScreen, !Audio.Muted);
        }

        void QuitGame()
        {
            PlayerPrefs.Save();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        IEnumerator HitStop()
        {
            Time.timeScale = 0.06f;
            yield return new WaitForSecondsRealtime(0.06f);
            if (State != GameState.Paused) Time.timeScale = 1f;
        }

        void Shake(float duration, float magnitude)
        {
            if (shakeRoutine != null) StopCoroutine(shakeRoutine);
            shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
        }

        IEnumerator ShakeRoutine(float duration, float magnitude)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float falloff = 1f - Mathf.Clamp01(t / duration);
                Vector2 offset = Random.insideUnitCircle * magnitude * falloff;
                gameCamera.transform.position = cameraHome + new Vector3(offset.x, offset.y, 0f);
                yield return null;
            }

            gameCamera.transform.position = cameraHome;
            shakeRoutine = null;
        }

        static bool JumpPressed()
        {
            var keyboard = Keyboard.current;
            return keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame);
        }

        static bool RestartPressed()
        {
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.rKey.wasPressedThisFrame;
        }

        static bool EscapePressed()
        {
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
        }
    }
}
