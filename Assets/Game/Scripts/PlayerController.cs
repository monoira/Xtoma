using UnityEngine;
using UnityEngine.InputSystem;

namespace JetpackJoe
{
    public class PlayerController : MonoBehaviour
    {
        public const float GroundY = -4f;
        const float StartX = -5.2f;
        const float InvulnerableSeconds = 1.6f;

        GameManager manager;
        Rigidbody2D body;
        CapsuleCollider2D capsule;
        Transform visual;
        Transform legLeft;
        Transform legRight;
        Transform arm;
        SpriteRenderer flameOuter;
        SpriteRenderer flameInner;
        SpriteRenderer shadow;
        SpriteRenderer[] bodyParts;
        Color[] bodyColors;

        int jumpsUsed;
        float lastGroundedAt;
        float invulnerableUntil;
        float flashUntil;
        float runPhase;
        float emberTimer;
        float previousVerticalSpeed;
        bool wasGrounded;

        public bool IsInvulnerable => Time.time < invulnerableUntil;

        public void Init(GameManager owner)
        {
            manager = owner;
            BuildPhysics();
            BuildVisuals();
            ResetForRun(true);
        }

        void BuildPhysics()
        {
            body = gameObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 3.2f;
            body.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            capsule = gameObject.AddComponent<CapsuleCollider2D>();
            capsule.size = new Vector2(0.5f, 1.24f);
            capsule.offset = new Vector2(0f, 0.66f);
            capsule.direction = CapsuleDirection2D.Vertical;
        }

