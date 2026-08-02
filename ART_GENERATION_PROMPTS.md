# Axiom 모델링·애니메이션·스킬 VFX 생성 프롬프트

아래 프롬프트는 다른 이미지·3D·애니메이션 AI에 그대로 복사해 사용할 수 있습니다. 생성된 콘셉트 이미지는 [`Documentation/ConceptArt`](Documentation/ConceptArt)에 있습니다.

- [Tank 모델·애니메이션 시트](Documentation/ConceptArt/axiom-tank-model-animation-sheet.webp)
- [Mage 모델·애니메이션 시트](Documentation/ConceptArt/axiom-mage-model-animation-sheet.webp)
- [Assassin 모델·애니메이션 시트](Documentation/ConceptArt/axiom-assassin-model-animation-sheet.webp)
- [7속성 스킬 VFX 시트](Documentation/ConceptArt/axiom-seven-element-vfx-sheet.webp)

이미지는 저장소 용량을 줄이기 위해 WebP로 보관하며 WebGL 빌드에는 포함되지 않습니다.

## 공통 아트 디렉션

```text
Project: Axiom, a Unity 6 URP 3D quarter-view PvP arena game.
Visual goal: original stylized fantasy combat design with clean mid-poly forms, strong silhouettes and high readability from a distant fixed quarter-view camera.
Performance goal: WebGL-friendly assets, small download size, limited materials, 512-1024px textures, no unnecessary bones or hidden geometry.
Character scale: adult humanoid around 2 meters tall in Unity, feet at origin, forward direction +Z.
Rig: Unity Humanoid compatible, single root, no gameplay collider or scripts inside the art prefab.
Animation: in-place because game code controls movement; Root Motion disabled.
Materials: URP Lit or URP Unlit only, one or two materials per character.
Originality: do not copy existing game characters, logos, costumes or trademarked designs.
```

## Tank 콘셉트 이미지

```text
Use case: stylized-concept
Asset type: Unity 6 game character modeling and animation reference sheet
Primary request: original Tank character for Axiom, a fast 3D quarter-view PvP arena game. Heavy frontline defender with broad readable silhouette, massive rectangular shield, layered plate armor, reinforced gauntlet, no ranged weapon.
Scene/backdrop: clean neutral light-gray studio concept sheet background, no environment.
Subject: one consistent adult-proportioned stylized Tank character. Show three large orthographic turnaround views in neutral A-pose: front, exact side, back. Along the bottom show five smaller consistent key poses: idle guard, forward run in-place, shield melee strike, short defensive cast, hit reaction.
Style/medium: polished stylized 3D game character concept, clean hard-surface forms, optimized mid-poly Unity asset aesthetic, original design, readable at distant quarter-view camera.
Composition/framing: wide landscape production sheet, full body visible in every view, evenly spaced, consistent scale and proportions, feet aligned.
Lighting/mood: soft neutral studio lighting, clear material separation.
Color palette: gunmetal and charcoal plate armor, restrained cyan-blue team accents, small warm leather details.
Materials/textures: large simple armor panels, matte metal, limited material count, no micro-detail.
Constraints: strong shield silhouette; humanoid rig friendly; feet at ground plane; forward direction visually obvious; no cape covering the body; no text; no labels; no logos; no watermark.
Avoid: photorealism, tiny intricate ornaments, guns, bows, floating weapons, extreme spikes, bulky geometry that blocks limb deformation, cinematic background, multiple different character designs.
```

## Mage 콘셉트 이미지

