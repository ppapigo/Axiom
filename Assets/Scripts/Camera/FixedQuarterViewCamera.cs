using Axiom.Data;
using UnityEngine;

namespace Axiom.Camera
{
    [DisallowMultipleComponent]
    public sealed class FixedQuarterViewCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private CameraFollowProfile followProfile;
        [SerializeField] private bool snapOnEnable = true;

        private Vector3 _followVelocity;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            _followVelocity = Vector3.zero;
            SnapToTarget();
        }

        private void OnEnable()
        {
            _followVelocity = Vector3.zero;

            if (snapOnEnable)
            {
                SnapToTarget();
            }
        }

        private void LateUpdate()
        {
            if (!TryGetDesiredPose(out CameraPose desiredPose, out float smoothTime))
            {
                return;
            }

            transform.position = smoothTime <= 0f
                ? desiredPose.Position
                : Vector3.SmoothDamp(
                    transform.position,
                    desiredPose.Position,
                    ref _followVelocity,
                    smoothTime);

            transform.rotation = desiredPose.Rotation;
        }

        private void SnapToTarget()
        {
            if (!TryGetDesiredPose(out CameraPose desiredPose, out _))
            {
                return;
            }

            transform.SetPositionAndRotation(desiredPose.Position, desiredPose.Rotation);
        }

        private bool TryGetDesiredPose(out CameraPose pose, out float smoothTime)
        {
            if (target == null || followProfile == null)
            {
                pose = default;
                smoothTime = 0f;
                return false;
            }

            CameraFollowParameters parameters = followProfile.Parameters;
            pose = QuarterViewCameraPoseCalculator.Calculate(target.position, parameters);
            smoothTime = parameters.SmoothTime;
            return true;
        }
    }
}

