using System.Collections.Generic;

namespace Axiom.Skill
{
    public sealed class SkillCooldownTracker
    {
        private readonly Dictionary<SkillSlot, float> _readyTimes =
            new Dictionary<SkillSlot, float>();

        public bool TryStart(SkillSlot slot, float currentTime, float cooldown)
        {
            if (_readyTimes.TryGetValue(slot, out float readyTime) && currentTime < readyTime)
            {
                return false;
            }

            _readyTimes[slot] = currentTime + cooldown;
            return true;
        }

        public float GetRemaining(SkillSlot slot, float currentTime)
        {
            return _readyTimes.TryGetValue(slot, out float readyTime)
                ? UnityEngine.Mathf.Max(0f, readyTime - currentTime)
                : 0f;
        }

        public void Reset()
        {
            _readyTimes.Clear();
        }
    }
}
