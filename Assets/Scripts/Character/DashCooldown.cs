using UnityEngine;

namespace Axiom.Character
{
    public sealed class DashCooldown
    {
        private float _readyTime = float.NegativeInfinity;

        public bool TryStart(float currentTime, float cooldown)
        {
            if (currentTime < _readyTime)
            {
                return false;
            }

            _readyTime = currentTime + Mathf.Max(0f, cooldown);
            return true;
        }

        public float GetRemaining(float currentTime)
        {
            return Mathf.Max(0f, _readyTime - currentTime);
        }

        public void Reset()
        {
            _readyTime = float.NegativeInfinity;
        }
    }
}

