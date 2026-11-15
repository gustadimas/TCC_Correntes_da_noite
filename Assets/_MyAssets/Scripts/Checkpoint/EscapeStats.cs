using UnityEngine;

namespace CorrentesDaNoite.Checkpoint
{
    public class EscapeStats : MonoBehaviour
    {
        const string TIME_KEY = "EscapeTime";
        const string CAPTURES_KEY = "EscapeCaptures";

        public static float PlayTime => PlayerPrefs.GetFloat(TIME_KEY);
        public static int Captures => PlayerPrefs.GetInt(CAPTURES_KEY);

        float _time;

        private void Start() => _time = PlayTime;
        private void Update() => _time += Time.deltaTime;
        private void OnDisable() => PlayerPrefs.SetFloat(TIME_KEY, _time);

        public static void AddCapture() => PlayerPrefs.SetInt(CAPTURES_KEY, Captures + 1);

        public static void Clear()
        {
            PlayerPrefs.DeleteKey(TIME_KEY);
            PlayerPrefs.DeleteKey(CAPTURES_KEY);
        }
    }
}