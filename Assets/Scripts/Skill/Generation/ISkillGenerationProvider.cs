using System.Threading;
using System.Threading.Tasks;
using Axiom.Role;

namespace Axiom.Skill.Generation
{
    public interface ISkillGenerationProvider
    {
        Task<SkillGenerationResponseDto> GenerateAsync(
            string prompt,
            CharacterRoleId role,
            SkillSlot slot,
            CancellationToken cancellationToken = default);
    }
}
