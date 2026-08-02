using System;
using UnityEngine;

namespace Axiom.Demo
{
    [DisallowMultipleComponent]
    public sealed class DemoProjectile : MonoBehaviour
    {
        private readonly RaycastHit[] _hits = new RaycastHit[16];
        private Transform _owner;
        private Vector3 _direction;
        private float _speed;
        private float _radius;
        private float _remainingDistance;
        private Action<Vector3> _onImpact;
        private bool _isActive;

        public Vector3 Direction => _direction;
        public float RemainingDistance => _remainingDistance;
        public Transform Owner => _owner;

        public void Initialize(
            Transform owner,
            Vector3 direction,
            float speed,
            float radius,
            float maximumDistance,
            Action<Vector3> onImpact)
        {
            _owner = owner;
            direction.y = 0f;
            _direction = direction.sqrMagnitude <= Mathf.Epsilon
                ? Vector3.forward
                : direction.normalized;
            _speed = Mathf.Max(0.1f, speed);
            _radius = Mathf.Max(0.05f, radius);
            _remainingDistance = Mathf.Max(0f, maximumDistance);
            _onImpact = onImpact;
            _isActive = true;
        }

        private void Update()
        {
            if (!_isActive)
            {
                return;
            }

            float travel = Mathf.Min(_speed * Time.deltaTime, _remainingDistance);
            if (travel <= 0f)
            {
                Impact(transform.position);
                return;
            }

            int hitCount = Physics.SphereCastNonAlloc(
                transform.position,
                _radius,
                _direction,
                _hits,
                travel,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore);
            bool hasImpact = false;
            float nearestDistance = travel;
            for (int i = 0; i < hitCount; i++)
            {
                Collider hitCollider = _hits[i].collider;
                if (hitCollider == null || IsOwnerCollider(hitCollider.transform))
                {
                    continue;
                }

                if (!hasImpact || _hits[i].distance < nearestDistance)
                {
                    hasImpact = true;
                    nearestDistance = _hits[i].distance;
                }
            }

            transform.position += _direction * nearestDistance;
            _remainingDistance -= nearestDistance;
            if (hasImpact || _remainingDistance <= Mathf.Epsilon)
            {
                Impact(transform.position);
            }
        }

        private bool IsOwnerCollider(Transform candidate)
        {
            return _owner != null &&
                   (candidate == _owner || candidate.IsChildOf(_owner));
        }

        private void Impact(Vector3 position)
        {
            if (!_isActive)
            {
                return;
            }

            _isActive = false;
            Action<Vector3> callback = _onImpact;
            _onImpact = null;
            callback?.Invoke(position);
            Destroy(gameObject);
        }
    }
}
