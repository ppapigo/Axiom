using System.Collections.Generic;
using Axiom.Role;
using UnityEngine;

namespace Axiom.AI
{
    public static class AITargetSelector
    {
        public static Transform Select(
            CharacterRoleId role,
            Vector3 selfPosition,
            IReadOnlyList<AITargetCandidate> candidates)
        {
            Transform selected = null;
            float bestHealth = float.MaxValue;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < candidates.Count; i++)
            {
                AITargetCandidate candidate = candidates[i];
                if (candidate.Transform == null)
                {
                    continue;
                }

                float distance = (candidate.Transform.position - selfPosition).sqrMagnitude;
                bool isBetter = role == CharacterRoleId.Assassin
                    ? candidate.HealthRatio < bestHealth ||
                      (Mathf.Approximately(candidate.HealthRatio, bestHealth) &&
                       distance < bestDistance)
                    : distance < bestDistance;

                if (!isBetter)
                {
                    continue;
                }

                selected = candidate.Transform;
                bestHealth = candidate.HealthRatio;
                bestDistance = distance;
            }

            return selected;
        }
    }
}
