using UnityEngine;

namespace CindarsHope.Combat.Weapon
{
    /// <summary>
    /// Builds fully functional projectile GameObjects at runtime when no authored prefab is
    /// available (pre-art phase). Generates cached procedural sprites (circle + shaft) so the
    /// factory works in player builds, adds trail + procedural animation, and wires the same
    /// ProjectileBehaviour used by authored prefabs — gameplay path is identical either way.
    /// </summary>
    public static class RuntimeProjectileFactory
    {
        private static Sprite s_circleSprite;
        private static Sprite s_shaftSprite;
        private static Material s_trailMaterial;

        public static GameObject Create(ProjectileVisualStyle style, DamageType damageType)
        {
            var resolvedStyle = ResolveStyle(style, damageType);
            var projectile = new GameObject($"Projectile_{resolvedStyle}_{damageType}");

            var rigidbody = projectile.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0f;
            rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = projectile.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.12f;

            var renderer = projectile.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 5;

            bool isArrow = resolvedStyle == ProjectileVisualStyle.Arrow;
            Color tint = ResolveTint(resolvedStyle, damageType);
            if (isArrow)
            {
                renderer.sprite = GetShaftSprite();
                renderer.color = tint;
                projectile.transform.localScale = new Vector3(0.55f, 0.12f, 1f);
            }
            else
            {
                renderer.sprite = GetCircleSprite();
                renderer.color = tint;
                projectile.transform.localScale = Vector3.one * 0.32f;
            }

            AttachTrail(projectile, tint, isArrow);

            var animator = projectile.AddComponent<ProjectileVisualAnimator>();
            animator.Configure(renderer, pulse: !isArrow, spin: false);

            projectile.AddComponent<ProjectileBehaviour>();
            return projectile;
        }

        /// <summary>
        /// Garante que um projetil tenha um SpriteRenderer com sprite VISIVEL. Prefabs autorados de
        /// projetil (ex.: Projectile_Arrow) podem referenciar um sprite built-in que nao resolve em
        /// runtime (m_WasSpriteAssigned: 0) — a flecha voa invisivel. Neste caso aplica o sprite
        /// procedural (shaft p/ Arrow, circulo p/ magia), preservando o gameplay e a tint do prefab.
        /// No-op se ja houver um sprite valido.
        /// </summary>
        public static void EnsureVisibleSprite(GameObject projectile, ProjectileVisualStyle style, DamageType damageType)
        {
            if (projectile == null) return;

            var renderer = projectile.GetComponentInChildren<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = projectile.AddComponent<SpriteRenderer>();
                renderer.sortingOrder = 5;
            }

            // Sprite nulo OU built-in placeholder (ex.: Projectile_Arrow referencia um sprite built-in
            // que nao renderiza em runtime) => substitui pelo procedural. Sprite real importado fica.
            bool usable = renderer.sprite != null && !IsBuiltinPlaceholder(renderer.sprite);
            if (usable) return;

            var resolvedStyle = ResolveStyle(style, damageType);
            bool isArrow = resolvedStyle == ProjectileVisualStyle.Arrow;
            renderer.sprite = isArrow ? GetShaftSprite() : GetCircleSprite();

            // Material valido (Sprites-Default) — built-in placeholder pode nao ter material renderavel.
            if (renderer.sharedMaterial == null)
            {
                renderer.sharedMaterial = GetTrailMaterial();
            }

            // Tint legivel (preserva a do prefab se ja for opaca).
            if (renderer.color.a <= 0f)
            {
                renderer.color = ResolveTint(resolvedStyle, damageType);
            }

            // Escala que faz a flecha LER como um shaft (o prefab placeholder vem em 0.2x0.2 = ponto).
            if (isArrow)
            {
                projectile.transform.localScale = new Vector3(0.55f, 0.12f, 1f);
            }
            else if (projectile.transform.localScale == Vector3.zero)
            {
                projectile.transform.localScale = Vector3.one * 0.32f;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!s_repairLogged)
            {
                s_repairLogged = true;
                Debug.Log($"CombatLog: ProjectileSpriteRepaired. Style={resolvedStyle}, DamageType={damageType}, " +
                          $"AppliedProceduralSprite=True (prefab sprite era nulo/placeholder).");
            }
#endif
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static bool s_repairLogged;
#endif

