using System.Threading;
using System.Threading.Tasks;
using Axiom.Data;
using Axiom.Role;
using Axiom.Skill;
using Axiom.Skill.Generation;
using NUnit.Framework;
using UnityEngine;

namespace Axiom.Tests.EditMode
{
    public sealed class SkillGenerationTests
    {
        [Test]
        public async Task MockProvider_UsesPromptKeywords()
        {
            var provider = new MockSkillGenerationProvider();

            SkillGenerationResponseDto result = await provider.GenerateAsync(
                "폭발하는 얼음 투사체로 적을 기절시키고 싶어",
                CharacterRoleId.Mage,
                SkillSlot.Q);

            Assert.That(result.element, Is.EqualTo(nameof(SkillElement.Ice)));
            Assert.That(result.skillType, Is.EqualTo(nameof(SkillType.GroundArea)));
            Assert.That(result.crowdControl, Is.EqualTo(nameof(CrowdControlType.Stun)));
            Assert.That(result.description, Does.Contain("얼음"));
        }

        [TestCase(CharacterRoleId.Tank, SkillType.Cone, SkillElement.Earth)]
        [TestCase(CharacterRoleId.Mage, SkillType.GroundArea, SkillElement.Fire)]
        [TestCase(CharacterRoleId.Assassin, SkillType.Target, SkillElement.Poison)]
        public async Task MockProvider_ReturnsSafeRolePreset(
            CharacterRoleId role,
            SkillType expectedType,
            SkillElement expectedElement)
        {
            var provider = new MockSkillGenerationProvider();

            SkillGenerationResponseDto result = await provider.GenerateAsync(
                string.Empty,
                role,
                SkillSlot.E);

            Assert.That(result.skillType, Is.EqualTo(expectedType.ToString()));
            Assert.That(result.element, Is.EqualTo(expectedElement.ToString()));
            Assert.That(result.displayName, Does.Contain("E"));
        }

        [Test]
        public void MockProvider_HonorsCancellation()
        {
            var provider = new MockSkillGenerationProvider();
            var cancellation = new CancellationToken(canceled: true);

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await provider.GenerateAsync(
                    "불 스킬",
                    CharacterRoleId.Mage,
                    SkillSlot.Q,
                    cancellation));
        }

        [Test]
        public void SkillDraftMapper_MapsSupportedResponse()
        {
            var response = new SkillGenerationResponseDto
            {
                skillType = "Ground Area",
                crowdControl = "Stun",
                element = "Fire",
                damageIncreasePercent = 20f,
                radiusIncrease = 2f,
                rangeIncrease = 1f,
                cooldownReduction = 1f,
                addsMobility = true,
                createsShield = true,
                heals = true
            };

            SkillDraftMappingResult result = SkillDraftMapper.Map(response, SkillSlot.Q);

            Assert.That(result.IsSuccess, Is.True, string.Join("\n", result.Errors));
            Assert.That(result.Draft.Type, Is.EqualTo(SkillType.GroundArea));
            Assert.That(result.Draft.Element, Is.EqualTo(SkillElement.Fire));
            Assert.That(result.Draft.Slot, Is.EqualTo(SkillSlot.Q));
            Assert.That(result.Draft.Modifiers.AppliesStun, Is.True);
            Assert.That(result.Draft.Modifiers.AddsMobility, Is.True);
            Assert.That(result.Draft.Modifiers.CreatesShield, Is.True);
            Assert.That(result.Draft.Modifiers.Heals, Is.True);
        }

        [Test]
        public void SkillDraftMapper_NormalizesEnumNamesAndAllowsNoElement()
        {
            var response = new SkillGenerationResponseDto
            {
                skillType = "self-area",
                crowdControl = "none",
                element = "None"
            };

            SkillDraftMappingResult result = SkillDraftMapper.Map(
                response,
                SkillSlot.Ultimate);

            Assert.That(result.IsSuccess, Is.True, string.Join("\n", result.Errors));
            Assert.That(result.Draft.Type, Is.EqualTo(SkillType.SelfArea));
            Assert.That(result.Draft.Element, Is.Null);
        }

