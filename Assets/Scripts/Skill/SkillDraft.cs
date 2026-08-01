namespace Axiom.Skill
{
    public readonly struct SkillDraft
    {
        public SkillDraft(
            in SkillPointModifiers modifiers,
            SkillElement? element,
            SkillType? type = null,
            SkillSlot slot = SkillSlot.Q)
        {
            Modifiers = modifiers;
            Element = element;
            Type = type;
            Slot = slot;
        }

        public SkillPointModifiers Modifiers { get; }
        public SkillElement? Element { get; }
        public SkillType? Type { get; }
        public SkillSlot Slot { get; }
        public int SelectedElementCount => Element.HasValue ? 1 : 0;
    }
}
