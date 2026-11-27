using UnityEngine;

namespace JetpackJoe
{
    public class WorldSpawner : MonoBehaviour
    {
        const float SpawnX = 11.5f;

        GameManager manager;
        float spawnTravel;
        float nextGap = 20f;
        float heartTravel;

        public void Init(GameManager owner)
        {
            manager = owner;
        }

        public void ResetWorld()
        {
            for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);
            spawnTravel = 0f;
            heartTravel = 0f;
            nextGap = 20f;
        }

        void Update()
        {
            if (manager == null || manager.State != GameState.Playing) return;
            float travel = manager.CurrentSpeed * Time.deltaTime;
            spawnTravel += travel;
            heartTravel += travel;

            if (spawnTravel >= nextGap)
            {
                spawnTravel -= nextGap;
                SpawnWave();
                float difficulty = Mathf.Clamp01(manager.DistanceMeters / 1200f);
                nextGap = Mathf.Lerp(18f, 13f, difficulty) * Random.Range(0.9f, 1.1f);
            }
        }

        void SpawnWave()
        {
            float difficulty = Mathf.Clamp01(manager.DistanceMeters / 1200f);
            float roll = Random.value;
            float heartY = -1f;

            if (difficulty < 0.1f)
            {
                SpawnSpikes(SpawnX, 1f);
                heartY = 1.35f;
            }
            else if (difficulty < 0.28f)
            {
                if (roll < 0.55f)
                {
                    SpawnSpikes(SpawnX, 1f);
                    heartY = 1.35f;
                }
                else if (roll < 0.85f)
                {
                    SpawnSpikes(SpawnX, 1f);
                    SpawnSpikes(SpawnX + 2.7f, 0.85f);
                    heartY = 1.7f;
                }
                else
                {
                    SpawnPillar(SpawnX);
                    heartY = 2.0f;
                }
            }
            else if (difficulty < 0.5f)
            {
                if (roll < 0.26f)
                {
                    SpawnSpikes(SpawnX, 1f);
                    heartY = 1.35f;
                }
                else if (roll < 0.5f)
                {
                    SpawnSpikes(SpawnX, 1f);
                    SpawnSpikes(SpawnX + 2.8f, 0.9f);
                    heartY = 1.8f;
                }
                else if (roll < 0.74f)
                {
                    SpawnPillar(SpawnX);
                    heartY = 2.1f;
                }
                else
                {
                    SpawnLaser(SpawnX);
                    heartY = 0.1f;
                }
            }
            else if (difficulty < 0.76f)
            {
                if (roll < 0.18f)
                {
                    SpawnPillar(SpawnX);
                    heartY = 2.1f;
                }
                else if (roll < 0.4f)
                {
                    SpawnSpikes(SpawnX, 1f);
                    SpawnSpikes(SpawnX + 2.9f, 0.9f);
                    heartY = 1.9f;
                }
                else if (roll < 0.64f)
                {
                    SpawnLaser(SpawnX);
                    heartY = 0.1f;
                }
                else if (roll < 0.84f)
                {
                    SpawnOrb(SpawnX, -0.2f);
                    SpawnSpikes(SpawnX + 3.2f, 0.9f);
                    heartY = -1f;
                }
                else
                {
                    SpawnPillar(SpawnX);
                    SpawnSpikes(SpawnX + 4.4f, 0.85f);
                    heartY = 2.2f;
                }
            }
            else
            {
                if (roll < 0.16f)
                {
                    SpawnPillar(SpawnX);
                    SpawnSpikes(SpawnX + 4.3f, 0.85f);
                    heartY = 2.2f;
                }
                else if (roll < 0.36f)
                {
                    SpawnLaser(SpawnX);
                    heartY = 0.1f;
                }
                else if (roll < 0.6f)
                {
                    SpawnOrb(SpawnX, -0.1f);
                    SpawnSpikes(SpawnX + 3.4f, 0.9f);
                    heartY = -1f;
                }
                else if (roll < 0.82f)
                {
                    SpawnSpikes(SpawnX, 1f);
                    SpawnSpikes(SpawnX + 2.7f, 0.9f);
                    SpawnSpikes(SpawnX + 5.4f, 0.8f);
                    heartY = 2.0f;
                }
                else
                {
                    SpawnPillar(SpawnX);
                    heartY = 2.2f;
                }
            }

            float heartGap = manager.Hearts >= GameManager.MaxHearts ? 150f : manager.Hearts == 1 ? 58f : 90f;
            if (heartTravel >= heartGap && heartY > 0f && Random.value < 0.9f)
            {
                SpawnHeart(SpawnX + Random.Range(-0.3f, 0.3f), heartY);
                heartTravel = 0f;
            }
        }

