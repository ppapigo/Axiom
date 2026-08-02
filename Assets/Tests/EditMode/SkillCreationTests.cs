using System.Linq;
using System.Threading.Tasks;
using Axiom.Character;
using Axiom.Combat;
using Axiom.Data;
using Axiom.Demo;
using Axiom.Role;
using Axiom.Skill;
using Axiom.Skill.Generation;
using Axiom.UI;
using NUnit.Framework;
using UnityEngine;

namespace Axiom.Tests.EditMode
{
    public sealed class SkillCreationTests
    {
        [Test]
        public void SkillData_ProvidesExpectedEditableDefaults()
        {
            SkillData data = ScriptableObject.CreateInstance<SkillData>();

            SkillDefinition definition = data.Definition;

            Assert.That(definition.DamageCoefficient, Is.EqualTo(1.2f));
            Assert.That(definition.Cooldown, Is.EqualTo(5f));
            Assert.That(definition.CastDelay, Is.EqualTo(0.3f));
            Assert.That(definition.Type, Is.EqualTo(SkillType.Projectile));
            Object.DestroyImmediate(data);
        }

        [Test]
        public void SkillBalance_DefaultBudget_IsOneHundredPoints()
        {
            SkillBalanceProfile balance = CreateBalance();

            Assert.That(balance.LoadoutPointBudget, Is.EqualTo(100));
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void SkillBalance_UsesDifferentAttackTypeCosts()
        {
            SkillBalanceProfile balance = CreateBalance();

            Assert.That(balance.GetSkillTypeCost(SkillType.Projectile), Is.Zero);
            Assert.That(balance.GetSkillTypeCost(SkillType.Target), Is.EqualTo(8));
            Assert.That(balance.GetSkillTypeCost(SkillType.Cone), Is.EqualTo(8));
            Assert.That(balance.GetSkillTypeCost(SkillType.SelfArea), Is.EqualTo(12));
            Assert.That(balance.GetSkillTypeCost(SkillType.GroundArea), Is.EqualTo(15));
            Assert.That(balance.GetSkillTypeCost(SkillType.Global), Is.EqualTo(35));
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void SkillPointCost_UsesConfiguredBaselineCosts()
        {
            SkillBalanceProfile balance = CreateBalance();
            var modifiers = new SkillPointModifiers(
                damageIncreasePercent: 20f,
                radiusIncrease: 2f,
                rangeIncrease: 1f,
                cooldownReduction: 1f,
                appliesSlow: true,
                appliesStun: true,
                appliesKnockUp: true,
                addsMobility: true,
                createsShield: true,
                heals: true);

            int cost = balance.CalculatePointCost(modifiers, selectedElementCount: 1);

            Assert.That(cost, Is.EqualTo(146));
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void SkillPointCost_RoundsPartialIncrementsUp()
        {
            SkillBalanceProfile balance = CreateBalance();
            var modifiers = new SkillPointModifiers(
                damageIncreasePercent: 1f,
                radiusIncrease: 0.1f,
                rangeIncrease: 0.1f,
                cooldownReduction: 0.1f);

            int cost = balance.CalculatePointCost(modifiers);

            Assert.That(cost, Is.EqualTo(22));
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void SkillBuilderModel_TracksOptionsAndCalculatedCost()
        {
            SkillBalanceProfile balance = CreateBalance();
            var builder = new SkillBuilderModel();
            builder.AdjustDamage(2);
            builder.AdjustRadius(1);
            builder.AdjustRange(1);
            builder.AdjustCooldownReduction(1);
            builder.Toggle(SkillPointEffect.Stun);
            builder.Toggle(SkillPointEffect.Shield);

            Assert.That(builder.DamageIncreasePercent, Is.EqualTo(20f));
            Assert.That(builder.GetPointCost(balance), Is.EqualTo(62));
            Assert.That(builder.IsWithinBudget(balance), Is.True);
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void SkillBuilderModel_RejectsOverBudgetDraft()
        {
            SkillBalanceProfile balance = CreateBalance();
            var builder = new SkillBuilderModel();
            builder.AdjustDamage(20);
            builder.Toggle(SkillPointEffect.Mobility);

            Assert.That(builder.GetPointCost(balance), Is.EqualTo(125));
            Assert.That(builder.IsWithinBudget(balance), Is.False);
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void SkillBuilderModel_AllowsOnlyOneCrowdControlEffect()
        {
            var builder = new SkillBuilderModel();

            builder.Toggle(SkillPointEffect.Slow);
            builder.Toggle(SkillPointEffect.Stun);

            Assert.That(builder.IsEnabled(SkillPointEffect.Slow), Is.False);
            Assert.That(builder.IsEnabled(SkillPointEffect.Stun), Is.True);
            Assert.That(builder.IsEnabled(SkillPointEffect.KnockUp), Is.False);
        }

        [Test]
        public void SkillBuilderModel_StoresOneSelectedAttackType()
        {
            var builder = new SkillBuilderModel();

            builder.SelectType(SkillType.GroundArea);
            builder.SelectType(SkillType.Cone);

            Assert.That(builder.IsTypeSelected(SkillType.GroundArea), Is.False);
            Assert.That(builder.IsTypeSelected(SkillType.Cone), Is.True);
            Assert.That(builder.CreateDraft().Type, Is.EqualTo(SkillType.Cone));
        }

        [Test]
        public void SkillBuilderModel_SelectsOnePointCostedElement()
        {
            SkillBalanceProfile balance = CreateBalance();
            var builder = new SkillBuilderModel();

            Assert.That(builder.ToggleElement(SkillElement.Fire), Is.True);
            Assert.That(builder.ToggleElement(SkillElement.Ice), Is.True);
            Assert.That(builder.SelectedElementCount, Is.EqualTo(1));
            Assert.That(builder.IsElementSelected(SkillElement.Fire), Is.False);
            Assert.That(builder.IsElementSelected(SkillElement.Ice), Is.True);
            Assert.That(builder.GetPointCost(balance), Is.EqualTo(10));

            SkillDraft draft = builder.CreateDraft();
            Assert.That(draft.Element, Is.EqualTo(SkillElement.Ice));
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void RoleElementPool_LimitsDistinctElementsAcrossRoleSlots()
        {
            var pool = new RoleElementPool();

            Assert.That(pool.TryAssign(
                CharacterRoleId.Mage, SkillSlot.Q, SkillElement.Fire), Is.True);
            Assert.That(pool.TryAssign(
                CharacterRoleId.Mage, SkillSlot.E, SkillElement.Ice), Is.True);
            Assert.That(pool.TryAssign(
                CharacterRoleId.Mage, SkillSlot.Ultimate, SkillElement.Lightning), Is.False);
            Assert.That(pool.TryAssign(
                CharacterRoleId.Mage, SkillSlot.Ultimate, SkillElement.Fire), Is.True);
            Assert.That(pool.TryAssign(
                CharacterRoleId.Assassin, SkillSlot.Ultimate, SkillElement.Lightning), Is.True);
            Assert.That(pool.GetDistinctElementCount(CharacterRoleId.Mage), Is.EqualTo(2));
        }

        [Test]
        public void SkillBuilderPanel_IsUnavailableUntilRoleSelection()
        {
            SkillBalanceProfile balance = CreateBalance();
            var gameObject = new GameObject("Skill Builder Test");
            SkillBuilderPanel panel = gameObject.AddComponent<SkillBuilderPanel>();
            panel.Configure(balance);

            Assert.That(panel.IsAvailable, Is.False);
            panel.ToggleVisibility();
            Assert.That(panel.IsVisible, Is.False);

            panel.SetContext(
                CharacterRoleId.Mage,
                SkillSlot.Q,
                new RoleElementPool());
            panel.ToggleVisibility();
            Assert.That(panel.IsVisible, Is.True);
            Object.DestroyImmediate(gameObject);
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void SkillBuilderPanel_StoresSeparateDraftsForQERSlots()
        {
            SkillBalanceProfile balance = CreateBalance();
            var gameObject = new GameObject("QER Skill Builder Test");
            SkillBuilderPanel panel = gameObject.AddComponent<SkillBuilderPanel>();
            var pool = new RoleElementPool();
            panel.Configure(balance);

            panel.SetContext(CharacterRoleId.Mage, SkillSlot.Q, pool);
            panel.Model.AdjustDamage(1);
            Assert.That(panel.TrySaveDraft(), Is.True);
            panel.SetContext(CharacterRoleId.Mage, SkillSlot.E, pool);
            panel.Model.AdjustRadius(1);
            Assert.That(panel.TrySaveDraft(), Is.True);
            panel.SetContext(CharacterRoleId.Mage, SkillSlot.Ultimate, pool);
            Assert.That(panel.TrySaveDraft(), Is.True);

            Assert.That(panel.TryGetSavedDraft(SkillSlot.Q, out SkillDraft q), Is.True);
            Assert.That(panel.TryGetSavedDraft(SkillSlot.E, out SkillDraft e), Is.True);
            Assert.That(panel.TryGetSavedDraft(
                SkillSlot.Ultimate, out SkillDraft ultimate), Is.True);
            Assert.That(q.Modifiers.DamageIncreasePercent, Is.EqualTo(10f));
            Assert.That(e.Modifiers.RadiusIncrease, Is.EqualTo(1f));
            Assert.That(ultimate.Slot, Is.EqualTo(SkillSlot.Ultimate));
            Object.DestroyImmediate(gameObject);
            Object.DestroyImmediate(balance);
        }

        [Test]
        public async Task SkillBuilderPanel_GeneratesPreviewAndConfirmsDraft()
        {
            SkillBalanceProfile balance = CreateBalance();
            MageRoleDefinition role = ScriptableObject.CreateInstance<MageRoleDefinition>();
            var gameObject = new GameObject("AI Skill Builder Test");
            SkillBuilderPanel panel = gameObject.AddComponent<SkillBuilderPanel>();
            panel.Configure(balance);
            panel.ConfigureGeneration(
                new MockSkillGenerationProvider(),
                DemoSkillDefinitionFactory.Create);
            panel.SetContext(role, SkillSlot.Q, new RoleElementPool());

            bool generated = await panel.TryGenerateDraftAsync(
                "fire ground area with slow");

            Assert.That(generated, Is.True);
            Assert.That(panel.GenerationResult, Is.Not.Null);
            Assert.That(panel.GenerationResult.PointCost.Total,
                Is.EqualTo(panel.GenerationResult.Validation.PointCost));
            Assert.That(panel.TryConfirmGeneratedDraft(), Is.True);
            Assert.That(panel.TryGetSavedDraft(SkillSlot.Q, out SkillDraft saved), Is.True);
            Assert.That(saved.Type, Is.EqualTo(SkillType.GroundArea));
            Assert.That(saved.Element, Is.EqualTo(SkillElement.Fire));
            Object.DestroyImmediate(gameObject);
            Object.DestroyImmediate(role);
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void SkillDraftApplier_AppliesQNumericModifiersAndCost()
        {
            SkillBalanceProfile balance = CreateBalance();
            SkillDefinition baseSkill = CreateSkill(SkillSlot.Q, SkillType.Projectile);
            var modifiers = new SkillPointModifiers(
                damageIncreasePercent: 20f,
                radiusIncrease: 1f,
                rangeIncrease: 2f,
                cooldownReduction: 1f,
                appliesStun: true);

            SkillDefinition result = SkillDraftApplier.Apply(
                baseSkill, modifiers, null, balance);

            Assert.That(result.DamageCoefficient, Is.EqualTo(1.44f).Within(0.001f));
            Assert.That(result.Radius, Is.EqualTo(2f));
            Assert.That(result.Range, Is.EqualTo(8f));
            Assert.That(result.Cooldown, Is.EqualTo(4f));
            Assert.That(result.CrowdControl, Is.EqualTo(CrowdControlType.Stun));
            Assert.That(result.PointCost, Is.EqualTo(52));
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void SkillDraftApplier_AppliesSelectedAttackType()
        {
            SkillBalanceProfile balance = CreateBalance();
            SkillDefinition baseSkill = CreateSkill(SkillSlot.Q, SkillType.Projectile);
            var draft = new SkillDraft(
                new SkillPointModifiers(),
                SkillElement.Fire,
                SkillType.SelfArea);

            SkillDefinition result = SkillDraftApplier.Apply(
                baseSkill, draft, null, balance);

            Assert.That(result.Type, Is.EqualTo(SkillType.SelfArea));
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void SkillDraftApplier_AppliesSelectedUtilityEffects()
        {
            SkillBalanceProfile balance = CreateBalance();
            SkillDefinition baseSkill = CreateSkill(SkillSlot.Q, SkillType.Projectile);
            var modifiers = new SkillPointModifiers(
                addsMobility: true,
                createsShield: true,
                heals: true);

            SkillDefinition result = SkillDraftApplier.Apply(
                baseSkill, modifiers, null, balance);

            Assert.That(result.AddsMobility, Is.True);
            Assert.That(result.CreatesShield, Is.True);
            Assert.That(result.Heals, Is.True);
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void SkillDraftApplier_PreservesTankNonUltimateRangeRule()
        {
            SkillBalanceProfile balance = CreateBalance();
            TankRoleDefinition tank = ScriptableObject.CreateInstance<TankRoleDefinition>();
            SkillDefinition baseSkill = new SkillDefinition(
                "Tank Q", SkillSlot.Q, SkillType.Cone,
                1.2f, 4f, 0.3f, 3f, 1.5f, 0f,
                CrowdControlType.None, SkillElement.Earth, 0);
            var modifiers = new SkillPointModifiers(rangeIncrease: 5f);

            SkillDefinition result = SkillDraftApplier.Apply(
                baseSkill, modifiers, tank, balance);

            Assert.That(result.Range, Is.EqualTo(balance.TankMaximumNonUltimateRange));
            Object.DestroyImmediate(tank);
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void CrowdControlState_AppliesBaselineMovementAndActionRules()
        {
            var state = new CrowdControlState();
            state.Apply(CrowdControlType.Slow, 10f, 2f);
            state.Apply(CrowdControlType.Root, 10f, 1.5f);

            Assert.That(state.GetMovementSpeedMultiplier(11f, 0.7f), Is.EqualTo(0.7f));
            Assert.That(state.IsMovementBlocked(11f), Is.True);
            Assert.That(state.IsActionBlocked(11f), Is.False);
            Assert.That(state.IsMovementBlocked(11.6f), Is.False);
            Assert.That(state.GetMovementSpeedMultiplier(12f, 0.7f), Is.EqualTo(1f));
        }

        [Test]
        public void CrowdControlState_StunAndKnockUpBlockActionsForTheirDurations()
        {
            var state = new CrowdControlState();
            state.Apply(CrowdControlType.Stun, 5f, 1f);
            state.Apply(CrowdControlType.KnockUp, 5f, 0.7f);

            Assert.That(state.GetActiveEffect(5.5f), Is.EqualTo(CrowdControlType.Stun));
            Assert.That(state.IsActionBlocked(5.9f), Is.True);
            Assert.That(state.IsActionBlocked(6f), Is.False);
        }

        [Test]
        public void CrowdControlState_ReportsVisibleRemainingDuration()
        {
            var state = new CrowdControlState();
            state.Apply(CrowdControlType.Stun, 10f, 1f);

            Assert.That(state.GetRemainingDuration(10.25f), Is.EqualTo(0.75f));
            Assert.That(state.GetRemainingDuration(11f), Is.Zero);
        }

        [Test]
        public void SkillBalance_UsesRequestedCrowdControlBaselines()
        {
            SkillBalanceProfile balance = CreateBalance();

            Assert.That(balance.SlowMovementMultiplier, Is.EqualTo(0.7f));
            Assert.That(balance.GetCrowdControlDuration(CrowdControlType.Slow), Is.EqualTo(2f));
            Assert.That(balance.GetCrowdControlDuration(CrowdControlType.Root), Is.EqualTo(1.5f));
            Assert.That(balance.GetCrowdControlDuration(CrowdControlType.Stun), Is.EqualTo(1f));
            Assert.That(balance.GetCrowdControlDuration(CrowdControlType.KnockUp), Is.EqualTo(0.7f));
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void ElementDamageOverTime_AppliesFourBurnTicks()
        {
            var state = new ElementDamageOverTimeState();
            state.ApplyBurn(10f, 4f, 1f, 8f);

            Assert.That(state.ConsumeDamage(14f), Is.EqualTo(32f));
            Assert.That(state.ConsumeDamage(15f), Is.Zero);
        }

        [Test]
        public void ElementDamageOverTime_AppliesFivePoisonTicks()
        {
            var state = new ElementDamageOverTimeState();
            state.ApplyPoison(2f, 5f, 1f, 10f);

            Assert.That(state.ConsumeDamage(7f), Is.EqualTo(50f));
        }

        [Test]
        public void SkillBalance_UsesRequestedElementBaselines()
        {
            SkillBalanceProfile balance = CreateBalance();

            Assert.That(balance.BurnDuration, Is.EqualTo(4f));
            Assert.That(balance.BurnAttackCoefficient, Is.EqualTo(0.08f));
            Assert.That(balance.PoisonDuration, Is.EqualTo(5f));
            Assert.That(balance.PoisonMaximumHealthCoefficient, Is.EqualTo(0.01f));
            Assert.That(balance.GetElementDamageMultiplier(SkillElement.Lightning), Is.EqualTo(1.2f));
            Assert.That(balance.WaterHealingRatio, Is.EqualTo(0.1f));
            Assert.That(balance.ElementMarkDuration, Is.EqualTo(5f));
            Assert.That(balance.FireWaterDamageMultiplier, Is.EqualTo(1.25f));
            Assert.That(balance.WaterIceDamageMultiplier, Is.EqualTo(1.15f));
            Assert.That(balance.FireIceDamageMultiplier, Is.EqualTo(1.35f));
            Assert.That(balance.WaterLightningDuration, Is.EqualTo(3f));
            Assert.That(balance.WaterLightningAttackCoefficient, Is.EqualTo(0.06f));
            Assert.That(balance.WaterLightningStunDuration, Is.EqualTo(0.5f));
            Assert.That(balance.IceLightningDamageTakenMultiplier, Is.EqualTo(1.2f));
            Assert.That(balance.IceLightningDuration, Is.EqualTo(4f));
            Assert.That(balance.FirePoisonBurnMultiplier, Is.EqualTo(1.5f));
            Assert.That(balance.WindSpreadRadius, Is.EqualTo(5f));
            Assert.That(balance.EarthShieldMaximumHealthRatio, Is.EqualTo(0.15f));
            Assert.That(balance.EarthShieldDuration, Is.EqualTo(5f));
            Assert.That(balance.EarthAttackPowerMultiplier, Is.EqualTo(0.85f));
            Assert.That(balance.EarthAttackReductionDuration, Is.EqualTo(4f));
            Object.DestroyImmediate(balance);
        }

        [TestCase(
            SkillElement.Fire,
            SkillElement.Water,
            ElementReactionType.FireWater,
            1.25f,
            CrowdControlType.None)]
        [TestCase(
            SkillElement.Water,
            SkillElement.Ice,
            ElementReactionType.WaterIce,
            1.15f,
            CrowdControlType.Root)]
        [TestCase(
            SkillElement.Fire,
            SkillElement.Ice,
            ElementReactionType.FireIce,
            1.35f,
            CrowdControlType.None)]
        [TestCase(
            SkillElement.Water,
            SkillElement.Lightning,
            ElementReactionType.WaterLightning,
            1f,
            CrowdControlType.Stun)]
        [TestCase(
            SkillElement.Ice,
            SkillElement.Lightning,
            ElementReactionType.IceLightning,
            1f,
            CrowdControlType.None)]
        [TestCase(
            SkillElement.Fire,
            SkillElement.Poison,
            ElementReactionType.FirePoison,
            1f,
            CrowdControlType.None)]
        public void ElementReactionResolver_UsesRequestedDamageReactions(
            SkillElement first,
            SkillElement second,
            ElementReactionType expectedType,
            float expectedMultiplier,
            CrowdControlType expectedCrowdControl)
        {
            ElementReactionResult result = ElementReactionResolver.Resolve(
                first,
                second,
                1.25f,
                1.15f,
                1.35f);

            Assert.That(result.Type, Is.EqualTo(expectedType));
            Assert.That(result.DamageMultiplier, Is.EqualTo(expectedMultiplier));
            Assert.That(result.CrowdControl, Is.EqualTo(expectedCrowdControl));
        }

        [Test]
        public void ElementDamageOverTime_AppliesWaterLightningTicks()
        {
            var state = new ElementDamageOverTimeState();
            state.ApplyLightning(10f, 3f, 1f, 6f);

            Assert.That(state.ConsumeDamage(13f), Is.EqualTo(18f));
            Assert.That(state.ConsumeDamage(14f), Is.Zero);
        }

        [Test]
        public void ElementDamageOverTime_FirePoisonAmplifiesActiveBurn()
        {
            var state = new ElementDamageOverTimeState();
            state.ApplyBurn(10f, 4f, 1f, 8f);
            state.MultiplyActiveBurnDamage(10.5f, 1.5f);

            Assert.That(state.ConsumeDamage(14f), Is.EqualTo(48f));
        }

        [Test]
        public void ElementReactionResolver_WindSpreadsPairedElement()
        {
            ElementReactionResult result = ElementReactionResolver.Resolve(
                SkillElement.Wind,
                SkillElement.Fire,
                1.25f,
                1.15f,
                1.35f);

            Assert.That(result.Type, Is.EqualTo(ElementReactionType.WindSpread));
            Assert.That(result.SpreadElement, Is.EqualTo(SkillElement.Fire));
        }

        [Test]
        public void ElementReactionResolver_EarthCreatesWard()
        {
            ElementReactionResult result = ElementReactionResolver.Resolve(
                SkillElement.Ice,
                SkillElement.Earth,
                1.25f,
                1.15f,
                1.35f);

            Assert.That(result.Type, Is.EqualTo(ElementReactionType.EarthWard));
            Assert.That(result.SpreadElement, Is.Null);
        }

        [Test]
        public void AttackPowerModifier_ExpiresAndCanBeCleared()
        {
            var state = new AttackPowerModifierState();
            state.Apply(0.85f, 10f, 4f);

            Assert.That(state.GetMultiplier(13f), Is.EqualTo(0.85f));
            Assert.That(state.GetMultiplier(14f), Is.EqualTo(1f));

            state.Apply(0.85f, 20f, 4f);
            state.Clear();
            Assert.That(state.GetMultiplier(21f), Is.EqualTo(1f));
        }

        [Test]
        public void SkillBalance_UsesUtilityEffectBaselines()
        {
            SkillBalanceProfile balance = CreateBalance();

            Assert.That(balance.SkillMobilityDistance, Is.EqualTo(4f));
            Assert.That(balance.SkillShieldMaximumHealthRatio, Is.EqualTo(0.15f));
            Assert.That(balance.SkillShieldDuration, Is.EqualTo(5f));
            Assert.That(balance.SkillHealingMaximumHealthRatio, Is.EqualTo(0.15f));
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void ShieldState_AbsorbsDamageBeforeExpiring()
        {
            var shield = new ShieldState();
            shield.Apply(150f, 10f, 5f);

            Assert.That(shield.Absorb(80f, 11f), Is.Zero);
            Assert.That(shield.GetAmount(11f), Is.EqualTo(70f));
            Assert.That(shield.Absorb(100f, 12f), Is.EqualTo(30f));
            Assert.That(shield.GetAmount(15f), Is.Zero);
        }

        [Test]
        public void MageProjectile_WithSupportedValues_IsValid()
        {
            SkillBalanceProfile balance = CreateBalance();
            MageRoleDefinition mage = ScriptableObject.CreateInstance<MageRoleDefinition>();
            SkillDefinition skill = CreateSkill(SkillSlot.Q, SkillType.Projectile);

            SkillValidationResult result = SkillValidator.Validate(skill, mage, balance);

            Assert.That(result.IsValid, Is.True, string.Join("\n", result.Errors));
            Object.DestroyImmediate(balance);
            Object.DestroyImmediate(mage);
        }

        [Test]
        public void UnsupportedCastDelay_IsRejected()
        {
            SkillBalanceProfile balance = CreateBalance();
            SkillDefinition skill = new SkillDefinition(
                "Invalid Delay", SkillSlot.Q, SkillType.Projectile,
                1.2f, 5f, 0.5f, 6f, 0.5f, 10f,
                CrowdControlType.None, SkillElement.Fire, 1);

            SkillValidationResult result = SkillValidator.Validate(skill, null, balance);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(error => error.Contains("Cast delay")), Is.True);
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void Tank_NonUltimateProjectile_IsRejected()
        {
            SkillBalanceProfile balance = CreateBalance();
            TankRoleDefinition tank = ScriptableObject.CreateInstance<TankRoleDefinition>();

            SkillValidationResult normal = SkillValidator.Validate(
                CreateSkill(SkillSlot.Q, SkillType.Projectile), tank, balance);
            SkillValidationResult ultimate = SkillValidator.Validate(
                CreateSkill(SkillSlot.Ultimate, SkillType.Projectile), tank, balance);

            Assert.That(normal.IsValid, Is.False);
            Assert.That(ultimate.IsValid, Is.True, string.Join("\n", ultimate.Errors));
            Object.DestroyImmediate(balance);
            Object.DestroyImmediate(tank);
        }

        [Test]
        public void Assassin_AreaPlanClampsRadius()
        {
            AssassinRoleDefinition assassin = ScriptableObject.CreateInstance<AssassinRoleDefinition>();
            SkillDefinition skill = CreateSkill(SkillSlot.E, SkillType.GroundArea, radius: 8f);

            bool created = SkillCastPlanner.TryCreate(
                skill,
                assassin,
                Vector3.zero,
                Vector3.forward * 3f,
                out SkillCastPlan plan);

            Assert.That(created, Is.True);
            Assert.That(plan.Radius, Is.EqualTo(3f));
            Object.DestroyImmediate(assassin);
        }

        [TestCase(SkillType.SelfArea, 1f, 1.2f)]
        [TestCase(SkillType.SelfArea, 3f, 1f)]
        [TestCase(SkillType.SelfArea, 5f, 0.8f)]
        [TestCase(SkillType.GroundArea, 1f, 1f)]
        [TestCase(SkillType.GroundArea, 3f, 0.8f)]
        [TestCase(SkillType.GroundArea, 5f, 0.6f)]
        public void AreaFalloff_UsesSteppedMultiplier(
            SkillType type,
            float distance,
            float expected)
        {
            Assert.That(SkillAreaFalloff.GetMultiplier(type, distance), Is.EqualTo(expected));
        }

        [Test]
        public void Mage_AreaSkillDamageUsesRoleCap()
        {
            SkillBalanceProfile balance = CreateBalance();
            MageRoleDefinition mage = ScriptableObject.CreateInstance<MageRoleDefinition>();
            SkillDefinition skill = CreateSkill(
                SkillSlot.E,
                SkillType.GroundArea,
                coefficient: 3f);

            DamageRequest request = SkillRuntimeRules.CreateDamageRequest(
                null, 500f, skill, mage, balance, 0f);

            Assert.That(DamageCalculator.Calculate(request), Is.EqualTo(300f));
            Object.DestroyImmediate(balance);
            Object.DestroyImmediate(mage);
        }

        [Test]
        public void Loadout_RejectsDuplicateSlotsAndExcessPointCost()
        {
            SkillBalanceProfile balance = CreateBalance();
            SkillDefinition first = CreateSkill(SkillSlot.Q, SkillType.Projectile, pointCost: 60);
            SkillDefinition second = CreateSkill(SkillSlot.Q, SkillType.Projectile, pointCost: 60);

            SkillValidationResult result = SkillLoadoutValidator.Validate(
                new[] { first, second }, null, balance);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(error => error.Contains("assigned more than once")), Is.True);
            Assert.That(result.Errors.Any(error => error.Contains("Total skill point")), Is.True);
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void CastPlanner_RejectsAimBeyondRange()
        {
            SkillDefinition skill = CreateSkill(SkillSlot.Q, SkillType.Projectile);

            bool created = SkillCastPlanner.TryCreate(
                skill, null, Vector3.zero, Vector3.forward * 10f, out _);

            Assert.That(created, Is.False);
        }

        [Test]
        public void GlobalCastPlanner_DoesNotRequireAimOrRange()
        {
            SkillDefinition skill = CreateSkill(SkillSlot.Q, SkillType.Global);

            bool created = SkillCastPlanner.TryCreate(
                skill, null, Vector3.zero, Vector3.zero, out SkillCastPlan plan);

            Assert.That(created, Is.True);
            Assert.That(plan.Type, Is.EqualTo(SkillType.Global));
            Assert.That(SkillRuntimeRules.IsArea(SkillType.Global), Is.True);
        }

        [Test]
        public void CooldownTracker_BlocksUntilReady()
        {
            var cooldown = new SkillCooldownTracker();

            Assert.That(cooldown.TryStart(SkillSlot.Q, 10f, 5f), Is.True);
            Assert.That(cooldown.TryStart(SkillSlot.Q, 14.9f, 5f), Is.False);
            Assert.That(cooldown.GetRemaining(SkillSlot.Q, 14f), Is.EqualTo(1f));
            Assert.That(cooldown.TryStart(SkillSlot.Q, 15f, 5f), Is.True);
        }

        private static SkillBalanceProfile CreateBalance()
        {
            return ScriptableObject.CreateInstance<SkillBalanceProfile>();
        }

        private static SkillDefinition CreateSkill(
            SkillSlot slot,
            SkillType type,
            float coefficient = 1.2f,
            float radius = 1f,
            int pointCost = 1)
        {
            return new SkillDefinition(
                "Test Skill",
                slot,
                type,
                coefficient,
                5f,
                0.3f,
                6f,
                radius,
                10f,
                CrowdControlType.None,
                SkillElement.Fire,
                pointCost);
        }
    }
}
