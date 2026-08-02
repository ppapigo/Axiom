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
        private enum BuiltInAppearanceStyle
        {
            Classic,
            Obsidian,
            Ivory,
            Custom
        }

        private readonly List<InputAction> _runtimeActions = new List<InputAction>();
        private readonly List<CharacterHealth> _teamAHealth = new List<CharacterHealth>();
        private readonly List<CharacterHealth> _teamBHealth = new List<CharacterHealth>();
        private readonly Dictionary<CharacterRoleId, CharacterRoleDefinition> _roles =
            new Dictionary<CharacterRoleId, CharacterRoleDefinition>();
        private readonly RoleElementPool _roleElementPool = new RoleElementPool();

        [SerializeField] private EquipmentAppearanceDefinition[] equipmentAppearances;
        [SerializeField] private SkillGenerationApiSettings skillGenerationApiSettings;
        [SerializeField] private SkillVfxLibrary skillVfxLibrary;

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
        private EquipmentAppearanceDefinition _selectedEquipmentAppearance;
        private BuiltInAppearanceStyle _selectedAppearanceStyle;
        private bool _isChoosingAppearance;
        private bool _gameStarted;
        private TeamId? _winner;
        private string _roundMessage = "Choose your role";

        public bool IsChoosingAppearance => _isChoosingAppearance;
        public string SelectedAppearanceName => GetSelectedAppearanceName();

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
                    if (_isChoosingAppearance)
                    {
                        DrawAppearanceSelection(_selectedRole.Value);
                    }
                    else
                    {
                        DrawSkillSetup(
                            _selectedRole.Value,
                            _skillBuilderPanel.CurrentSlot);
                    }
                }
                else
                {
                    DrawRoleSelection();
                }

                return;
            }

            GUI.Box(
                new Rect(12f, Screen.height - 58f, 650f, 42f),
                "WASD MOVE | MOUSE AIM | LMB ATTACK | Q / E / R SKILLS | SPACE DASH | B FORGE");
            DrawTeamHealth();
            DrawRoundBanner();

            if (_winner.HasValue)
            {
                DrawMatchResult(_winner.Value);
            }
        }

        private void DrawRoleSelection()
        {
            const float width = 760f;
            const float height = 390f;
            float left = (Screen.width - width) * 0.5f;
            float top = Mathf.Max(72f, (Screen.height - height) * 0.44f);
            GUI.Box(new Rect(left, top, width, height), string.Empty);
            GUI.Label(
                new Rect(left + 24f, top + 18f, width - 48f, 48f),
                "AXIOM",
                CreateCenteredStyle(34, new Color(0.25f, 0.85f, 1f), FontStyle.Bold));
            GUI.Label(
                new Rect(left + 24f, top + 62f, width - 48f, 30f),
                "CREATE YOUR SKILLS. ENTER THE ARENA. OUTPLAY THE AI.",
                CreateCenteredStyle(14, Color.white, FontStyle.Bold));
            GUI.Box(
                new Rect(left + 76f, top + 98f, width - 152f, 42f),
                "3 VS 3  |  TEAM WIPE = 1 ROUND  |  FIRST TO 2 WINS");
            GUI.Label(
                new Rect(left + 24f, top + 148f, width - 48f, 24f),
                "SELECT YOUR ROLE",
                CreateCenteredStyle(16, new Color(1f, 0.82f, 0.25f), FontStyle.Bold));

            const float cardWidth = 220f;
            const float cardHeight = 160f;
            if (GUI.Button(
                    new Rect(left + 30f, top + 184f, cardWidth, cardHeight),
                    "TANK\n\nHP 1400  |  ATK 80\nMOVE 95%  |  DASH 4m\n\nFRONTLINE / TAUNT"))
            {
                StartDemo(CharacterRoleId.Tank);
            }

            if (GUI.Button(
                    new Rect(left + 270f, top + 184f, cardWidth, cardHeight),
                    "MAGE\n\nHP 900  |  ATK 115\nMOVE 100%  |  DASH 4m\n\nRANGED / AREA"))
            {
                StartDemo(CharacterRoleId.Mage);
            }

            if (GUI.Button(
                    new Rect(left + 510f, top + 184f, cardWidth, cardHeight),
                    "ASSASSIN\n\nHP 900  |  ATK 115\nMOVE 110%  |  DASH 8m\n\nDIVE / EXECUTE"))
            {
                StartDemo(CharacterRoleId.Assassin);
            }

            GUI.Label(
                new Rect(left + 24f, top + 354f, width - 48f, 22f),
                "NEXT: BUILD Q, E AND R WITH THE 100 POINT SKILL FORGE",
                CreateCenteredStyle(12, new Color(0.72f, 0.78f, 0.86f), FontStyle.Normal));
        }

        private static void DrawSkillSetup(CharacterRoleId role, SkillSlot slot)
        {
            const float width = 620f;
            float left = (Screen.width - width) * 0.5f;
            const float top = 12f;
            int step = slot == SkillSlot.Q ? 1 : slot == SkillSlot.E ? 2 : 3;
            GUI.Box(
                new Rect(left, top, width, 86f),
                $"{role.ToString().ToUpperInvariant()} LOADOUT  |  STEP {step} / 3");
            GUI.Label(
                new Rect(left + 24f, top + 34f, width - 48f, 24f),
                $"{GetSlotMarker(slot, SkillSlot.Q)} Q     " +
                $"{GetSlotMarker(slot, SkillSlot.E)} E     " +
                $"{GetSlotMarker(slot, SkillSlot.Ultimate)} R",
                CreateCenteredStyle(15, Color.white, FontStyle.Bold));
            GUI.Label(
                new Rect(left + 24f, top + 59f, width - 48f, 20f),
                "DESCRIBE OR MANUALLY BUILD THE SKILL, THEN CONFIRM & SAVE",
                CreateCenteredStyle(11, new Color(0.72f, 0.78f, 0.86f), FontStyle.Normal));
        }

        private void DrawAppearanceSelection(CharacterRoleId role)
        {
            const float width = 820f;
            const float height = 430f;
            float left = (Screen.width - width) * 0.5f;
            float top = Mathf.Max(70f, (Screen.height - height) * 0.46f);
            GUI.Box(new Rect(left, top, width, height), string.Empty);
            GUI.Label(
                new Rect(left + 24f, top + 18f, width - 48f, 38f),
                "CHOOSE APPEARANCE ITEM",
                CreateCenteredStyle(26, new Color(0.25f, 0.85f, 1f), FontStyle.Bold));
            GUI.Label(
                new Rect(left + 24f, top + 58f, width - 48f, 24f),
                $"{role.ToString().ToUpperInvariant()}  |  SKILLS SAVED 3 / 3  |  FINAL STEP",
                CreateCenteredStyle(13, Color.white, FontStyle.Bold));
            GUI.Label(
                new Rect(left + 24f, top + 88f, width - 48f, 22f),
                "BUILT-IN ITEMS USE NO MODEL ASSETS. CUSTOM PREFABS APPEAR BELOW WHEN CONNECTED.",
                CreateCenteredStyle(11, new Color(0.72f, 0.78f, 0.86f), FontStyle.Normal));

            DrawBuiltInAppearanceButton(
                new Rect(left + 35f, top + 126f, 235f, 104f),
                BuiltInAppearanceStyle.Classic,
                "CLASSIC KIT",
                "Role default colours");
            DrawBuiltInAppearanceButton(
                new Rect(left + 293f, top + 126f, 235f, 104f),
                BuiltInAppearanceStyle.Obsidian,
                "OBSIDIAN KIT",
                "Dark arena armour");
            DrawBuiltInAppearanceButton(
                new Rect(left + 551f, top + 126f, 235f, 104f),
                BuiltInAppearanceStyle.Ivory,
                "IVORY KIT",
                "Light ceremonial armour");

            int customCount = 0;
            if (equipmentAppearances != null)
            {
                foreach (EquipmentAppearanceDefinition appearance in equipmentAppearances)
                {
                    if (appearance == null || appearance.Role != role || customCount >= 3)
                    {
                        continue;
                    }

                    float customLeft = left + 35f + (customCount * 258f);
                    bool selected = _selectedAppearanceStyle == BuiltInAppearanceStyle.Custom &&
                                    _selectedEquipmentAppearance == appearance;
                    string marker = selected ? "[SELECTED]" : "[CUSTOM MODEL]";
                    if (GUI.Button(
                            new Rect(customLeft, top + 250f, 235f, 72f),
                            $"{marker} {appearance.DisplayName}\n{appearance.Description}"))
                    {
                        _selectedAppearanceStyle = BuiltInAppearanceStyle.Custom;
                        _selectedEquipmentAppearance = appearance;
                    }
                    customCount++;
                }
            }

            if (customCount == 0)
            {
                GUI.Box(
                    new Rect(left + 160f, top + 252f, width - 320f, 58f),
                    "CUSTOM MODEL SLOTS READY\nConnect EquipmentAppearanceDefinition assets later");
            }

            GUI.Label(
                new Rect(left + 24f, top + 332f, width - 48f, 22f),
                $"SELECTED: {GetSelectedAppearanceName()}",
                CreateCenteredStyle(13, new Color(1f, 0.82f, 0.25f), FontStyle.Bold));
            if (GUI.Button(
                    new Rect(left + 280f, top + 365f, width - 560f, 48f),
                    "START 3 VS 3 MATCH"))
            {
                ConfirmAppearanceSelection();
            }
        }

        private void DrawBuiltInAppearanceButton(
            Rect rect,
            BuiltInAppearanceStyle style,
            string title,
            string description)
        {
            bool selected = _selectedAppearanceStyle == style;
            string marker = selected ? "[SELECTED]" : "[SELECT]";
            if (GUI.Button(rect, $"{marker}\n{title}\n{description}"))
            {
                _selectedAppearanceStyle = style;
                _selectedEquipmentAppearance = null;
            }
        }

        private string GetSelectedAppearanceName()
        {
            return _selectedAppearanceStyle == BuiltInAppearanceStyle.Custom &&
                   _selectedEquipmentAppearance != null
                ? _selectedEquipmentAppearance.DisplayName
                : _selectedAppearanceStyle switch
                {
                    BuiltInAppearanceStyle.Obsidian => "OBSIDIAN KIT",
                    BuiltInAppearanceStyle.Ivory => "IVORY KIT",
                    _ => "CLASSIC KIT"
                };
        }

        private Color? GetSelectedBuiltInTint()
        {
            return _selectedAppearanceStyle switch
            {
                BuiltInAppearanceStyle.Obsidian => new Color(0.08f, 0.09f, 0.12f),
                BuiltInAppearanceStyle.Ivory => new Color(0.78f, 0.75f, 0.65f),
                _ => null
            };
        }

        private void DrawRoundBanner()
        {
            const float width = 390f;
            float left = (Screen.width - width) * 0.5f;
            GUI.Box(new Rect(left, 12f, width, 54f), _roundMessage);
            GUI.Label(
                new Rect(left + 12f, 37f, width - 24f, 20f),
                "FIRST TO 2  |  ELIMINATE ALL 3 ENEMIES",
                CreateCenteredStyle(11, new Color(0.7f, 0.78f, 0.88f), FontStyle.Normal));
        }

        private void DrawMatchResult(TeamId winner)
        {
            Color previousColor = GUI.color;
            GUI.color = new Color(0.01f, 0.015f, 0.03f, 0.82f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previousColor;

            const float width = 520f;
            const float height = 290f;
            float left = (Screen.width - width) * 0.5f;
            float top = (Screen.height - height) * 0.46f;
            bool playerWon = winner == TeamId.TeamA;
            Color resultColor = playerWon
                ? new Color(0.25f, 0.85f, 1f)
                : new Color(1f, 0.3f, 0.25f);
            GUI.Box(new Rect(left, top, width, height), string.Empty);
            GUI.Label(
                new Rect(left + 24f, top + 30f, width - 48f, 58f),
                playerWon ? "VICTORY" : "DEFEAT",
                CreateCenteredStyle(38, resultColor, FontStyle.Bold));
            GUI.Label(
                new Rect(left + 24f, top + 91f, width - 48f, 28f),
                playerWon ? "BLUE TEAM CLAIMS THE ARENA" : "RED TEAM CLAIMS THE ARENA",
                CreateCenteredStyle(15, Color.white, FontStyle.Bold));
            GUI.Box(
                new Rect(left + 115f, top + 132f, width - 230f, 48f),
                $"BLUE  {_matchManager.TeamAWins}  :  {_matchManager.TeamBWins}  RED");
            GUI.Label(
                new Rect(left + 24f, top + 190f, width - 48f, 22f),
                "BEST OF 3 COMPLETE",
                CreateCenteredStyle(12, new Color(0.72f, 0.78f, 0.86f), FontStyle.Normal));
            if (GUI.Button(
                    new Rect(left + 145f, top + 224f, width - 290f, 48f),
                    "PLAY AGAIN"))
            {
                _winner = null;
                _roundMessage = "Restarting match";
                _matchManager.StartMatch();
            }
        }

        private static string GetSlotMarker(SkillSlot current, SkillSlot slot)
        {
            int currentIndex = current == SkillSlot.Q ? 0 :
                current == SkillSlot.E ? 1 : 2;
            int slotIndex = slot == SkillSlot.Q ? 0 : slot == SkillSlot.E ? 1 : 2;
            return slotIndex < currentIndex ? "[SAVED]" :
                slotIndex == currentIndex ? "[BUILDING]" : "[LOCKED]";
        }

        private static GUIStyle CreateCenteredStyle(
            int fontSize,
            Color color,
            FontStyle fontStyle)
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = fontSize,
                fontStyle = fontStyle,
                wordWrap = true
            };
            style.normal.textColor = color;
            return style;
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
            _isChoosingAppearance = false;
            _selectedEquipmentAppearance = null;
            _selectedAppearanceStyle = BuiltInAppearanceStyle.Classic;
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
                    BeginAppearanceSelection();
                    break;
            }
        }

        private void BeginAppearanceSelection()
        {
            _isChoosingAppearance = true;
            _selectedEquipmentAppearance = null;
            _selectedAppearanceStyle = BuiltInAppearanceStyle.Classic;
            _roundMessage = $"{_selectedRole}: Choose appearance";
        }

        public void ConfirmAppearanceSelection()
        {
            if (!_isChoosingAppearance || !_selectedRole.HasValue || _gameStarted)
            {
                return;
            }

            _isChoosingAppearance = false;
            StartMatch(_selectedRole.Value);
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
                isPlayer ? _selectedEquipmentAppearance : null,
                isPlayer ? GetSelectedBuiltInTint() : null);

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
            DemoCombatAudio audioFeedback = character.AddComponent<DemoCombatAudio>();
            audioFeedback.Configure(health, basicAttack, isPlayer);
            DemoSkillVfxPlayer skillVfx = character.AddComponent<DemoSkillVfxPlayer>();
            skillVfx.Configure(skillVfxLibrary);

            var combatBehaviours = new List<Behaviour>();
            combatBehaviours.Add(status);
            combatBehaviours.Add(elementStatus);
            combatBehaviours.Add(audioFeedback);
            combatBehaviours.Add(skillVfx);
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
            cameraObject.AddComponent<AudioListener>();
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
                SkillGenerationProviderFactory.Create(skillGenerationApiSettings),
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
