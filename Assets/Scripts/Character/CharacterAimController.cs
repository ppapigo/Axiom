using Axiom.Input;
using UnityEngine;

namespace Axiom.Character
{
    [DisallowMultipleComponent]
    public sealed class CharacterAimController : MonoBehaviour
    {
        [SerializeField] private InputActionBasicAttackSource inputSourceBehaviour;
        [SerializeField, Min(0f)] private float turnSpeed = 1080f;

        private IBasicAttackInputSource _inputSource;

        public void Configure(InputActionBasicAttackSource inputSource)
        {
            inputSourceBehaviour = inputSource;
            _inputSource = inputSource;
        }

        private void Awake()
        {
            _inputSource = inputSourceBehaviour;
        }

        private void Update()
        {
            if (_inputSource == null || !_inputSource.TryGetAimPoint(out Vector3 aimPoint))
            {
                return;
            }

            Vector3 direction = aimPoint - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = turnSpeed <= 0f
                ? targetRotation
                : Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    turnSpeed * Time.deltaTime);
        }

        private void OnValidate()
        {
            turnSpeed = Mathf.Max(0f, turnSpeed);
        }
    }
}
