using UnityEngine;

namespace Axiom.Camera
{
    public static class QuarterViewCameraPoseCalculator
    {
        public static CameraPose Calculate(
            Vector3 targetPosition,
            in CameraFollowParameters parameters)
        {
            Vector3 focusPosition = targetPosition + (Vector3.up * parameters.LookAtHeight);
            Quaternion rotation = Quaternion.Euler(
                parameters.PitchAngle,
                parameters.YawAngle,
                0f);

            Vector3 forward = rotation * Vector3.forward;
            float distanceToFocus = parameters.Height / -forward.y;
            Vector3 position = focusPosition - (forward * distanceToFocus);

            return new CameraPose(position, rotation);
        }
    }
}

