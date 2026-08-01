namespace Axiom.Skill
{
    public readonly struct SkillDraft
    {
        public SkillDraft(
            in SkillPointModifiers modifiers,
            SkillElement? element,
            SkillType? type = null)
        {
            Modifiers = modifiers;
            Element = element;
            Type = type;
        }

        public SkillPointModifiers Modifiers { get; }
        public SkillElement? Element { get; }
        public SkillType? Type { get; }
        public int SelectedElementCount => Element.HasValue ? 1 : 0;
    }
}
