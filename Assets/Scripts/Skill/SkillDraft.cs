namespace Axiom.Skill
{
    public readonly struct SkillDraft
    {
        public SkillDraft(
            in SkillPointModifiers modifiers,
            SkillElement? element)
        {
            Modifiers = modifiers;
            Element = element;
        }

        public SkillPointModifiers Modifiers { get; }
        public SkillElement? Element { get; }
        public int SelectedElementCount => Element.HasValue ? 1 : 0;
    }
}
