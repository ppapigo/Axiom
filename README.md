# Axiom

Unity 6 기반 3D 쿼터뷰 PvP Arena 프로토타입입니다. Battlerite 스타일의 논타겟 전투와 ScriptableObject 기반 사용자 스킬 제작 시스템을 목표로 합니다.

기능은 작은 단위로 구현하고 EditMode 테스트를 통과한 뒤 다음 단계로 진행합니다.

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

현재 Unity EditMode 테스트 65개가 모두 통과합니다.

## 구현된 시스템

- New Input System 기반 WASD 이동, 마우스 조준, 좌클릭 기본 공격, Space 회피
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

1. `Create > Axiom > Skill > Balance Profile`로 포인트 예산과 시전 시간 피해 곡선을 설정합니다.
2. `Create > Axiom > Skill > Skill Data`로 각 스킬의 타입, 피해 계수, 쿨다운, 사거리, 반경, 투사체 속도, CC, 속성과 비용을 설정합니다.
3. `Create > Axiom > Skill > Loadout`에 역할, Balance Profile과 슬롯별 Skill Data를 연결합니다.
4. `ValidateLoadout` 결과가 유효한 데이터만 전투 실행 계층에 전달합니다.

## GitHub 브라우저 실행

Unity WebGL 빌드를 GitHub Pages에 배포하면 별도 설치 없이 링크로 실행할 수 있습니다. 현재 저장소에는 실행 씬이 아직 등록되지 않아 Pages 배포 전 단계입니다.

EditMode 테스트는 `Window > General > Test Runner`에서 실행합니다.

## 다음 단계

플레이 가능한 데모 씬을 구성하고 Unity WebGL 빌드와 GitHub Pages 자동 배포를 연결합니다. 완료 후 README 상단에 심사용 실행 링크를 제공합니다.
