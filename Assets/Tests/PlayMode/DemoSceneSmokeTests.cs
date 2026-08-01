using System.Collections;
using Axiom.Combat;
using Axiom.Demo;
using Axiom.Manager;
using Axiom.Role;
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
            bootstrap.StartDemo(CharacterRoleId.Mage);
            yield return null;
            yield return null;

            CharacterHealth[] combatants = Object.FindObjectsByType<CharacterHealth>(
                FindObjectsSortMode.None);
            ThreeVsThreeMatchManager match =
                Object.FindFirstObjectByType<ThreeVsThreeMatchManager>();
            WorldHealthBar[] healthBars = Object.FindObjectsByType<WorldHealthBar>(
                FindObjectsSortMode.None);
            SkillBuilderPanel skillBuilder = Object.FindFirstObjectByType<SkillBuilderPanel>();

            Assert.That(combatants, Has.Length.EqualTo(6));
            Assert.That(healthBars, Has.Length.EqualTo(6));
            Assert.That(healthBars, Has.All.Matches<WorldHealthBar>(bar => bar.Health != null));
            Assert.That(match, Is.Not.Null);
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.RoundActive));
            Assert.That(UnityEngine.Camera.main, Is.Not.Null);
            Assert.That(skillBuilder, Is.Not.Null);
            Assert.That(skillBuilder.IsConfigured, Is.True);
            Assert.That(GameObject.Find("Tank Visual"), Is.Not.Null);
            Assert.That(GameObject.Find("Mage Visual"), Is.Not.Null);
            Assert.That(GameObject.Find("Assassin Visual"), Is.Not.Null);
        }
    }
}