        void BuildVisuals()
        {
            visual = new GameObject("Visual").transform;
            visual.SetParent(transform, false);
            visual.localScale = Vector3.one * 1.05f;
            GameArt.NewSprite(visual, "PlayerGlow", GameArt.Glow, Palette.A(Palette.Blue, 0.09f), 12,
                new Vector3(0f, 0.8f, 0f), new Vector3(2.4f, 2.8f, 1f));

            GameArt.NewSprite(visual, "JetpackOutline",
                GameArt.RoundedRect(36, 60, 13, Palette.Crust), Palette.Crust, 13,
                new Vector3(-0.27f, 0.83f, 0f));
            var jetpack = GameArt.NewSprite(visual, "Jetpack",
                GameArt.RoundedRect(30, 54, 11, Palette.Surface2), Palette.Surface2, 14,
                new Vector3(-0.27f, 0.83f, 0f));
            GameArt.NewSprite(jetpack.transform, "NozzleOutline",
                GameArt.RoundedRect(24, 14, 5, Palette.Crust), Palette.Crust, 13,
                new Vector3(0f, -0.3f, 0f));
            GameArt.NewSprite(jetpack.transform, "Nozzle",
                GameArt.RoundedRect(22, 12, 4, Palette.Overlay0), Palette.Overlay0, 14,
                new Vector3(0f, -0.3f, 0f));
            GameArt.NewSprite(jetpack.transform, "Antenna",
                GameArt.RoundedRect(3, 16, 1, Palette.Yellow), Palette.Yellow, 14,
                new Vector3(-0.05f, 0.4f, 0f));
            GameArt.NewSprite(jetpack.transform, "AntennaTip",
                GameArt.Circle(8, Palette.Yellow), Palette.Yellow, 15,
                new Vector3(-0.05f, 0.49f, 0f));

            flameOuter = GameArt.NewSprite(visual, "FlameOuter",
                GameArt.RoundedRect(20, 46, 9, Palette.Peach, 100f, new Vector2(0.5f, 1f)),
                Palette.A(Palette.Peach, 0.85f), 12, new Vector3(-0.27f, 0.55f, 0f));
            flameInner = GameArt.NewSprite(visual, "FlameInner",
                GameArt.RoundedRect(11, 28, 5, Palette.Yellow, 100f, new Vector2(0.5f, 1f)),
                Palette.A(Palette.Rosewater, 0.95f), 13, new Vector3(-0.27f, 0.53f, 0f));

            legLeft = MakeLeg("LegLeft", -0.1f);
            legRight = MakeLeg("LegRight", 0.1f);

            GameArt.NewSprite(visual, "BodyOutline",
                GameArt.RoundedRect(52, 62, 18, Palette.Crust), Palette.Crust, 14,
                new Vector3(0f, 0.8f, 0f));
            GameArt.NewSprite(visual, "Body",
                GameArt.RoundedRect(46, 56, 15, Palette.Lavender), Palette.Lavender, 15,
                new Vector3(0f, 0.8f, 0f));
            GameArt.NewSprite(visual, "Chest",
                GameArt.RoundedRect(28, 22, 8, Palette.Blue), Palette.Blue, 16,
                new Vector3(0.04f, 0.88f, 0f));
            GameArt.NewSprite(visual, "Belt",
                GameArt.RoundedRect(40, 7, 3, Palette.Yellow), Palette.Yellow, 17,
                new Vector3(0f, 0.56f, 0f));

            arm = new GameObject("Arm").transform;
            arm.SetParent(visual, false);
            arm.localPosition = new Vector3(0.2f, 1.02f, 0f);
            arm.localRotation = Quaternion.Euler(0f, 0f, -24f);
            GameArt.NewSprite(arm, "ArmOutline",
                GameArt.RoundedRect(18, 36, 8, Palette.Crust), Palette.Crust, 15,
                new Vector3(0f, -0.14f, 0f));
            GameArt.NewSprite(arm, "ArmLine",
                GameArt.RoundedRect(14, 32, 6, Palette.Subtext1), Palette.Subtext1, 16,
                new Vector3(0f, -0.14f, 0f));
            GameArt.NewSprite(arm, "GloveOutline",
                GameArt.Circle(19, Palette.Crust), Palette.Crust, 16,
                new Vector3(0f, -0.32f, 0f));
            GameArt.NewSprite(arm, "Glove",
                GameArt.Circle(15, Palette.Rosewater), Palette.Rosewater, 17,
                new Vector3(0f, -0.32f, 0f));

            GameArt.NewSprite(visual, "HeadOutline",
                GameArt.Circle(56, Palette.Crust), Palette.Crust, 16,
                new Vector3(0.02f, 1.26f, 0f));
            GameArt.NewSprite(visual, "Head",
                GameArt.Circle(50, Palette.Text), Palette.Text, 17,
                new Vector3(0.02f, 1.26f, 0f));
            GameArt.NewSprite(visual, "Visor",
                GameArt.RoundedRect(28, 15, 7, Palette.Sky), Palette.Sky, 18,
                new Vector3(0.13f, 1.27f, 0f));

            shadow = GameArt.NewSprite(null, "PlayerShadow", GameArt.Circle(64, Color.white),
                Palette.A(Palette.Crust, 0.55f), 9, new Vector3(StartX, GroundY + 0.03f, 0f),
                new Vector3(0.72f, 0.16f, 1f));

            var allParts = visual.GetComponentsInChildren<SpriteRenderer>();
            var parts = new System.Collections.Generic.List<SpriteRenderer>();
            foreach (var part in allParts)
                if (part != flameOuter && part != flameInner)
                    parts.Add(part);
            bodyParts = parts.ToArray();
            bodyColors = new Color[bodyParts.Length];
            for (int i = 0; i < bodyParts.Length; i++) bodyColors[i] = bodyParts[i].color;
        }

        Transform MakeLeg(string name, float x)
        {
            var root = new GameObject(name).transform;
            root.SetParent(visual, false);
            root.localPosition = new Vector3(x, 0.52f, 0f);
            GameArt.NewSprite(root, "LegOutline",
                GameArt.RoundedRect(19, 42, 7, Palette.Crust), Palette.Crust, 13,
                new Vector3(0f, -0.17f, 0f));
            GameArt.NewSprite(root, "Leg",
                GameArt.RoundedRect(15, 38, 6, Palette.Overlay2), Palette.Overlay2, 14,
                new Vector3(0f, -0.17f, 0f));
            GameArt.NewSprite(root, "BootOutline",
                GameArt.RoundedRect(21, 13, 5, Palette.Crust), Palette.Crust, 14,
                new Vector3(0.02f, -0.37f, 0f));
            GameArt.NewSprite(root, "Boot",
                GameArt.RoundedRect(19, 11, 4, Palette.Surface1), Palette.Surface1, 15,
                new Vector3(0.02f, -0.37f, 0f));
            return root;
        }

