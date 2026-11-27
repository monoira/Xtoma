using System.Collections.Generic;
using UnityEngine;

namespace JetpackJoe
{
    // one companion orb per heart, circling the player and leaving a short trail
    public class HeartOrbs : MonoBehaviour
    {
        const float OrbitRadiusX = 0.95f;
        const float OrbitRadiusY = 0.6f;
        const float OrbitCenterY = 0.95f;
        const float OrbitSpeed = 1.8f;
        const float TrailInterval = 0.045f;

        GameManager manager;
        readonly List<Orb> orbs = new List<Orb>();

        class Orb
        {
            public Transform Root;
            public SpriteRenderer Glow;
            public SpriteRenderer Art;
            public float Phase;
            public float TrailTimer;
            public float BornAt;
            public bool Gold;
        }

        public void Init(GameManager owner)
        {
            manager = owner;
        }

        void Update()
        {
            if (manager == null) return;

            bool gold = manager.Hearts >= GameManager.MaxHearts;
            int wanted = manager.State == GameState.Dead
                ? 0
                : Mathf.Clamp(manager.Hearts, 0, GameManager.MaxHearts);
            while (orbs.Count < wanted) AddOrb(orbs.Count);
            while (orbs.Count > wanted) RemoveOrb(orbs.Count - 1);

            float now = Time.time;
            for (int i = 0; i < orbs.Count; i++)
            {
                var orb = orbs[i];
                if (orb.Gold != gold)
                {
                    orb.Gold = gold;
                    orb.Art.sprite = gold ? GameArt.OrbGold : GameArt.Orb;
                    orb.Glow.color = Palette.A(ColorOf(gold), 0.3f);
                }

                float angle = now * OrbitSpeed + orb.Phase;
                orb.Root.localPosition = new Vector3(
                    Mathf.Cos(angle) * OrbitRadiusX,
                    OrbitCenterY + Mathf.Sin(angle) * OrbitRadiusY + Mathf.Sin(angle * 2.3f) * 0.06f,
                    0f);
                float grow = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((now - orb.BornAt) / 0.25f));
                orb.Root.localScale = new Vector3(grow, grow, 1f);

                if (manager.State != GameState.Playing) continue;
                orb.TrailTimer -= Time.deltaTime;
                if (orb.TrailTimer > 0f) continue;
                orb.TrailTimer = TrailInterval;
                GameArt.SpawnParticle(orb.Root.position, Random.insideUnitCircle * 0.12f, 0f, 0.32f, 0.1f,
                    ColorOf(gold));
            }
        }

        void AddOrb(int index)
        {
            var root = new GameObject("HeartOrb" + index).transform;
            root.SetParent(transform, false);
            var glow = GameArt.NewSprite(root, "Glow", GameArt.Glow, Palette.A(Palette.Red, 0.3f), 19,
                Vector3.zero, new Vector3(0.9f, 0.9f, 1f));
            var art = GameArt.NewSprite(root, "Art", GameArt.Orb, Color.white, 20, Vector3.zero,
                new Vector3(0.3f, 0.3f, 1f));
            orbs.Add(new Orb
            {
                Root = root,
                Glow = glow,
                Art = art,
                Phase = Mathf.PI * 2f * index / GameManager.MaxHearts + 0.6f,
                BornAt = Time.time,
                Gold = false
            });
        }

        void RemoveOrb(int index)
        {
            Destroy(orbs[index].Root.gameObject);
            orbs.RemoveAt(index);
        }

        static Color ColorOf(bool gold)
        {
            return gold ? Palette.Yellow : Palette.Red;
        }
    }
}