```text
Use case: stylized-concept
Asset type: Unity 6 game character modeling and animation reference sheet
Primary request: original Mage character for Axiom, a fast 3D quarter-view PvP arena game. Mobile ranged spellcaster with a clear hoodless head silhouette, layered combat robe, armored boots and forearms, practical arcane staff with a compact crystal focus.
Scene/backdrop: clean neutral light-gray studio concept sheet background, no environment.
Subject: one consistent adult-proportioned stylized Mage character. Show three large orthographic turnaround views in neutral A-pose: front, exact side, back. Along the bottom show five smaller consistent key poses: calm idle with staff, forward run in-place, basic staff projectile attack, wide ground-area cast, hit reaction.
Style/medium: polished stylized 3D game character concept, clean sculpted shapes, optimized mid-poly Unity asset aesthetic, original design, readable at distant quarter-view camera.
Composition/framing: wide landscape production sheet, full body visible in every view, evenly spaced, consistent scale and proportions, feet aligned.
Lighting/mood: soft neutral studio lighting, clear material separation.
Color palette: deep indigo and muted charcoal robe, warm brown staff, cyan-violet arcane accents.
Materials/textures: broad cloth panels, a few sturdy leather straps, simple metal guards, limited material count, no micro-detail.
Constraints: staff and robe silhouette must stay clear from above; humanoid rig friendly; split lower robe panels for running; feet at ground plane; no text; no labels; no logos; no watermark.
Avoid: photorealism, oversized wizard hat, floor-length robe that blocks the legs, excessive jewelry, floating books, guns, tiny ornaments, cinematic background, multiple different character designs.
```

## Assassin 콘셉트 이미지

```text
Use case: stylized-concept
Asset type: Unity 6 game character modeling and animation reference sheet
Primary request: original Assassin character for Axiom, a fast 3D quarter-view PvP arena game. Agile melee executioner with a strong hooded silhouette, fitted layered leather armor, light bracers and boots, two short practical daggers, no ranged weapon.
Scene/backdrop: clean neutral light-gray studio concept sheet background, no environment.
Subject: one consistent adult-proportioned stylized Assassin character. Show three large orthographic turnaround views in neutral A-pose: front, exact side, back. Along the bottom show five smaller consistent key poses: alert crouched idle, fast forward run in-place, dual-dagger basic strike, long forward dash attack, hit reaction.
Style/medium: polished stylized 3D game character concept, clean sculpted shapes, optimized mid-poly Unity asset aesthetic, original design, readable at distant quarter-view camera.
Composition/framing: wide landscape production sheet, full body visible in every view, evenly spaced, consistent scale and proportions, feet aligned.
Lighting/mood: soft neutral studio lighting, clear material separation.
Color palette: charcoal and near-black leather, muted steel, restrained toxic green and cyan accents.
Materials/textures: broad leather and cloth panels, simple dagger blades, limited material count, no micro-detail.
Constraints: hood does not hide the entire face; both daggers clearly visible; humanoid rig friendly; coat tails short and split for running; feet at ground plane; no text; no labels; no logos; no watermark.
Avoid: photorealism, giant fantasy blades, guns, bow, excessive belts, loose cloth covering limbs, extreme spikes, cinematic background, multiple different character designs.
```

## 3D 모델 생성 프롬프트

역할명과 해당 콘셉트 이미지를 함께 입력합니다.

```text
Create a production-ready Unity humanoid 3D character from the supplied Axiom concept sheet.
Preserve the front, side and back proportions, silhouette, equipment placement and palette from the reference.
Deliver FBX plus source file. Use a single Root and a Unity Humanoid-compatible skeleton. Character height is approximately 2 meters, feet at world origin, forward +Z, scale 1.
Separate Body, Head, MainWeapon and Offhand when practical. Keep deformation-friendly shoulder, hip, knee and elbow topology. Remove hidden geometry under solid armor when safe.
Target a lightweight stylized mid-poly game model. Use one 1024px texture set and no more than two URP-compatible materials. Include Base Color, Normal and Mask/Metallic maps only when visually useful.
Do not include colliders, physics, gameplay scripts, cameras, lights, baked environment, Root Motion or copyrighted logos.
```

## 애니메이션 생성 프롬프트