        void SpawnSpikes(float x, float scale)
        {
            var hazard = MakeHazard("Spikes", x, PlayerController.GroundY);
            var glow = GameArt.NewSprite(hazard.transform, "Glow", GameArt.Glow, Palette.A(Palette.Red, 0.14f), 9,
                new Vector3(0f, 0.2f, 0f), new Vector3(1.6f * scale, 0.5f, 1f));
            GameArt.NewSprite(hazard.transform, "SpikeArt", GameArt.Spikes, Color.white, 10,
                Vector3.zero, new Vector3(0.96f * scale, 1f, 1f));
            var collider = hazard.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(1.65f * scale, 0.55f);
            collider.offset = new Vector2(0f, 0.3f);
        }

        void SpawnPillar(float x)
        {
            var hazard = MakeHazard("Pillar", x, PlayerController.GroundY);
            GameArt.NewSprite(hazard.transform, "PillarGlow", GameArt.Glow, Palette.A(Palette.Red, 0.12f), 9,
                new Vector3(0f, 1.15f, 0f), new Vector3(1.5f, 2.7f, 1f));
            GameArt.NewSprite(hazard.transform, "PillarArt", GameArt.Pillar, Color.white, 10,
                Vector3.zero, new Vector3(1f, 0.94f, 1f));
            var light = GameArt.NewSprite(hazard.transform, "PillarLight", GameArt.Glow, Palette.A(Palette.Red, 0.5f),
                9, new Vector3(0f, 2.35f, 0f), new Vector3(0.9f, 0.9f, 1f));
            GameArt.NewSprite(light.transform, "PillarDot", GameArt.Circle(16, Color.white), Palette.Red, 11,
                Vector3.zero, new Vector3(0.35f, 0.35f, 1f));
            var collider = hazard.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(0.52f, 2.15f);
            collider.offset = new Vector2(0f, 1.08f);
        }

        void SpawnLaser(float x)
        {
            float y = PlayerController.GroundY + 1.3f;
            var hazard = MakeHazard("Laser", x, y);
            GameArt.NewSprite(hazard.transform, "LaserGlow", GameArt.Glow, Palette.A(Palette.Red, 0.12f), 9,
                Vector3.zero, new Vector3(4.7f, 0.8f, 1f));
            GameArt.NewSprite(hazard.transform, "LaserArt", GameArt.Laser, Color.white, 10, Vector3.zero,
                new Vector3(0.88f, 1f, 1f));
            GameArt.NewSprite(hazard.transform, "LaserCore",
                GameArt.RoundedRect(512, 8, 4, Color.white), Palette.Rosewater, 12, Vector3.zero,
                new Vector3(0.88f, 1.5f, 1f));
            GameArt.NewSprite(hazard.transform, "EmitterL", GameArt.RoundedRect(18, 64, 6, Palette.Surface1),
                Palette.Surface1, 11, new Vector3(-2.27f, 0f, 0f), new Vector3(1f, 1f, 1f));
            GameArt.NewSprite(hazard.transform, "EmitterR", GameArt.RoundedRect(18, 64, 6, Palette.Surface1),
                Palette.Surface1, 11, new Vector3(2.27f, 0f, 0f), new Vector3(1f, 1f, 1f));
            GameArt.NewSprite(hazard.transform, "EmitterLightL", GameArt.Circle(16, Color.white), Palette.Yellow,
                12, new Vector3(-2.16f, 0f, 0f), new Vector3(0.3f, 0.3f, 1f));
            GameArt.NewSprite(hazard.transform, "EmitterLightR", GameArt.Circle(16, Color.white), Palette.Yellow,
                12, new Vector3(2.16f, 0f, 0f), new Vector3(0.3f, 0.3f, 1f));
            var collider = hazard.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(4.3f, 0.3f);
            collider.offset = Vector2.zero;
        }

        void SpawnOrb(float x, float y)
        {
            var hazard = MakeHazard("Orb", x, y);
            GameArt.NewSprite(hazard.transform, "OrbGlow", GameArt.Glow, Palette.A(Palette.Red, 0.28f), 9,
                Vector3.zero, new Vector3(2.2f, 2.2f, 1f));
            GameArt.NewSprite(hazard.transform, "OrbArt", GameArt.Orb, Color.white, 11, Vector3.zero,
                new Vector3(0.58f, 0.58f, 1f));
            var collider = hazard.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.32f;
        }

        HeartPickup SpawnHeart(float x, float y)
        {
            var go = new GameObject("Heart");
            go.transform.SetParent(transform, false);
            go.transform.position = new Vector3(x, y, 0f);
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            var glow = GameArt.NewSprite(go.transform, "Glow", GameArt.Glow, Palette.A(Palette.Pink, 0.24f), 11,
                Vector3.zero, new Vector3(1.7f, 1.7f, 1f));
            glow.name = "Glow";
            GameArt.NewSprite(go.transform, "HeartArt", GameArt.HeartFull, Color.white, 12, Vector3.zero,
                new Vector3(0.66f, 0.66f, 1f));
            var collider = go.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.36f;
            var pickup = go.AddComponent<HeartPickup>();
            pickup.Init(manager, y);
            return pickup;
        }

        GameObject MakeHazard(string name, float x, float y)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.position = new Vector3(x, y, 0f);
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            var hazard = go.AddComponent<Hazard>();
            hazard.Manager = manager;
            return go;
        }
    }
}
