using UnityEngine;

namespace Axiom.Skill
{
    public static class SkillCastDelayRules
    {
        private static readonly float[] SupportedDelays = { 0f, 0.3f, 0.6f, 1f, 1.5f };

        public static bool IsSupported(float castDelay)
        {
            foreach (float supportedDelay in SupportedDelays)
            {
                if (Mathf.Approximately(castDelay, supportedDelay))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
