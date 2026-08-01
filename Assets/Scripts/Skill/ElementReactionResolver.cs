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