        // Built-in/placeholder = sem textura, ou sprite/textura built-in da Unity (UnityWhite, UISprite,
        // Background, Knob...). Esses nao renderam como projetil legivel em world-space; tratamos como
        // ausentes para forcar o visual procedural. (Projectile_Arrow vem com 'UISprite' placeholder.)
        private static bool IsBuiltinPlaceholder(Sprite sprite)
        {
            var tex = sprite.texture;
            if (tex == null)
            {
                return true;
            }

            string spriteName = sprite.name ?? string.Empty;
            string texName = tex.name ?? string.Empty;

            if (string.IsNullOrEmpty(texName) || texName.StartsWith("Unity") || spriteName.StartsWith("Unity"))
            {
                return true;
            }

            switch (spriteName)
            {
                case "UISprite":
                case "Background":
                case "UIMask":
                case "InputFieldBackground":
                case "Knob":
                case "Checkmark":
                case "DropdownArrow":
                    return true;
                default:
                    return false;
            }
        }

        private static ProjectileVisualStyle ResolveStyle(ProjectileVisualStyle style, DamageType damageType)
        {
            if (style != ProjectileVisualStyle.Auto)
            {
                return style;
            }

            return damageType == DamageType.Physical
                ? ProjectileVisualStyle.Arrow
                : ProjectileVisualStyle.MagicBolt;
        }

        private static Color ResolveTint(ProjectileVisualStyle style, DamageType damageType)
        {
            if (style == ProjectileVisualStyle.Arrow)
            {
                return new Color(0.78f, 0.62f, 0.38f);
            }

            switch (damageType)
            {
                case DamageType.Fire: return new Color(1f, 0.45f, 0.15f);
                case DamageType.Ice: return new Color(0.45f, 0.8f, 1f);
                case DamageType.Toxic: return new Color(0.45f, 0.85f, 0.3f);
                case DamageType.Lightning: return new Color(1f, 0.95f, 0.4f);
                case DamageType.Arcane: return new Color(0.72f, 0.45f, 0.95f);
                case DamageType.True: return new Color(0.95f, 0.95f, 0.95f);
                default: return new Color(0.85f, 0.8f, 0.7f);
            }
        }

        private static void AttachTrail(GameObject projectile, Color tint, bool isArrow)
        {
            var trail = projectile.AddComponent<TrailRenderer>();
            trail.time = isArrow ? 0.08f : 0.22f;
            trail.startWidth = isArrow ? 0.05f : 0.18f;
            trail.endWidth = 0f;
            trail.material = GetTrailMaterial();
            trail.startColor = new Color(tint.r, tint.g, tint.b, 0.65f);
            trail.endColor = new Color(tint.r, tint.g, tint.b, 0f);
            trail.sortingOrder = 4;
        }

        private static Material GetTrailMaterial()
        {
            if (s_trailMaterial == null)
            {
                var shader = Shader.Find("Sprites/Default");
                s_trailMaterial = shader != null ? new Material(shader) : null;
            }

            return s_trailMaterial;
        }

        private static Sprite GetCircleSprite()
        {
            if (s_circleSprite == null)
            {
                s_circleSprite = BuildCircleSprite(32);
            }

            return s_circleSprite;
        }

        private static Sprite GetShaftSprite()
        {
            if (s_shaftSprite == null)
            {
                s_shaftSprite = BuildSolidSprite(32, 32);
            }

            return s_shaftSprite;
        }

        private static Sprite BuildCircleSprite(int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.name = "ProceduralProjectileCircle";
            float center = (size - 1) * 0.5f;
            float radius = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    // Soft edge over the outer two pixels so the orb does not look like a hard square dot.
                    float alpha = Mathf.Clamp01((radius - distance) / 2f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static Sprite BuildSolidSprite(int width, int height)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.name = "ProceduralProjectileShaft";
            var pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), width);
        }
    }
}
