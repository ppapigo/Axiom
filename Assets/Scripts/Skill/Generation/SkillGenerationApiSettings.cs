using UnityEngine;

namespace Axiom.Skill.Generation
{
    [CreateAssetMenu(
        fileName = "SkillGenerationApiSettings",
        menuName = "Axiom/Skill Generation API Settings")]
    public sealed class SkillGenerationApiSettings : ScriptableObject
    {
        [SerializeField] private bool useServerlessProvider;
        [SerializeField] private string endpointUrl = string.Empty;
        [SerializeField, Min(1)] private int timeoutSeconds = 15;

        public bool UseServerlessProvider => useServerlessProvider;
        public string EndpointUrl => endpointUrl == null ? string.Empty : endpointUrl.Trim();
        public int TimeoutSeconds => Mathf.Max(1, timeoutSeconds);

        public bool HasValidEndpoint =>
            System.Uri.TryCreate(EndpointUrl, System.UriKind.Absolute, out System.Uri uri) &&
            (uri.Scheme == System.Uri.UriSchemeHttps || uri.Scheme == System.Uri.UriSchemeHttp);

        public void Configure(bool enabled, string endpoint, int timeout = 15)
        {
            useServerlessProvider = enabled;
            endpointUrl = endpoint ?? string.Empty;
            timeoutSeconds = Mathf.Max(1, timeout);
        }
    }
}
