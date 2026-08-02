using System;
using System.Threading;
using System.Threading.Tasks;
using Axiom.Role;
using UnityEngine;

namespace Axiom.Skill.Generation
{
    public sealed class ServerlessSkillGenerationProvider :
        ISkillGenerationProvider,
        ISkillGenerationProviderInfo
    {
        private readonly string _endpointUrl;
        private readonly int _timeoutSeconds;
        private readonly ISkillGenerationHttpTransport _transport;

        public string DisplayName => "SERVERLESS";
        public bool UsesRemoteEndpoint => true;

        public ServerlessSkillGenerationProvider(
            string endpointUrl,
            int timeoutSeconds = 15,
            ISkillGenerationHttpTransport transport = null)
        {
            if (!Uri.TryCreate(endpointUrl, UriKind.Absolute, out Uri endpoint) ||
                (endpoint.Scheme != Uri.UriSchemeHttps &&
                 endpoint.Scheme != Uri.UriSchemeHttp))
            {
                throw new ArgumentException(
                    "A valid HTTP or HTTPS endpoint is required.",
                    nameof(endpointUrl));
            }

            _endpointUrl = endpoint.AbsoluteUri;
            _timeoutSeconds = Math.Max(1, timeoutSeconds);
            _transport = transport ?? new UnityWebRequestSkillGenerationTransport();
        }

        public async Task<SkillGenerationResponseDto> GenerateAsync(
            string prompt,
            CharacterRoleId role,
            SkillSlot slot,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var request = new RequestDto
            {
                prompt = (prompt ?? string.Empty).Trim(),
                role = role.ToString(),
                slot = slot.ToString()
            };
            string json = JsonUtility.ToJson(request);
            string responseJson = await _transport.PostJsonAsync(
                _endpointUrl,
                json,
                _timeoutSeconds,
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(responseJson))
            {
                throw new InvalidOperationException(
                    "The serverless generation endpoint returned an empty response.");
            }

            try
            {
                return JsonUtility.FromJson<SkillGenerationResponseDto>(responseJson) ??
                    throw new InvalidOperationException(
                        "The serverless response did not contain a skill draft.");
            }
            catch (ArgumentException exception)
            {
                throw new InvalidOperationException(
                    "The serverless response was not valid skill JSON.",
                    exception);
            }
        }

        [Serializable]
        private sealed class RequestDto
        {
            public string prompt;
            public string role;
            public string slot;
        }
    }
}
