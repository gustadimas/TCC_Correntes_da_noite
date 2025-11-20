# Player sound system

## 1) Visao geral
- Script: Player/PlayerSoundEmitter.cs (requer PlayerController e CharacterController).
- Cria Audio.SoundData e dispara via Audio.SoundEmitter.EmitSound para qualquer IHear (EnemySoundListener).
- Emite sons de caminhada, corrida, salto e aterrissagem; crouch nao emite.

## 2) Como funciona
- Update aborta se PlayerController.IsCaptured for true.
- CheckMovementSounds: so no chao e com velocidade >= movementSoundMinSpeed; pula se IsCrouching; Running a cada runSoundCooldown (runSoundRange) senao Walking a cada walkSoundCooldown (walkSoundRange).
- CheckLandingSound: detecta transicao ar->chao e emite Landing (landSoundRange).
- Metodos publicos: EmitJumpSound(), EmitWalkSound(), EmitRunSound(), EmitLandSound() (para animacoes/eventos).
- Debug: showDebugLogs, enableDebugGizmos (esferas e ultimo som).

## 3) Configuracao no Inspector
- Ranges: walkSoundRange 5m; runSoundRange 15m; jumpSoundRange 10m; landSoundRange 12m.
- Cooldowns: walkSoundCooldown 0.5s; runSoundCooldown 0.3s.
- movementSoundMinSpeed: 0.5 (nao emitir quase parado).
- Debug: ligue apenas quando necessario.

## 4) Fluxo ate os inimigos
1) PlayerSoundEmitter chama SoundEmitter.EmitSound com posicao/raio/SoundType.
2) Audio.SoundEmitter faz Physics.OverlapSphere e chama IHear.OnSoundHeard.
3) EnemySoundListener filtra por layer/emissor e sensitividade do tipo; se aprovado, repassa para EnemyController.
4) EnemyController pode rotacionar para o som (Patrol/Idle) se flags e distancia permitirem.

## 5) Dicas de tuning
- Mais stealth: diminua walkSoundRange ou aumente movementSoundMinSpeed.
- Punir corrida: aumente runSoundRange ou reduza runSoundCooldown.
- Terrenos/estados especiais: ajuste ranges dinamicamente ou chame EmitSound(soundType, range) interno.

## 6) Checklist rapida
- PlayerSoundEmitter anexado ao jogador com ranges/cooldowns configurados.
- EnemySoundListener com hearingRange adequado e soundLayerMask incluindo o layer do Player.
- EnemyController com enableSoundReactions ligado e flags/minSoundDistanceToReact ajustados ao perfil desejado.
