using UnityEngine;

namespace Axiom.Data
{
    [CreateAssetMenu(
        fileName = "CharacterStatsProfile",
        menuName = "Axiom/Character/Stats Profile")]
    public sealed class CharacterStatsProfile : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float maximumHealth = 100f;
        [SerializeField, Min(0f)] private float attackPower = 100f;

        public float MaximumHealth => maximumHealth;
        public float AttackPower => attackPower;

        private void OnValidate()
        {
            maximumHealth = Mathf.Max(0.01f, maximumHealth);
            attackPower = Mathf.Max(0f, attackPower);
        }
    }
}

