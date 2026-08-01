using Axiom.Combat;
using Axiom.Manager;
using UnityEngine;

namespace Axiom.UI
{
    [DisallowMultipleComponent]
    public sealed class WorldHealthBar : MonoBehaviour
    {
        [SerializeField] private CharacterHealth health;
        [SerializeField] private UnityEngine.Camera worldCamera;
        [SerializeField] private TeamId team;
        [SerializeField] private string displayName;
        [SerializeField] private CharacterStatusController status;
        [SerializeField] private ElementStatusController elementStatus;
        [SerializeField] private CharacterShieldController shield;
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.45f, 0f);
        [SerializeField, Min(24f)] private float width = 92f;
        [SerializeField, Min(4f)] private float height = 11f;
        [SerializeField, Min(1f)] private float healthPerSegment = 100f;

        public CharacterHealth Health => health;
        public CharacterStatusController Status => status;

        public void Configure(
            CharacterHealth targetHealth,
            UnityEngine.Camera camera,
            TeamId targetTeam,
            string characterName)
        {
            health = targetHealth;
            worldCamera = camera;
            team = targetTeam;
            displayName = characterName;
            status = targetHealth == null
                ? null
                : targetHealth.GetComponent<CharacterStatusController>();
            elementStatus = targetHealth == null
                ? null
                : targetHealth.GetComponent<ElementStatusController>();
            shield = targetHealth == null
                ? null
                : targetHealth.GetComponent<CharacterShieldController>();
        }

        private void OnGUI()
        {
            if (health == null || worldCamera == null || health.MaximumHealth <= 0f)
            {
                return;
            }

            Vector3 screenPoint = worldCamera.WorldToScreenPoint(
                transform.position + worldOffset);
            if (screenPoint.z <= 0f)
            {
                return;
            }

            float left = screenPoint.x - (width * 0.5f);
            float top = Screen.height - screenPoint.y;
            var barRect = new Rect(left, top, width, height);
            float ratio = Mathf.Clamp01(health.CurrentHealth / health.MaximumHealth);

            DrawRect(new Rect(left - 2f, top - 2f, width + 4f, height + 4f), Color.black);
            DrawRect(barRect, new Color(0.08f, 0.08f, 0.08f, 0.96f));
            Color healthColor = team == TeamId.TeamA
                ? new Color(0.15f, 0.65f, 1f)
                : new Color(1f, 0.22f, 0.16f);
            DrawRect(new Rect(left, top, width * ratio, height), healthColor);

            int segmentCount = Mathf.Clamp(
                Mathf.CeilToInt(health.MaximumHealth / healthPerSegment),
                1,
                20);
            for (int i = 1; i < segmentCount; i++)
            {
                float x = left + (width * i / segmentCount);
                DrawRect(new Rect(x, top, 1f, height), new Color(0f, 0f, 0f, 0.7f));
            }

            string statusText = string.Empty;
            if (elementStatus != null && elementStatus.ActiveDamageOverTime.HasValue)
            {
                statusText += $" [{elementStatus.ActiveDamageOverTime.Value.ToString().ToUpperInvariant()}]";
            }
            GUI.Label(
                new Rect(left - 50f, top - 18f, width + 100f, 18f),
                $"{displayName}  {Mathf.CeilToInt(health.CurrentHealth)}" +
                $"{(shield != null && shield.CurrentShield > 0f ? $" +{Mathf.CeilToInt(shield.CurrentShield)} SH" : string.Empty)}" +
                statusText);

            if (status != null && status.ActiveEffect != Skill.CrowdControlType.None)
            {
                DrawCrowdControlBadge(
                    new Rect(left, top + height + 5f, width, 20f),
                    status.ActiveEffect,
                    status.ActiveRemainingDuration);
            }
        }

        internal static void DrawCrowdControlBadge(
            Rect rect,
            Skill.CrowdControlType effect,
            float remaining)
        {
            Color color = effect switch
            {
                Skill.CrowdControlType.Stun => new Color(1f, 0.72f, 0.08f, 0.96f),
                Skill.CrowdControlType.KnockUp => new Color(0.72f, 0.38f, 1f, 0.96f),
                Skill.CrowdControlType.Root => new Color(0.25f, 0.82f, 0.45f, 0.96f),
                Skill.CrowdControlType.Slow => new Color(0.2f, 0.7f, 1f, 0.96f),
                _ => new Color(0.8f, 0.3f, 0.3f, 0.96f)
            };
            DrawRect(new Rect(rect.x - 1f, rect.y - 1f, rect.width + 2f, rect.height + 2f),
                Color.black);
            DrawRect(rect, color);
            GUI.Label(
                new Rect(rect.x + 5f, rect.y + 1f, rect.width - 10f, rect.height - 2f),
                $"CC  {effect.ToString().ToUpperInvariant()}  {remaining:0.0}s");
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private void OnValidate()
        {
            width = Mathf.Max(24f, width);
            height = Mathf.Max(4f, height);
            healthPerSegment = Mathf.Max(1f, healthPerSegment);
        }
    }
}
