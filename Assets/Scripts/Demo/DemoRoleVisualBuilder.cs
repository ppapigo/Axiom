using Axiom.Data;
using Axiom.Role;
using UnityEngine;

namespace Axiom.Demo
{
    internal static class DemoRoleVisualBuilder
    {
        public static Transform Build(
            Transform character,
            CharacterRoleId role,
            bool blueTeam,
            EquipmentAppearanceDefinition equipmentAppearance = null,
            Color? builtInTint = null)
        {
            var visualRoot = new GameObject($"{role} Visual").transform;
            visualRoot.SetParent(character, false);

            Color teamAccent = blueTeam
                ? new Color(0.35f, 0.85f, 1f)
                : new Color(1f, 0.55f, 0.15f);
            Color bodyColor = blueTeam
                ? new Color(0.12f, 0.32f, 0.72f)
                : new Color(0.72f, 0.16f, 0.12f);
            BuildStickFigure(visualRoot, bodyColor, teamAccent);

            if (equipmentAppearance != null &&
                equipmentAppearance.Role == role &&
                equipmentAppearance.HasParts)
            {
                BuildCustomEquipment(visualRoot, equipmentAppearance, teamAccent);
                return visualRoot;
            }

            switch (role)
            {
                case CharacterRoleId.Tank:
                    BuildTank(visualRoot, teamAccent, builtInTint);
                    break;
                case CharacterRoleId.Mage:
                    BuildMage(visualRoot, teamAccent, builtInTint);
                    break;
                case CharacterRoleId.Assassin:
                    BuildAssassin(visualRoot, teamAccent, builtInTint);
                    break;
            }

            return visualRoot;
        }

        private static void BuildStickFigure(
            Transform root,
            Color bodyColor,
            Color teamAccent)
        {
            CreatePart(
                root,
                "Stick Head",
                PrimitiveType.Sphere,
                new Vector3(0f, 0.72f, 0f),
                Quaternion.identity,
                Vector3.one * 0.42f,
                bodyColor);
            CreatePart(
                root,
                "Facing Marker",
                PrimitiveType.Cube,
                new Vector3(0f, 0.72f, 0.22f),
                Quaternion.identity,
                new Vector3(0.22f, 0.08f, 0.05f),
                teamAccent);
            CreatePart(
                root,
                "Stick Torso",
                PrimitiveType.Cylinder,
                new Vector3(0f, 0.18f, 0f),
                Quaternion.identity,
                new Vector3(0.13f, 0.28f, 0.13f),
                bodyColor);
            CreatePart(
                root,
                "Stick Left Arm",
                PrimitiveType.Cylinder,
                new Vector3(-0.31f, 0.24f, 0f),
                Quaternion.Euler(0f, 0f, 38f),
                new Vector3(0.065f, 0.27f, 0.065f),
                bodyColor);
            CreatePart(
                root,
                "Stick Right Arm",
                PrimitiveType.Cylinder,
                new Vector3(0.31f, 0.24f, 0f),
                Quaternion.Euler(0f, 0f, -38f),
                new Vector3(0.065f, 0.27f, 0.065f),
                bodyColor);
            CreatePart(
                root,
                "Stick Left Leg",
                PrimitiveType.Cylinder,
                new Vector3(-0.13f, -0.46f, 0f),
                Quaternion.Euler(0f, 0f, -8f),
                new Vector3(0.075f, 0.32f, 0.075f),
                bodyColor);
            CreatePart(
                root,
                "Stick Right Leg",
                PrimitiveType.Cylinder,
                new Vector3(0.13f, -0.46f, 0f),
                Quaternion.Euler(0f, 0f, 8f),
                new Vector3(0.075f, 0.32f, 0.075f),
                bodyColor);
        }

        private static void BuildCustomEquipment(
            Transform root,
            EquipmentAppearanceDefinition appearance,
            Color teamAccent)
        {
            foreach (EquipmentVisualPart definition in appearance.Parts)
            {
                GameObject part = definition.Prefab != null
                    ? Object.Instantiate(definition.Prefab, root)
                    : GameObject.CreatePrimitive(definition.FallbackPrimitive);
                part.name = definition.PartName;
                part.transform.SetParent(root, false);
                part.transform.localPosition = definition.LocalPosition;
                part.transform.localRotation = definition.LocalRotation;
                part.transform.localScale = definition.LocalScale;

                foreach (Collider collider in part.GetComponentsInChildren<Collider>())
                {
                    Object.Destroy(collider);
                }

                if (definition.Prefab == null || definition.OverrideMaterial)
                {
                    Color color = definition.UseTeamAccent
                        ? teamAccent
                        : definition.Color;
                    foreach (Renderer renderer in part.GetComponentsInChildren<Renderer>())
                    {
                        renderer.material = DemoArenaBootstrap.CreateDemoMaterial(color);
                    }
                }
            }
        }

        private static void BuildTank(Transform root, Color accent, Color? builtInTint)
        {
            Color armour = builtInTint ?? new Color(0.32f, 0.36f, 0.43f);
            CreatePart(
                root,
                "Tank Shield",
                PrimitiveType.Cube,
                new Vector3(-0.62f, 0f, 0.3f),
                Quaternion.Euler(0f, -12f, 0f),
                new Vector3(0.72f, 1.05f, 0.16f),
                armour);
            CreatePart(
                root,
                "Tank Shield Emblem",
                PrimitiveType.Cube,
                new Vector3(-0.62f, 0f, 0.4f),
                Quaternion.Euler(0f, -12f, 0f),
                new Vector3(0.28f, 0.28f, 0.05f),
                accent);
        }

        private static void BuildMage(Transform root, Color accent, Color? builtInTint)
        {
            Color staff = builtInTint ?? new Color(0.35f, 0.2f, 0.1f);
            CreatePart(
                root,
                "Mage Staff",
                PrimitiveType.Cylinder,
                new Vector3(0.68f, 0.05f, 0f),
                Quaternion.Euler(0f, 0f, -8f),
                new Vector3(0.07f, 0.78f, 0.07f),
                staff);
            CreatePart(
                root,
                "Mage Orb",
                PrimitiveType.Sphere,
                new Vector3(0.83f, 0.92f, 0f),
                Quaternion.identity,
                Vector3.one * 0.26f,
                accent);
        }

        private static void BuildAssassin(Transform root, Color accent, Color? builtInTint)
        {
            Color blade = builtInTint ?? accent;
            CreatePart(
                root,
                "Assassin Left Dagger",
                PrimitiveType.Cube,
                new Vector3(-0.4f, -0.18f, 0.55f),
                Quaternion.Euler(28f, 0f, 28f),
                new Vector3(0.09f, 0.42f, 0.08f),
                blade);
            CreatePart(
                root,
                "Assassin Right Dagger",
                PrimitiveType.Cube,
                new Vector3(0.4f, -0.18f, 0.55f),
                Quaternion.Euler(28f, 0f, -28f),
                new Vector3(0.09f, 0.42f, 0.08f),
                blade);
        }

        private static void CreatePart(
            Transform parent,
            string partName,
            PrimitiveType primitive,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale,
            Color color)
        {
            GameObject part = GameObject.CreatePrimitive(primitive);
            part.name = partName;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;
            part.transform.localScale = localScale;
            Object.Destroy(part.GetComponent<Collider>());
            part.GetComponent<Renderer>().material =
                DemoArenaBootstrap.CreateDemoMaterial(color);
        }
    }
}
