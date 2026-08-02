using Axiom.Combat;
using UnityEngine;

namespace Axiom.Demo
{
    [DisallowMultipleComponent]
    public sealed class DemoCombatAudio : MonoBehaviour
    {
        private const int SampleRate = 22050;
        private static AudioClip _attackClip;
        private static AudioClip _hitClip;
        private static AudioClip _skillCastClip;
        private static AudioClip _skillImpactClip;

        private AudioSource _source;
        private CharacterHealth _health;
        private BasicAttackController _basicAttack;
        private float _lastHealth;
        private bool _subscribed;

        public bool IsReady => _source != null &&
                               _attackClip != null &&
                               _hitClip != null &&
                               _skillCastClip != null &&
                               _skillImpactClip != null;

        public static int GeneratedSampleCount =>
            (_attackClip == null ? 0 : _attackClip.samples) +
            (_hitClip == null ? 0 : _hitClip.samples) +
            (_skillCastClip == null ? 0 : _skillCastClip.samples) +
            (_skillImpactClip == null ? 0 : _skillImpactClip.samples);

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            if (_source == null)
            {
                _source = gameObject.AddComponent<AudioSource>();
            }

            _source.playOnAwake = false;
            _source.spatialBlend = 0.7f;
            _source.rolloffMode = AudioRolloffMode.Linear;
            _source.minDistance = 2f;
            _source.maxDistance = 18f;
            _source.volume = 0.24f;
            _health = GetComponent<CharacterHealth>();
            _basicAttack = GetComponent<BasicAttackController>();
            _lastHealth = _health == null ? 0f : _health.CurrentHealth;
            EnsureClips();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Configure(
            CharacterHealth health,
            BasicAttackController basicAttack,
            bool isPlayer)
        {
            Unsubscribe();
            _health = health;
            _basicAttack = basicAttack;
            _lastHealth = _health == null ? 0f : _health.CurrentHealth;
            if (_source != null)
            {
                _source.spatialBlend = isPlayer ? 0.2f : 0.75f;
            }
            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        public void PlaySkillCast()
        {
            Play(_skillCastClip, 0.55f);
        }

        public void PlaySkillImpact()
        {
            Play(_skillImpactClip, 0.75f);
        }

        private void Subscribe()
        {
            if (_subscribed)
            {
                return;
            }

            if (_health != null)
            {
                _health.HealthChanged += HandleHealthChanged;
            }
            if (_basicAttack != null)
            {
                _basicAttack.AttackPerformed += HandleAttackPerformed;
            }
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed)
            {
                return;
            }

            if (_health != null)
            {
                _health.HealthChanged -= HandleHealthChanged;
            }
            if (_basicAttack != null)
            {
                _basicAttack.AttackPerformed -= HandleAttackPerformed;
            }
            _subscribed = false;
        }

        private void HandleAttackPerformed()
        {
            Play(_attackClip, 0.45f);
        }

        private void HandleHealthChanged(float currentHealth, float maximumHealth)
        {
            if (currentHealth < _lastHealth - 0.01f)
            {
                Play(_hitClip, 0.62f);
            }
            _lastHealth = currentHealth;
        }

        private void Play(AudioClip clip, float volumeScale)
        {
            if (_source != null && clip != null && isActiveAndEnabled)
            {
                _source.PlayOneShot(clip, volumeScale);
            }
        }

        private static void EnsureClips()
        {
            if (_attackClip != null)
            {
                return;
            }

            _attackClip = CreateClip("Axiom Attack", 185f, 0.08f, 0.08f, 1u);
            _hitClip = CreateClip("Axiom Hit", 95f, 0.11f, 0.58f, 2u);
            _skillCastClip = CreateClip("Axiom Skill Cast", 520f, 0.13f, 0.04f, 3u);
            _skillImpactClip = CreateClip("Axiom Skill Impact", 125f, 0.16f, 0.32f, 4u);
        }

        private static AudioClip CreateClip(
            string clipName,
            float frequency,
            float duration,
            float noiseAmount,
            uint seed)
        {
            int sampleCount = Mathf.CeilToInt(SampleRate * duration);
            var samples = new float[sampleCount];
            uint state = seed;
            for (int i = 0; i < sampleCount; i++)
            {
                float normalized = i / (float)Mathf.Max(1, sampleCount - 1);
                float envelope = Mathf.Sin(normalized * Mathf.PI);
                float tone = Mathf.Sin(2f * Mathf.PI * frequency * i / SampleRate);
                state = (state * 1664525u) + 1013904223u;
                float noise = (((state >> 8) & 0x00FFFFFFu) / 8388607.5f) - 1f;
                samples[i] = ((tone * (1f - noiseAmount)) +
                              (noise * noiseAmount)) * envelope * 0.5f;
            }

            AudioClip clip = AudioClip.Create(
                clipName,
                sampleCount,
                1,
                SampleRate,
                false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
