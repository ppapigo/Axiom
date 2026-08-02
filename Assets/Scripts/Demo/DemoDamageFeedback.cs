using System.Collections.Generic;
using Axiom.Combat;
using UnityEngine;

namespace Axiom.Demo
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterHealth))]
    public sealed class DemoDamageFeedback : MonoBehaviour
    {
        private sealed class RendererBinding
        {
            public RendererBinding(Material material, Color originalColor)
            {
                Material = material;
                OriginalColor = originalColor;
            }

            public Material Material { get; }
            public Color OriginalColor { get; }
        }

        [SerializeField] private CharacterHealth health;
        [SerializeField] private UnityEngine.Camera worldCamera;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Vector3 numberWorldOffset = new Vector3(0f, 1.75f, 0f);
        [SerializeField, Min(0.01f)] private float flashDuration = 0.12f;
        [SerializeField, Min(0.05f)] private float numberDuration = 0.72f;
        [SerializeField, Min(0f)] private float numberRisePixels = 34f;
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private Color numberColor = new Color(1f, 0.78f, 0.16f, 1f);

        private readonly DamageFeedbackState _state = new DamageFeedbackState();
        private readonly List<RendererBinding> _rendererBindings =
            new List<RendererBinding>();
        private GUIStyle _numberStyle;
        private float _flashStartedAt = float.NegativeInfinity;
        private bool _subscribed;

        public bool IsConfigured => health != null && worldCamera != null && visualRoot != null;
        public bool IsShowingDamage => _state.IsVisible(Time.time);
        public float DisplayedDamage => _state.DamageAmount;

        private void Awake()
        {
            health ??= GetComponent<CharacterHealth>();
            worldCamera ??= UnityEngine.Camera.main;
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            _state.Clear();
            RestoreVisuals();
        }

        private void OnDestroy()
        {
            Unsubscribe();
            RestoreVisuals();
        }

        public void Configure(
            CharacterHealth targetHealth,
            UnityEngine.Camera camera,
            Transform targetVisualRoot)
        {
            Unsubscribe();
            RestoreVisuals();
            health = targetHealth;
            worldCamera = camera;
            visualRoot = targetVisualRoot;
            CacheRenderers();
            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        private void Update()
        {
            if (_rendererBindings.Count == 0)
            {
                return;
            }

            float elapsed = Time.time - _flashStartedAt;
            if (elapsed < 0f || elapsed >= flashDuration)
            {
                RestoreVisuals();
                return;
            }

            float progress = Mathf.Clamp01(elapsed / flashDuration);
            foreach (RendererBinding binding in _rendererBindings)
            {
                if (binding.Material != null)
                {
                    binding.Material.color = Color.Lerp(
                        flashColor,
                        binding.OriginalColor,
                        progress);
                }
            }
        }

        private void OnGUI()
        {
            if (worldCamera == null || !_state.IsVisible(Time.time))
            {
                return;
            }

            Vector3 screenPoint = worldCamera.WorldToScreenPoint(
                transform.position + numberWorldOffset);
            if (screenPoint.z <= 0f)
            {
                return;
            }

            _numberStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };

            float age = _state.GetNormalizedAge(Time.time);
            float alpha = 1f - (age * age);
            float top = Screen.height - screenPoint.y - (numberRisePixels * age);
            var rect = new Rect(screenPoint.x - 48f, top - 14f, 96f, 28f);
            string damageText = $"-{Mathf.CeilToInt(_state.DamageAmount)}";
            Color previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, alpha * 0.9f);
            GUI.Label(new Rect(rect.x + 2f, rect.y + 2f, rect.width, rect.height),
                damageText, _numberStyle);
            GUI.color = new Color(numberColor.r, numberColor.g, numberColor.b,
                numberColor.a * alpha);
            GUI.Label(rect, damageText, _numberStyle);
            GUI.color = previousColor;
        }

        private void HandleDamageTaken(float appliedDamage)
        {
            if (!_state.Register(appliedDamage, Time.time, numberDuration))
            {
                return;
            }

            _flashStartedAt = Time.time;
            foreach (RendererBinding binding in _rendererBindings)
            {
                if (binding.Material != null)
                {
                    binding.Material.color = flashColor;
                }
            }
        }

        private void CacheRenderers()
        {
            _rendererBindings.Clear();
            if (visualRoot == null)
            {
                return;
            }

            foreach (Renderer targetRenderer in
                     visualRoot.GetComponentsInChildren<Renderer>(true))
            {
                Material material = targetRenderer.material;
                if (material != null && material.HasProperty("_Color"))
                {
                    _rendererBindings.Add(
                        new RendererBinding(material, material.color));
                }
            }
        }

        private void Subscribe()
        {
            if (_subscribed || health == null)
            {
                return;
            }

            health.DamageTaken += HandleDamageTaken;
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed)
            {
                return;
            }

            if (health != null)
            {
                health.DamageTaken -= HandleDamageTaken;
            }
            _subscribed = false;
        }

        private void RestoreVisuals()
        {
            foreach (RendererBinding binding in _rendererBindings)
            {
                if (binding.Material != null)
                {
                    binding.Material.color = binding.OriginalColor;
                }
            }
        }

        private void OnValidate()
        {
            flashDuration = Mathf.Max(0.01f, flashDuration);
            numberDuration = Mathf.Max(0.05f, numberDuration);
            numberRisePixels = Mathf.Max(0f, numberRisePixels);
        }
    }
}
