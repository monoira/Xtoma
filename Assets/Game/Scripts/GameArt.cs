using UnityEngine;

namespace JetpackJoe
{
    // every sprite in the game is generated here from palette colours, so the
    // project ships without external art and no engine default material slips in
    public static class GameArt
    {
        static Material spriteMaterial;
        static Sprite square;
        static Sprite glow;
        static Sprite heartFull;
        static Sprite heartGold;
        static Sprite heartEmpty;
        static Sprite roundedPanel;
        static Sprite spikes;
        static Sprite pillar;
        static Sprite laser;
        static Sprite orb;
        static Sprite orbGold;
        static Sprite moon;
        static Sprite groundTile;

        public static Material SpriteMaterial
        {
            get
            {
                if (spriteMaterial == null)
                {
                    spriteMaterial = Resources.Load<Material>("GameSpriteUnlit");
                    if (spriteMaterial == null)
                    {
                        var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                        if (shader == null) shader = Shader.Find("Sprites/Default");
                        spriteMaterial = new Material(shader) { name = "GameSpriteUnlit" };
                    }
                }

                return spriteMaterial;
            }
        }

        public static Sprite Square => square != null ? square : square = RoundedRect(100, 100, 30, Color.white, 100f);
        public static Sprite Glow => glow != null ? glow : glow = MakeGlow(128);
        public static Sprite HeartFull => heartFull != null ? heartFull : heartFull =
            MakeHeart("HeartFull", Palette.Red, Palette.A(Palette.Pink, 0.85f), true);
        public static Sprite HeartGold => heartGold != null ? heartGold : heartGold =
            MakeHeart("HeartGold", Palette.Yellow, Palette.A(Palette.Rosewater, 0.9f), true);
        public static Sprite HeartEmpty => heartEmpty != null ? heartEmpty : heartEmpty =
            MakeHeart("HeartEmpty", Palette.Surface0, Palette.A(Palette.Pink, 0.85f), false);
        public static Sprite RoundedPanel => roundedPanel != null ? roundedPanel : roundedPanel = MakePanel();
        public static Sprite Spikes => spikes != null ? spikes : spikes = MakeSpikes(3);
        public static Sprite Pillar => pillar != null ? pillar : pillar = MakePillar();
        public static Sprite Laser => laser != null ? laser : laser = MakeLaser();
        public static Sprite Orb => orb != null ? orb : orb =
            MakeOrb("Orb", Palette.Red, Palette.Rosewater, Palette.Flamingo);
        public static Sprite OrbGold => orbGold != null ? orbGold : orbGold =
            MakeOrb("OrbGold", Palette.Yellow, Palette.Rosewater, Palette.Peach);
        public static Sprite Moon => moon != null ? moon : moon = MakeMoon();
        public static Sprite GroundTile => groundTile != null ? groundTile : groundTile = MakeGround();

        public static SpriteRenderer NewSprite(Transform parent, string name, Sprite sprite, Color color, int order,
            Vector3 localPosition, Vector3? localScale = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale ?? Vector3.one;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = order;
            renderer.sharedMaterial = SpriteMaterial;
            return renderer;
        }

        public static Sprite MakeSprite(Texture2D texture, float pixelsPerUnit = 100f, Vector2? pivot = null,
            Vector4? border = null)
        {
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            var rect = new Rect(0, 0, texture.width, texture.height);
            return Sprite.Create(texture, rect, pivot ?? new Vector2(0.5f, 0.5f), pixelsPerUnit, 0,
                SpriteMeshType.FullRect, border ?? Vector4.zero);
        }

