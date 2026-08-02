namespace Axiom.Skill.Generation
{
    public static class SkillGenerationProviderFactory
    {
        public static ISkillGenerationProvider Create(
            SkillGenerationApiSettings settings,
            ISkillGenerationHttpTransport transport = null)
        {
            if (settings == null ||
                !settings.UseServerlessProvider ||
                !settings.HasValidEndpoint)
            {
                return new MockSkillGenerationProvider();
            }

            return new ServerlessSkillGenerationProvider(
                settings.EndpointUrl,
                settings.TimeoutSeconds,
                transport);
        }
    }
}