        public void ResetForRun(bool atStart)
        {
            body.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            body.rotation = 0f;
            body.angularVelocity = 0f;
            body.linearVelocity = Vector2.zero;
            Vector3 position = new Vector3(StartX, GroundY + 0.02f, 0f);
            if (atStart)
            {
                transform.position = position;
            }

            body.position = position;
            capsule.enabled = true;
            jumpsUsed = 0;
            invulnerableUntil = 0f;
            flashUntil = 0f;
            runPhase = 0f;
            emberTimer = 0f;
            previousVerticalSpeed = 0f;
            wasGrounded = true;
            visual.localRotation = Quaternion.identity;
            visual.localScale = Vector3.one * 1.05f;
            for (int i = 0; i < bodyParts.Length; i++)
            {
                bodyParts[i].enabled = true;
                bodyParts[i].color = bodyColors[i];
            }

            if (shadow != null) shadow.enabled = true;
        }

        void Update()
        {
            if (manager == null) return;
            float dt = Time.deltaTime;
            bool playing = manager.State == GameState.Playing;

            if (body.constraints != RigidbodyConstraints2D.None)
            {
                if (playing)
                {
                    HandleJumpInput();
                    Animate(dt);
                }
                else if (manager.State == GameState.Title)
                {
                    IdleAnimation(dt);
                }
            }

            UpdateFlicker();
            UpdateShadow();
            previousVerticalSpeed = body.linearVelocity.y;
        }

        bool JumpPressed()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;
            return keyboard.spaceKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame;
        }

        bool IsGrounded()
        {
            return body.position.y <= GroundY + 0.1f && body.linearVelocity.y <= 0.25f;
        }

        void HandleJumpInput()
        {
            bool grounded = IsGrounded();
            if (grounded)
            {
                jumpsUsed = 0;
                lastGroundedAt = Time.time;
                if (!wasGrounded && previousVerticalSpeed < -6f)
                {
                    GameArt.Burst(transform.position + Vector3.up * 0.08f, Palette.Overlay1, 8, 2.2f, 0.1f,
                        0.35f, 3f);
                    manager.Audio.PlayLand();
                }
            }

            wasGrounded = grounded;
            if (!JumpPressed()) return;

            bool canGroundJump = jumpsUsed == 0 && (grounded || Time.time - lastGroundedAt <= 0.1f);
            if (canGroundJump)
            {
                jumpsUsed = 1;
                Jump(13.2f);
                GameArt.Burst(transform.position + Vector3.up * 0.1f, Palette.Peach, 7, 3.2f, 0.11f, 0.4f, 5f);
                manager.Audio.PlayJump();
                return;
            }

            if (jumpsUsed < 2)
            {
                jumpsUsed = 2;
                Jump(12.5f);
                GameArt.Burst(transform.position + new Vector3(-0.25f, 0.4f, 0f), Palette.Peach, 14, 4.5f, 0.13f,
                    0.45f, 4f);
                manager.Audio.PlayDoubleJump();
            }
        }

        void Jump(float velocity)
        {
            body.linearVelocity = new Vector2(0f, velocity);
            lastGroundedAt = -10f;
            wasGrounded = false;
        }

