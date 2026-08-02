using System;
using Axiom.Role;
using UnityEngine;

namespace Axiom.Data
{
    [Serializable]
    public struct EquipmentVisualPart
    {
        [SerializeField] private string partName;
        [SerializeField] private GameObject prefab;
        [SerializeField] private PrimitiveType fallbackPrimitive;
        [SerializeField] private Vector3 localPosition;
        [SerializeField] private Vector3 localEulerAngles;
        [SerializeField] private Vector3 localScale;
        [SerializeField] private Color color;
        [SerializeField] private bool useTeamAccent;
        [SerializeField] private bool overrideMaterial;

        public string PartName => string.IsNullOrWhiteSpace(partName)
            ? "Equipment Part"
            : partName;
        public GameObject Prefab => prefab;
        public PrimitiveType FallbackPrimitive => fallbackPrimitive;
        public Vector3 LocalPosition => localPosition;
        public Quaternion LocalRotation => Quaternion.Euler(localEulerAngles);
        public Vector3 LocalScale => localScale == Vector3.zero ? Vector3.one : localScale;
        public Color Color => color;
        public bool UseTeamAccent => useTeamAccent;
        public bool OverrideMaterial => overrideMaterial;
    }

    [CreateAssetMenu(
        fileName = "EquipmentAppearance",
        menuName = "Axiom/Appearance/Equipment Appearance")]
    public sealed class EquipmentAppearanceDefinition : ScriptableObject
    {
        [SerializeField] private CharacterRoleId role;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField, TextArea] private string description = string.Empty;
        [SerializeField] private EquipmentVisualPart[] parts =
            Array.Empty<EquipmentVisualPart>();

        public CharacterRoleId Role => role;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName)
            ? $"{role} Equipment"
            : displayName.Trim();
        public string Description => string.IsNullOrWhiteSpace(description)
            ? "Custom model equipment"
            : description.Trim();
        public EquipmentVisualPart[] Parts => parts ?? Array.Empty<EquipmentVisualPart>();
        public bool HasParts => Parts.Length > 0;
    }
}
