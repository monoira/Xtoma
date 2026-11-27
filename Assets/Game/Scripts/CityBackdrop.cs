using UnityEngine;

namespace JetpackJoe
{
    public class CityBackdrop : MonoBehaviour
    {
        GameManager manager;

        class Row
        {
            public SpriteRenderer[] Tiles;
            public float Width;
            public float Factor;
        }

        Row stars;
        Row farCity;
        Row nearCity;
        Row ground;

        public void Init(GameManager owner)
        {
            manager = owner;

            var sky = GameArt.NewSprite(transform, "Sky", GameArt.SkyGradient(), Color.white, -100,
                new Vector3(0f, 0f, 0f), new Vector3(60f, 1.05f, 1f));
            sky.transform.localPosition = new Vector3(0f, 0f, 0f);

            stars = MakeRow("Stars", GameArt.StarTile(7), Color.white, -95, -5f, new Vector2(1f, 1.35f), 0.07f);
            MakeMoon();

            farCity = MakeRow("CityFar", GameArt.Skyline(false, 41), Color.white, -80, -4f, new Vector2(1f, 0.64f),
                0.18f);
            nearCity = MakeRow("CityNear", GameArt.Skyline(true, 93), Color.white, -70, -4f, new Vector2(1f, 0.76f),
                0.4f);
            ground = MakeRow("Ground", GameArt.GroundTile, Color.white, -60, -4f, Vector2.one, 1f);

            var colliderGo = new GameObject("GroundCollider");
            colliderGo.transform.SetParent(transform, false);
            var collider = colliderGo.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(80f, 1f);
            collider.offset = new Vector2(0f, -4.5f);
        }

        void MakeMoon()
        {
            var halo = GameArt.NewSprite(transform, "MoonHalo", GameArt.Glow, Palette.A(Palette.Rosewater, 0.16f), -91,
                new Vector3(5.1f, 3.1f, 0f), new Vector3(8f, 8f, 1f));
            halo.transform.rotation = Quaternion.identity;
            GameArt.NewSprite(transform, "Moon", GameArt.Moon, Color.white, -90, new Vector3(5.1f, 3.1f, 0f),
                new Vector3(1.35f, 1.35f, 1f));
        }

        Row MakeRow(string name, Sprite sprite, Color color, int order, float y, Vector2 scale, float factor)
        {
            var tiles = new SpriteRenderer[3];
            float width = sprite.bounds.size.x * scale.x;
            for (int i = 0; i < 3; i++)
            {
                tiles[i] = GameArt.NewSprite(transform, name + i, sprite, color, order,
                    new Vector3((i - 1) * width, y, 0f), new Vector3(scale.x, scale.y, 1f));
            }

            return new Row { Tiles = tiles, Width = width, Factor = factor };
        }

        void Update()
        {
            if (manager == null) return;
            float speed = 0f;
            if (manager.State == GameState.Playing) speed = manager.CurrentSpeed;
            else if (manager.State == GameState.Title) speed = 0.75f;

            float dt = Time.deltaTime;
            Scroll(stars, speed * stars.Factor * dt);
            Scroll(farCity, speed * farCity.Factor * dt);
            Scroll(nearCity, speed * nearCity.Factor * dt);
            Scroll(ground, speed * dt);
        }

        void Scroll(Row row, float distance)
        {
            if (row == null || distance <= 0f) return;
            foreach (var tile in row.Tiles)
            {
                var position = tile.transform.localPosition;
                position.x -= distance;
                if (position.x < -row.Width * 1.5f) position.x += row.Width * 3f;
                tile.transform.localPosition = position;
            }
        }
    }
}
