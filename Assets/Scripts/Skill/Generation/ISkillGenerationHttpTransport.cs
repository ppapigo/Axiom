using System.Threading;
using System.Threading.Tasks;

namespace Axiom.Skill.Generation
{
    public interface ISkillGenerationHttpTransport
    {
        Task<string> PostJsonAsync(
            string endpointUrl,
            string json,
            int timeoutSeconds,
            CancellationToken cancellationToken = default);
    }
}
