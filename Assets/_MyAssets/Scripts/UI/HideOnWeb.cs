using UnityEngine;

namespace CorrentesDaNoite.UI
{
    public class HideOnWeb : MonoBehaviour
    {
#if UNITY_WEBGL
        private void Awake() => gameObject.SetActive(false);
#endif
    }
}