using Axiom.Data;
using Axiom.Skill;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Axiom.UI
{
    [DisallowMultipleComponent]
    public sealed class SkillBuilderPanel : MonoBehaviour
    {
        private readonly SkillBuilderModel _model = new SkillBuilderModel();
        private SkillBalanceProfile _balance;
        private bool _isVisible;
        private bool _hasSavedDraft;
        private SkillPointModifiers _savedDraft;

        public bool IsConfigured => _balance != null;
        public bool IsVisible => _isVisible;
        public bool HasSavedDraft => _hasSavedDraft;
        public SkillPointModifiers SavedDraft => _savedDraft;
        public SkillBuilderModel Model => _model;

        public void Configure(SkillBalanceProfile balance)
        {
            _balance = balance;
        }

        public void ToggleVisibility()
        {
            _isVisible = !_isVisible;
        }

        public bool TrySaveDraft()
        {
            if (_balance == null || !_model.IsWithinBudget(_balance))
            {
                return false;
            }

            _savedDraft = _model.CreateModifiers();
            _hasSavedDraft = true;
            _isVisible = false;
            return true;
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
            {
                ToggleVisibility();
            }
        }

        private void OnGUI()
        {
            if (_balance == null)
            {
                return;
            }

            if (!_isVisible)
            {
                if (GUI.Button(new Rect(326f, 12f, 170f, 38f), "SKILL FORGE [B]"))
                {
                    _isVisible = true;
                }

                return;
            }

            float width = 520f;
            float height = 570f;
            float left = (Screen.width - width) * 0.5f;
            float top = Mathf.Max(18f, (Screen.height - height) * 0.5f);
            GUI.Box(new Rect(left, top, width, height), "SKILL FORGE - 100 POINT BUILD");

            DrawStepper(left, top + 42f, "Damage", $"+{_model.DamageIncreasePercent:0}%", _balance.DamageCostPerTenPercent, _model.AdjustDamage);
            DrawStepper(left, top + 82f, "Radius", $"+{_model.RadiusIncrease:0}m", _balance.RadiusCostPerMeter, _model.AdjustRadius);
            DrawStepper(left, top + 122f, "Range", $"+{_model.RangeIncrease:0}m", _balance.RangeCostPerMeter, _model.AdjustRange);
            DrawStepper(left, top + 162f, "Cooldown", $"-{_model.CooldownReduction:0}s", _balance.CooldownCostPerSecond, _model.AdjustCooldownReduction);

            GUI.Label(new Rect(left + 24f, top + 210f, 460f, 24f), "EFFECTS");
            DrawEffect(left, top + 240f, SkillPointEffect.BurnOrPoison, "Burn / Poison");
            DrawEffect(left, top + 278f, SkillPointEffect.Slow, "Slow");
            DrawEffect(left, top + 316f, SkillPointEffect.Stun, "Stun");
            DrawEffect(left, top + 354f, SkillPointEffect.KnockUp, "Knock Up");
            DrawEffect(left + 250f, top + 240f, SkillPointEffect.Mobility, "Mobility");
            DrawEffect(left + 250f, top + 278f, SkillPointEffect.Shield, "Shield");
            DrawEffect(left + 250f, top + 316f, SkillPointEffect.Healing, "Healing");

            int cost = _model.GetPointCost(_balance);
            bool valid = cost <= _balance.LoadoutPointBudget;
            Color previousColor = GUI.color;
            GUI.color = valid ? new Color(0.4f, 1f, 0.65f) : new Color(1f, 0.35f, 0.3f);
            string status = valid ? "READY" : $"OVER BUDGET +{cost - _balance.LoadoutPointBudget}";
            GUI.Box(
                new Rect(left + 24f, top + 406f, width - 48f, 58f),
                $"{cost} / {_balance.LoadoutPointBudget} POINTS   {status}");
            GUI.color = previousColor;

            if (GUI.Button(new Rect(left + 24f, top + 488f, 135f, 48f), "RESET"))
            {
                _model.Reset();
            }

            GUI.enabled = valid;
            if (GUI.Button(new Rect(left + 176f, top + 488f, 190f, 48f), "SAVE DRAFT"))
            {
                TrySaveDraft();
            }

            GUI.enabled = true;
            if (GUI.Button(new Rect(left + 383f, top + 488f, 113f, 48f), "CLOSE"))
            {
                _isVisible = false;
            }
        }

        private static void DrawStepper(
            float left,
            float top,
            string label,
            string value,
            int pointCost,
            System.Action<int> adjust)
        {
            GUI.Label(new Rect(left + 24f, top, 170f, 30f), $"{label} ({pointCost}P / step)");
            if (GUI.Button(new Rect(left + 250f, top, 44f, 30f), "-"))
            {
                adjust(-1);
            }

            GUI.Box(new Rect(left + 302f, top, 116f, 30f), value);
            if (GUI.Button(new Rect(left + 426f, top, 44f, 30f), "+"))
            {
                adjust(1);
            }
        }

        private void DrawEffect(
            float left,
            float top,
            SkillPointEffect effect,
            string label)
        {
            bool enabled = _model.IsEnabled(effect);
            string marker = enabled ? "[ON]" : "[  ]";
            int pointCost = _balance.GetEffectCost(effect);
            if (GUI.Button(new Rect(left + 24f, top, 220f, 30f), $"{marker} {label}  {pointCost}P"))
            {
                _model.Toggle(effect);
            }
        }
    }
}
