
// Extension para UniversalAudioComponent incluir footsteps facilmente
public static class UniversalAudioComponentExtensions
{
    public static void SetupFootstepTriggers(this UniversalAudioComponent audioComponent)
    {
        audioComponent.AddAudioTrigger("footstep_calcada", AudioEvent.FootstepCalcada);
        audioComponent.AddAudioTrigger("footstep_metal", AudioEvent.FootstepMetal);
        audioComponent.AddAudioTrigger("footstep_normal", AudioEvent.FootstepNormal);
        audioComponent.AddAudioTrigger("footstep_teto_barraca", AudioEvent.FootstepTetoBarraca);
    }
}
