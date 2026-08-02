namespace Axiom.Skill.Generation
{
    public interface ISkillGenerationProviderInfo
    {
        string DisplayName { get; }
        bool UsesRemoteEndpoint { get; }
    }
}
