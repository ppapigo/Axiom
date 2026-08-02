using System.Threading;
using System.Threading.Tasks;
using Axiom.Role;
using Axiom.Skill;
using Axiom.Skill.Generation;
using NUnit.Framework;

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

        [TestCase(CharacterRoleId.Tank, SkillType.SelfArea, SkillElement.Earth)]
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
    }
}
