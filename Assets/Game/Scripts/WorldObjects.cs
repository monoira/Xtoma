using UnityEngine;

namespace JetpackJoe
{
    public class Hazard : MonoBehaviour
    {
        public GameManager Manager;
        bool broken;

        void Update()
        {
            if (Manager == null || Manager.State != GameState.Playing) return;
            transform.position += Vector3.left * (Manager.CurrentSpeed * Time.deltaTime);
            if (transform.position.x < -13f) Destroy(gameObject);
        }

        public void Break()
        {
            if (broken) return;
            broken = true;
            GameArt.Burst(transform.position + Vector3.up * 0.6f, Palette.Red, 16, 5.5f, 0.16f, 0.5f, 5f);
            Destroy(gameObject);
        }
    }

    public class HeartPickup : MonoBehaviour
    {
        public GameManager Manager;
        float baseY;
        float phase;
        bool collected;
        SpriteRenderer glow;

        public void Init(GameManager owner, float height)
        {
            Manager = owner;
            baseY = height;
            phase = Random.Range(0f, Mathf.PI * 2f);
            glow = transform.Find("Glow")?.GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            if (Manager == null || Manager.State != GameState.Playing) return;
            transform.position += Vector3.left * (Manager.CurrentSpeed * Time.deltaTime);
            float bob = Mathf.Sin(Time.time * 3.2f + phase) * 0.12f;
            transform.position = new Vector3(transform.position.x, baseY + bob, 0f);
            if (glow != null)
            {
                float pulse = 1.45f + Mathf.Sin(Time.time * 4f + phase) * 0.18f;
                glow.transform.localScale = new Vector3(pulse, pulse, 1f);
                glow.color = Palette.A(Palette.Pink, 0.22f + Mathf.Sin(Time.time * 4f + phase) * 0.07f);
            }

            transform.Rotate(0f, 0f, Mathf.Sin(Time.time * 2f + phase) * 8f * Time.deltaTime);
            if (transform.position.x < -13f) Destroy(gameObject);
        }

        public void Collect()
        {
            if (collected) return;
            collected = true;
            Manager.AddHeart();
            GameArt.Burst(transform.position, Palette.Pink, 16, 5f, 0.15f, 0.55f, 8f);
            Destroy(gameObject);
        }
    }
}
