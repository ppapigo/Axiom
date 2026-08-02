using System.Collections.Generic;
using Axiom.AI;
using Axiom.Camera;
using Axiom.Character;
using Axiom.Combat;
using Axiom.Data;
using Axiom.Input;
using Axiom.Manager;
using Axiom.Role;
using Axiom.Skill;
using Axiom.Skill.Generation;
using Axiom.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Axiom.Demo
{
    [DisallowMultipleComponent]
    public sealed class DemoArenaBootstrap : MonoBehaviour
    {
        private readonly List<InputAction> _runtimeActions = new List<InputAction>();
        private readonly List<CharacterHealth> _teamAHealth = new List<CharacterHealth>();
        private readonly List<CharacterHealth> _teamBHealth = new List<CharacterHealth>();
        private readonly Dictionary<CharacterRoleId, CharacterRoleDefinition> _roles =
            new Dictionary<CharacterRoleId, CharacterRoleDefinition>();
        private readonly RoleElementPool _roleElementPool = new RoleElementPool();

        [SerializeField] private EquipmentAppearanceDefinition[] equipmentAppearances;

        private UnityEngine.Camera _mainCamera;
        private CharacterMovementProfile _movementProfile;
        private CameraFollowProfile _cameraProfile;
        private AIBehaviourProfile _aiProfile;
        private SkillBalanceProfile _skillBalance;
        private BasicAttackProfile _meleeAttack;
        private BasicAttackProfile _rangedAttack;
        private ThreeVsThreeMatchManager _matchManager;
        private CharacterHealth _playerHealth;
        private SkillBuilderPanel _skillBuilderPanel;
        private CharacterRoleId? _selectedRole;
        private bool _gameStarted;
        private TeamId? _winner;
        private string _roundMessage = "Choose your role";

        private void Awake()
        {
            CreateEnvironment();
            CreateRuntimeProfiles();
        }

        private void OnDestroy()
        {
            if (_skillBuilderPanel != null)
            {
                _skillBuilderPanel.DraftSaved -= StartMatchAfterSkillSaved;
            }

            foreach (InputAction action in _runtimeActions)
            {
                action.Dispose();
            }
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(12f, 12f, 300f, 118f), "AXIOM");
            GUI.Label(new Rect(28f, 42f, 270f, 24f), _roundMessage);
            if (_matchManager != null)
            {
                GUI.Label(
                    new Rect(28f, 66f, 270f, 24f),
                    $"BLUE {_matchManager.TeamAWins}  :  {_matchManager.TeamBWins} RED");
            }

            if (_playerHealth != null)
            {
                GUI.Label(
                    new Rect(28f, 90f, 270f, 24f),
                    $"HP {Mathf.CeilToInt(_playerHealth.CurrentHealth)} / " +
                    $"{Mathf.CeilToInt(_playerHealth.MaximumHealth)}");
            }

            if (!_gameStarted)
            {
                if (_selectedRole.HasValue)
                {
                    DrawSkillSetup(
                        _selectedRole.Value,
                        _skillBuilderPanel.CurrentSlot);
                }
                else
                {
                    DrawRoleSelection();
                }

                return;
            }

            GUI.Box(
                new Rect(12f, Screen.height - 58f, 360f, 42f),
                "WASD Move | Mouse Aim | LMB Attack | B Skill Forge");
            DrawTeamHealth();

            if (_winner.HasValue && GUI.Button(
                    new Rect((Screen.width - 220f) * 0.5f, Screen.height * 0.56f, 220f, 52f),
                    "PLAY AGAIN"))
            {
                _winner = null;
                _roundMessage = "Restarting match";
                _matchManager.StartMatch();
            }
        }

        private void DrawRoleSelection()
        {
            float width = 420f;
            float left = (Screen.width - width) * 0.5f;
            float top = Screen.height * 0.32f;
            GUI.Box(new Rect(left, top, width, 210f), "SELECT YOUR ROLE");
            if (GUI.Button(new Rect(left + 30f, top + 50f, 110f, 110f), "TANK\n1400 HP"))
            {
                StartDemo(CharacterRoleId.Tank);
            }

            if (GUI.Button(new Rect(left + 155f, top + 50f, 110f, 110f), "MAGE\nAREA"))
            {
                StartDemo(CharacterRoleId.Mage);
            }

            if (GUI.Button(new Rect(left + 280f, top + 50f, 110f, 110f), "ASSASSIN\nFAST"))
            {
                StartDemo(CharacterRoleId.Assassin);
            }
        }

        private static void DrawSkillSetup(CharacterRoleId role, SkillSlot slot)
        {
            float width = 420f;
            float left = (Screen.width - width) * 0.5f;
            float top = Screen.height * 0.22f;
            GUI.Box(new Rect(left, top, width, 86f), $"{role} {slot} SKILL SETUP");
            GUI.Label(
                new Rect(left + 34f, top + 38f, width - 68f, 28f),
                "Save Q, E and R drafts to start the match.");
        }

        private void DrawTeamHealth()
        {
            float right = Screen.width - 230f;
            GUI.Box(new Rect(right, 12f, 218f, 172f), "COMBATANTS");
            for (int i = 0; i < _teamAHealth.Count; i++)
            {
                DrawHealthLine(_teamAHealth[i], right + 12f, 42f + (i * 22f), "BLUE");
                DrawHealthLine(_teamBHealth[i], right + 12f, 112f + (i * 22f), "RED");
            }
        }

        private static void DrawHealthLine(
            CharacterHealth health,
            float x,
            float y,
            string team)
        {
            string state = health.IsDead
                ? "DEAD"
                : $"{Mathf.CeilToInt(health.CurrentHealth)}";
            GUI.Label(new Rect(x, y, 194f, 20f), $"{team} {health.name}: {state}");
        }

        public void StartDemo(CharacterRoleId selectedRole)
        {
            if (_gameStarted)
            {
                return;
            }

            _selectedRole = selectedRole;
            RegisterDefaultElements(selectedRole);
            PrepareSkillSlot(SkillSlot.Q);
        }

        private void PrepareSkillSlot(SkillSlot slot)
        {
            if (!_selectedRole.HasValue)
            {
                return;
            }

            _roundMessage = $"{_selectedRole.Value}: Create {slot} skill";
            _skillBuilderPanel.SetContext(
                _roles[_selectedRole.Value],
                slot,
                _roleElementPool);
            if (!_skillBuilderPanel.IsVisible)
            {
                _skillBuilderPanel.ToggleVisibility();
            }
        }

        private void StartMatchAfterSkillSaved(SkillDraft draft)
        {
            if (_gameStarted || !_selectedRole.HasValue)
            {
                return;
            }

            switch (draft.Slot)
            {
                case SkillSlot.Q:
                    PrepareSkillSlot(SkillSlot.E);
                    break;
                case SkillSlot.E:
                    PrepareSkillSlot(SkillSlot.Ultimate);
                    break;
                case SkillSlot.Ultimate:
                    StartMatch(_selectedRole.Value);
                    break;
            }
        }

        private void StartMatch(CharacterRoleId selectedRole)
        {
            _gameStarted = true;
            _roundMessage = "Preparing round 1";

            CharacterRoleId[] teamARoles = BuildPlayerTeam(selectedRole);
            CharacterRoleId[] teamBRoles =
            {
                CharacterRoleId.Tank,
                CharacterRoleId.Mage,
                CharacterRoleId.Assassin
            };
            Vector3[] teamASpawns =
            {
                new Vector3(-6f, 1f, 0f),
                new Vector3(-7f, 1f, -3f),
                new Vector3(-7f, 1f, 3f)
            };
            Vector3[] teamBSpawns =
            {
                new Vector3(6f, 1f, 0f),
                new Vector3(7f, 1f, 3f),
                new Vector3(7f, 1f, -3f)
            };

            var teamA = new MatchParticipant[3];
            var teamB = new MatchParticipant[3];
            for (int i = 0; i < 3; i++)
            {
                teamA[i] = CreateCharacter(
                    $"{teamARoles[i]}", TeamId.TeamA, teamARoles[i],
                    teamASpawns[i], i == 0);
                teamB[i] = CreateCharacter(
                    $"{teamBRoles[i]}", TeamId.TeamB, teamBRoles[i],
                    teamBSpawns[i], false);
            }

            GameObject managerObject = new GameObject("ThreeVsThreeMatch");
            _matchManager = managerObject.AddComponent<ThreeVsThreeMatchManager>();
            _matchManager.Configure(teamA, teamB, 2, 2f, true);
            _matchManager.RoundStarted += round => _roundMessage = $"ROUND {round} - FIGHT!";
            _matchManager.RoundEnded += result =>
                _roundMessage = $"ROUND {result.RoundNumber}: {result.Winner} WINS";
            _matchManager.MatchEnded += winner =>
            {
                _winner = winner;
                _roundMessage = $"MATCH WINNER: {winner}";
            };
        }

        private MatchParticipant CreateCharacter(
            string characterName,
            TeamId team,
            CharacterRoleId roleId,
            Vector3 spawnPosition,
            bool isPlayer)
        {
            GameObject character = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            character.name = characterName;
            character.transform.position = spawnPosition;
            Destroy(character.GetComponent<CapsuleCollider>());
            Renderer renderer = character.GetComponent<Renderer>();
            SetColor(renderer, team == TeamId.TeamA
                ? RoleColor(roleId, true)
                : RoleColor(roleId, false));
            DemoRoleVisualBuilder.Build(
                character.transform,
                roleId,
                team == TeamId.TeamA,
                FindEquipmentAppearance(roleId));

            CharacterController controller = character.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.5f;
            TeamMember teamMember = character.AddComponent<TeamMember>();
            teamMember.SetTeam(team);
            CharacterRole role = character.AddComponent<CharacterRole>();
            role.SetDefinition(_roles[roleId]);
            character.AddComponent<CharacterStats>();
            CharacterShieldController shield =
                character.AddComponent<CharacterShieldController>();
            shield.Configure(_skillBalance);
            CharacterHealth health = character.AddComponent<CharacterHealth>();
            CharacterStatusController status =
                character.AddComponent<CharacterStatusController>();
            status.Configure(_skillBalance);
            ElementStatusController elementStatus =
                character.AddComponent<ElementStatusController>();
            elementStatus.Configure(_skillBalance);
            WorldHealthBar healthBar = character.AddComponent<WorldHealthBar>();
            healthBar.Configure(health, _mainCamera, team, characterName);
            BasicAttackController basicAttack = character.AddComponent<BasicAttackController>();

            var combatBehaviours = new List<Behaviour>();
            combatBehaviours.Add(status);
            combatBehaviours.Add(elementStatus);
            if (isPlayer)
            {
                ConfigurePlayer(character, basicAttack, roleId, combatBehaviours);
                _playerHealth = health;
                FixedQuarterViewCamera follow = _mainCamera.gameObject.AddComponent<FixedQuarterViewCamera>();
                follow.SetProfile(_cameraProfile);
                follow.SetTarget(character.transform);
            }
            else
            {
                basicAttack.Configure(GetAttackProfile(roleId));
                DemoSkillController aiSkills =
                    character.AddComponent<DemoSkillController>();
                DemoAISkillUser aiSkillUser =
                    character.AddComponent<DemoAISkillUser>();
                aiSkillUser.Configure(_skillBalance);
                AIController ai = character.AddComponent<AIController>();
                ai.Configure(_aiProfile, aiSkillUser);
                combatBehaviours.Add(aiSkills);
                combatBehaviours.Add(aiSkillUser);
                combatBehaviours.Add(ai);
            }

            Transform spawn = new GameObject($"Spawn_{team}_{characterName}").transform;
            spawn.SetPositionAndRotation(spawnPosition, Quaternion.identity);
            var participant = new MatchParticipant();
            participant.Configure(teamMember, spawn, combatBehaviours.ToArray());
            health.Died += () => participant.SetCombatEnabled(false);

            if (team == TeamId.TeamA)
            {
                _teamAHealth.Add(health);
            }
            else
            {
                _teamBHealth.Add(health);
            }

            return participant;
        }

        private EquipmentAppearanceDefinition FindEquipmentAppearance(CharacterRoleId roleId)
        {
            if (equipmentAppearances == null)
            {
                return null;
            }

            foreach (EquipmentAppearanceDefinition appearance in equipmentAppearances)
            {
                if (appearance != null && appearance.Role == roleId)
                {
                    return appearance;
                }
            }

            return null;
        }

        private void ConfigurePlayer(
            GameObject character,
            BasicAttackController basicAttack,
            CharacterRoleId roleId,
            ICollection<Behaviour> combatBehaviours)
        {
            InputAction move = CreateMoveAction();
            InputAction aim = AddAction(new InputAction(
                "Aim", InputActionType.PassThrough, "<Pointer>/position", expectedControlType: "Vector2"));
            InputAction attack = AddAction(new InputAction(
                "Attack", InputActionType.Button, "<Mouse>/leftButton"));
            InputAction dash = AddAction(new InputAction(
                "Dash", InputActionType.Button, "<Keyboard>/space"));

            InputActionMovementSource movementSource =
                character.AddComponent<InputActionMovementSource>();
            movementSource.Configure(move);
            InputActionBasicAttackSource attackSource =
                character.AddComponent<InputActionBasicAttackSource>();
            attackSource.Configure(aim, attack, _mainCamera);
            InputActionDashSource dashSource = character.AddComponent<InputActionDashSource>();
            dashSource.Configure(dash);

            CharacterMovement movement = character.AddComponent<CharacterMovement>();
            movement.Configure(_movementProfile, movementSource);
            CharacterAimController aimController =
                character.AddComponent<CharacterAimController>();
            aimController.Configure(attackSource);
            CharacterDashController dashController = character.AddComponent<CharacterDashController>();
            dashController.Configure(dashSource, movementSource);
            basicAttack.Configure(GetAttackProfile(roleId), attackSource);
            DemoSkillController skills = character.AddComponent<DemoSkillController>();
            skills.Configure(_mainCamera, _skillBalance, _skillBuilderPanel);
            CombatHud combatHud = character.AddComponent<CombatHud>();
            combatHud.Configure(
                character.GetComponent<CharacterHealth>(),
                skills,
                dashController,
                character.GetComponent<CharacterRole>());

            combatBehaviours.Add(movement);
            combatBehaviours.Add(aimController);
            combatBehaviours.Add(dashController);
            combatBehaviours.Add(basicAttack);
            combatBehaviours.Add(skills);
        }

        private void RegisterDefaultElements(CharacterRoleId role)
        {
            _roleElementPool.TryAssign(
                role,
                SkillSlot.Q,
                DemoSkillDefinitionFactory.GetDefaultElement(role, SkillSlot.Q));
            _roleElementPool.TryAssign(
                role,
                SkillSlot.E,
                DemoSkillDefinitionFactory.GetDefaultElement(role, SkillSlot.E));
            _roleElementPool.TryAssign(
                role,
                SkillSlot.Ultimate,
                DemoSkillDefinitionFactory.GetDefaultElement(role, SkillSlot.Ultimate));
        }

        private void CreateEnvironment()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            _mainCamera = cameraObject.AddComponent<UnityEngine.Camera>();
            _mainCamera.clearFlags = CameraClearFlags.SolidColor;
            _mainCamera.backgroundColor = new Color(0.025f, 0.035f, 0.06f);

            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.3f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            CreateBlock("Arena Floor", new Vector3(0f, -0.3f, 0f),
                new Vector3(24f, 0.6f, 18f), new Color(0.08f, 0.1f, 0.15f));
            CreateBlock("North Wall", new Vector3(0f, 1f, 9f),
                new Vector3(24f, 2f, 0.5f), new Color(0.18f, 0.2f, 0.28f));
            CreateBlock("South Wall", new Vector3(0f, 1f, -9f),
                new Vector3(24f, 2f, 0.5f), new Color(0.18f, 0.2f, 0.28f));
            CreateBlock("West Wall", new Vector3(-12f, 1f, 0f),
                new Vector3(0.5f, 2f, 18f), new Color(0.18f, 0.2f, 0.28f));
            CreateBlock("East Wall", new Vector3(12f, 1f, 0f),
                new Vector3(0.5f, 2f, 18f), new Color(0.18f, 0.2f, 0.28f));
            CreateBlock("Left Cover", new Vector3(-1.5f, 1f, -3.5f),
                new Vector3(1.2f, 2f, 4f), new Color(0.25f, 0.27f, 0.34f));
            CreateBlock("Right Cover", new Vector3(1.5f, 1f, 3.5f),
                new Vector3(1.2f, 2f, 4f), new Color(0.25f, 0.27f, 0.34f));
        }

        private void CreateRuntimeProfiles()
        {
            _movementProfile = ScriptableObject.CreateInstance<CharacterMovementProfile>();
            _cameraProfile = ScriptableObject.CreateInstance<CameraFollowProfile>();
            _aiProfile = ScriptableObject.CreateInstance<AIBehaviourProfile>();
            _aiProfile.ConfigureForDemo(20f, 4.5f, 4f, 6f);
            _skillBalance = ScriptableObject.CreateInstance<SkillBalanceProfile>();
            _skillBuilderPanel = gameObject.AddComponent<SkillBuilderPanel>();
            _skillBuilderPanel.Configure(_skillBalance);
            _skillBuilderPanel.ConfigureGeneration(
                new MockSkillGenerationProvider(),
                DemoSkillDefinitionFactory.Create);
            _skillBuilderPanel.DraftSaved += StartMatchAfterSkillSaved;
            _meleeAttack = ScriptableObject.CreateInstance<BasicAttackProfile>();
            _meleeAttack.Configure(BasicAttackDeliveryType.Melee, 2.2f, 0.6f, 0.75f);
            _rangedAttack = ScriptableObject.CreateInstance<BasicAttackProfile>();
            _rangedAttack.Configure(BasicAttackDeliveryType.Ranged, 7f, 0.45f, 0.8f);
            _roles.Add(CharacterRoleId.Tank, ScriptableObject.CreateInstance<TankRoleDefinition>());
            _roles.Add(CharacterRoleId.Mage, ScriptableObject.CreateInstance<MageRoleDefinition>());
            _roles.Add(CharacterRoleId.Assassin, ScriptableObject.CreateInstance<AssassinRoleDefinition>());
        }

        private BasicAttackProfile GetAttackProfile(CharacterRoleId roleId)
        {
            return roleId == CharacterRoleId.Tank ? _meleeAttack : _rangedAttack;
        }

        private InputAction CreateMoveAction()
        {
            var action = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            action.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            return AddAction(action);
        }

        private InputAction AddAction(InputAction action)
        {
            _runtimeActions.Add(action);
            action.Enable();
            return action;
        }

        private static CharacterRoleId[] BuildPlayerTeam(CharacterRoleId selected)
        {
            var result = new List<CharacterRoleId> { selected };
            foreach (CharacterRoleId role in new[]
                     {
                         CharacterRoleId.Tank,
                         CharacterRoleId.Mage,
                         CharacterRoleId.Assassin
                     })
            {
                if (role != selected)
                {
                    result.Add(role);
                }
            }

            return result.ToArray();
        }

        private static Color RoleColor(CharacterRoleId role, bool blueTeam)
        {
            float accent = role == CharacterRoleId.Tank ? 0.15f :
                role == CharacterRoleId.Mage ? 0.35f : 0.55f;
            return blueTeam
                ? new Color(accent, 0.45f, 1f)
                : new Color(1f, 0.15f + accent, 0.18f);
        }

        private static void CreateBlock(
            string blockName,
            Vector3 position,
            Vector3 scale,
            Color color)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = blockName;
            block.transform.position = position;
            block.transform.localScale = scale;
            SetColor(block.GetComponent<Renderer>(), color);
        }

        private static void SetColor(Renderer renderer, Color color)
        {
            renderer.material = CreateDemoMaterial(color);
        }

        internal static Material CreateDemoMaterial(Color color)
        {
            Shader shader = Shader.Find("Axiom/DemoUnlit");
            if (shader == null)
            {
                throw new MissingReferenceException(
                    "The Axiom/DemoUnlit shader must be included in the player build.");
            }

            var material = new Material(shader);
            material.color = color;
            return material;
        }
    }
}