```text
Create a Unity Humanoid animation set for the supplied Axiom character model.
All motion clips must be In-Place with Root Motion disabled because movement distance is controlled by game code.
Deliver separate clips named Idle, Run, BasicAttack, CastQ, CastE, CastR, Dash, Hit and Death.
Idle: guarded combat idle, seamless loop, 1.0-1.5 seconds.
Run: fast readable arena run, seamless loop, 0.6-0.9 seconds.
BasicAttack: role weapon strike with contact around 45-60% of the clip.
CastQ and CastE: compact readable casts, 0.4-0.8 seconds.
CastR: stronger ultimate anticipation and release, 1.0-1.5 seconds.
Dash: aggressive forward lean without translating the root, 0.35-0.55 seconds.
Hit: short upper-body reaction, 0.25-0.4 seconds.
Death: clear collapse readable from a high quarter-view camera, 1.0-1.5 seconds, no loop.
Keep feet stable, avoid mesh intersections, preserve weapon grips, and avoid acrobatics that make the gameplay silhouette unreadable.
```

역할별 동작 차이:

```text
Tank: heavy anticipation, shield always leads, low center of gravity, short controlled steps.
Mage: staff-driven casting arcs, open off-hand gestures, robe-safe leg spacing.
Assassin: compact crouched posture, quick dual-dagger strikes, strongest forward lean during Dash.
```

## 7속성 스킬 VFX 콘셉트 이미지

```text
Use case: stylized-concept
Asset type: Unity URP WebGL skill VFX concept sheet
Primary request: original elemental combat effect library for Axiom. Show seven clearly separated effect families with no text: fire, ice, lightning, poison, water, wind and earth. Each family combines a flat circular ground cast telegraph, a compact moving projectile core and a short radial impact burst.
Scene/backdrop: dark neutral charcoal studio background with subtle tile separation, no arena environment.
Fire: orange-red embers and sharp flame arcs. Ice: pale cyan shards and frost rings. Lightning: violet-white branching bolts. Poison: acid green droplets and thorn-like wisps. Water: deep blue rings and clean splash ribbons. Wind: mint-white crescent trails and spiral lines. Earth: ochre rock fragments and hexagonal shield geometry.
Style/medium: polished real-time game VFX concept reproducible with Unity Shuriken ParticleSystem, simple meshes and URP Unlit additive materials.
Composition/framing: wide landscape reference sheet, seven balanced isolated clusters, readable from a high quarter-view camera.
Constraints: each family differs by shape and color; compact 0.2-1.5 second effects; low particle count; no characters; no environment; no text; no labels; no logos; no watermark.
Avoid: photoreal smoke, cinematic explosions, screen-filling bloom, dense fog, realistic fluid simulation, VFX Graph complexity and tiny unreadable particles.
```

## Unity VFX Prefab 생성 프롬프트

```text
Build WebGL-friendly Unity 6 URP skill VFX prefabs from the supplied Axiom elemental concept sheet.
Use Shuriken ParticleSystem, simple low-poly meshes and URP Unlit additive or alpha-blended materials only. Do not use VFX Graph, real-time lights, physics damage, collision logic or gameplay scripts.
For every element deliver Cast, Projectile, Impact and Hit prefabs. Keep the local origin at the gameplay hit center and orient projectile travel toward local +Z.
Effects must remain readable from a fixed high quarter-view camera. Duration 0.2-1.5 seconds, limited overdraw, low particle counts, small texture atlas, no screen-filling bloom.
Fire, Ice, Lightning, Poison, Water, Wind and Earth must be recognizable by both silhouette and color. Prefabs are visual-only; Axiom code owns range, collision, damage and crowd control.
```

## 납품 전 확인

- FBX가 Unity Humanoid Avatar로 오류 없이 매핑되는가
- Idle/Run만 Loop이고 모든 클립이 In-Place인가
- 모델의 발이 원점에 있고 +Z를 보는가
- Prefab에 Collider, Camera, Light, 외부 Script가 없는가
- VFX가 Shuriken과 URP 재질만 사용하는가
- 텍스처가 역할당 512~1024px이고 재질 수가 1~2개인가
- 콘셉트 이미지와 결과물이 기존 상용 게임 캐릭터를 복제하지 않는가
