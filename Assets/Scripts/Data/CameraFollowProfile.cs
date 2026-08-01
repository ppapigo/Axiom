using Axiom.Camera;
using UnityEngine;

namespace Axiom.Data
{
    [CreateAssetMenu(
        fileName = "CameraFollowProfile",
        menuName = "Axiom/Camera/Follow Profile")]
    public sealed class CameraFollowProfile : ScriptableObject
    {
        [Header("Quarter View")]
        [SerializeField, Min(0.01f)] private float height = 10f;
        [SerializeField, Range(1f, 89f)] private float pitchAngle = 50f;
        [SerializeField] private float yawAngle = 45f;
        [SerializeField] private float lookAtHeight = 1f;

        [Header("Follow")]
        [SerializeField, Min(0f)] private float smoothTime = 0.12f;

        public CameraFollowParameters Parameters => new CameraFollowParameters(
            height,
            pitchAngle,
            yawAngle,
            lookAtHeight,
            smoothTime);

        private void OnValidate()
        {
            height = Mathf.Max(0.01f, height);
            pitchAngle = Mathf.Clamp(pitchAngle, 1f, 89f);
            smoothTime = Mathf.Max(0f, smoothTime);
        }
    }
}

