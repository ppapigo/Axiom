using Axiom.Data;
using Axiom.Role;
using UnityEngine;

namespace Axiom.Demo
{
    internal static class DemoRoleVisualBuilder
    {
        public static void Build(
            Transform character,
            CharacterRoleId role,
            bool blueTeam,
            EquipmentAppearanceDefinition equipmentAppearance = null)
        {
            var visualRoot = new GameObject($"{role} Visual").transform;
            visualRoot.SetParent(character, false);

            Color teamAccent = blueTeam
                ? new Color(0.35f, 0.85f, 1f)
                : new Color(1f, 0.55f, 0.15f);
            CreatePart(
                visualRoot,
                "Facing Visor",
                PrimitiveType.Cube,
                new Vector3(0f, 0.3f, 0.52f),
                Quaternion.identity,
                new Vector3(0.42f, 0.2f, 0.1f),
                teamAccent);

            if (equipmentAppearance != null &&
                equipmentAppearance.Role == role &&
                equipmentAppearance.HasParts)
            {
                BuildCustomEquipment(visualRoot, equipmentAppearance, teamAccent);
                return;
            }

            switch (role)
            {
                case CharacterRoleId.Tank:
                    BuildTank(visualRoot, teamAccent);
                    break;
                case CharacterRoleId.Mage:
                    BuildMage(visualRoot, teamAccent);
                    break;
                case CharacterRoleId.Assassin:
                    BuildAssassin(visualRoot, teamAccent);
                    break;
            }
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

        private static void BuildTank(Transform root, Color accent)
        {
            Color armour = new Color(0.32f, 0.36f, 0.43f);
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
            CreatePart(
                root,
                "Tank Shoulder",
                PrimitiveType.Cube,
                new Vector3(0.48f, 0.38f, 0f),
                Quaternion.Euler(0f, 0f, 12f),
                new Vector3(0.32f, 0.28f, 0.55f),
                armour);
        }

        private static void BuildMage(Transform root, Color accent)
        {
            Color staff = new Color(0.35f, 0.2f, 0.1f);
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
            CreatePart(
                root,
                "Mage Robe",
                PrimitiveType.Cylinder,
                new Vector3(0f, -0.62f, 0f),
                Quaternion.identity,
                new Vector3(0.58f, 0.28f, 0.58f),
                new Color(0.2f, 0.12f, 0.38f));
        }

        private static void BuildAssassin(Transform root, Color accent)
        {
            Color hood = new Color(0.12f, 0.13f, 0.18f);
            CreatePart(
                root,
                "Assassin Hood",
                PrimitiveType.Sphere,
                new Vector3(0f, 0.62f, 0f),
                Quaternion.identity,
                new Vector3(0.63f, 0.42f, 0.63f),
                hood);
            CreatePart(
                root,
                "Assassin Left Dagger",
                PrimitiveType.Cube,
                new Vector3(-0.4f, -0.18f, 0.55f),
                Quaternion.Euler(28f, 0f, 28f),
                new Vector3(0.09f, 0.42f, 0.08f),
                accent);
            CreatePart(
                root,
                "Assassin Right Dagger",
                PrimitiveType.Cube,
                new Vector3(0.4f, -0.18f, 0.55f),
                Quaternion.Euler(28f, 0f, -28f),
                new Vector3(0.09f, 0.42f, 0.08f),
                accent);
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
