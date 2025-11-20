# Enemy sound system

## 1) Visao geral
- Pipeline: PlayerSoundEmitter -> Audio.SoundEmitter -> EnemySoundListener -> EnemyController.
- Resposta atual: rotacao suave para a origem do som apenas em Patrol/Idle; Chase/Capture/Spotted ignoram.
- Prioridade: luz > som; deteccao de luz chama CancelSoundRotation.

## 2) Fluxo detalhado
1) Player emite som (walk/run/jump/land) via PlayerSoundEmitter.
2) Audio.SoundEmitter faz Physics.OverlapSphere e chama IHear.OnSoundHeard.
3) EnemySoundListener valida layer, distancia efetiva (sound.range * sensitividade) e guarda o ultimo som.
4) EnemyController.OnSoundHeard valida estado, flags por tipo e minSoundDistanceToReact (ignora se for mais perto que o valor). Se valido, chama RotateTowardsSound.
5) UpdateSoundRotation roda no Update, faz Quaternion.Slerp ate alinhar (<5 graus) ou estourar timeout de 2s.

## 3) Configuracao no Inspector
- EnemyController (Sound Reaction Settings):
  - enableSoundReactions: liga/desliga.
  - soundRotationSpeed: velocidade ao virar para o som.
  - minSoundDistanceToReact: ignora sons mais proximos que este valor (use 0 para reagir a som colado).
  - reactToWalkingSounds / reactToRunningSounds / reactToJumpingSounds: flags por tipo.
- EnemySoundListener:
  - hearingRange: raio bruto (SphereCollider).
  - filterByLayer + soundLayerMask: camadas aceitas (inclua o layer do Player).
  - ignoreOwnSounds: evita eco do proprio inimigo.
  - Sensitividades: walking/running/jumping/landing multiplicam sound.range antes da comparacao.
  - Debug: showDebugLogs, showDebugGizmos.

## 4) Perfis de tuning (EnemyController)
- Gameplay tenso: soundRotationSpeed 5.0; minSoundDistanceToReact 1.0; walking=true; running=true; jumping=true.
- Gameplay balanceado: soundRotationSpeed 3.0; minSoundDistanceToReact 3.0; walking=true; running=true; jumping=false.
- Gameplay relaxado: soundRotationSpeed 2.0; minSoundDistanceToReact 5.0; walking=false; running=true; jumping=false.

## 5) Checklist rapida
- PlayerSoundEmitter presente no Player e emitindo (walk/run/jump/land) com ranges coerentes.
- EnemySoundListener: hearingRange >= alcance dos sons desejados; soundLayerMask inclui Player.
- EnemyController: enableSoundReactions ligado; flags de tipo e minSoundDistanceToReact ajustados.

## 6) Ajustes e troubleshooting
- Inimigo nao reage: verifique enableSoundReactions, soundLayerMask, hearingRange, minSoundDistanceToReact e se o estado e Patrol/Idle.
- Rotacao muito rapida/lenta: ajuste soundRotationSpeed (2-5 e a faixa comum).
- Stealth maior: reduza walkingSensitivity ou walkSoundRange do player; aumente minSoundDistanceToReact.
- Punir corrida: aumente runningSensitivity ou runSoundRange.

## 7) Debug e extensoes
- Propriedade publica: IsRotatingToSound (UI/logs/estados).
- Metodo publico: CancelSoundRotation() para eventos prioritarios.
- TODO no codigo: EnemyInvestigateState (ir ate a origem, procurar, voltar para Patrol/Idle se nada achar).

## 8) Performance
- Fora de rotacao: early return (~0.001ms).
- Rotacionando: Quaternion.Slerp + Quaternion.Angle (~0.01-0.05ms), tipicamente um inimigo por vez.
