using Axiom.Character;
using UnityEngine;

namespace Axiom.Data
{
    [CreateAssetMenu(
        fileName = "CharacterMovementProfile",
        menuName = "Axiom/Character/Movement Profile")]
    public sealed class CharacterMovementProfile : ScriptableObject
    {
        [Header("Planar Movement")]
        [SerializeField, Min(0f)] private float maximumSpeed = 5f;
        [SerializeField, Min(0f)] private float acceleration = 30f;
        [SerializeField, Min(0f)] private float deceleration = 40f;

        [Header("Vertical Movement")]
        [SerializeField] private float gravity = -25f;
        [SerializeField] private float groundedVerticalSpeed = -2f;

        public MovementParameters Parameters => new MovementParameters(
            maximumSpeed,
            acceleration,
            deceleration,
            gravity,
            groundedVerticalSpeed);

        private void OnValidate()
        {
            maximumSpeed = Mathf.Max(0f, maximumSpeed);
            acceleration = Mathf.Max(0f, acceleration);
            deceleration = Mathf.Max(0f, deceleration);
            gravity = Mathf.Min(0f, gravity);
            groundedVerticalSpeed = Mathf.Min(0f, groundedVerticalSpeed);
        }
    }
}

