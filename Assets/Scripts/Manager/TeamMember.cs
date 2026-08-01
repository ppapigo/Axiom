using UnityEngine;

namespace Axiom.Manager
{
    [DisallowMultipleComponent]
    public sealed class TeamMember : MonoBehaviour
    {
        [SerializeField] private TeamId team;

        public TeamId Team => team;
    }
}
