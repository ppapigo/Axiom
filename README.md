# Axiom

Unity 6 기반 3D 쿼터뷰 PvP Arena 프로토타입입니다. Battlerite 스타일의 논타겟 전투와 ScriptableObject 기반 사용자 제작 스킬 시스템을 목표로 합니다.

기능은 한 번에 구현하지 않습니다. 각 개발 단계를 구현하고 테스트가 통과한 뒤에만 다음 단계로 진행합니다.

## 현재 단계

1. 캐릭터 이동 — 구현 및 테스트 대상
2. 카메라 — 미구현
3. 기본 공격 — 미구현
4. 체력 및 피해 시스템 — 미구현
5. Tank — 미구현
6. Mage — 미구현
7. Assassin — 미구현
8. AI — 미구현
9. 3vs3 경기 시스템 — 미구현
10. 스킬 제작 시스템 — 미구현

## 1단계 실행 설정

1. `Create > Axiom > Character > Movement Profile`로 이동 프로필을 생성합니다.
2. 플레이어 GameObject에 `CharacterController`, `InputActionMovementSource`, `CharacterMovement`를 추가합니다.
3. `AxiomInputActions/Gameplay/Move`를 입력 소스의 Move Action에 연결합니다.
4. 이동 프로필과 입력 소스를 `CharacterMovement`에 연결합니다.
5. Collider가 있는 바닥 위에서 Play Mode를 실행하고 WASD를 확인합니다.

EditMode 테스트는 `Window > General > Test Runner`에서 실행합니다.