        void Animate(float dt)
        {
            bool grounded = IsGrounded();
            if (grounded) runPhase += dt * 13f;
            float swing = grounded ? Mathf.Sin(runPhase) * 28f : 0f;
            legLeft.localRotation = Quaternion.Euler(0f, 0f, grounded ? swing : 26f);
            legRight.localRotation = Quaternion.Euler(0f, 0f, grounded ? -swing : -32f);
            arm.localRotation = Quaternion.Euler(0f, 0f, grounded ? -24f + swing * 0.4f : -50f);

            float tilt = grounded ? 0f : Mathf.Clamp(body.linearVelocity.y * 0.55f, -11f, 13f);
            visual.localRotation = Quaternion.Euler(0f, 0f, tilt);

            float bob = grounded ? Mathf.Abs(Mathf.Sin(runPhase)) * 0.018f : 0f;
            visual.localPosition = new Vector3(0f, bob, 0f);

            bool jetting = !grounded;
            flameOuter.enabled = jetting;
            flameInner.enabled = jetting;
            if (jetting)
            {
                float flicker = 0.78f + Mathf.Abs(Mathf.Sin(Time.time * 38f)) * 0.38f;
                flameOuter.transform.localScale = new Vector3(1f, flicker, 1f);
                flameInner.transform.localScale = new Vector3(0.9f, flicker * 0.9f, 1f);
                flameOuter.color = Palette.A(Palette.Peach, 0.7f + Mathf.Abs(Mathf.Sin(Time.time * 29f)) * 0.25f);
                emberTimer -= dt;
                if (emberTimer <= 0f)
                {
                    emberTimer = 0.055f;
                    Vector3 nozzle = transform.position + new Vector3(-0.27f, 0.45f, 0f);
                    GameArt.SpawnParticle(nozzle, new Vector2(Random.Range(-1.2f, -0.2f), Random.Range(-3.5f, -1.5f)),
                        0f, Random.Range(0.2f, 0.35f), Random.Range(0.08f, 0.14f),
                        Random.value < 0.4f ? Palette.Yellow : Palette.Peach);
                }
            }

            if (flashUntil > Time.time)
            {
                for (int i = 0; i < bodyParts.Length; i++)
                {
                    var c = Palette.Rosewater;
                    c.a = bodyColors[i].a;
                    bodyParts[i].color = c;
                }
            }
            else
            {
                for (int i = 0; i < bodyParts.Length; i++) bodyParts[i].color = bodyColors[i];
            }
        }

        void IdleAnimation(float dt)
        {
            runPhase += dt * 2.2f;
            legLeft.localRotation = Quaternion.Euler(0f, 0f, -4f);
            legRight.localRotation = Quaternion.Euler(0f, 0f, 4f);
            arm.localRotation = Quaternion.Euler(0f, 0f, -24f);
            visual.localPosition = new Vector3(0f, Mathf.Sin(runPhase) * 0.018f, 0f);
            visual.localRotation = Quaternion.identity;
            flameOuter.enabled = false;
            flameInner.enabled = false;
        }

        void UpdateFlicker()
        {
            if (Time.time >= invulnerableUntil)
            {
                invulnerableUntil = 0f;
                for (int i = 0; i < bodyParts.Length; i++) bodyParts[i].enabled = true;
                return;
            }

            bool visible = (Time.time * 14f) % 2f < 1f;
            for (int i = 0; i < bodyParts.Length; i++) bodyParts[i].enabled = visible;
        }

        void UpdateShadow()
        {
            if (shadow == null) return;
            if (manager.State == GameState.Dead)
            {
                shadow.enabled = false;
                return;
            }

            float height = Mathf.Max(0f, transform.position.y - GroundY);
            float t = Mathf.Clamp01(height / 4.5f);
            shadow.transform.position = new Vector3(transform.position.x, GroundY + 0.03f, 0f);
            shadow.transform.localScale = new Vector3(0.72f * (1f - t * 0.45f), 0.16f * (1f - t * 0.35f), 1f);
            shadow.color = Palette.A(Palette.Crust, 0.55f * (1f - t * 0.75f));
        }

        public void TakeHit()
        {
            invulnerableUntil = Time.time + InvulnerableSeconds;
            flashUntil = Time.time + 0.13f;
            body.linearVelocity = new Vector2(0f, 7.5f);
            jumpsUsed = Mathf.Max(jumpsUsed, 1);
            GameArt.Burst(transform.position + Vector3.up * 0.9f, Palette.Red, 18, 6f, 0.17f, 0.6f, 6f);
        }

        public void Die()
        {
            body.constraints = RigidbodyConstraints2D.None;
            body.linearVelocity = new Vector2(-2.2f, 6.5f);
            body.angularVelocity = -300f;
            flameOuter.enabled = false;
            flameInner.enabled = false;
            if (shadow != null) shadow.enabled = false;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (manager == null || manager.State != GameState.Playing) return;

            var hazard = other.GetComponent<Hazard>();
            if (hazard != null)
            {
                if (manager.TryDamage()) hazard.Break();
                return;
            }

            var heart = other.GetComponent<HeartPickup>();
            if (heart != null) heart.Collect();
        }
    }
}
