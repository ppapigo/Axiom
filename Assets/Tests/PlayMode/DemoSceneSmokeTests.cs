using System.Collections;
using System.Linq;
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

            Assert.That(skillBuilder.IsConfigured, Is.True);
            Assert.That(skillBuilder.IsAvailable, Is.True);
            Assert.That(skillBuilder.IsVisible, Is.True);
            Assert.That(skillBuilder.GenerationProviderName, Is.EqualTo("SERVERLESS"));
            Assert.That(bootstrap.IsBootstrapOverlaySuppressed, Is.True);
            Assert.That(Object.FindFirstObjectByType<ThreeVsThreeMatchManager>(), Is.Null);
            Assert.That(Object.FindObjectsByType<CharacterHealth>(
                FindObjectsSortMode.None), Is.Empty);
            skillBuilder.Model.AdjustDamage(1);
            skillBuilder.Model.AdjustCooldownReduction(1);
            Assert.That(skillBuilder.Model.ToggleElement(SkillElement.Fire), Is.True);
            Assert.That(skillBuilder.TrySaveDraft(), Is.True);
            yield return null;

            Assert.That(skillBuilder.CurrentSlot, Is.EqualTo(SkillSlot.E));
            Assert.That(skillBuilder.IsVisible, Is.True);
            Assert.That(Object.FindFirstObjectByType<ThreeVsThreeMatchManager>(), Is.Null);
            Assert.That(skillBuilder.TrySaveDraft(), Is.True);
            yield return null;

            Assert.That(skillBuilder.CurrentSlot, Is.EqualTo(SkillSlot.Ultimate));
            Assert.That(skillBuilder.IsVisible, Is.True);
            Assert.That(Object.FindFirstObjectByType<ThreeVsThreeMatchManager>(), Is.Null);
            Assert.That(skillBuilder.TrySaveDraft(), Is.True);
            yield return null;

            Assert.That(bootstrap.IsChoosingAppearance, Is.True);
            Assert.That(bootstrap.SelectedAppearanceName, Is.EqualTo("CLASSIC KIT"));
            Assert.That(Object.FindFirstObjectByType<ThreeVsThreeMatchManager>(), Is.Null);
            Assert.That(Object.FindObjectsByType<CharacterHealth>(
                FindObjectsSortMode.None), Is.Empty);
            bootstrap.ConfirmAppearanceSelection();
            yield return null;

            CharacterHealth[] combatants = Object.FindObjectsByType<CharacterHealth>(
                FindObjectsSortMode.None);
            CharacterStatusController[] statuses =
                Object.FindObjectsByType<CharacterStatusController>(FindObjectsSortMode.None);
            ElementStatusController[] elementStatuses =
                Object.FindObjectsByType<ElementStatusController>(FindObjectsSortMode.None);
            CharacterShieldController[] shields =
                Object.FindObjectsByType<CharacterShieldController>(FindObjectsSortMode.None);
            ThreeVsThreeMatchManager match =
                Object.FindFirstObjectByType<ThreeVsThreeMatchManager>();
            WorldHealthBar[] healthBars = Object.FindObjectsByType<WorldHealthBar>(
                FindObjectsSortMode.None);
            CombatHud combatHud = Object.FindFirstObjectByType<CombatHud>();

            Assert.That(combatants, Has.Length.EqualTo(6));
            Assert.That(statuses, Has.Length.EqualTo(6));
            Assert.That(elementStatuses, Has.Length.EqualTo(6));
            Assert.That(shields, Has.Length.EqualTo(6));
            Assert.That(statuses[0].Apply(CrowdControlType.Stun, Time.time), Is.True);
            Assert.That(statuses[0].ActiveEffect, Is.EqualTo(CrowdControlType.Stun));
            Assert.That(statuses[0].IsActionBlocked, Is.True);
            statuses[0].Clear();
            Assert.That(healthBars, Has.Length.EqualTo(6));
            Assert.That(healthBars, Has.All.Matches<WorldHealthBar>(bar => bar.Health != null));
            Assert.That(healthBars, Has.All.Matches<WorldHealthBar>(bar => bar.Status != null));
            Assert.That(match, Is.Not.Null);
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.RoundActive));
            Assert.That(UnityEngine.Camera.main, Is.Not.Null);
            Assert.That(UnityEngine.Camera.main.GetComponent<AudioListener>(), Is.Not.Null);
            Assert.That(combatHud, Is.Not.Null);
            Assert.That(combatHud.IsConfigured, Is.True);
            Assert.That(combatHud.Health, Is.Not.Null);
            DemoCombatAudio[] combatAudio = Object.FindObjectsByType<DemoCombatAudio>(
                FindObjectsSortMode.None);
            Assert.That(combatAudio, Has.Length.EqualTo(6));
            Assert.That(combatAudio,
                Has.All.Matches<DemoCombatAudio>(audio => audio.IsReady));
            Assert.That(DemoCombatAudio.GeneratedSampleCount, Is.InRange(1, 20000));
            DemoDamageFeedback[] damageFeedback =
                Object.FindObjectsByType<DemoDamageFeedback>(FindObjectsSortMode.None);
            Assert.That(damageFeedback, Has.Length.EqualTo(6));
            Assert.That(damageFeedback,
                Has.All.Matches<DemoDamageFeedback>(feedback => feedback.IsConfigured));
            DemoDamageFeedback feedbackTarget = damageFeedback[0];
            CharacterHealth feedbackHealth =
                feedbackTarget.GetComponent<CharacterHealth>();
            Assert.That(feedbackHealth.ApplyDamage(
                new DamageRequest(null, 1f, 1f, 1f, 1f)), Is.EqualTo(1f));
            Assert.That(feedbackTarget.IsShowingDamage, Is.True);
            Assert.That(feedbackTarget.DisplayedDamage, Is.EqualTo(1f));
            feedbackHealth.ResetHealth();
            DemoSkillVfxPlayer[] skillVfxPlayers =
                Object.FindObjectsByType<DemoSkillVfxPlayer>(FindObjectsSortMode.None);
            Assert.That(skillVfxPlayers, Has.Length.EqualTo(6));
            DemoAISkillUser[] aiSkillUsers = Object.FindObjectsByType<DemoAISkillUser>(
                FindObjectsSortMode.None);
            Assert.That(aiSkillUsers, Has.Length.EqualTo(5));
            Assert.That(aiSkillUsers,
                Has.All.Matches<DemoAISkillUser>(user =>
                    user.GetComponent<DemoSkillController>() != null));
            DemoSkillController playerSkills =
                Object.FindObjectsByType<DemoSkillController>(FindObjectsSortMode.None)
                    .Single(skills => skills.GetComponent<DemoAISkillUser>() == null);
            Assert.That(playerSkills, Is.Not.Null);
            Assert.That(playerSkills.QSkillDefinition.DamageCoefficient,
                Is.EqualTo(1.32f).Within(0.001f));
            Assert.That(playerSkills.QSkillDefinition.Cooldown, Is.EqualTo(3f));
            Assert.That(playerSkills.QSkillDefinition.Element, Is.EqualTo(SkillElement.Fire));
            SkillDefinition playerQ = playerSkills.QSkillDefinition;
            Vector3 aimPoint = playerSkills.transform.position +
                               (playerSkills.transform.forward * 5f);
            Assert.That(playerSkills.TryCastAt(SkillSlot.Q, aimPoint, Time.time), Is.True);
            Assert.That(playerSkills.IsCasting, Is.True);
            yield return new WaitForSeconds(playerQ.CastDelay + 0.05f);
            yield return null;
            Assert.That(playerSkills.IsCasting, Is.False);
            DemoProjectile playerProjectile =
                Object.FindObjectsByType<DemoProjectile>(FindObjectsSortMode.None)
                    .FirstOrDefault(projectile =>
                        projectile.Owner == playerSkills.transform);
            Assert.That(playerProjectile, Is.Not.Null);
            Assert.That(playerProjectile.RemainingDistance, Is.LessThan(playerQ.Range));
            Assert.That(
                playerProjectile.GetComponentInChildren<ParticleSystem>(),
                Is.Not.Null);
            Assert.That(GameObject.Find("Tank Visual"), Is.Not.Null);
            Assert.That(GameObject.Find("Mage Visual"), Is.Not.Null);
            Assert.That(GameObject.Find("Assassin Visual"), Is.Not.Null);
            Assert.That(GameObject.Find("Stick Head"), Is.Not.Null);
            Assert.That(GameObject.Find("Stick Torso"), Is.Not.Null);
            Assert.That(GameObject.Find("Tank Shield"), Is.Not.Null);
            Assert.That(GameObject.Find("Mage Staff"), Is.Not.Null);
            Assert.That(GameObject.Find("Assassin Left Dagger"), Is.Not.Null);
            Assert.That(GameObject.Find("Assassin Right Dagger"), Is.Not.Null);
            Assert.That(GameObject.Find("Tank Shoulder"), Is.Null);
            Assert.That(GameObject.Find("Mage Robe"), Is.Null);
            Assert.That(GameObject.Find("Assassin Hood"), Is.Null);
            DemoStickFigureAnimator[] stickAnimators =
                Object.FindObjectsByType<DemoStickFigureAnimator>(FindObjectsSortMode.None);
            Assert.That(stickAnimators, Has.Length.EqualTo(6));
            Assert.That(stickAnimators,
                Has.All.Matches<DemoStickFigureAnimator>(animator => animator.IsConfigured));
        }

        [UnityTest]
        public IEnumerator DemoProjectile_MovesAndImpactsBlockingWall()
        {
            var owner = new GameObject("Projectile Owner");
            owner.transform.position = new Vector3(30f, 1f, 30f);
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Projectile Test Wall";
            wall.transform.position = new Vector3(32f, 1f, 30f);
            wall.transform.localScale = new Vector3(1f, 2f, 2f);
            var projectileObject = new GameObject("Projectile Collision Test");
            projectileObject.transform.position = owner.transform.position;
            DemoProjectile projectile = projectileObject.AddComponent<DemoProjectile>();
            bool impacted = false;
            Vector3 impactPoint = Vector3.zero;
            projectile.Initialize(
                owner.transform,
                Vector3.right,
                speed: 10f,
                radius: 0.1f,
                maximumDistance: 5f,
                onImpact: position =>
                {
                    impacted = true;
                    impactPoint = position;
                });

            float timeout = Time.time + 1f;
            while (!impacted && Time.time < timeout)
            {
                yield return null;
            }

            Assert.That(impacted, Is.True);
            Assert.That(impactPoint.x, Is.LessThan(32f));
            Object.Destroy(owner);
            Object.Destroy(wall);
        }
    }
}
