using System;
using UnityEngine;

namespace Axiom.Skill
{
    public sealed class CrowdControlState
    {
        private float _slowUntil;
        private float _rootUntil;
        private float _stunUntil;
        private float _knockUpUntil;

        public void Apply(CrowdControlType type, float currentTime, float duration)
        {
            if (currentTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(currentTime));
            }

            float until = currentTime + Mathf.Max(0f, duration);
            switch (type)
            {
                case CrowdControlType.Slow:
                    _slowUntil = Mathf.Max(_slowUntil, until);
                    break;
                case CrowdControlType.Root:
                    _rootUntil = Mathf.Max(_rootUntil, until);
                    break;
                case CrowdControlType.Stun:
                    _stunUntil = Mathf.Max(_stunUntil, until);
                    break;
                case CrowdControlType.KnockUp:
                    _knockUpUntil = Mathf.Max(_knockUpUntil, until);
                    break;
            }
        }

        public bool IsMovementBlocked(float currentTime)
        {
            return _rootUntil > currentTime || IsActionBlocked(currentTime);
        }

        public bool IsActionBlocked(float currentTime)
        {
            return _stunUntil > currentTime || _knockUpUntil > currentTime;
        }

        public float GetMovementSpeedMultiplier(float currentTime, float slowMultiplier)
        {
            return _slowUntil > currentTime ? Mathf.Clamp01(slowMultiplier) : 1f;
        }

        public CrowdControlType GetActiveEffect(float currentTime)
        {
            if (_stunUntil > currentTime)
            {
                return CrowdControlType.Stun;
            }

            if (_knockUpUntil > currentTime)
            {
                return CrowdControlType.KnockUp;
            }

            if (_rootUntil > currentTime)
            {
                return CrowdControlType.Root;
            }

            return _slowUntil > currentTime
                ? CrowdControlType.Slow
                : CrowdControlType.None;
        }

        public float GetRemainingDuration(float currentTime)
        {
            float until = GetActiveEffect(currentTime) switch
            {
                CrowdControlType.Stun => _stunUntil,
                CrowdControlType.KnockUp => _knockUpUntil,
                CrowdControlType.Root => _rootUntil,
                CrowdControlType.Slow => _slowUntil,
                _ => currentTime
            };
            return Mathf.Max(0f, until - currentTime);
        }

        public void Clear()
        {
            _slowUntil = 0f;
            _rootUntil = 0f;
            _stunUntil = 0f;
            _knockUpUntil = 0f;
        }
    }
}
