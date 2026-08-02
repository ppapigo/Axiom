# Axiom 외부 아트 에셋 제작 규격

다른 AI 또는 외부 도구에서 만든 결과물은 원본 Unity 프로젝트가 아니라 아래 규격의 에셋 묶음으로 전달합니다. 코드, 충돌 판정과 밸런스 데이터는 포함하지 않습니다.

## 캐릭터 모델

- 역할별 FBX: Tank, Mage, Assassin
- Humanoid 리그, 단일 Root, 발바닥 원점, 정면 +Z
- Unity 기준 실제 키 약 2m, Scale 1
- 메시 Collider와 외부 스크립트 제외
- 역할당 재질 1~2개, 텍스처 512~1024px
- URP Lit 또는 URP Unlit 호환 텍스처 사용
- 장비를 교체해야 하면 Body, Head, Weapon, Offhand처럼 분리

## 애니메이션

- Idle, Run, BasicAttack, CastQ, CastE, CastR, Dash, Hit, Death
- 이동과 회피 거리는 게임 코드가 처리하므로 모두 In-Place
- Idle과 Run은 Loop, 나머지는 Loop 해제
- Humanoid Avatar를 공유할 수 있는 FBX 또는 AnimationClip
- Root Motion 비활성화를 전제로 제작

## 스킬 VFX

- Cast, Projectile, Impact, Hit 네 종류를 우선 제작
- Unity Shuriken ParticleSystem Prefab 사용
- WebGL 호환 URP Unlit 재질 사용
- VFX Graph, 실시간 조명, 물리 충돌과 피해 스크립트 제외
- 시각 효과의 로컬 원점은 실제 판정 중심과 일치
- 0.2~1.5초 내 종료하고 불필요한 오브젝트를 남기지 않음
- Fire, Ice, Lightning, Poison, Water, Wind, Earth는 색상 변형으로 재사용 가능하게 구성

## 전달 폴더

```text
Assets/Art/Characters/<Role>/Models
Assets/Art/Characters/<Role>/Animations
Assets/Art/Characters/<Role>/Materials
Assets/Art/VFX/Skills
Assets/Art/Textures
```

외형 선택 목록에는 `EquipmentAppearanceDefinition`의 Display Name, Description과 Prefab 파트를 연결합니다. 모델과 애니메이션 규격이 확인되기 전까지 현재 Primitive 기반 캐릭터와 코드 기반 VFX가 fallback으로 유지됩니다.
