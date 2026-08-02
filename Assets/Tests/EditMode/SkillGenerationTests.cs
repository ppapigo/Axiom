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
    }
}
