using System.Collections;
using Axiom.Combat;
using Axiom.Demo;
using Axiom.Manager;
using Axiom.Role;
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

            Assert.That(combatants, Has.Length.EqualTo(6));
            Assert.That(match, Is.Not.Null);
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.RoundActive));
            Assert.That(UnityEngine.Camera.main, Is.Not.Null);
        }
    }
}