        public static Sprite RoundedRect(int width, int height, int radius, Color color, float pixelsPerUnit = 100f,
            Vector2? pivot = null)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            float hw = (width - 1) * 0.5f;
            float hh = (height - 1) * 0.5f;
            float r = Mathf.Min(radius, Mathf.Min(hw, hh));

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float qx = Mathf.Abs(x - hw) - (hw - r);
                    float qy = Mathf.Abs(y - hh) - (hh - r);
                    float outside = Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) + Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f));
                    float inside = Mathf.Min(Mathf.Max(qx, qy), 0f);
                    float distance = outside + inside - r;
                    float alpha = Mathf.Clamp01(0.5f - distance);
                    var c = color;
                    c.a *= alpha;
                    pixels[y * width + x] = c;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            tex.name = "RoundedRect";
            return MakeSprite(tex, pixelsPerUnit, pivot);
        }

        public static Sprite Circle(int size, Color color, float pixelsPerUnit = 100f)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            float half = (size - 1) * 0.5f;
            float r = half - 1f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - half;
                    float dy = y - half;
                    float alpha = Mathf.Clamp01(0.5f - (Mathf.Sqrt(dx * dx + dy * dy) - r));
                    var c = color;
                    c.a *= alpha;
                    pixels[y * size + x] = c;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            tex.name = "Circle";
            return MakeSprite(tex, pixelsPerUnit);
        }

        static Sprite MakeGlow(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            float half = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half;
                    float dy = (y - half) / half;
                    float d = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy));
                    float alpha = Mathf.Pow(1f - d, 2.6f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            tex.name = "Glow";
            return MakeSprite(tex, 100f);
        }

        static float HeartField(float x, float y)
        {
            float a = x * x + y * y - 1f;
            return a * a * a - x * x * y * y * y;
        }

        static bool InHeart(float x, float y)
        {
            return HeartField(x * 1.06f, y * 1.06f) <= 0f;
        }

        static Sprite MakeHeart(string name, Color fill, Color highlight, bool filled)
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            Color outline = filled ? Palette.Crust : Palette.Overlay0;

            for (int py = 0; py < size; py++)
            {
                for (int px = 0; px < size; px++)
                {
                    float coverageOuter = 0f;
                    float coverageInner = 0f;
                    float coverageShine = 0f;

                    for (int sy = 0; sy < 3; sy++)
                    {
                        for (int sx = 0; sx < 3; sx++)
                        {
                            float x = ((px + (sx + 0.5f) / 3f) / size) * 2.6f - 1.3f;
                            float y = ((py + (sy + 0.5f) / 3f) / size) * 2.6f - 1.3f;
                            if (InHeart(x * 1.12f, y * 1.12f)) coverageOuter += 1f / 9f;
                            if (InHeart(x, y)) coverageInner += 1f / 9f;

                            float hx = (x + 0.33f) / 0.19f;
                            float hy = (y - 0.3f) / 0.1f;
                            if (hx * hx + hy * hy < 1f) coverageShine += 1f / 9f;
                        }
                    }

                    Color c = Color.Lerp(new Color(0, 0, 0, 0), outline, coverageOuter);
                    c = Color.Lerp(c, fill, coverageInner);
                    if (filled) c = Color.Lerp(c, highlight, coverageShine * coverageInner);
                    pixels[py * size + px] = c;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            tex.name = name;
            return MakeSprite(tex, 100f);
        }

        static Sprite MakePanel()
        {
            // white with alpha so Image.color tints it; border lets it stay crisp when sliced
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            var pixels = new Color32[64 * 64];
            const int radius = 18;
            float hw = 31.5f;
            float hh = 31.5f;
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float qx = Mathf.Abs(x - hw) - (hw - radius);
                    float qy = Mathf.Abs(y - hh) - (hh - radius);
                    float outside = Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) + Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f));
                    float inside = Mathf.Min(Mathf.Max(qx, qy), 0f);
                    float alpha = Mathf.Clamp01(0.5f - (outside + inside - radius));
                    pixels[y * 64 + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            tex.name = "RoundedPanel";
            var sprite = MakeSprite(tex, 100f, new Vector2(0.5f, 0.5f), new Vector4(18, 18, 18, 18));
            return sprite;
        }

        static Sprite MakeSpikes(int count)
        {
            int width = 64 * count;
            int height = 88;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = (x % 64) / 64f;
                    float v = y / (float)(height - 1);
                    bool outer = v < 0.16f ? u <= 1f : Mathf.Abs(u - 0.5f) <= 0.5f * (1f - (v - 0.16f) / 0.84f) + 0.05f;
                    bool inner = v < 0.12f ? u <= 1f : Mathf.Abs(u - 0.5f) <= 0.5f * (1f - (v - 0.12f) / 0.88f);
                    Color c = new Color(0, 0, 0, 0);
                    if (outer) c = Palette.Crust;
                    if (inner) c = Color.Lerp(Palette.Maroon, Palette.Red, Mathf.Clamp01((v - 0.3f) / 0.7f));
                    if (inner && v > 0.82f) c = Palette.Rosewater;
                    if (v < 0.12f) c = Palette.Surface1;
                    pixels[y * width + x] = c;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            tex.name = "Spikes";
            return MakeSprite(tex, 100f, new Vector2(0.5f, 0f));
        }

        static Sprite MakePillar()
        {
            const int width = 64;
            const int height = 256;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color c = Palette.Surface0;
                    if (x < 2 || x > width - 3) c = Palette.Overlay0;
                    if (y > height - 8) c = Palette.Red;
                    if (y > height - 14 && y <= height - 8) c = Palette.Maroon;
                    if (y < height - 40 && y > height - 150 && ((x + y) % 40) < 11) c = Palette.A(Palette.Red, 0.75f);
                    if (y < 16) c = Palette.Surface1;
                    if (y % 64 == 0) c = Palette.Surface1;
                    pixels[y * width + x] = c;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            tex.name = "Pillar";
            return MakeSprite(tex, 100f, new Vector2(0.5f, 0f));
        }

        static Sprite MakeLaser()
        {
            const int width = 512;
            const int height = 56;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                float d = Mathf.Abs(y - (height - 1) * 0.5f) / ((height - 1) * 0.5f);
                float alpha = Mathf.Pow(1f - d, 1.8f);
                Color c = Color.Lerp(Palette.Red, Palette.Rosewater, Mathf.Pow(Mathf.Clamp01(1f - d * 1.5f), 2f));
                c.a = alpha;
                if (d < 0.16f) c = Palette.Rosewater;
                for (int x = 0; x < width; x++)
                {
                    if (x < 6 || x > width - 7) c.a *= 0.25f;
                    pixels[y * width + x] = c;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            tex.name = "Laser";
            return MakeSprite(tex, 100f);
        }

        static Sprite MakeOrb(string name, Color color, Color core, Color center)
        {
            const int size = 192;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            float half = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half;
                    float dy = (y - half) / half;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    Color c = new Color(0, 0, 0, 0);
                    if (d < 0.92f) c = Palette.A(color, Mathf.Pow(1f - d, 2.2f) * 0.55f);
                    if (d < 0.62f && d > 0.47f) c = color;
                    if (d < 0.52f && d > 0.47f) c = core;
                    if (d < 0.32f) c = core;
                    if (d < 0.26f) c = center;
                    pixels[y * size + x] = c;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            tex.name = name;
            return MakeSprite(tex, 100f);
        }

        static Sprite MakeMoon()
        {
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            float half = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half;
                    float dy = (y - half) / half;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    Color c = new Color(0, 0, 0, 0);
                    if (d < 0.98f) c = Palette.A(Palette.Rosewater, Mathf.Pow(1f - d, 3f) * 0.3f);
                    if (d < 0.52f) c = Palette.Rosewater;
                    // craters, kept subtle so the moon stays a graphic shape
                    float c1 = Mathf.Sqrt((dx + 0.14f) * (dx + 0.14f) + (dy - 0.12f) * (dy - 0.12f));
                    float c2 = Mathf.Sqrt((dx - 0.1f) * (dx - 0.1f) + (dy + 0.15f) * (dy + 0.15f));
                    float c3 = Mathf.Sqrt((dx + 0.2f) * (dx + 0.2f) + (dy + 0.18f) * (dy + 0.18f));
                    if (c1 < 0.11f || c2 < 0.08f || c3 < 0.06f) c = Palette.A(Palette.Maroon, 0.35f);
                    pixels[y * size + x] = c;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            tex.name = "Moon";
            return MakeSprite(tex, 100f);
        }

        static Sprite MakeGround()
        {
            const int width = 1024;
            const int height = 192;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color c = Palette.Mantle;
                    if (y > height - 6) c = Palette.Surface1;
                    else if (y > height - 20) c = Palette.Surface0;
                    else if (y < 10) c = Palette.Crust;
                    if (x % 160 == 0 && y < height - 12) c = Palette.A(Palette.Surface1, 0.55f);
                    if (y > height - 34 && y < height - 26 && (x + y * 3) % 97 < 5) c = Palette.Surface2;
                    if (y > height - 15 && y < height - 9 && x % 160 == 80) c = Palette.Yellow;
                    if (y > height - 15 && y < height - 9 && x % 160 == 40) c = Palette.Red;
                    pixels[y * width + x] = c;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            tex.name = "Ground";
            return MakeSprite(tex, 64f, new Vector2(0.5f, 1f));
        }

        public static Sprite Skyline(bool near, int seed)
        {
            const int width = 1024;
            const int height = 512;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0, 0, 0, 0);
            var rng = new System.Random(seed);
            Color body = near ? Palette.Crust : Palette.Mantle;
            Color roof = near ? Palette.Surface0 : Palette.Surface1;
            Color windowWarm = near ? Palette.A(Palette.Yellow, 0.75f) : Palette.A(Palette.Overlay1, 0.55f);
            Color windowCool = near ? Palette.A(Palette.Blue, 0.6f) : Palette.A(Palette.Overlay0, 0.5f);

            void Set(int x, int y, Color c)
            {
                if (y < 0 || y >= height) return;
                int wx = ((x % width) + width) % width;
                pixels[y * width + wx] = c;
            }

            int start = 0;
            while (start < width)
            {
                int bw = rng.Next(70, 150);
                int bh = rng.Next(near ? 110 : 80, near ? 390 : 300);
                for (int x = start; x < start + bw; x++)
                {
                    for (int y = 0; y < bh; y++)
                    {
                        Color c = body;
                        if (y >= bh - 3) c = roof;
                        if (x == start || x == start + bw - 1) c = Palette.A(roof, 0.8f);
                        Set(x, y, c);
                    }

                    if (rng.NextDouble() < (near ? 0.3 : 0.16))
                    {
                        int wx = start + 8 + rng.Next(Mathf.Max(1, bw - 20));
                        int wy = 16 + rng.Next(Mathf.Max(1, bh - 32));
                        Color wc = rng.NextDouble() < 0.72 ? windowWarm : windowCool;
                        for (int yy = 0; yy < 8; yy++)
                            for (int xx = 0; xx < 5; xx++)
                                if (wy + yy < bh - 6) Set(wx + xx, wy + yy, wc);
                    }
                }

                if (near && bh > 260)
                {
                    int antennaX = start + bw / 2;
                    for (int y = bh; y < bh + 26; y++) Set(antennaX, y, Palette.Surface1);
                    Set(antennaX, bh + 26, Palette.Red);
                }

                start += bw + rng.Next(4, 22);
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            tex.name = near ? "SkylineNear" : "SkylineFar";
            return MakeSprite(tex, 64f, new Vector2(0.5f, 0f));
        }

        public static Sprite StarTile(int seed)
        {
            const int width = 1024;
            const int height = 512;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0, 0, 0, 0);
            var rng = new System.Random(seed);
            Color[] colors = { Palette.Text, Palette.Rosewater, Palette.Sky, Palette.Pink, Palette.Overlay2 };
            for (int i = 0; i < 70; i++)
            {
                int x = rng.Next(width);
                int y = rng.Next(40, height);
                int s = rng.Next(1, 4);
                Color c = colors[rng.Next(colors.Length)];
                c.a = (float)(0.25 + rng.NextDouble() * 0.6);
                for (int yy = 0; yy < s; yy++)
                    for (int xx = 0; xx < s; xx++)
                        pixels[(y + yy) * width + x + xx] = c;
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            tex.name = "Stars";
            return MakeSprite(tex, 64f, new Vector2(0.5f, 0f));
        }

        public static Sprite SkyGradient()
        {
            const int width = 16;
            const int height = 512;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                float t = y / (float)(height - 1);
                Color c = Color.Lerp(Palette.Mantle, Palette.Base, t);
                if (t < 0.3f) c = Color.Lerp(Palette.Mauve * 0.5f + Palette.Mantle * 0.5f, c, t / 0.3f);
                for (int x = 0; x < width; x++) pixels[y * width + x] = c;
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            tex.name = "SkyGradient";
            return MakeSprite(tex, 50f);
        }

        public static void Burst(Vector3 position, Color color, int count, float speed, float size = 0.16f,
            float life = 0.55f, float gravity = 9f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * 0.85f + 0.4f).normalized;
                SpawnParticle(position, direction * speed * Random.Range(0.35f, 1f), gravity,
                    life * Random.Range(0.7f, 1.25f), size * Random.Range(0.55f, 1.25f),
                    i % 4 == 0 ? Palette.Rosewater : color);
            }
        }

        public static void SpawnParticle(Vector3 position, Vector2 velocity, float gravity, float life, float size,
            Color color)
        {
            var go = new GameObject("fx");
            go.transform.position = position;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = Square;
            renderer.color = color;
            renderer.sortingOrder = 30;
            renderer.sharedMaterial = SpriteMaterial;
            go.transform.localScale = Vector3.one * size;
            go.AddComponent<FxParticle>().Init(velocity, gravity, life, size, color);
        }
    }

    public class FxParticle : MonoBehaviour
    {
        Vector2 velocity;
        float gravity;
        float life;
        float baseSize;
        float age;
        Color color;
        SpriteRenderer renderer;

        public void Init(Vector2 initialVelocity, float gravityForce, float lifetime, float size, Color tint)
        {
            velocity = initialVelocity;
            gravity = gravityForce;
            life = lifetime;
            baseSize = size;
            color = tint;
            renderer = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            float dt = Time.deltaTime;
            age += dt;
            if (age >= life)
            {
                Destroy(gameObject);
                return;
            }

            velocity.y -= gravity * dt;
            transform.position += (Vector3)(velocity * dt);
            float t = age / life;
            transform.localScale = Vector3.one * baseSize * (1f - t * 0.65f);
            Color c = color;
            c.a = 1f - t;
            renderer.color = c;
        }
    }
}
