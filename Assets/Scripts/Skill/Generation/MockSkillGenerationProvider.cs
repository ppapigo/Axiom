using System;
using System.Threading;
using System.Threading.Tasks;
using Axiom.Role;

namespace Axiom.Skill.Generation
{
    public sealed class MockSkillGenerationProvider : ISkillGenerationProvider
    {
        public Task<SkillGenerationResponseDto> GenerateAsync(
            string prompt,
            CharacterRoleId role,
            SkillSlot slot,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string normalizedPrompt = (prompt ?? string.Empty).Trim();
            SkillGenerationResponseDto response = CreateRolePreset(role, slot);
            ApplyPromptKeywords(response, normalizedPrompt);
            response.description = string.IsNullOrWhiteSpace(normalizedPrompt)
                ? "안전한 역할별 기본 스킬 초안"
                : normalizedPrompt;
            return Task.FromResult(response);
        }

        private static SkillGenerationResponseDto CreateRolePreset(
            CharacterRoleId role,
            SkillSlot slot)
        {
            string slotName = slot.ToString();
            return role switch
            {
                CharacterRoleId.Tank => new SkillGenerationResponseDto
                {
                    displayName = $"Aegis {slotName}",
                    skillType = nameof(SkillType.SelfArea),
                    crowdControl = nameof(CrowdControlType.Stun),
                    element = nameof(SkillElement.Earth),
                    damageIncreasePercent = 10f,
                    radiusIncrease = 1f,
                    createsShield = true
                },
                CharacterRoleId.Assassin => new SkillGenerationResponseDto
                {
                    displayName = $"Shadow {slotName}",
                    skillType = nameof(SkillType.Target),
                    crowdControl = nameof(CrowdControlType.None),
                    element = nameof(SkillElement.Poison),
                    damageIncreasePercent = 20f,
                    rangeIncrease = 1f,
                    addsMobility = true
                },
                _ => new SkillGenerationResponseDto
                {
                    displayName = $"Arcane {slotName}",
                    skillType = nameof(SkillType.GroundArea),
                    crowdControl = nameof(CrowdControlType.Slow),
                    element = nameof(SkillElement.Fire),
                    damageIncreasePercent = 20f,
                    radiusIncrease = 1f,
                    rangeIncrease = 1f,
                    cooldownReduction = 1f
                }
            };
        }

        private static void ApplyPromptKeywords(
            SkillGenerationResponseDto response,
            string prompt)
        {
            if (Contains(prompt, "불", "화염", "fire"))
            {
                response.element = nameof(SkillElement.Fire);
            }
            else if (Contains(prompt, "얼음", "빙결", "ice"))
            {
                response.element = nameof(SkillElement.Ice);
            }
            else if (Contains(prompt, "번개", "전기", "lightning"))
            {
                response.element = nameof(SkillElement.Lightning);
            }
            else if (Contains(prompt, "독", "poison"))
            {
                response.element = nameof(SkillElement.Poison);
            }
            else if (Contains(prompt, "물", "water"))
            {
                response.element = nameof(SkillElement.Water);
            }
            else if (Contains(prompt, "바람", "wind"))
            {
                response.element = nameof(SkillElement.Wind);
            }
            else if (Contains(prompt, "대지", "earth"))
            {
                response.element = nameof(SkillElement.Earth);
            }

            if (Contains(prompt, "전체", "global"))
            {
                response.skillType = nameof(SkillType.Global);
            }
            else if (Contains(prompt, "자기 중심", "self area"))
            {
                response.skillType = nameof(SkillType.SelfArea);
            }
            else if (Contains(prompt, "부채꼴", "cone"))
            {
                response.skillType = nameof(SkillType.Cone);
            }
            else if (Contains(prompt, "폭발", "지역", "ground area"))
            {
                response.skillType = nameof(SkillType.GroundArea);
            }
            else if (Contains(prompt, "투사체", "발사", "projectile"))
            {
                response.skillType = nameof(SkillType.Projectile);
            }

            if (Contains(prompt, "기절", "stun"))
            {
                response.crowdControl = nameof(CrowdControlType.Stun);
            }
            else if (Contains(prompt, "둔화", "느리", "slow"))
            {
                response.crowdControl = nameof(CrowdControlType.Slow);
            }
            else if (Contains(prompt, "에어본", "띄우", "knock up"))
            {
                response.crowdControl = nameof(CrowdControlType.KnockUp);
            }

            response.addsMobility = response.addsMobility ||
                Contains(prompt, "돌진", "이동", "dash");
            response.createsShield = response.createsShield ||
                Contains(prompt, "보호막", "shield");
            response.heals = response.heals ||
                Contains(prompt, "회복", "치유", "heal");
        }

        private static bool Contains(string source, params string[] keywords)
        {
            foreach (string keyword in keywords)
            {
                if (source.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
