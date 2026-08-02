using System;
using System.Collections.Generic;
using Axiom.Data;

namespace Axiom.Skill.Generation
{
    public enum SkillPointCostCategory
    {
        SkillType,
        Damage,
        Radius,
        Range,
        Cooldown,
        Element,
        Slow,
        Stun,
        KnockUp,
        Mobility,
        Shield,
        Healing
    }

    public readonly struct SkillPointCostItem
    {
        public SkillPointCostItem(SkillPointCostCategory category, int points)
        {
            Category = category;
            Points = Math.Max(0, points);
        }

        public SkillPointCostCategory Category { get; }
        public int Points { get; }
    }

    public sealed class SkillPointCostBreakdown
    {
        private readonly SkillPointCostItem[] _items;

        private SkillPointCostBreakdown(
            IReadOnlyCollection<SkillPointCostItem> items,
            int total)
        {
            _items = new SkillPointCostItem[items.Count];
            int index = 0;
            foreach (SkillPointCostItem item in items)
            {
                _items[index++] = item;
            }

            Total = Math.Max(0, total);
        }

        public IReadOnlyList<SkillPointCostItem> Items => _items;
        public int Total { get; }

        public static SkillPointCostBreakdown Create(
            in SkillDraft draft,
            SkillBalanceProfile balance)
        {
            if (balance == null)
            {
                throw new ArgumentNullException(nameof(balance));
            }

            var items = new List<SkillPointCostItem>();
            SkillPointModifiers modifiers = draft.Modifiers;
            if (draft.Type.HasValue)
            {
                Add(items, SkillPointCostCategory.SkillType,
                    balance.GetSkillTypeCost(draft.Type.Value));
            }

            AddIncrement(items, SkillPointCostCategory.Damage,
                modifiers.DamageIncreasePercent, 10f, balance.DamageCostPerTenPercent);
            AddIncrement(items, SkillPointCostCategory.Radius,
                modifiers.RadiusIncrease, 1f, balance.RadiusCostPerMeter);
            AddIncrement(items, SkillPointCostCategory.Range,
                modifiers.RangeIncrease, 1f, balance.RangeCostPerMeter);
            AddIncrement(items, SkillPointCostCategory.Cooldown,
                modifiers.CooldownReduction, 1f, balance.CooldownCostPerSecond);
            Add(items, SkillPointCostCategory.Element,
                draft.Element.HasValue ? balance.ElementCost : 0);
            Add(items, SkillPointCostCategory.Slow,
                modifiers.AppliesSlow ? balance.GetEffectCost(SkillPointEffect.Slow) : 0);
            Add(items, SkillPointCostCategory.Stun,
                modifiers.AppliesStun ? balance.GetEffectCost(SkillPointEffect.Stun) : 0);
            Add(items, SkillPointCostCategory.KnockUp,
                modifiers.AppliesKnockUp ? balance.GetEffectCost(SkillPointEffect.KnockUp) : 0);
            Add(items, SkillPointCostCategory.Mobility,
                modifiers.AddsMobility ? balance.GetEffectCost(SkillPointEffect.Mobility) : 0);
            Add(items, SkillPointCostCategory.Shield,
                modifiers.CreatesShield ? balance.GetEffectCost(SkillPointEffect.Shield) : 0);
            Add(items, SkillPointCostCategory.Healing,
                modifiers.Heals ? balance.GetEffectCost(SkillPointEffect.Healing) : 0);

            int total = balance.CalculatePointCost(
                modifiers,
                draft.Element.HasValue ? 1 : 0,
                draft.Type);
            return new SkillPointCostBreakdown(items, total);
        }

        private static void AddIncrement(
            ICollection<SkillPointCostItem> items,
            SkillPointCostCategory category,
            float amount,
            float increment,
            int costPerIncrement)
        {
            if (amount <= 0f || increment <= 0f || costPerIncrement <= 0)
            {
                return;
            }

            Add(items, category,
                (int)Math.Ceiling(amount / increment) * costPerIncrement);
        }

        private static void Add(
            ICollection<SkillPointCostItem> items,
            SkillPointCostCategory category,
            int points)
        {
            if (points > 0)
            {
                items.Add(new SkillPointCostItem(category, points));
            }
        }
    }
}
