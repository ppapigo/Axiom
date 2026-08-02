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

Unity EditMode 전체 테스트 141개, 스킬 제작·자연어 생성 대상 테스트 83개, AI·스킬 대상 테스트 70개와 최신 데모 PlayMode 테스트 2개가 통과합니다.

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
- 전투 AI 5명은 플레이어 초안을 복사하지 않고 역할별 독립 Q/E/R 프리셋을 장착
- Tank는 가까운 적 2명 이상일 때 Q 기절·보호막, Mage는 적 2명 이상 밀집 시 E 광역 둔화, Assassin은 체력 40% 이하 대상에게 Q 돌진·독 공격 사용
- AI 스킬은 슬롯 존재, 쿨타임, 시전 상태, 대상 사거리와 역할 조건을 공통 검사
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
- 선택형 서버리스 생성 provider: 엔드포인트 미설정 시 Mock을 유지하고 서버 응답도 동일한 검증·자동 보정을 적용
- CastDelay 동안 시전 잠금과 원형 범위 표시를 유지한 뒤 판정 실행
- Projectile은 프레임 이동 구간 SphereCast로 벽·캐릭터 충돌을 검사하고 첫 충돌 또는 최대 사거리에서 폭발
- 스킬 속성색 폭발 원판과 피격 구체 VFX를 Unity 기본 도형으로 생성해 추가 에셋 용량 없이 연출
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
3. `100 Point Build Modifiers`에서 피해·범위·사거리·쿨타임 보정과 CC·이동·보호막·회복 효과를 선택하면 Balance Profile 기준으로 비용이 자동 계산됩니다. 공격 방식은 Target 8P, Projectile(논타겟) 0P, Self Area(자기 중심) 12P, Ground Area(지역 지정) 15P, Global 35P, Cone(부채꼴) 8P 중 정확히 하나를 선택합니다. 비용은 모두 Balance Profile에서 수정할 수 있습니다. CC도 Slow, Stun, KnockUp 중 하나만 선택할 수 있습니다. 속성은 별도 속성 칸에서 선택하며 하나당 기본 10포인트를 사용합니다.
4. `Create > Axiom > Skill > Loadout`에 역할, Balance Profile과 슬롯별 Skill Data를 연결합니다.
5. `ValidateLoadout` 결과가 유효하고 총비용이 100포인트 이하인 데이터만 전투 실행 계층에 전달합니다.
6. 데모는 `역할 선택 → Q 제작·저장 → E 제작·저장 → R 제작·저장 → 3vs3 전투 시작` 순서로 진행됩니다. 세 스킬을 모두 저장하기 전에는 전투 캐릭터와 매치가 생성되지 않습니다. 각 슬롯은 별도의 피해·범위·사거리·쿨타임·공격 방식·CC·속성 설정을 보존합니다. 각 스킬은 속성 1개를 선택하며, 같은 역할군의 Q/E/R 전체에서 서로 다른 속성은 최대 2개까지만 사용할 수 있습니다. 이미 채택한 속성은 여러 스킬에서 재사용할 수 있습니다.
7. 저장한 초안의 피해, 범위, 사거리, 쿨타임과 대표 CC 데이터는 플레이어 Q 스킬에 즉시 반영됩니다. 탱커의 비궁극기 사거리 제한 등 역할 규칙은 자동으로 유지됩니다.
8. Q 적중 시 둔화 30%/2초, 속박 1.5초, 기절 1초, 에어본 0.7초가 실제 이동·공격·회피·스킬 행동에 적용됩니다. 활성 CC는 월드 체력바 아래에 색상 배지와 남은 시간으로 표시되며, 플레이어가 CC에 걸리면 하단 전투 HUD에도 같은 배지가 표시됩니다.
9. Mobility는 조준 방향으로 기본 4m 이동하고, Shield는 최대 체력의 15%를 5초간 보호막으로 부여하며, Healing은 최대 체력의 15%를 회복합니다. 보호막은 체력보다 먼저 피해를 흡수하고 월드 체력바와 전투 HUD에 남은 수치가 표시됩니다. 수치는 Balance Profile에서 수정할 수 있습니다.
10. 적중한 속성은 대상에게 기본 5초간 표식으로 남습니다. 다른 속성으로 적중하면 불+물은 피해 25% 증가, 물+얼음은 피해 15% 증가와 1.5초 속박, 불+얼음은 피해 35% 증가 반응을 일으키고 기존 표식을 소비합니다. 표식과 발생한 반응은 월드 체력바 이름 옆에 표시됩니다.
9. 전투 중 화면 하단 HUD에서 플레이어 HP와 Q/E/R/Space 회피 쿨타임을 0.1초 단위로 확인할 수 있습니다.
10. Mage Q 화상은 4초간 초당 공격력 8%, Assassin Q 독은 5초간 초당 최대체력 1% 피해를 줍니다. Mage E 얼음은 30% 둔화, 궁극기 번개는 피해 +20%, 물 속성은 시전자 체력 10% 회복 규칙을 사용합니다. 물+번개는 3초간 초당 공격력 6% 피해와 0.5초 기절, 얼음+번개는 4초간 받는 피해 20% 증가, 불+독은 화상 피해 50% 증가를 적용합니다.
11. 바람+속성은 결합된 속성을 주변 5m 적에게 확산합니다. 대지+속성은 시전자에게 최대 체력 15% 보호막을 5초간 부여하고, 대상의 공격력을 4초간 15% 감소시킵니다. 반경, 비율, 지속시간은 Balance Profile에서 조정할 수 있습니다.
12. `ISkillGenerationProvider` 계약과 `MockSkillGenerationProvider`, 직렬화 가능한 AI 응답 DTO를 제공합니다. Mock provider는 역할별 안전 프리셋을 반환하고 자연어의 속성, 공격 방식, CC, 이동, 보호막, 회복 키워드를 반영하므로 실제 API 없이도 생성 흐름을 개발하고 시연할 수 있습니다.
13. `SkillDraftMapper`는 AI 응답의 공격 방식, 속성, CC 문자열을 정규화해 `SkillDraft`로 변환합니다. 지원하지 않는 enum, 음수, NaN, Infinity, 아직 제작 UI에서 제공하지 않는 Root와 Taunt는 예외 대신 오류 목록으로 반환합니다.
14. `SkillRuleValidator`는 기존 포인트 계산, `SkillValidator`, `RoleElementPool`을 재사용해 100포인트, CC 한 개, 역할군 속성 두 개, Tank 사거리와 Assassin 광역 반경 제한을 검사합니다. 결과에는 오류 목록과 미리보기에 사용할 계산된 포인트 및 `SkillDefinition`이 포함됩니다.
15. `SkillAutoCorrector`는 생성 수치를 제작 단위로 정규화하고 CC 중복, Tank 근접 범위, Assassin 광역 반경, 세 번째 역할 속성과 초과 포인트를 안전하게 줄입니다. 수정 후에도 유효하지 않으면 Tank Cone, Mage Projectile, Assassin Target 역할 프리셋을 반환하며 적용한 변경 내역을 함께 제공합니다.
16. `SkillGenerationPipeline`은 Provider → Mapper → Validator → Auto Corrector 흐름을 하나의 비동기 호출로 연결합니다. 결과에는 최종 `SkillDraft`, 미리보기용 `SkillDefinition`, 항목별 포인트 내역, 자동 보정 변경 내역과 오류 목록이 포함됩니다. AI 응답 파싱 또는 공급자 호출이 실패해도 역할별 안전 프리셋을 반환합니다.
17. 역할 선택 후 열리는 스킬 제작 화면 오른쪽에서 자연어를 입력하고 Mock AI 초안을 생성할 수 있습니다. 결과 카드에서 공격 방식, 속성, CC, 수치, 항목별 포인트와 자동 보정 내역을 확인한 뒤 `CONFIRM & SAVE`로 Q/E/R에 저장합니다. 기존 수동 100포인트 제작 방식도 함께 사용할 수 있습니다.
18. 플레이어가 저장한 Q/E/R은 `DemoSkillController`에서 실제 피해, CC, 속성, 이동, 보호막과 회복 효과로 시전됩니다. AI는 같은 실행 계층을 재사용하지만 각 역할에 맞는 별도 Q/E/R 프리셋을 사용하며, 최소 역할 조건이 성립할 때 우선 슬롯 하나를 시전합니다.
19. 모든 스킬은 데이터의 CastDelay 후 실행됩니다. Projectile 타입은 실제 투사체가 속도와 사거리만큼 이동하고 벽 또는 캐릭터에 충돌하면 원형 범위로 폭발합니다. 시전 범위, 폭발과 피격 VFX는 기존 단색 셰이더와 기본 도형만 사용합니다.
20. `SkillGenerationApiSettings`에서 서버리스 엔드포인트 사용 여부, URL과 제한 시간을 설정할 수 있습니다. 활성화하지 않았거나 URL이 유효하지 않으면 Mock provider를 사용합니다. 활성화 시 클라이언트는 `prompt`, `role`, `slot` JSON을 POST하고 기존 `SkillGenerationResponseDto` JSON을 응답으로 받습니다. 실제 AI API 키는 WebGL 빌드에 포함하지 않고 서버리스 함수에서만 보관해야 합니다.

### 서버리스 생성 API 계약

요청 예시:

```json
{"prompt":"적을 느리게 하는 얼음 투사체","role":"Mage","slot":"Q"}
```

응답은 `SkillGenerationResponseDto`와 같은 필드를 가진 JSON이어야 합니다. WebGL에서 호출하려면 서버가 배포 주소 `https://ppapigo.github.io`를 허용하는 CORS 헤더를 반환해야 합니다. Unity에서 `Assets > Create > Axiom > Skill Generation API Settings`로 설정 에셋을 만든 뒤 데모 씬의 `DemoArenaBootstrap`에 연결하면 활성화됩니다.

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

제출용 서버리스 함수를 배포해 실제 자연어 모델과 연결하고, 생성 상태·실패 원인을 UI에서 더 명확히 표시합니다.
