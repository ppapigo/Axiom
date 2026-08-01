using System;
using Axiom.Data;
using Axiom.Role;
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
        private bool _isAvailable;
        private bool _hasContext;
        private bool _hasSavedDraft;
        private SkillDraft _savedDraft;
        private RoleElementPool _roleElementPool;
        private CharacterRoleId _role;
        private SkillSlot _slot;

        public event Action<SkillDraft> DraftSaved;

        public bool IsConfigured => _balance != null;
        public bool IsVisible => _isVisible;
        public bool IsAvailable => _isAvailable;
        public bool HasSavedDraft => _hasSavedDraft;
        public SkillDraft SavedDraft => _savedDraft;
        public SkillBuilderModel Model => _model;

        public void Configure(SkillBalanceProfile balance)
        {
            _balance = balance;
        }

        public void ToggleVisibility()
        {
            if (_isAvailable)
            {
                _isVisible = !_isVisible;
            }
        }

        public void SetContext(
            CharacterRoleId role,
            SkillSlot slot,
            RoleElementPool roleElementPool)
        {
            _role = role;
            _slot = slot;
            _roleElementPool = roleElementPool ??
                throw new ArgumentNullException(nameof(roleElementPool));
            _hasContext = true;
            _isAvailable = true;
            if (!_model.Type.HasValue)
            {
                _model.SelectType(GetDefaultType());
            }
        }

        public bool TrySaveDraft()
        {
            if (_balance == null || !_hasContext || !_model.Type.HasValue ||
                !_model.IsWithinBudget(_balance))
            {
                return false;
            }

            _savedDraft = _model.CreateDraft();
            if (_savedDraft.Element.HasValue &&
                !_roleElementPool.TryAssign(
                    _role,
                    _slot,
                    _savedDraft.Element.Value))
            {
                return false;
            }

            _hasSavedDraft = true;
            _isVisible = false;
            DraftSaved?.Invoke(_savedDraft);
            return true;
        }

        private void Update()
        {
            if (_isAvailable && Keyboard.current != null &&
                Keyboard.current.bKey.wasPressedThisFrame)
            {
                ToggleVisibility();
            }
        }

        private void OnGUI()
        {
            if (_balance == null || !_isAvailable)
            {
                return;
            }

            if (!_isVisible)
            {
                string buttonLabel = _hasSavedDraft
                    ? "SKILL FORGE [B] - Q DRAFT"
                    : "SKILL FORGE [B]";
                if (GUI.Button(new Rect(326f, 12f, 210f, 38f), buttonLabel))
                {
                    _isVisible = true;
                }

                return;
            }

            float width = 560f;
            float height = 690f;
            float left = (Screen.width - width) * 0.5f;
            float top = Mathf.Max(8f, (Screen.height - height) * 0.5f);
            GUI.Box(new Rect(left, top, width, height), "SKILL FORGE - 100 POINT BUILD");

            DrawStepper(left, top + 42f, "Damage", $"+{_model.DamageIncreasePercent:0}%", _balance.DamageCostPerTenPercent, _model.AdjustDamage);
            DrawStepper(left, top + 82f, "Radius", $"+{_model.RadiusIncrease:0}m", _balance.RadiusCostPerMeter, _model.AdjustRadius);
            DrawStepper(left, top + 122f, "Range", $"+{_model.RangeIncrease:0}m", _balance.RangeCostPerMeter, _model.AdjustRange);
            DrawStepper(left, top + 162f, "Cooldown", $"-{_model.CooldownReduction:0}s", _balance.CooldownCostPerSecond, _model.AdjustCooldownReduction);

            GUI.Label(new Rect(left + 24f, top + 202f, 500f, 24f), "ATTACK TYPE (SELECT 1)");
            DrawType(left + 24f, top + 228f, SkillType.Target, "TARGET");
            DrawType(left + 198f, top + 228f, SkillType.Projectile, "PROJECTILE");
            DrawType(left + 372f, top + 228f, SkillType.SelfArea, "SELF AREA");
            DrawType(left + 24f, top + 260f, SkillType.GroundArea, "GROUND AREA");
            DrawType(left + 198f, top + 260f, SkillType.Global, "GLOBAL");
            DrawType(left + 372f, top + 260f, SkillType.Cone, "CONE");

            GUI.Label(
                new Rect(left + 24f, top + 298f, 500f, 24f),
                $"ELEMENT ({_balance.ElementCost}P, 1 PER SKILL)  " +
                $"ROLE POOL {_roleElementPool.GetDistinctElementCount(_role)}/2");
            SkillElement[] elements =
            {
                SkillElement.Fire,
                SkillElement.Ice,
                SkillElement.Lightning,
                SkillElement.Poison,
                SkillElement.Water,
                SkillElement.Wind,
                SkillElement.Earth
            };
            for (int i = 0; i < elements.Length; i++)
            {
                int column = i % 3;
                int row = i / 3;
                DrawElement(
                    left + 24f + (column * 154f),
                    top + 324f + (row * 32f),
                    elements[i]);
            }

            GUI.Label(
                new Rect(left + 24f, top + 426f, 500f, 24f),
                "CC (SELECT MAX 1, APPLIED ON HIT)");
            DrawEffect(left, top + 454f, SkillPointEffect.Slow,
                $"Slow {_balance.GetCrowdControlDuration(CrowdControlType.Slow):0.0}s");
            DrawEffect(left, top + 488f, SkillPointEffect.Stun,
                $"Stun {_balance.GetCrowdControlDuration(CrowdControlType.Stun):0.0}s");
            DrawEffect(left, top + 522f, SkillPointEffect.KnockUp,
                $"Knock Up {_balance.GetCrowdControlDuration(CrowdControlType.KnockUp):0.0}s");
            DrawEffect(left + 280f, top + 454f, SkillPointEffect.Mobility, "Mobility");
            DrawEffect(left + 280f, top + 488f, SkillPointEffect.Shield, "Shield");
            DrawEffect(left + 280f, top + 522f, SkillPointEffect.Healing, "Healing");

            int cost = _model.GetPointCost(_balance);
            bool valid = cost <= _balance.LoadoutPointBudget;
            Color previousColor = GUI.color;
            GUI.color = valid ? new Color(0.4f, 1f, 0.65f) : new Color(1f, 0.35f, 0.3f);
            string status = valid ? "READY" : $"OVER BUDGET +{cost - _balance.LoadoutPointBudget}";
            GUI.Box(
                new Rect(left + 24f, top + 562f, width - 48f, 48f),
                $"{cost} / {_balance.LoadoutPointBudget} POINTS   {status}");
            GUI.color = previousColor;

            if (GUI.Button(new Rect(left + 24f, top + 626f, 135f, 44f), "RESET"))
            {
                _model.Reset();
                _model.SelectType(GetDefaultType());
            }

            GUI.enabled = valid;
            if (GUI.Button(new Rect(left + 184f, top + 626f, 190f, 44f), "SAVE DRAFT"))
            {
                TrySaveDraft();
            }

            GUI.enabled = true;
            if (GUI.Button(new Rect(left + 399f, top + 626f, 137f, 44f), "CLOSE"))
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

        private void DrawElement(float left, float top, SkillElement element)
        {
            bool selected = _model.IsElementSelected(element);
            string marker = selected ? "[ON]" : "[  ]";
            bool canSelect = selected ||
                             _roleElementPool.CanAssign(_role, _slot, element);
            GUI.enabled = canSelect;
            if (GUI.Button(
                    new Rect(left, top, 146f, 28f),
                    $"{marker} {element}"))
            {
                _model.ToggleElement(element);
            }

            GUI.enabled = true;
        }

        private void DrawType(float left, float top, SkillType type, string label)
        {
            bool selected = _model.IsTypeSelected(type);
            bool allowed = IsTypeAllowed(type);
            GUI.enabled = allowed;
            string marker = selected ? "[ON]" : "[  ]";
            int pointCost = _balance.GetSkillTypeCost(type);
            if (GUI.Button(
                    new Rect(left, top, 164f, 28f),
                    $"{marker} {label}  {pointCost}P"))
            {
                _model.SelectType(type);
            }

            GUI.enabled = true;
        }

        private bool IsTypeAllowed(SkillType type)
        {
            if (_role != CharacterRoleId.Tank || _slot == SkillSlot.Ultimate)
            {
                return true;
            }

            return type == SkillType.Cone || type == SkillType.SelfArea;
        }

        private SkillType GetDefaultType()
        {
            return _role == CharacterRoleId.Tank
                ? SkillType.Cone
                : SkillType.Projectile;
        }
    }
}
