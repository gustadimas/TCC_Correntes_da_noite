using UnityEngine;
using UnityEngine.Audio;

namespace CorrentesDaNoite.Audio
{
    public class AudioStateController : MonoBehaviour
    {
        [Header("Mixer")]
        [SerializeField] internal AudioMixer audioMixer;

        [Header("Snapshots")]
        [SerializeField] internal AudioMixerSnapshot explorationSnapshot;
        [SerializeField] internal AudioMixerSnapshot chaseSnapshot;
        [SerializeField] internal AudioMixerSnapshot menuSnapshot;

        [Header("Transition")]
        [SerializeField] internal float defaultTransitionTime = 0.6f;

        public void SetExplorationState(float? transitionTime = null)
        {
            TransitionTo(explorationSnapshot, transitionTime);
        }

        public void SetChaseState(float? transitionTime = null)
        {
            TransitionTo(chaseSnapshot, transitionTime);
        }

        public void SetMenuState(float? transitionTime = null)
        {
            TransitionTo(menuSnapshot, transitionTime);
        }

        protected void TransitionTo(AudioMixerSnapshot snapshot, float? customTime)
        {
            if (snapshot == null)
                return;

            float time = customTime.HasValue ? Mathf.Max(0f, customTime.Value) : defaultTransitionTime;
            snapshot.TransitionTo(time);
        }
    }
}