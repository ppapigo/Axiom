using System;

namespace Axiom.Camera
{
    public readonly struct CameraFollowParameters
    {
        public CameraFollowParameters(
            float height,
            float pitchAngle,
            float yawAngle,
            float lookAtHeight,
            float smoothTime)
        {
            if (height <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            if (pitchAngle <= 0f || pitchAngle >= 90f)
            {
                throw new ArgumentOutOfRangeException(nameof(pitchAngle));
            }

            if (smoothTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(smoothTime));
            }

            Height = height;
            PitchAngle = pitchAngle;
            YawAngle = yawAngle;
            LookAtHeight = lookAtHeight;
            SmoothTime = smoothTime;
        }

        public float Height { get; }
        public float PitchAngle { get; }
        public float YawAngle { get; }
        public float LookAtHeight { get; }
        public float SmoothTime { get; }
    }
}

