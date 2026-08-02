using System.Collections.Generic;
using Axiom.Skill;
using UnityEngine;

namespace Axiom.Demo
{
    [DisallowMultipleComponent]
    public sealed class DemoSkillVfxPlayer : MonoBehaviour
    {
        private static readonly Dictionary<SkillElement, Material> Materials =
            new Dictionary<SkillElement, Material>();

        [SerializeField] private SkillVfxLibrary library;

        public SkillVfxLibrary Library => library;

        public void Configure(SkillVfxLibrary skillVfxLibrary)
        {
            library = skillVfxLibrary;
        }

        public void PlayCast(SkillElement element, Vector3 position, float radius)
        {
            SpawnOneShot(
                element,
                SkillVfxStage.Cast,
                position + (Vector3.up * 0.08f),
                Mathf.Max(0.5f, radius));
        }

        public void AttachProjectile(
            GameObject projectile,
            SkillElement element,
            float collisionRadius)
        {
            if (projectile == null)
            {
                return;
            }

            if (TrySpawnPrefab(
                    element,
                    SkillVfxStage.Projectile,
                    projectile.transform.position,
                    Mathf.Max(0.2f, collisionRadius * 2f),
                    out GameObject instance))
            {
                instance.transform.SetParent(projectile.transform, true);
                return;
            }

            ParticleSystem particles = CreateFallback(
                element,
                SkillVfxStage.Projectile,
                projectile.transform.position,
                Mathf.Max(0.2f, collisionRadius * 2f));
            particles.transform.SetParent(projectile.transform, true);
            particles.Play();
        }

        public void PlayImpact(SkillElement element, Vector3 position, float radius)
        {
            SpawnOneShot(
                element,
                SkillVfxStage.Impact,
                position + (Vector3.up * 0.12f),
                Mathf.Max(0.5f, radius));
        }

        public void PlayHit(SkillElement element, Vector3 position)
        {
            SpawnOneShot(
                element,
                SkillVfxStage.Hit,
                position + (Vector3.up * 1f),
                1f);
        }

        public static Color GetElementColor(SkillElement element)
        {
            return element switch
            {
                SkillElement.Fire => new Color(1f, 0.22f, 0.035f, 1f),
                SkillElement.Ice => new Color(0.32f, 0.88f, 1f, 1f),
                SkillElement.Lightning => new Color(0.78f, 0.45f, 1f, 1f),
                SkillElement.Poison => new Color(0.42f, 1f, 0.12f, 1f),
                SkillElement.Water => new Color(0.08f, 0.48f, 1f, 1f),
                SkillElement.Wind => new Color(0.48f, 1f, 0.72f, 1f),
                SkillElement.Earth => new Color(0.76f, 0.43f, 0.12f, 1f),
                _ => Color.white
            };
        }

        private void SpawnOneShot(
            SkillElement element,
            SkillVfxStage stage,
            Vector3 position,
            float size)
        {
            if (TrySpawnPrefab(element, stage, position, size, out GameObject instance))
            {
                Destroy(instance, GetPrefabLifetime(instance));
                return;
            }

            CreateFallback(element, stage, position, size).Play();
        }

        private bool TrySpawnPrefab(
            SkillElement element,
            SkillVfxStage stage,
            Vector3 position,
            float size,
            out GameObject instance)
        {
            if (library == null ||
                !library.TryGetPrefab(element, stage, out GameObject prefab))
            {
                instance = null;
                return false;
            }

            instance = Instantiate(prefab, position, Quaternion.identity);
            instance.name = $"{element} {stage} VFX";
            instance.transform.localScale *= size;
            foreach (ParticleSystem particles in
                     instance.GetComponentsInChildren<ParticleSystem>(true))
            {
                particles.Play(true);
            }
            return true;
        }

        private static ParticleSystem CreateFallback(
            SkillElement element,
            SkillVfxStage stage,
            Vector3 position,
            float size)
        {
            var effectObject = new GameObject($"{element} {stage} Particles");
            effectObject.transform.position = position;
            if (stage != SkillVfxStage.Projectile)
            {
                effectObject.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            }
            ParticleSystem particles = effectObject.AddComponent<ParticleSystem>();
            ConfigureMain(particles, element, stage, size);
            ConfigureEmission(particles, element, stage);
            ConfigureShape(particles, stage, size);
            ConfigureRenderer(particles, element, stage);
            return particles;
        }

        private static void ConfigureMain(
            ParticleSystem particles,
            SkillElement element,
            SkillVfxStage stage,
            float size)
        {
            ParticleSystem.MainModule main = particles.main;
            bool projectile = stage == SkillVfxStage.Projectile;
            main.loop = projectile;
            main.playOnAwake = false;
            main.duration = projectile ? 1f : 0.45f;
            main.startLifetime = projectile
                ? new ParticleSystem.MinMaxCurve(0.18f, 0.32f)
                : new ParticleSystem.MinMaxCurve(0.28f, 0.58f);
            main.startSpeed = GetSpeed(element, stage) * Mathf.Sqrt(size);
            main.startSize = GetParticleSize(element, stage) * Mathf.Sqrt(size);
            main.startColor = new ParticleSystem.MinMaxGradient(GetElementColor(element));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = projectile ? 48 : 72;
            main.stopAction = projectile
                ? ParticleSystemStopAction.None
                : ParticleSystemStopAction.Destroy;
            if (element == SkillElement.Earth && !projectile)
            {
                main.gravityModifier = 0.65f;
            }
        }