        [Test]
        public void SkillDraftMapper_RejectsUnsupportedEnums()
        {
            var response = new SkillGenerationResponseDto
            {
                skillType = "ChainLaser",
                crowdControl = "Fear",
                element = "Light"
            };

            SkillDraftMappingResult result = SkillDraftMapper.Map(response, SkillSlot.E);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors.Count, Is.EqualTo(3));
        }

        [Test]
        public void SkillDraftMapper_RejectsNonFiniteAndNegativeNumbers()
        {
            var response = new SkillGenerationResponseDto
            {
                skillType = "Projectile",
                crowdControl = "None",
                element = "Ice",
                damageIncreasePercent = -10f,
                radiusIncrease = float.NaN,
                rangeIncrease = float.PositiveInfinity
            };

            SkillDraftMappingResult result = SkillDraftMapper.Map(response, SkillSlot.Q);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors.Count, Is.EqualTo(3));
        }

        [Test]
        public void SkillDraftMapper_RejectsUnavailableForgeCrowdControl()
        {
            var response = new SkillGenerationResponseDto
            {
                skillType = "Target",
                crowdControl = "Taunt",
                element = "Earth"
            };

            SkillDraftMappingResult result = SkillDraftMapper.Map(response, SkillSlot.Q);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors[0], Does.Contain("not available"));
        }

        [Test]
        public void SkillRuleValidator_AcceptsValidMageDraft()
        {
            SkillBalanceProfile balance = ScriptableObject.CreateInstance<SkillBalanceProfile>();
            MageRoleDefinition role = ScriptableObject.CreateInstance<MageRoleDefinition>();
            var pool = new RoleElementPool();
            SkillDefinition baseDefinition = CreateBaseDefinition(
                SkillSlot.Q,
                SkillType.Projectile,
                range: 7f,
                radius: 1f);
            var draft = new SkillDraft(
                new SkillPointModifiers(damageIncreasePercent: 20f, rangeIncrease: 1f),
                SkillElement.Fire,
                SkillType.Projectile,
                SkillSlot.Q);

            SkillRuleValidationResult result = SkillRuleValidator.Validate(
                draft,
                baseDefinition,
                role,
                balance,
                pool);

            Assert.That(result.IsValid, Is.True, string.Join("\n", result.Errors));
            Assert.That(result.HasDefinition, Is.True);
            Assert.That(result.PointCost, Is.EqualTo(25));
            Object.DestroyImmediate(role);
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void SkillRuleValidator_RejectsOverBudgetDraft()
        {
            SkillBalanceProfile balance = ScriptableObject.CreateInstance<SkillBalanceProfile>();
            MageRoleDefinition role = ScriptableObject.CreateInstance<MageRoleDefinition>();
            var draft = new SkillDraft(
                new SkillPointModifiers(damageIncreasePercent: 300f),
                SkillElement.Fire,
                SkillType.Projectile,
                SkillSlot.Q);

            SkillRuleValidationResult result = SkillRuleValidator.Validate(
                draft,
                CreateBaseDefinition(SkillSlot.Q, SkillType.Projectile),
                role,
                balance,
                new RoleElementPool());

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.PointCost, Is.GreaterThan(balance.LoadoutPointBudget));
            Assert.That(string.Join(" ", result.Errors), Does.Contain("budget"));
            Object.DestroyImmediate(role);
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void SkillRuleValidator_RejectsMultipleCrowdControls()
        {
            SkillBalanceProfile balance = ScriptableObject.CreateInstance<SkillBalanceProfile>();
            MageRoleDefinition role = ScriptableObject.CreateInstance<MageRoleDefinition>();
            var modifiers = new SkillPointModifiers(
                appliesSlow: true,
                appliesStun: true);
            var draft = new SkillDraft(
                modifiers,
                SkillElement.Ice,
                SkillType.Projectile,
                SkillSlot.E);

            SkillRuleValidationResult result = SkillRuleValidator.Validate(
                draft,
                CreateBaseDefinition(SkillSlot.E, SkillType.Projectile),
                role,
                balance,
                new RoleElementPool());

            Assert.That(result.IsValid, Is.False);
            Assert.That(string.Join(" ", result.Errors), Does.Contain("at most one"));
            Object.DestroyImmediate(role);
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void SkillRuleValidator_RejectsThirdRoleElement()
        {
            SkillBalanceProfile balance = ScriptableObject.CreateInstance<SkillBalanceProfile>();
            MageRoleDefinition role = ScriptableObject.CreateInstance<MageRoleDefinition>();
            var pool = new RoleElementPool();
            pool.TryAssign(CharacterRoleId.Mage, SkillSlot.Q, SkillElement.Fire);
            pool.TryAssign(CharacterRoleId.Mage, SkillSlot.E, SkillElement.Ice);
            var draft = new SkillDraft(
                new SkillPointModifiers(),
                SkillElement.Lightning,
                SkillType.Projectile,
                SkillSlot.Ultimate);

            SkillRuleValidationResult result = SkillRuleValidator.Validate(
                draft,
                CreateBaseDefinition(SkillSlot.Ultimate, SkillType.Projectile),
                role,
                balance,
                pool);

            Assert.That(result.IsValid, Is.False);
            Assert.That(string.Join(" ", result.Errors), Does.Contain("two distinct elements"));
            Object.DestroyImmediate(role);
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void SkillRuleValidator_RejectsTankRangedSkill()
        {
            SkillBalanceProfile balance = ScriptableObject.CreateInstance<SkillBalanceProfile>();
            TankRoleDefinition role = ScriptableObject.CreateInstance<TankRoleDefinition>();
            var draft = new SkillDraft(
                new SkillPointModifiers(rangeIncrease: 5f),
                SkillElement.Earth,
                SkillType.Projectile,
                SkillSlot.Q);

            SkillRuleValidationResult result = SkillRuleValidator.Validate(
                draft,
                CreateBaseDefinition(SkillSlot.Q, SkillType.Cone, range: 3f),
                role,
                balance,
                new RoleElementPool());

            Assert.That(result.IsValid, Is.False);
            Assert.That(string.Join(" ", result.Errors), Does.Contain("range"));
            Object.DestroyImmediate(role);
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void SkillRuleValidator_RejectsAssassinAreaBeyondLimit()
        {
            SkillBalanceProfile balance = ScriptableObject.CreateInstance<SkillBalanceProfile>();
            AssassinRoleDefinition role = ScriptableObject.CreateInstance<AssassinRoleDefinition>();
            var draft = new SkillDraft(
                new SkillPointModifiers(radiusIncrease: 4f),
                SkillElement.Poison,
                SkillType.SelfArea,
                SkillSlot.E);

            SkillRuleValidationResult result = SkillRuleValidator.Validate(
                draft,
                CreateBaseDefinition(
                    SkillSlot.E,
                    SkillType.SelfArea,
                    radius: 1f),
                role,
                balance,
                new RoleElementPool());

            Assert.That(result.IsValid, Is.False);
            Assert.That(string.Join(" ", result.Errors), Does.Contain("radius"));
            Object.DestroyImmediate(role);
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void SkillAutoCorrector_LeavesValidDraftUnchanged()
        {
            SkillBalanceProfile balance = ScriptableObject.CreateInstance<SkillBalanceProfile>();
            MageRoleDefinition role = ScriptableObject.CreateInstance<MageRoleDefinition>();
            var draft = new SkillDraft(
                new SkillPointModifiers(damageIncreasePercent: 20f),
                SkillElement.Fire,
                SkillType.Projectile,
                SkillSlot.Q);
            SkillDefinition baseDefinition = CreateBaseDefinition(
                SkillSlot.Q,
                SkillType.Projectile);

            SkillAutoCorrectionResult result = SkillAutoCorrector.Correct(
                draft,
                baseDefinition,
                role,
                balance,
                new RoleElementPool());

            Assert.That(result.WasCorrected, Is.False);
            Assert.That(result.UsedFallback, Is.False);
            Assert.That(result.Validation.IsValid, Is.True);
            Assert.That(result.Draft.Modifiers.DamageIncreasePercent, Is.EqualTo(20f));
            Object.DestroyImmediate(role);
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void SkillAutoCorrector_TrimsOverBudgetDraft()
        {
            SkillBalanceProfile balance = ScriptableObject.CreateInstance<SkillBalanceProfile>();
            MageRoleDefinition role = ScriptableObject.CreateInstance<MageRoleDefinition>();
            var draft = new SkillDraft(
                new SkillPointModifiers(
                    damageIncreasePercent: 500f,
                    radiusIncrease: 10f,
                    rangeIncrease: 10f,
                    cooldownReduction: 10f,
                    appliesStun: true,
                    addsMobility: true,
                    createsShield: true,
                    heals: true),
                SkillElement.Lightning,
                SkillType.Global,
                SkillSlot.Ultimate);

            SkillAutoCorrectionResult result = SkillAutoCorrector.Correct(
                draft,
                CreateBaseDefinition(SkillSlot.Ultimate, SkillType.Projectile),
                role,
                balance,
                new RoleElementPool());

            Assert.That(result.WasCorrected, Is.True);
            Assert.That(result.Validation.IsValid, Is.True,
                string.Join("\n", result.Validation.Errors));
            Assert.That(result.Validation.PointCost,
                Is.LessThanOrEqualTo(balance.LoadoutPointBudget));
            Assert.That(result.Changes.Count, Is.GreaterThan(0));
            Object.DestroyImmediate(role);
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void SkillAutoCorrector_KeepsOnlyHighestPriorityCrowdControl()
        {
            SkillBalanceProfile balance = ScriptableObject.CreateInstance<SkillBalanceProfile>();
            MageRoleDefinition role = ScriptableObject.CreateInstance<MageRoleDefinition>();
            var draft = new SkillDraft(
                new SkillPointModifiers(
                    appliesSlow: true,
                    appliesStun: true,
                    appliesKnockUp: true),
                SkillElement.Ice,
                SkillType.Projectile,
                SkillSlot.E);

            SkillAutoCorrectionResult result = SkillAutoCorrector.Correct(
                draft,
                CreateBaseDefinition(SkillSlot.E, SkillType.Projectile),
                role,
                balance,
                new RoleElementPool());

            Assert.That(result.Validation.IsValid, Is.True,
                string.Join("\n", result.Validation.Errors));
            Assert.That(result.Draft.Modifiers.AppliesStun, Is.True);
            Assert.That(result.Draft.Modifiers.AppliesSlow, Is.False);
            Assert.That(result.Draft.Modifiers.AppliesKnockUp, Is.False);
            Object.DestroyImmediate(role);
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void SkillAutoCorrector_RemovesThirdRoleElement()
        {
            SkillBalanceProfile balance = ScriptableObject.CreateInstance<SkillBalanceProfile>();
            MageRoleDefinition role = ScriptableObject.CreateInstance<MageRoleDefinition>();
            var pool = new RoleElementPool();
            pool.TryAssign(CharacterRoleId.Mage, SkillSlot.Q, SkillElement.Fire);
            pool.TryAssign(CharacterRoleId.Mage, SkillSlot.E, SkillElement.Ice);
            var draft = new SkillDraft(
                new SkillPointModifiers(),
                SkillElement.Lightning,
                SkillType.Projectile,
                SkillSlot.Ultimate);

            SkillAutoCorrectionResult result = SkillAutoCorrector.Correct(
                draft,
                CreateBaseDefinition(SkillSlot.Ultimate, SkillType.Projectile),
                role,
                balance,
                pool);

            Assert.That(result.Validation.IsValid, Is.True,
                string.Join("\n", result.Validation.Errors));
            Assert.That(result.Draft.Element, Is.Null);
            Object.DestroyImmediate(role);
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void SkillAutoCorrector_ConvertsTankSkillToMeleeCone()
        {
            SkillBalanceProfile balance = ScriptableObject.CreateInstance<SkillBalanceProfile>();
            TankRoleDefinition role = ScriptableObject.CreateInstance<TankRoleDefinition>();
            var draft = new SkillDraft(
                new SkillPointModifiers(rangeIncrease: 8f),
                SkillElement.Earth,
                SkillType.Global,
                SkillSlot.Q);

            SkillAutoCorrectionResult result = SkillAutoCorrector.Correct(
                draft,
                CreateBaseDefinition(
                    SkillSlot.Q,
                    SkillType.Cone,
                    range: 3f,
                    radius: 1f),
                role,
                balance,
                new RoleElementPool());

            Assert.That(result.Validation.IsValid, Is.True,
                string.Join("\n", result.Validation.Errors));
            Assert.That(result.Draft.Type, Is.EqualTo(SkillType.Cone));
            Assert.That(result.Draft.Modifiers.RangeIncrease, Is.Zero);
            Assert.That(result.UsedFallback, Is.False);
            Object.DestroyImmediate(role);
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void SkillAutoCorrector_ReturnsRoleFallbackWhenBaseIsInvalid()
        {
            SkillBalanceProfile balance = ScriptableObject.CreateInstance<SkillBalanceProfile>();
            AssassinRoleDefinition role = ScriptableObject.CreateInstance<AssassinRoleDefinition>();
            var invalidBase = new SkillDefinition(
                string.Empty,
                SkillSlot.Q,
                SkillType.Projectile,
                1f,
                5f,
                0.3f,
                6f,
                1f,
                12f,
                CrowdControlType.None,
                SkillElement.Poison,
                0);
            var draft = new SkillDraft(
                new SkillPointModifiers(),
                SkillElement.Poison,
                SkillType.Projectile,
                SkillSlot.Q);

            SkillAutoCorrectionResult result = SkillAutoCorrector.Correct(
                draft,
                invalidBase,
                role,
                balance,
                new RoleElementPool());

            Assert.That(result.UsedFallback, Is.True);
            Assert.That(result.Draft.Type, Is.EqualTo(SkillType.Target));
            Assert.That(result.Changes[result.Changes.Count - 1], Does.Contain("fallback"));
            Object.DestroyImmediate(role);
            Object.DestroyImmediate(balance);
        }

        private static SkillDefinition CreateBaseDefinition(
            SkillSlot slot,
            SkillType type,
            float range = 6f,
            float radius = 1f)
        {
            return new SkillDefinition(
                "Generated Base",
                slot,
                type,
                1.2f,
                5f,
                0.3f,
                range,
                radius,
                12f,
                CrowdControlType.None,
                SkillElement.Fire,
                0);
        }
    }
}
