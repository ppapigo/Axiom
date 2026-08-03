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
            BuiltInEquipmentStyle builtInStyle = BuiltInEquipmentStyle.Classic)
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
                    BuildTank(visualRoot, teamAccent, builtInStyle);
                    break;
                case CharacterRoleId.Mage:
                    BuildMage(visualRoot, teamAccent, builtInStyle);
                    break;
                case CharacterRoleId.Assassin:
                    BuildAssassin(visualRoot, teamAccent, builtInStyle);
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

        private static void BuildTank(
            Transform root,
            Color accent,
            BuiltInEquipmentStyle style)
        {
            Color armour = GetEquipmentColor(style, new Color(0.32f, 0.36f, 0.43f));
            PrimitiveType shieldPrimitive = style == BuiltInEquipmentStyle.Ivory
                ? PrimitiveType.Sphere
                : PrimitiveType.Cube;
            Vector3 shieldScale = style switch
            {
                BuiltInEquipmentStyle.Obsidian => new Vector3(0.8f, 1.18f, 0.16f),
                BuiltInEquipmentStyle.Ivory => new Vector3(0.82f, 1.06f, 0.18f),
                _ => new Vector3(0.72f, 1.05f, 0.16f)
            };
            Transform shield = CreatePart(
                root,
                "Tank Shield",
                shieldPrimitive,
                new Vector3(-0.62f, 0f, 0.3f),
                Quaternion.Euler(0f, -12f, 0f),
                shieldScale,
                armour);
            CreatePart(
                root,
                "Tank Shield Emblem",
                PrimitiveType.Cube,
                new Vector3(-0.62f, 0f, 0.4f),
                Quaternion.Euler(0f, -12f, 0f),
                new Vector3(0.28f, 0.28f, 0.05f),
                accent);

            if (style == BuiltInEquipmentStyle.Obsidian)
            {
                CreatePart(
                    shield,
                    "Tank Obsidian Crest",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0.52f, 0.7f),
                    Quaternion.Euler(0f, 0f, 45f),
                    new Vector3(0.32f, 0.32f, 0.4f),
                    accent);
                CreatePart(
                    shield,
                    "Tank Obsidian Spine",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0f, 0.7f),
                    Quaternion.identity,
                    new Vector3(0.14f, 0.82f, 0.38f),
                    accent);
            }
            else if (style == BuiltInEquipmentStyle.Ivory)
            {
                CreatePart(
                    shield,
                    "Tank Ivory Boss",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0f, 0.68f),
                    Quaternion.identity,
                    new Vector3(0.42f, 0.3f, 0.5f),
                    accent);
            }
        }

        private static void BuildMage(
            Transform root,
            Color accent,
            BuiltInEquipmentStyle style)
        {
            Color staff = GetEquipmentColor(style, new Color(0.35f, 0.2f, 0.1f));
            float staffLength = style == BuiltInEquipmentStyle.Obsidian ? 0.9f : 0.78f;
            CreatePart(
                root,
                "Mage Staff",
                PrimitiveType.Cylinder,
                new Vector3(0.68f, 0.05f, 0f),
                Quaternion.Euler(0f, 0f, -8f),
                new Vector3(0.07f, staffLength, 0.07f),
                staff);
            Transform orb = CreatePart(
                root,
                "Mage Orb",
                style == BuiltInEquipmentStyle.Obsidian
                    ? PrimitiveType.Cube
                    : PrimitiveType.Sphere,
                new Vector3(0.83f, 0.92f, 0f),
                style == BuiltInEquipmentStyle.Obsidian
                    ? Quaternion.Euler(0f, 0f, 45f)
                    : Quaternion.identity,
                Vector3.one * (style == BuiltInEquipmentStyle.Ivory ? 0.32f : 0.26f),
                accent);

            if (style == BuiltInEquipmentStyle.Obsidian)
            {
                CreatePart(
                    orb,
                    "Mage Obsidian Crown",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0.75f, 0f),
                    Quaternion.Euler(0f, 0f, 45f),
                    new Vector3(0.35f, 0.35f, 0.35f),
                    staff);
            }
            else if (style == BuiltInEquipmentStyle.Ivory)
            {
                CreatePart(
                    orb,
                    "Mage Ivory Crown",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.78f, 0f),
                    Quaternion.identity,
                    new Vector3(0.48f, 0.28f, 0.48f),
                    staff);
                CreatePart(
                    orb,
                    "Mage Ivory Halo",
                    PrimitiveType.Cylinder,
                    new Vector3(0f, 0f, 0f),
                    Quaternion.Euler(90f, 0f, 0f),
                    new Vector3(1.35f, 0.12f, 1.35f),
                    staff);
            }
        }

        private static void BuildAssassin(
            Transform root,
            Color accent,
            BuiltInEquipmentStyle style)
        {
            Color blade = GetEquipmentColor(style, accent);
            Vector3 bladeScale = style switch
            {
                BuiltInEquipmentStyle.Obsidian => new Vector3(0.07f, 0.55f, 0.07f),
                BuiltInEquipmentStyle.Ivory => new Vector3(0.14f, 0.36f, 0.08f),
                _ => new Vector3(0.09f, 0.42f, 0.08f)
            };
            PrimitiveType bladePrimitive = style == BuiltInEquipmentStyle.Ivory
                ? PrimitiveType.Capsule
                : PrimitiveType.Cube;
            Transform leftDagger = CreatePart(
                root,
                "Assassin Left Dagger",
                bladePrimitive,
                new Vector3(-0.4f, -0.18f, 0.55f),
                Quaternion.Euler(28f, 0f, 28f),
                bladeScale,
                blade);
            Transform rightDagger = CreatePart(
                root,
                "Assassin Right Dagger",
                bladePrimitive,
                new Vector3(0.4f, -0.18f, 0.55f),
                Quaternion.Euler(28f, 0f, -28f),
                bladeScale,
                blade);

            if (style != BuiltInEquipmentStyle.Classic)
            {
                string styleName = style == BuiltInEquipmentStyle.Obsidian
                    ? "Obsidian"
                    : "Ivory";
                PrimitiveType guardPrimitive = style == BuiltInEquipmentStyle.Obsidian
                    ? PrimitiveType.Cube
                    : PrimitiveType.Sphere;
                CreateDaggerGuard(leftDagger, $"Assassin {styleName} Left Guard", guardPrimitive, accent);
                CreateDaggerGuard(rightDagger, $"Assassin {styleName} Right Guard", guardPrimitive, accent);
            }
        }

        private static void CreateDaggerGuard(
            Transform dagger,
            string partName,
            PrimitiveType primitive,
            Color color)
        {
            CreatePart(
                dagger,
                partName,
                primitive,
                new Vector3(0f, -0.72f, 0f),
                Quaternion.Euler(0f, 0f, 90f),
                new Vector3(2.4f, 0.15f, 1.8f),
                color);
        }

        private static Color GetEquipmentColor(
            BuiltInEquipmentStyle style,
            Color classicColor)
        {
            return style switch
            {
                BuiltInEquipmentStyle.Obsidian => new Color(0.08f, 0.09f, 0.12f),
                BuiltInEquipmentStyle.Ivory => new Color(0.78f, 0.75f, 0.65f),
                _ => classicColor
            };
        }

        private static Transform CreatePart(
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
            return part.transform;
        }
    }
}
