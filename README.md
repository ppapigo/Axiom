# Axiom

Unity 6 기반 3D 쿼터뷰 PvP Arena 프로토타입입니다. Battlerite 스타일의 논타겟 전투와 ScriptableObject 기반 사용자 제작 스킬 시스템을 목표로 합니다.

기능은 한 번에 구현하지 않습니다. 각 개발 단계를 구현하고 테스트가 통과한 뒤에만 다음 단계로 진행합니다.

## 현재 단계

1. 캐릭터 이동 — 완료
2. 고정 쿼터뷰 카메라 — 완료
3. 기본 공격 — 완료
4. 체력 및 피해 시스템 — 완료
5. Tank — 완료
6. Mage — 완료
7. Assassin — 미구현
8. AI — 미구현
9. 3vs3 경기 시스템 — 미구현
10. 스킬 제작 시스템 — 미구현

현재 Unity EditMode 테스트는 32개이며 모두 통과합니다.

## 구현된 시스템

- New Input System 기반 WASD 이동, 마우스 조준, 좌클릭 기본 공격, Space 회피
- 높이와 각도를 Inspector에서 조정하는 고정 쿼터뷰 추적 카메라
- 논타겟 캡슐 판정 기반 기본 공격
- `Attack × DamageCoefficient × CastDelayBonus × DistanceMultiplier` 피해 공식
- 체력, 회복, 사망 및 상태 변경 이벤트
- ScriptableObject 기반 이동·공격·카메라·역할 데이터
- Tank: HP 1400, Attack 80, 이동 배율 0.95, 4m 회피, 12초 쿨다운
- Tank의 장거리 기본 공격 차단
- Mage: HP 900, Attack 115, 이동 배율 1.0, 4m 회피, 12초 쿨다운
- Mage의 장거리·광역 공격 허용 및 Inspector 기반 광역 피해 상한

## Unity 구성

1. `Create > Axiom > Character > Movement Profile`로 이동 프로필을 생성합니다.
2. `Create > Axiom > Role` 메뉴에서 Tank 또는 Mage 역할 데이터를 생성합니다.
3. `Create > Axiom > Combat > Basic Attack Profile`로 근접 기본 공격 데이터를 생성합니다.
4. 플레이어에 `CharacterController`, 입력 소스, `CharacterMovement`, `CharacterRole`, `CharacterStats`, `CharacterHealth`, `BasicAttackController`, `CharacterDashController`를 추가합니다.
5. `AxiomInputActions/Gameplay`의 Move, Aim, BasicAttack, Dash 액션을 각 입력 소스에 연결합니다.
6. Main Camera에 `FixedQuarterViewCamera`를 추가하고 플레이어와 `Camera Follow Profile`을 연결합니다.
7. 공격 대상에 Collider와 `CharacterHealth`를 추가하고 Basic Attack Profile의 Target Layers를 설정합니다.

EditMode 테스트는 `Window > General > Test Runner`에서 실행합니다.

## 다음 단계

Assassin 역할을 구현합니다. HP 900, Attack 115, 이동 배율 1.10, 8m 회피와 5초 쿨다운을 적용하고 광역 반경 제한을 역할 정책으로 추가합니다.
