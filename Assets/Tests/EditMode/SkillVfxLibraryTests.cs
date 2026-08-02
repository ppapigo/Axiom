using System.Linq;
using Axiom.Demo;
using Axiom.Skill;
using NUnit.Framework;
using UnityEngine;

namespace Axiom.Tests.EditMode
{
    public sealed class SkillVfxLibraryTests
    {
        [Test]
        public void Library_ReturnsPrefabForMatchingElementAndStage()
        {
            SkillVfxLibrary library = ScriptableObject.CreateInstance<SkillVfxLibrary>();
            var cast = new GameObject("Fire Cast Prefab");
            var impact = new GameObject("Fire Impact Prefab");
            library.Configure(new ElementSkillVfxSet(
                element: SkillElement.Fire,
                castPrefab: cast,
                projectilePrefab: null,
                impactPrefab: impact,
                hitPrefab: null));

            Assert.That(
                library.TryGetPrefab(
                    SkillElement.Fire,
                    SkillVfxStage.Impact,
                    out GameObject result),
                Is.True);
            Assert.That(result, Is.SameAs(impact));

            Object.DestroyImmediate(cast);
            Object.DestroyImmediate(impact);
            Object.DestroyImmediate(library);
        }

        [Test]
        public void Library_MissingPrefab_UsesFallbackSignal()
        {
            SkillVfxLibrary library = ScriptableObject.CreateInstance<SkillVfxLibrary>();
            library.Configure(new ElementSkillVfxSet(
                SkillElement.Ice,
                castPrefab: null,
                projectilePrefab: null,
                impactPrefab: null,
                hitPrefab: null));

            bool found = library.TryGetPrefab(
                SkillElement.Ice,
                SkillVfxStage.Projectile,
                out GameObject result);

            Assert.That(found, Is.False);
            Assert.That(result, Is.Null);
            Object.DestroyImmediate(library);
        }

        [Test]
        public void FallbackColors_AreDistinctForEveryElement()
        {
            Color[] colors = System.Enum.GetValues(typeof(SkillElement))
                .Cast<SkillElement>()
                .Select(DemoSkillVfxPlayer.GetElementColor)
                .ToArray();

            Assert.That(colors.Distinct().Count(), Is.EqualTo(colors.Length));
        }
    }
}
