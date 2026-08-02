namespace Axiom.Skill
{
    public static class ElementReactionResolver
    {
        public static ElementReactionResult Resolve(
            SkillElement first,
            SkillElement second,
            float fireWaterMultiplier,
            float waterIceMultiplier,
            float fireIceMultiplier)
        {
            if (Matches(first, second, SkillElement.Fire, SkillElement.Water))
            {
                return new ElementReactionResult(
                    ElementReactionType.FireWater,
                    fireWaterMultiplier,
                    CrowdControlType.None);
            }

            if (Matches(first, second, SkillElement.Water, SkillElement.Ice))
            {
                return new ElementReactionResult(
                    ElementReactionType.WaterIce,
                    waterIceMultiplier,
                    CrowdControlType.Root);
            }

            if (Matches(first, second, SkillElement.Fire, SkillElement.Ice))
            {
                return new ElementReactionResult(
                    ElementReactionType.FireIce,
                    fireIceMultiplier,
                    CrowdControlType.None);
            }

            if (Matches(first, second, SkillElement.Water, SkillElement.Lightning))
            {
                return new ElementReactionResult(
                    ElementReactionType.WaterLightning,
                    1f,
                    CrowdControlType.Stun);
            }

            if (Matches(first, second, SkillElement.Ice, SkillElement.Lightning))
            {
                return new ElementReactionResult(
                    ElementReactionType.IceLightning,
                    1f,
                    CrowdControlType.None);
            }

            if (Matches(first, second, SkillElement.Fire, SkillElement.Poison))
            {
                return new ElementReactionResult(
                    ElementReactionType.FirePoison,
                    1f,
                    CrowdControlType.None);
            }

            return ElementReactionResult.None;
        }

        private static bool Matches(
            SkillElement first,
            SkillElement second,
            SkillElement left,
            SkillElement right)
        {
            return first == left && second == right ||
                   first == right && second == left;
        }
    }
}
