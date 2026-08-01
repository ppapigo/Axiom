using Axiom.Character;
using Axiom.Combat;
using Axiom.Demo;
using Axiom.Role;
using Axiom.Skill;
using UnityEngine;

namespace Axiom.UI
{
    [DisallowMultipleComponent]
    public sealed class CombatHud : MonoBehaviour
    {
        [SerializeField] private CharacterHealth health;
        [SerializeField] private DemoSkillController skills;
        [SerializeField] private CharacterDashController dash;
        [SerializeField] private CharacterRole role;

        public bool IsConfigured => health != null && skills != null && dash != null && role != null;
        public CharacterHealth Health => health;

        public void Configure(
            CharacterHealth playerHealth,
            DemoSkillController skillController,
            CharacterDashController dashController,
            CharacterRole characterRole)
        {
            health = playerHealth;
            skills = skillController;
            dash = dashController;
            role = characterRole;
        }

        private void OnGUI()
        {
            if (!IsConfigured || health.MaximumHealth <= 0f)
            {
                return;
            }

            float center = Screen.width * 0.5f;
            float slotTop = Screen.height - 100f;
            DrawHealth(center, slotTop - 42f);
            const float slotSize = 68f;
            const float gap = 8f;
            float left = center - (((slotSize * 4f) + (gap * 3f)) * 0.5f);
            DrawSkillSlot(new Rect(left, slotTop, slotSize, slotSize), "Q",
                skills.QSkillDefinition.Cooldown,
                skills.GetCooldownRemaining(SkillSlot.Q, Time.time));
            DrawSkillSlot(new Rect(left + slotSize + gap, slotTop, slotSize, slotSize), "E",
                skills.ESkillDefinition.Cooldown,
                skills.GetCooldownRemaining(SkillSlot.E, Time.time));
            DrawSkillSlot(new Rect(left + ((slotSize + gap) * 2f), slotTop, slotSize, slotSize), "R",
                skills.UltimateDefinition.Cooldown,
                skills.GetCooldownRemaining(SkillSlot.Ultimate, Time.time));
            DrawSkillSlot(new Rect(left + ((slotSize + gap) * 3f), slotTop, slotSize, slotSize), "SPACE",
                dash.CooldownDuration,
                dash.GetCooldownRemaining(Time.time));
        }

        private void DrawHealth(float center, float top)
        {
            const float width = 296f;
            const float height = 28f;
            float left = center - (width * 0.5f);
            float ratio = Mathf.Clamp01(health.CurrentHealth / health.MaximumHealth);
            DrawRect(new Rect(left - 2f, top - 2f, width + 4f, height + 4f), Color.black);
            DrawRect(new Rect(left, top, width, height), new Color(0.08f, 0.08f, 0.1f, 0.96f));
            DrawRect(new Rect(left, top, width * ratio, height), new Color(0.12f, 0.78f, 0.32f));
            string roleName = role.IsConfigured
                ? role.Definition.RoleId.ToString().ToUpperInvariant()
                : "PLAYER";
            GUI.Label(new Rect(left + 8f, top + 4f, width - 16f, 22f),
                $"{roleName}   HP {Mathf.CeilToInt(health.CurrentHealth)} / " +
                $"{Mathf.CeilToInt(health.MaximumHealth)}");
        }

        private static void DrawSkillSlot(Rect rect, string key, float duration, float remaining)
        {
            bool ready = remaining <= 0f;
            Color border = ready
                ? new Color(0.2f, 0.85f, 1f)
                : new Color(0.35f, 0.38f, 0.45f);
            DrawRect(new Rect(rect.x - 2f, rect.y - 2f, rect.width + 4f, rect.height + 4f), border);
            DrawRect(rect, new Color(0.08f, 0.09f, 0.13f, 0.98f));
            if (!ready)
            {
                float ratio = duration <= 0f ? 0f : Mathf.Clamp01(remaining / duration);
                DrawRect(new Rect(rect.x, rect.y, rect.width, rect.height * ratio),
                    new Color(0f, 0f, 0f, 0.72f));
            }

            GUI.Label(new Rect(rect.x + 7f, rect.y + 7f, rect.width - 14f, 22f), key);
            GUI.Label(new Rect(rect.x + 7f, rect.y + 35f, rect.width - 14f, 24f),
                ready ? "READY" : $"{remaining:0.0}s");
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
