using UnityEngine;

namespace Axiom.Demo
{
    public readonly struct StickFigureMotionPose
    {
        public StickFigureMotionPose(
            float bobOffset,
            float limbSwingDegrees,
            float attackWeight)
        {
            BobOffset = bobOffset;
            LimbSwingDegrees = limbSwingDegrees;
            AttackWeight = attackWeight;
        }

        public float BobOffset { get; }
        public float LimbSwingDegrees { get; }
        public float AttackWeight { get; }
    }

    public static class StickFigureMotionCalculator
    {
        public static StickFigureMotionPose Calculate(
            float planarSpeed,
            float elapsedTime,
            float attackElapsed,
            float referenceSpeed,
            float walkFrequency,
            float maximumLimbSwing,
            float idleBobHeight,
            float movingBobHeight,
            float attackDuration)
        {
            float speedRatio = Mathf.Clamp01(
                Mathf.Max(0f, planarSpeed) / Mathf.Max(0.01f, referenceSpeed));
            float phase = Mathf.Max(0f, elapsedTime) *
                          Mathf.Max(0f, walkFrequency) *
                          Mathf.Lerp(0.35f, 1f, speedRatio);
            float limbSwing = Mathf.Sin(phase) *
                              Mathf.Max(0f, maximumLimbSwing) *
                              speedRatio;
            float bobHeight = Mathf.Lerp(
                Mathf.Max(0f, idleBobHeight),
                Mathf.Max(0f, movingBobHeight),
                speedRatio);
            float bobOffset = Mathf.Sin(phase * 2f) * bobHeight;

            float safeAttackDuration = Mathf.Max(0.01f, attackDuration);
            float attackWeight = attackElapsed < 0f ||
                                 attackElapsed > safeAttackDuration
                ? 0f
                : Mathf.Sin(Mathf.Clamp01(attackElapsed / safeAttackDuration) * Mathf.PI);
            return new StickFigureMotionPose(
                bobOffset,
                limbSwing,
                attackWeight);
        }
    }
}
