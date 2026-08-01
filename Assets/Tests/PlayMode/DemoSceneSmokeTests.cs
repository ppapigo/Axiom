using System.Collections;
using Axiom.Combat;
using Axiom.Demo;
using Axiom.Manager;
using Axiom.Role;
using Axiom.Skill;
using Axiom.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Axiom.Tests.PlayMode
{
    public sealed class DemoSceneSmokeTests
    {
        [UnityTest]
        public IEnumerator DemoScene_StartsPlayableThreeVsThreeMatch()
        {
            SceneManager.LoadScene("AxiomDemo");
            yield return null;

            DemoArenaBootstrap bootstrap = Object.FindFirstObjectByType<DemoArenaBootstrap>();
            Assert.That(bootstrap, Is.Not.Null);
            SkillBuilderPanel skillBuilder = Object.FindFirstObjectByType<SkillBuilderPanel>();
            Assert.That(skillBuilder, Is.Not.Null);
            Assert.That(skillBuilder.IsAvailable, Is.False);
            bootstrap.StartDemo(CharacterRoleId.Mage);
            yield return null;
            yield return null;

            CharacterHealth[] combatants = Object.FindObjectsByType<CharacterHealth>(
                FindObjectsSortMode.None);
            CharacterStatusController[] statuses =
                Object.FindObjectsByType<CharacterStatusController>(FindObjectsSortMode.None);
            ElementStatusController[] elementStatuses =
                Object.FindObjectsByType<ElementStatusController>(FindObjectsSortMode.None);
            ThreeVsThreeMatchManager match =
                Object.FindFirstObjectByType<ThreeVsThreeMatchManager>();
            WorldHealthBar[] healthBars = Object.FindObjectsByType<WorldHealthBar>(
                FindObjectsSortMode.None);
            CombatHud combatHud = Object.FindFirstObjectByType<CombatHud>();

            Assert.That(combatants, Has.Length.EqualTo(6));
            Assert.That(statuses, Has.Length.EqualTo(6));
            Assert.That(elementStatuses, Has.Length.EqualTo(6));
            Assert.That(statuses[0].Apply(CrowdControlType.Stun, Time.time), Is.True);
            Assert.That(statuses[0].ActiveEffect, Is.EqualTo(CrowdControlType.Stun));
            Assert.That(statuses[0].IsActionBlocked, Is.True);
            statuses[0].Clear();
            Assert.That(healthBars, Has.Length.EqualTo(6));
            Assert.That(healthBars, Has.All.Matches<WorldHealthBar>(bar => bar.Health != null));
            Assert.That(match, Is.Not.Null);
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.RoundActive));
            Assert.That(UnityEngine.Camera.main, Is.Not.Null);
            Assert.That(skillBuilder, Is.Not.Null);
            Assert.That(skillBuilder.IsConfigured, Is.True);
            Assert.That(skillBuilder.IsAvailable, Is.True);
            Assert.That(combatHud, Is.Not.Null);
            Assert.That(combatHud.IsConfigured, Is.True);
            Assert.That(combatHud.Health, Is.Not.Null);
            DemoSkillController playerSkills =
                Object.FindFirstObjectByType<DemoSkillController>();
            Assert.That(playerSkills, Is.Not.Null);
            skillBuilder.Model.AdjustDamage(1);
            skillBuilder.Model.AdjustCooldownReduction(1);
            Assert.That(skillBuilder.Model.ToggleElement(SkillElement.Fire), Is.True);
            Assert.That(skillBuilder.TrySaveDraft(), Is.True);
            yield return null;
            Assert.That(playerSkills.QSkillDefinition.DamageCoefficient,
                Is.EqualTo(1.32f).Within(0.001f));
            Assert.That(playerSkills.QSkillDefinition.Cooldown, Is.EqualTo(3f));
            Assert.That(playerSkills.QSkillDefinition.Element, Is.EqualTo(SkillElement.Fire));
            Assert.That(GameObject.Find("Tank Visual"), Is.Not.Null);
            Assert.That(GameObject.Find("Mage Visual"), Is.Not.Null);
            Assert.That(GameObject.Find("Assassin Visual"), Is.Not.Null);
        }
    }
}
