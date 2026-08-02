using System;
using UnityEngine;

namespace Axiom.Skill
{
    public enum SkillVfxStage
    {
        Cast,
        Projectile,
        Impact,
        Hit
    }

    [Serializable]
    public struct ElementSkillVfxSet
    {
        [SerializeField] private SkillElement element;
        [SerializeField] private GameObject castPrefab;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private GameObject impactPrefab;
        [SerializeField] private GameObject hitPrefab;

        public SkillElement Element => element;

        public ElementSkillVfxSet(
            SkillElement element,
            GameObject castPrefab,
            GameObject projectilePrefab,
            GameObject impactPrefab,
            GameObject hitPrefab)
        {
            this.element = element;
            this.castPrefab = castPrefab;
            this.projectilePrefab = projectilePrefab;
            this.impactPrefab = impactPrefab;
            this.hitPrefab = hitPrefab;
        }

        public GameObject GetPrefab(SkillVfxStage stage)
        {
            return stage switch
            {
                SkillVfxStage.Cast => castPrefab,
                SkillVfxStage.Projectile => projectilePrefab,
                SkillVfxStage.Impact => impactPrefab,
                SkillVfxStage.Hit => hitPrefab,
                _ => null
            };
        }
    }

    [CreateAssetMenu(
        fileName = "SkillVfxLibrary",
        menuName = "Axiom/Skill/VFX Library")]
    public sealed class SkillVfxLibrary : ScriptableObject
    {
        [SerializeField] private ElementSkillVfxSet[] elementSets =
            Array.Empty<ElementSkillVfxSet>();

        public bool TryGetPrefab(
            SkillElement element,
            SkillVfxStage stage,
            out GameObject prefab)
        {
            foreach (ElementSkillVfxSet set in elementSets)
            {
                if (set.Element != element)
                {
                    continue;
                }

                prefab = set.GetPrefab(stage);
                return prefab != null;
            }

            prefab = null;
            return false;
        }

        public void Configure(params ElementSkillVfxSet[] sets)
        {
            elementSets = sets ?? Array.Empty<ElementSkillVfxSet>();
        }
    }
}
