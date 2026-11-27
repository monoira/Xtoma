using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace JetpackJoe
{
    public class GameHud : MonoBehaviour
    {
        TMP_FontAsset regular;
        TMP_FontAsset bold;

        RectTransform hudRoot;
        CanvasGroup hudGroup;
        TextMeshProUGUI distanceValue;
        TextMeshProUGUI speedValue;
        RectTransform speedFill;
        Image[] hearts;
        CanvasGroup hintGroup;
        CanvasGroup toastGroup;
        TextMeshProUGUI toastText;

        RectTransform titleRoot;
        CanvasGroup titleGroup;
        TextMeshProUGUI titleBest;
        CanvasGroup promptGroup;

        RectTransform pauseRoot;
        CanvasGroup pauseGroup;
        TextMeshProUGUI fullscreenValue;
        TextMeshProUGUI volumeValue;
        Coroutine pauseRoutine;

        public System.Action ToggleFullscreenRequested;
        public System.Action ToggleVolumeRequested;
        public System.Action ExitRequested;

        RectTransform deadRoot;
        CanvasGroup deadGroup;
        TextMeshProUGUI deadDistance;
        TextMeshProUGUI deadBest;
        TextMeshProUGUI deadRecord;
        CanvasGroup deadPromptGroup;

        CanvasGroup fadeGroup;
        float lastStatsAt = -1f;
        Coroutine toastRoutine;
        Coroutine hintRoutine;
        int lastHeartCount = 1;

        public void Build()
        {
            regular = Resources.Load<TMP_FontAsset>("Fonts/NotoSansGeorgianSDF");
            bold = Resources.Load<TMP_FontAsset>("Fonts/NotoSansGeorgianBoldSDF");
            var latin = Resources.Load<TMP_FontAsset>("Fonts/NotoSansSDF");
            var latinBold = Resources.Load<TMP_FontAsset>("Fonts/NotoSansBoldSDF");
            if (regular == null) regular = TMP_Settings.defaultFontAsset;
            if (bold == null) bold = regular;
            AddFallback(regular, latin);
            AddFallback(bold, latinBold != null ? latinBold : latin);

            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            EnsureEventSystem();

            BuildHud();
            BuildTitle();
            BuildPause();
            BuildDead();
            BuildToast();
            BuildFade();
        }

        RectTransform FullScreen(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size,
            Vector2 pivot)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        Image Panel(string name, Transform parent, Vector2 position, Vector2 size, Color color)
        {
            var rect = Rect(name, parent, new Vector2(0.5f, 0.5f), position, size, new Vector2(0.5f, 0.5f));
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = GameArt.RoundedPanel;
            image.type = Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = false;
            var outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = Palette.A(Palette.Surface1, 0.7f);
            outline.effectDistance = new Vector2(2f, -2f);
            return image;
        }

        Image Block(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Color color)
        {
            var rect = Rect(name, parent, anchor, position, size, new Vector2(0.5f, 0.5f));
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        TextMeshProUGUI Text(string name, Transform parent, string content, float size, Color color,
            TextAlignmentOptions alignment, Vector2 anchor, Vector2 position, Vector2 rectSize, bool useBold = false,
            bool shadow = false)
        {
            var rect = Rect(name, parent, anchor, position, rectSize, anchor);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = useBold ? bold : regular;
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            if (shadow)
            {
                var drop = rect.gameObject.AddComponent<Shadow>();
                drop.effectColor = Palette.A(Palette.Crust, 0.9f);
                drop.effectDistance = new Vector2(4f, -5f);
            }

            return text;
        }

        static void AddFallback(TMP_FontAsset primary, TMP_FontAsset fallback)
        {
            if (primary == null || fallback == null) return;
            if (primary.fallbackFontAssetTable == null) primary.fallbackFontAssetTable = new List<TMP_FontAsset>();
            if (!primary.fallbackFontAssetTable.Contains(fallback)) primary.fallbackFontAssetTable.Add(fallback);
        }

        CanvasGroup Group(Component target)
        {
            var group = target.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 1f;
            return group;
        }

        void BuildHud()
        {
            hudRoot = FullScreen("HudRoot", transform);
            hudGroup = Group(hudRoot);

            Text("DistanceLabel", hudRoot, "მანძილი", 25f, Palette.Overlay2, TextAlignmentOptions.Left,
                new Vector2(0f, 1f), new Vector2(44f, -24f), new Vector2(400f, 32f));
            distanceValue = Text("DistanceValue", hudRoot, "0.00 კმ", 52f, Palette.Text,
                TextAlignmentOptions.Left, new Vector2(0f, 1f), new Vector2(42f, -54f), new Vector2(620f, 66f));

            Text("HeartsLabel", hudRoot, "სიცოცხლის გულები", 25f, Palette.Overlay2, TextAlignmentOptions.Right,
                new Vector2(1f, 1f), new Vector2(-40f, -26f), new Vector2(260f, 32f));
            hearts = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                var rect = Rect("Heart" + i, hudRoot, new Vector2(1f, 1f),
                    new Vector2(-40f - i * 68f, -68f), new Vector2(56f, 56f), new Vector2(1f, 1f));
                var image = rect.gameObject.AddComponent<Image>();
                image.sprite = GameArt.HeartEmpty;
                image.raycastTarget = false;
                hearts[i] = image;
            }

            speedValue = Text("SpeedValue", hudRoot, "სიჩქარე x1.0", 26f, Palette.Blue,
                TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(360f, 34f));
            var bar = Block("SpeedBar", hudRoot, new Vector2(0.5f, 1f), new Vector2(0f, -64f),
                new Vector2(220f, 6f), Palette.A(Palette.Surface0, 0.9f));
            var fill = new GameObject("SpeedFill", typeof(RectTransform));
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.SetParent(bar.rectTransform, false);
            fillRect.anchorMin = new Vector2(0f, 0.5f);
            fillRect.anchorMax = new Vector2(0f, 0.5f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.anchoredPosition = Vector2.zero;
            fillRect.sizeDelta = new Vector2(0f, 6f);
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = Palette.Blue;
            fillImage.raycastTarget = false;
            speedFill = fillRect;

            var hintRoot = Rect("HintRoot", hudRoot, new Vector2(0.5f, 0f), new Vector2(0f, 126f),
                new Vector2(860f, 46f), new Vector2(0.5f, 0f));
            var hintBg = hintRoot.gameObject.AddComponent<Image>();
            hintBg.sprite = GameArt.RoundedPanel;
            hintBg.type = Image.Type.Sliced;
            hintBg.color = Palette.A(Palette.Crust, 0.82f);
            hintBg.raycastTarget = false;
            hintGroup = Group(hintBg);
            Text("Hint", hintRoot, "SPACE ან ზედა ისარი -> ნახტომი", 24f, Palette.Subtext1,
                TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(840f, 40f));
        }

        void BuildTitle()
        {
            titleRoot = FullScreen("TitleRoot", transform);
            titleGroup = Group(titleRoot);
            Panel("TitlePanel", titleRoot, new Vector2(0f, 30f), new Vector2(980f, 500f),
                Palette.A(Palette.Crust, 0.78f));

            Block("Accent", titleRoot, new Vector2(0.5f, 0.5f), new Vector2(0f, 236f), new Vector2(180f, 5f),
                Palette.Blue);
            Text("GameTitle", titleRoot, "Xtoma", 110f, Palette.Blue, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(900f, 150f), true, true);
            Text("Subtitle", titleRoot, "იხტუნავე", 30f, Palette.Subtext1,
                TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0f, 62f), new Vector2(900f, 44f));

            var prompt = Text("Prompt", titleRoot, "დასაწყებად დააჭირე SPACE-ს ან ️ზედა ისარს", 32f,
                Palette.Text, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0f, -10f),
                new Vector2(900f, 48f));
            promptGroup = Group(prompt);
            StartCoroutine(Pulse(promptGroup, 2.4f, 0.45f, 1f));

            Text("Controls", titleRoot, "ორმაგი ნახტომი -> ორჯერ დააჭირე", 25f, Palette.Overlay2,
                TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0f, -72f), new Vector2(900f, 36f));
            Text("HeartsHint", titleRoot, "გულები გიცავს სიკვდილისგან · მაქსიმუმ სამჯერ!", 24f, Palette.Overlay1,
                TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0f, -112f), new Vector2(900f, 36f));
            titleBest = Text("Best", titleRoot, "რეკორდი: 0.00 კმ", 28f, Palette.Yellow,
                TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0f, -170f), new Vector2(900f, 40f));
        }

        void BuildPause()
        {
            pauseRoot = FullScreen("PauseRoot", transform);
            var scrim = FullScreen("Scrim", pauseRoot).gameObject.AddComponent<Image>();
            scrim.color = Palette.A(Palette.Crust, 0.72f);
            pauseGroup = Group(pauseRoot);
            Panel("PausePanel", pauseRoot, Vector2.zero, new Vector2(620f, 560f), Palette.A(Palette.Mantle, 0.97f));

            Text("PauseTitle", pauseRoot, "პაუზა", 56f, Palette.Blue, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 200f), new Vector2(560f, 84f), true, true);
            Block("PauseAccent", pauseRoot, new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(120f, 4f),
                Palette.Blue);

            Text("FullscreenLabel", pauseRoot, "სრული ეკრანი", 30f, Palette.Text, TextAlignmentOptions.Left,
                new Vector2(0.5f, 0.5f), new Vector2(-160f, 78f), new Vector2(240f, 40f));
            var fullscreenButton = UiButton("FullscreenButton", pauseRoot, new Vector2(160f, 78f),
                new Vector2(240f, 64f), out fullscreenValue);
            fullscreenButton.onClick.AddListener(() => ToggleFullscreenRequested?.Invoke());

            Text("VolumeLabel", pauseRoot, "ხმა", 30f, Palette.Text, TextAlignmentOptions.Left,
                new Vector2(0.5f, 0.5f), new Vector2(-160f, 0f), new Vector2(240f, 40f));
            var volumeButton = UiButton("VolumeButton", pauseRoot, new Vector2(160f, 0f), new Vector2(240f, 64f),
                out volumeValue);
            volumeButton.onClick.AddListener(() => ToggleVolumeRequested?.Invoke());

            var exitButton = UiButton("ExitButton", pauseRoot, new Vector2(0f, -96f), new Vector2(500f, 72f),
                out var exitLabel);
            exitLabel.text = "გასვლა";
            exitLabel.color = Palette.Red;
            var exitColors = exitButton.colors;
            exitColors.highlightedColor = Palette.A(Palette.Red, 0.28f);
            exitButton.colors = exitColors;
            exitButton.onClick.AddListener(() => ExitRequested?.Invoke());

            Text("PauseHint", pauseRoot, "ESC -> გაგრძელება · R -> თავიდან დაწყება", 23f, Palette.Overlay1,
                TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0f, -196f), new Vector2(560f, 34f));
            pauseRoot.gameObject.SetActive(false);
            SetSettings(Screen.fullScreen, AudioListener.volume > 0f);
        }

        Button UiButton(string name, Transform parent, Vector2 position, Vector2 size, out TextMeshProUGUI label)
        {
            var rect = Rect(name, parent, new Vector2(0.5f, 0.5f), position, size, new Vector2(0.5f, 0.5f));
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = GameArt.RoundedPanel;
            image.type = Image.Type.Sliced;
            image.color = Color.white;

            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = Palette.Surface0;
            colors.highlightedColor = Palette.Surface1;
            colors.pressedColor = Palette.Surface2;
            colors.selectedColor = Palette.Surface0;
            colors.disabledColor = Palette.Surface0;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            label = Text(name + "Label", rect, "", 28f, Palette.Text, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), Vector2.zero, size, true);
            return button;
        }

        public void SetSettings(bool fullscreen, bool soundOn)
        {
            fullscreenValue.text = fullscreen ? "ჩართულია" : "გამორთულია";
            fullscreenValue.color = fullscreen ? Palette.Green : Palette.Overlay1;
            volumeValue.text = soundOn ? "ჩართულია" : "გამორთულია";
            volumeValue.color = soundOn ? Palette.Green : Palette.Overlay1;
        }

        static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }

        void BuildDead()
        {
            deadRoot = FullScreen("DeadRoot", transform);
            var scrim = FullScreen("Scrim", deadRoot).gameObject.AddComponent<Image>();
            scrim.color = Palette.A(Palette.Crust, 0.84f);
            scrim.raycastTarget = false;
            deadGroup = Group(deadRoot);
            Panel("DeadPanel", deadRoot, Vector2.zero, new Vector2(860f, 520f), Palette.A(Palette.Mantle, 0.97f));
            Text("DeadTitle", deadRoot, "თამაში დასრულდა", 62f, Palette.Red, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 170f), new Vector2(800f, 80f), true, true);
            Text("DeadSubtitle", deadRoot, "გულები ამოიწურა", 24f, Palette.Overlay2,
                TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0f, 112f), new Vector2(800f, 34f));
            Text("DeadDistanceLabel", deadRoot, "მანძილი", 22f, Palette.Overlay2, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 62f), new Vector2(800f, 30f));
            deadDistance = Text("DeadDistance", deadRoot, "0.00 კმ", 82f, Palette.Text,
                TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0f, 4f), new Vector2(800f, 100f),
                true, true);
            deadBest = Text("DeadBest", deadRoot, "რეკორდი: 0.00 კმ", 28f, Palette.Yellow,
                TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0f, -64f), new Vector2(800f, 40f));
            deadRecord = Text("DeadRecord", deadRoot, "ახალი რეკორდი!", 32f, Palette.Yellow,
                TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0f, -110f), new Vector2(800f, 40f));
            var prompt = Text("DeadPrompt", deadRoot, "R -> თავიდან დაწყება", 30f, Palette.Text,
                TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0f, -180f), new Vector2(800f, 44f));
            deadPromptGroup = Group(prompt);
            StartCoroutine(Pulse(deadPromptGroup, 2.2f, 0.4f, 1f));
            deadRoot.gameObject.SetActive(false);
        }

        void BuildToast()
        {
            toastText = Text("Toast", transform, "", 28f, Palette.Yellow, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0f), new Vector2(0f, 172f), new Vector2(900f, 40f), false, true);
            toastGroup = Group(toastText);
            toastGroup.alpha = 0f;
        }

        void BuildFade()
        {
            var rect = FullScreen("Fade", transform);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = Palette.Crust;
            image.raycastTarget = false;
            fadeGroup = Group(rect);
            fadeGroup.alpha = 1f;
            fadeGroup.blocksRaycasts = false;
            fadeGroup.interactable = false;
        }

        public void ShowTitle(float bestKm)
        {
            titleRoot.gameObject.SetActive(true);
            titleGroup.alpha = 1f;
            titleBest.text = "რეკორდი: " + FormatKm(bestKm);
            hudRoot.gameObject.SetActive(false);
            pauseRoot.gameObject.SetActive(false);
            deadRoot.gameObject.SetActive(false);
        }

        public void BeginRun()
        {
            titleRoot.gameObject.SetActive(false);
            deadRoot.gameObject.SetActive(false);
            pauseRoot.gameObject.SetActive(false);
            hudRoot.gameObject.SetActive(true);
            hudGroup.alpha = 0f;
            StartCoroutine(FadeCanvas(hudGroup, 1f, 0.25f, false));
            hintGroup.alpha = 1f;
            if (hintRoutine != null) StopCoroutine(hintRoutine);
            hintRoutine = StartCoroutine(HintFade());
        }

        IEnumerator HintFade()
        {
            yield return new WaitForSecondsRealtime(6.5f);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / 0.8f;
                hintGroup.alpha = 1f - Mathf.Clamp01(t);
                yield return null;
            }
        }

        public void SetHearts(int count, bool pop)
        {
            int previous = lastHeartCount;
            count = Mathf.Clamp(count, 0, 3);
            lastHeartCount = count;
            var full = count >= GameManager.MaxHearts ? GameArt.HeartGold : GameArt.HeartFull;
            for (int i = 0; i < hearts.Length; i++)
                hearts[i].sprite = i < count ? full : GameArt.HeartEmpty;
            if (!pop) return;
            int index = count >= previous ? Mathf.Clamp(count - 1, 0, 2) : Mathf.Clamp(count, 0, 2);
            StartCoroutine(PopHeart(hearts[index].rectTransform));
        }

        IEnumerator PopHeart(RectTransform heart)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / 0.28f;
                float scale = 1f + Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI) * 0.3f;
                heart.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            heart.localScale = Vector3.one;
        }

        public void UpdateStats(float km, float speedMultiplier)
        {
            if (Time.unscaledTime - lastStatsAt < 0.06f) return;
            lastStatsAt = Time.unscaledTime;
            distanceValue.text = FormatKm(km);
            float ratio = Mathf.Clamp01((speedMultiplier - 1f) / 1f);
            speedValue.text = "სიჩქარე x" + speedMultiplier.ToString("0.0", CultureInfo.InvariantCulture);
            speedValue.color = Color.Lerp(Palette.Blue, Palette.Yellow, ratio);
            speedFill.sizeDelta = new Vector2(220f * ratio, 6f);
        }

        public void ShowGameOver(float km, float bestKm, bool isRecord)
        {
            hudRoot.gameObject.SetActive(false);
            pauseRoot.gameObject.SetActive(false);
            deadDistance.text = FormatKm(km);
            deadBest.text = "რეკორდი: " + FormatKm(bestKm);
            deadRecord.gameObject.SetActive(isRecord);
            deadRoot.gameObject.SetActive(true);
            deadGroup.alpha = 0f;
            StartCoroutine(ShowDeadRoutine());
        }

        IEnumerator ShowDeadRoutine()
        {
            yield return new WaitForSecondsRealtime(0.55f);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / 0.3f;
                deadGroup.alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
                yield return null;
            }
        }

        public void ShowPaused(bool paused)
        {
            if (pauseRoutine != null) StopCoroutine(pauseRoutine);
            if (paused)
            {
                pauseRoot.gameObject.SetActive(true);
                pauseGroup.alpha = 0f;
                pauseRoutine = StartCoroutine(FadeCanvas(pauseGroup, 1f, 0.15f, false));
            }
            else
            {
                pauseRoutine = StartCoroutine(FadeCanvas(pauseGroup, 0f, 0.12f, true));
            }
        }

        public void Toast(string message)
        {
            toastText.text = message;
            if (toastRoutine != null) StopCoroutine(toastRoutine);
            toastRoutine = StartCoroutine(ToastRoutine());
        }

        IEnumerator ToastRoutine()
        {
            toastGroup.alpha = 0f;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / 0.12f;
                toastGroup.alpha = Mathf.Clamp01(t);
                yield return null;
            }

            yield return new WaitForSecondsRealtime(1.1f);
            t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / 0.3f;
                toastGroup.alpha = 1f - Mathf.Clamp01(t);
                yield return null;
            }
        }

        IEnumerator FadeCanvas(CanvasGroup group, float target, float duration, bool hideAtEnd)
        {
            float start = group.alpha;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / duration;
                group.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(t));
                yield return null;
            }

            group.alpha = target;
            if (hideAtEnd) group.gameObject.SetActive(false);
        }

        IEnumerator Pulse(CanvasGroup group, float speed, float min, float max)
        {
            while (true)
            {
                if (group.gameObject.activeInHierarchy)
                    group.alpha = Mathf.Lerp(min, max, (Mathf.Sin(Time.unscaledTime * speed) + 1f) * 0.5f);
                yield return null;
            }
        }

        public void FadeFromBlack(float duration)
        {
            fadeGroup.alpha = 1f;
            StartCoroutine(FadeCanvas(fadeGroup, 0f, duration, false));
        }

        public void FadeToBlack(float duration)
        {
            StartCoroutine(FadeCanvas(fadeGroup, 1f, duration, false));
        }

        static string FormatKm(float km)
        {
            return km.ToString("0.00", CultureInfo.InvariantCulture) + " კმ";
        }
    }
}
