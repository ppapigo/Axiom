using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Axiom.Data;
using Axiom.Role;
using Axiom.Skill;
using Axiom.Skill.Generation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Axiom.UI
{
    [DisallowMultipleComponent]
    public sealed class SkillBuilderPanel : MonoBehaviour
    {
        private readonly SkillBuilderModel _model = new SkillBuilderModel();
        private readonly Dictionary<SkillSlot, SkillDraft> _savedDrafts =
            new Dictionary<SkillSlot, SkillDraft>();
        private SkillBalanceProfile _balance;
        private Func<CharacterRoleId, SkillSlot, SkillDefinition> _baseDefinitionFactory;
        private SkillGenerationPipeline _generationPipeline;
        private SkillGenerationPipelineResult _generationResult;
        private CancellationTokenSource _generationCancellation;
        private CharacterRoleDefinition _roleDefinition;
        private string _generationPrompt = string.Empty;
        private string _generationStatus = "Describe a skill, then generate a draft.";
        private string _generationProviderName = "NOT CONFIGURED";
        private bool _generationUsesRemoteEndpoint;
        private bool _isGenerating;
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
        public SkillSlot CurrentSlot => _slot;
        public SkillBuilderModel Model => _model;
        public bool IsGenerating => _isGenerating;
        public SkillGenerationPipelineResult GenerationResult => _generationResult;
        public string GenerationStatus => _generationStatus;
        public string GenerationProviderName => _generationProviderName;
        public bool GenerationUsesRemoteEndpoint => _generationUsesRemoteEndpoint;

        public bool TryGetSavedDraft(SkillSlot slot, out SkillDraft draft)
        {
            return _savedDrafts.TryGetValue(slot, out draft);
        }

        public void Configure(SkillBalanceProfile balance)
        {
            _balance = balance;
        }

        public void ConfigureGeneration(
            ISkillGenerationProvider provider,
            Func<CharacterRoleId, SkillSlot, SkillDefinition> baseDefinitionFactory)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            _generationPipeline = new SkillGenerationPipeline(
                provider);
            _baseDefinitionFactory = baseDefinitionFactory ??
                throw new ArgumentNullException(nameof(baseDefinitionFactory));
            if (provider is ISkillGenerationProviderInfo info)
            {
                _generationProviderName = string.IsNullOrWhiteSpace(info.DisplayName)
                    ? provider.GetType().Name
                    : info.DisplayName.Trim().ToUpperInvariant();
                _generationUsesRemoteEndpoint = info.UsesRemoteEndpoint;
            }
            else
            {
                _generationProviderName = provider.GetType().Name.ToUpperInvariant();
                _generationUsesRemoteEndpoint = false;
            }
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
            _roleDefinition = null;
            SetContextCore(role, slot, roleElementPool);
        }

        public void SetContext(
            CharacterRoleDefinition role,
            SkillSlot slot,
            RoleElementPool roleElementPool)
        {
            _roleDefinition = role ?? throw new ArgumentNullException(nameof(role));
            SetContextCore(role.RoleId, slot, roleElementPool);
        }

        private void SetContextCore(
            CharacterRoleId role,
            SkillSlot slot,
            RoleElementPool roleElementPool)
        {
            bool contextChanged = !_hasContext || _role != role || _slot != slot;
            _role = role;
            _slot = slot;
            _roleElementPool = roleElementPool ??
                throw new ArgumentNullException(nameof(roleElementPool));
            _hasContext = true;
            _isAvailable = true;
            if (contextChanged)
            {
                CancelGeneration();
                _generationResult = null;
                _generationPrompt = string.Empty;
                _generationStatus = "Describe a skill, then generate a draft.";
                _model.Reset();
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

            return TryStoreDraft(_model.CreateDraft(_slot));
        }

        public async Task<bool> TryGenerateDraftAsync(
            string prompt,
            CancellationToken cancellationToken = default)
        {
            if (_balance == null || !_hasContext || _roleDefinition == null ||
                _generationPipeline == null || _baseDefinitionFactory == null ||
                _isGenerating)
            {
                _generationStatus = "AI generation is not configured for this slot.";
                return false;
            }

            _generationPrompt = (prompt ?? string.Empty).Trim();
            _generationResult = null;
            _generationStatus = $"{_generationProviderName}: Generating and validating...";
            _isGenerating = true;
            var cancellation = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);
            _generationCancellation = cancellation;
            try
            {
                SkillDefinition baseDefinition = _baseDefinitionFactory(_role, _slot);
                _generationResult = await _generationPipeline.GenerateAsync(
                    _generationPrompt,
                    _roleDefinition,
                    _slot,
                    baseDefinition,
                    _balance,
                    _roleElementPool,
                    cancellation.Token);
                _generationStatus = _generationResult.UsedFallback
                    ? BuildFallbackStatus(_generationResult)
                    : _generationResult.WasAutoCorrected
                        ? $"{_generationProviderName}: Draft ready (auto-corrected)."
                        : $"{_generationProviderName}: Draft ready to confirm.";
                return _generationResult.Validation.IsValid;
            }
            catch (OperationCanceledException)
            {
                _generationStatus = $"{_generationProviderName}: Generation cancelled.";
                return false;
            }
            finally
            {
                if (_generationCancellation == cancellation)
                {
                    _generationCancellation = null;
                    _isGenerating = false;
                }

                cancellation.Dispose();
            }
        }

        public bool TryConfirmGeneratedDraft()
        {
            return _generationResult != null &&
                   _generationResult.Validation.IsValid &&
                   TryStoreDraft(_generationResult.Draft);
        }

        private bool TryStoreDraft(in SkillDraft draft)
        {
            if (_roleElementPool == null)
            {
                return false;
            }

            _savedDraft = draft;
            if (_savedDraft.Element.HasValue &&
                !_roleElementPool.TryAssign(
                    _role,
                    _slot,
                    _savedDraft.Element.Value))
            {
                return false;
            }

            _hasSavedDraft = true;
            _savedDrafts[_slot] = _savedDraft;
            _isVisible = false;
            DraftSaved?.Invoke(_savedDraft);
            return true;
        }

        private void OnDestroy()
        {
            CancelGeneration();
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
                    ? $"SKILL FORGE [B] - {_slot}"
                    : $"SKILL FORGE [B] - {_slot}";
                if (GUI.Button(new Rect(326f, 12f, 210f, 38f), buttonLabel))
                {
                    _isVisible = true;
                }

                return;
            }

            float width = 944f;
            float manualWidth = 560f;
            float height = 690f;
            float left = Mathf.Max(8f, (Screen.width - width) * 0.5f);
            float top = Mathf.Max(8f, (Screen.height - height) * 0.5f);
            GUI.Box(
                new Rect(left, top, width, height),
                $"SKILL FORGE - {_slot} - 100 POINT BUILD");

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
                new Rect(left + 24f, top + 562f, manualWidth - 48f, 48f),
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

            DrawGenerationPanel(left + manualWidth + 8f, top + 32f, 352f, 638f);
        }

        private void DrawGenerationPanel(float left, float top, float width, float height)
        {
            GUI.Box(
                new Rect(left, top, width, height),
                $"AI SKILL GENERATOR ({_generationProviderName})");
            string source = _generationUsesRemoteEndpoint
                ? "SOURCE: REMOTE SERVERLESS ENDPOINT"
                : "SOURCE: LOCAL MOCK - NO NETWORK";
            GUI.Label(new Rect(left + 14f, top + 28f, width - 28f, 20f), source);
            GUI.Label(new Rect(left + 14f, top + 48f, width - 28f, 22f),
                "Describe the effect, element, area and CC:");
            _generationPrompt = GUI.TextArea(
                new Rect(left + 14f, top + 72f, width - 28f, 68f),
                _generationPrompt,
                240);

            GUI.enabled = !_isGenerating && _roleDefinition != null &&
                          _generationPipeline != null;
            if (GUI.Button(
                    new Rect(left + 14f, top + 150f, width - 28f, 38f),
                    _isGenerating ? "GENERATING..." : "GENERATE DRAFT"))
            {
                _ = TryGenerateDraftAsync(_generationPrompt);
            }

            GUI.enabled = true;
            GUI.Box(new Rect(left + 14f, top + 198f, width - 28f, 46f),
                _generationStatus);
            if (_generationResult == null)
            {
                GUI.Label(
                    new Rect(left + 18f, top + 262f, width - 36f, 120f),
                    "Examples:\n- fire ground area that slows enemies\n" +
                    "- ice projectile with stun\n- dash and poison strike");
                return;
            }

            SkillDraft draft = _generationResult.Draft;
            string name = string.IsNullOrWhiteSpace(_generationResult.Response?.displayName)
                ? "Safe preset"
                : _generationResult.Response.displayName;
            string description = _generationResult.Response?.description;
            GUI.Label(
                new Rect(left + 18f, top + 258f, width - 36f, 24f),
                string.IsNullOrWhiteSpace(description)
                    ? name
                    : $"{name} - {description}");
            SkillPointModifiers modifiers = draft.Modifiers;
            string crowdControl = modifiers.AppliesStun
                ? "Stun"
                : modifiers.AppliesKnockUp
                    ? "KnockUp"
                    : modifiers.AppliesSlow ? "Slow" : "None";
            GUI.Box(
                new Rect(left + 14f, top + 286f, width - 28f, 78f),
                $"{draft.Type} | {draft.Element?.ToString() ?? "No Element"} | CC {crowdControl}\n" +
                $"DMG +{modifiers.DamageIncreasePercent:0}%  RAD +{modifiers.RadiusIncrease:0}m  " +
                $"RNG +{modifiers.RangeIncrease:0}m  CD -{modifiers.CooldownReduction:0}s\n" +
                $"{_generationResult.PointCost.Total} / {_balance.LoadoutPointBudget} POINTS");

            GUI.Label(
                new Rect(left + 18f, top + 372f, width - 36f, 112f),
                BuildPointCostText(_generationResult.PointCost));
            GUI.Label(
                new Rect(left + 18f, top + 490f, width - 36f, 74f),
                BuildGenerationNotes(_generationResult));

            GUI.enabled = _generationResult.Validation.IsValid;
            if (GUI.Button(
                    new Rect(left + 14f, top + height - 54f, width - 28f, 40f),
                    "CONFIRM & SAVE"))
            {
                TryConfirmGeneratedDraft();
            }

            GUI.enabled = true;
        }

        private static string BuildPointCostText(SkillPointCostBreakdown pointCost)
        {
            var builder = new StringBuilder("POINT BREAKDOWN\n");
            for (int i = 0; i < pointCost.Items.Count; i++)
            {
                SkillPointCostItem item = pointCost.Items[i];
                builder.Append(item.Category).Append("  +")
                    .Append(item.Points).Append('P');
                builder.Append(i % 2 == 0 ? "     " : "\n");
            }

            if (pointCost.Items.Count == 0)
            {
                builder.Append("Base projectile  0P");
            }

            return builder.ToString();
        }

        private static string BuildGenerationNotes(SkillGenerationPipelineResult result)
        {
            var builder = new StringBuilder();
            foreach (string change in result.Changes)
            {
                builder.Append("AUTO: ").Append(change).Append('\n');
            }
            foreach (string error in result.Errors)
            {
                builder.Append("INFO: ").Append(error).Append('\n');
            }

            return builder.Length == 0 ? "No automatic corrections." : builder.ToString();
        }

        private string BuildFallbackStatus(SkillGenerationPipelineResult result)
        {
            string reason = "Invalid response";
            if (result.Errors.Count > 0 && !string.IsNullOrWhiteSpace(result.Errors[0]))
            {
                reason = result.Errors[0].Trim();
                if (reason.Length > 72)
                {
                    reason = reason.Substring(0, 69) + "...";
                }
            }

            return $"{_generationProviderName}: SAFE PRESET\n{reason}";
        }

        private void CancelGeneration()
        {
            if (_generationCancellation != null &&
                !_generationCancellation.IsCancellationRequested)
            {
                _generationCancellation.Cancel();
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
            if (_slot == SkillSlot.Ultimate)
            {
                return _role == CharacterRoleId.Mage
                    ? SkillType.GroundArea
                    : SkillType.Projectile;
            }

            if (_slot == SkillSlot.E)
            {
                return _role == CharacterRoleId.Tank
                    ? SkillType.Cone
                    : SkillType.GroundArea;
            }

            return _role == CharacterRoleId.Tank ? SkillType.Cone : SkillType.Projectile;
        }
    }
}
