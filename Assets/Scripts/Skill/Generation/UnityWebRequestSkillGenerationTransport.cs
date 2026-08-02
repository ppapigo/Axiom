using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Axiom.Skill.Generation
{
    public sealed class UnityWebRequestSkillGenerationTransport : ISkillGenerationHttpTransport
    {
        public async Task<string> PostJsonAsync(
            string endpointUrl,
            string json,
            int timeoutSeconds,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var request = new UnityWebRequest(
                endpointUrl,
                UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(
                    Encoding.UTF8.GetBytes(json ?? string.Empty)),
                downloadHandler = new DownloadHandlerBuffer(),
                timeout = Math.Max(1, timeoutSeconds)
            };
            request.SetRequestHeader("Content-Type", "application/json");

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            using CancellationTokenRegistration registration =
                cancellationToken.Register(request.Abort);
            while (!operation.isDone)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new InvalidOperationException(
                    $"Serverless generation request failed " +
                    $"({request.responseCode}): {request.error}");
            }

            return request.downloadHandler.text;
        }
    }
}