        private static void ConfigureEmission(
            ParticleSystem particles,
            SkillElement element,
            SkillVfxStage stage)
        {
            ParticleSystem.EmissionModule emission = particles.emission;
            if (stage == SkillVfxStage.Projectile)
            {
                emission.rateOverTime = element == SkillElement.Lightning ? 30f : 20f;
                return;
            }

            emission.rateOverTime = 0f;
            short count = (short)(stage switch
            {
                SkillVfxStage.Cast => 20,
                SkillVfxStage.Impact => 32,
                _ => 12
            });
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });
        }

        private static void ConfigureShape(
            ParticleSystem particles,
            SkillVfxStage stage,
            float size)
        {
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.radius = Mathf.Max(0.04f, size * 0.32f);
            switch (stage)
            {
                case SkillVfxStage.Cast:
                    shape.shapeType = ParticleSystemShapeType.Circle;
                    shape.radius = Mathf.Max(0.2f, size * 0.72f);
                    shape.radiusThickness = 0.05f;
                    break;
                case SkillVfxStage.Projectile:
                    shape.shapeType = ParticleSystemShapeType.Sphere;
                    shape.radius = Mathf.Max(0.025f, size * 0.12f);
                    break;
                case SkillVfxStage.Impact:
                    shape.shapeType = ParticleSystemShapeType.Hemisphere;
                    break;
                default:
                    shape.shapeType = ParticleSystemShapeType.Cone;
                    shape.angle = 22f;
                    break;
            }
        }

        private static void ConfigureRenderer(
            ParticleSystem particles,
            SkillElement element,
            SkillVfxStage stage)
        {
            ParticleSystemRenderer particleRenderer =
                particles.GetComponent<ParticleSystemRenderer>();
            bool stretched = element == SkillElement.Ice ||
                             element == SkillElement.Lightning ||
                             element == SkillElement.Wind;
            particleRenderer.renderMode = stretched
                ? ParticleSystemRenderMode.Stretch
                : ParticleSystemRenderMode.Billboard;
            particleRenderer.lengthScale = stage == SkillVfxStage.Projectile ? 2.2f : 1.1f;
            particleRenderer.velocityScale = stretched ? 0.2f : 0f;
            particleRenderer.material = GetMaterial(element);
        }

        private static Material GetMaterial(SkillElement element)
        {
            if (Materials.TryGetValue(element, out Material material) && material != null)
            {
                return material;
            }

            Shader shader = Shader.Find("Axiom/ParticleUnlit");
            if (shader == null)
            {
                throw new MissingReferenceException(
                    "The Axiom/ParticleUnlit shader must be included in the player build.");
            }

            material = new Material(shader)
            {
                name = $"Runtime {element} Particle Material",
                color = Color.white,
                hideFlags = HideFlags.HideAndDontSave
            };
            Materials[element] = material;
            return material;
        }

        private static float GetSpeed(SkillElement element, SkillVfxStage stage)
        {
            float baseSpeed = stage == SkillVfxStage.Projectile ? 0.18f : 2.3f;
            return element switch
            {
                SkillElement.Fire => baseSpeed * 1.25f,
                SkillElement.Lightning => baseSpeed * 1.8f,
                SkillElement.Poison => baseSpeed * 0.65f,
                SkillElement.Water => baseSpeed * 0.8f,
                SkillElement.Wind => baseSpeed * 1.45f,
                SkillElement.Earth => baseSpeed * 0.7f,
                _ => baseSpeed
            };
        }

        private static float GetParticleSize(SkillElement element, SkillVfxStage stage)
        {
            float baseSize = stage == SkillVfxStage.Projectile ? 0.16f : 0.22f;
            return element switch
            {
                SkillElement.Lightning => baseSize * 0.55f,
                SkillElement.Poison => baseSize * 1.25f,
                SkillElement.Water => baseSize * 1.15f,
                SkillElement.Earth => baseSize * 1.4f,
                _ => baseSize
            };
        }

        private static float GetPrefabLifetime(GameObject instance)
        {
            float lifetime = 0.8f;
            foreach (ParticleSystem particles in
                     instance.GetComponentsInChildren<ParticleSystem>(true))
            {
                ParticleSystem.MainModule main = particles.main;
                lifetime = Mathf.Max(
                    lifetime,
                    main.duration + main.startLifetime.constantMax + 0.25f);
            }
            return lifetime;
        }
    }
}
