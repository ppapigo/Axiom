# Axiom

Unity 6 기반 3D 쿼터뷰 PvP Arena 프로토타입입니다. Battlerite 스타일의 논타겟 전투와 ScriptableObject 기반 사용자 스킬 제작 시스템을 목표로 합니다.

스킬 및 역할 수치의 초기 기준은 [스킬 밸런스 기준안](Documentation/SkillBalanceBaseline.md)에 정리되어 있습니다.

## 바로 플레이

**[GitHub Pages에서 Axiom 실행](https://ppapigo.github.io/Axiom/)**

별도 설치 없이 WebGL 지원 데스크톱 브라우저에서 실행할 수 있습니다.

- 시작 화면에서 Tank, Mage, Assassin 중 하나를 선택합니다.
- `WASD`: 이동
- 마우스: 조준
- 좌클릭: 기본 공격
- `Q`, `E`, `R`: 역할별 스킬
- `Space`: 회피
- 한 팀을 전멸시키면 라운드 1승이며, 먼저 2승하면 경기가 끝납니다.

기능은 작은 단위로 구현하고 테스트를 통과한 뒤 다음 단계로 진행합니다.

## 현재 단계

1. 캐릭터 이동 — 완료
2. 고정 쿼터뷰 카메라 — 완료
3. 기본 공격 — 완료
4. 체력 및 피해 시스템 — 완료
5. Tank — 완료
6. Mage — 완료
7. Assassin — 완료
8. 역할 기반 AI — 완료
9. 3vs3 경기 시스템 — 완료
10. 스킬 제작 시스템 — 완료

현재 Unity EditMode 테스트 81개가 통과합니다. 최신 PlayMode 재실행은 권한 거부로 생략했습니다.

## 구현된 시스템

- New Input System 기반 WASD 이동, 마우스 조준 및 몸 방향 회전, 좌클릭 기본 공격, Space 회피
- 높이와 각도를 Inspector에서 조정하는 고정 쿼터뷰 추적 카메라
- 캡슐 판정 기반 논타겟 기본 공격
- `Attack × DamageCoefficient × CastDelayBonus × DistanceMultiplier` 피해 공식
- 체력, 회복, 사망 및 상태 변경 이벤트
- ScriptableObject 기반 이동·공격·카메라·역할·AI 데이터
- Tank: HP 1400, Attack 80, 이동 배율 0.95, 4m 회피, 12초 쿨다운, 원거리 기본 공격 차단
- Mage: HP 900, Attack 115, 이동 배율 1.0, 4m 회피, 12초 쿨다운, 광역 피해 상한
- Assassin: HP 900, Attack 115, 이동 배율 1.10, 8m 회피, 5초 쿨다운, 광역 반경 제한
- AI 상태: Idle, FindTarget, Move, Attack, UseSkill, Retreat, Dead
- Tank AI: 가장 가까운 적 접근, 도발 범위에서 스킬 우선
- Mage AI: 설정된 거리 유지, 적 밀집 시 광역 스킬 우선
- Assassin AI: 체력이 가장 낮은 적 우선, 대상 후방으로 진입
- 팀 식별 기반 아군 제외 및 적 탐색
- 팀당 3명의 참가자와 개별 스폰 지점을 사용하는 3vs3 경기 관리
- 한 팀 전멸 시 상대 팀에 라운드 1승 부여
- 3판 2선승제: 2:0 또는 2:1에서 경기 종료
- 라운드 사이 전투 비활성화, 체력 회복, 위치 초기화 및 자동 재시작
- 라운드 시작·종료와 최종 승리 이벤트 제공
- ScriptableObject 기반 Skill Data, Balance Profile, Loadout
- Target, Projectile, Ground Area, Cone, Self Area 스킬 타입
- Slow, Root, Stun, KnockUp, Taunt CC와 7종 속성 데이터
- 기본 공격·Q·E·궁극기 슬롯, 포인트 예산과 중복 슬롯 검증
- 지원 시전 시간 검증과 Inspector AnimationCurve 기반 시전 피해 보너스
- Self Area와 Ground Area의 거리별 계단식 피해 감소
- Tank 원거리 제한, Mage 광역 피해 상한, Assassin 광역 반경 제한
- 자연어 스킬 생성 결과를 런타임 사용 전에 검증할 수 있는 `SkillDefinition` 파이프라인
- 런타임에서 생성되는 대칭형 Arena, 좌우 엄폐물과 투사체 차단 벽
- 역할 선택부터 AI 5명과의 3vs3 전투, 라운드 HUD, 재경기까지 이어지는 데모 씬
- GitHub Pages에서 직접 실행할 수 있는 Unity WebGL 빌드
- Brotli 압축과 브라우저 압축 해제 fallback을 적용한 약 10MB 배포 패키지
- 캐릭터 머리 위에 표시되는 팀 색상 체력바와 100 HP 단위 구간 눈금
- 화면 하단 전투 HUD의 현재/최대 HP와 Q·E·R·Space 회피 쿨타임 및 READY 표시
- Tank 방패·중갑, Mage 지팡이·오브·로브, Assassin 후드·쌍단검 경량 외형
- 프리팹 또는 기본 도형 파츠를 조합하는 ScriptableObject 장비 외형 커스터마이징

## Unity 구성

### 플레이어

1. `Create > Axiom > Character > Movement Profile`로 이동 프로필을 생성합니다.
2. `Create > Axiom > Role`에서 Tank, Mage 또는 Assassin 역할 데이터를 생성합니다.
3. `Create > Axiom > Combat > Basic Attack Profile`로 기본 공격 데이터를 생성합니다.
4. 플레이어에 `CharacterController`, 입력 소스, `CharacterMovement`, `CharacterRole`, `CharacterStats`, `CharacterHealth`, `BasicAttackController`, `CharacterDashController`, `TeamMember`를 추가합니다.
5. 입력 액션의 Move, Aim, BasicAttack, Dash를 각 입력 소스에 연결합니다.
6. Main Camera에 `FixedQuarterViewCamera`를 추가하고 플레이어와 Camera Follow Profile을 연결합니다.

### AI 캐릭터

1. `Create > Axiom > AI > Behaviour Profile`로 AI 프로필을 생성합니다.
2. AI 캐릭터에 `CharacterController`, `CharacterRole`, `CharacterStats`, `CharacterHealth`, `BasicAttackController`, `TeamMember`, `AIController`를 추가합니다.
3. `TeamMember`의 팀과 `AIController`의 Behaviour Profile을 지정합니다.
4. 탐지 대상 레이어를 AI 프로필의 Target Layers에 지정합니다.
5. 스킬 시스템 연결 시 `AISkillUserBehaviour` 구현체를 AI Controller의 Skill User에 연결합니다.

### 3vs3 경기

1. 빈 GameObject에 `ThreeVsThreeMatchManager`를 추가합니다.
2. Team A와 Team B 배열에 각각 정확히 3명의 `MatchParticipant`를 등록합니다.
3. 각 참가자에 `TeamMember`, `CharacterHealth`, 스폰 지점과 라운드 중 활성화할 조작·AI Behaviour를 지정합니다.
4. 기본값은 2승 필요, 라운드 간 대기 2초이며 Inspector에서 수정할 수 있습니다.
5. 한 팀의 세 캐릭터가 모두 사망하면 상대 팀이 1승하고, 먼저 2승한 팀이 최종 승리합니다.

### 스킬 제작

1. `Create > Axiom > Skill > Balance Profile`로 기본 100포인트 예산, 항목별 비용과 시전 시간 피해 곡선을 설정합니다.
2. `Create > Axiom > Skill > Skill Data`로 각 스킬의 타입, 피해 계수, 쿨다운, 사거리, 반경, 투사체 속도, CC와 속성 1개를 설정합니다.
3. `100 Point Build Modifiers`에서 피해·범위·사거리·쿨타임 보정과 CC·이동·보호막·회복 효과를 선택하면 Balance Profile 기준으로 비용이 자동 계산됩니다. 속성은 별도 속성 칸에서 선택하며 하나당 기본 10포인트를 사용합니다.
4. `Create > Axiom > Skill > Loadout`에 역할, Balance Profile과 슬롯별 Skill Data를 연결합니다.
5. `ValidateLoadout` 결과가 유효하고 총비용이 100포인트 이하인 데이터만 전투 실행 계층에 전달합니다.
6. 데모에서는 역할을 선택한 뒤에만 화면 상단의 `SKILL FORGE` 버튼과 `B` 키가 활성화됩니다. 각 스킬은 속성 1개를 선택하며, 같은 역할군의 Q/E/R 전체에서 서로 다른 속성은 최대 2개까지만 사용할 수 있습니다. 이미 채택한 속성은 여러 스킬에서 재사용할 수 있습니다.
7. 저장한 초안의 피해, 범위, 사거리, 쿨타임과 대표 CC 데이터는 플레이어 Q 스킬에 즉시 반영됩니다. 탱커의 비궁극기 사거리 제한 등 역할 규칙은 자동으로 유지됩니다.
8. Q 적중 시 둔화 30%/2초, 속박 1.5초, 기절 1초, 에어본 0.7초가 실제 이동·공격·회피·스킬 행동에 적용됩니다. 활성 CC는 월드 체력바 이름 옆에 표시됩니다.
9. 전투 중 화면 하단 HUD에서 플레이어 HP와 Q/E/R/Space 회피 쿨타임을 0.1초 단위로 확인할 수 있습니다.
10. Mage Q 화상은 4초간 초당 공격력 8%, Assassin Q 독은 5초간 초당 최대체력 1% 피해를 줍니다. Mage E 얼음은 30% 둔화, 궁극기 번개는 피해 +20%, 물 속성은 시전자 체력 10% 회복 규칙을 사용합니다.

### 장비 외형

1. `Create > Axiom > Appearance > Equipment Appearance`로 외형 데이터를 생성합니다.
2. 적용할 역할과 장비 파츠 배열을 설정합니다.
3. 각 파츠는 실제 저폴리 Prefab 또는 Unity 기본 Primitive를 사용할 수 있습니다.
4. 위치, 회전, 크기, 고유 색상 또는 팀 강조색 사용 여부를 Inspector에서 조정합니다.
5. `Axiom Demo Bootstrap`의 Equipment Appearances 배열에 역할별 데이터를 연결합니다.
6. 장비 파츠의 Collider는 전투 판정에 영향을 주지 않도록 런타임에서 제거됩니다.

## GitHub 브라우저 실행

배포 파일은 저장소의 `docs/`에 있으며 GitHub Pages가 `main` 브랜치의 `/docs`를 게시합니다.

- 실행 주소: https://ppapigo.github.io/Axiom/
- 데모 씬: `Assets/Scenes/AxiomDemo.unity`
- WebGL 빌드 메뉴: `Axiom > Create Demo Scene`, `Axiom > Build WebGL`
- 현재 전체 배포 파일 크기: 약 10MB

EditMode 및 PlayMode 테스트는 `Window > General > Test Runner`에서 실행합니다.

## 다음 단계

불+물, 물+얼음 등 속성 조합 효과를 전투에 적용하고, 이후 AI 자연어 요청을 검증된 `SkillDefinition`으로 변환하는 생성 계층을 추가합니다.
