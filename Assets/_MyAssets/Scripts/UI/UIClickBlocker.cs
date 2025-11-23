using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CorrentesDaNoite.UI
{
    /// <summary>
    /// Bloqueia cliques que passam atraves de um painel de UI.
    /// </summary>
    public class UIClickBlocker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        static int _uiOverlapCount;

        public static bool IsPointerOverBlockingUI => _uiOverlapCount > 0;

        [SerializeField] bool ensureImageComponent = true;

        CanvasGroup[] _cachedCanvasGroups;

        private void Awake()
        {
            if (ensureImageComponent && GetComponent<Graphic>() == null)
            {
                var image = gameObject.AddComponent<Image>();
                image.color = new Color(0, 0, 0, 0.01f);
                image.raycastTarget = true;
            }

            CacheCanvasGroups();
        }

        private void CacheCanvasGroups()
        {
            var parentGroups = GetComponentsInParent<CanvasGroup>();
            var childGroups = GetComponentsInChildren<CanvasGroup>();

            var allGroups = new System.Collections.Generic.List<CanvasGroup>();
            if (parentGroups != null)
            {
                allGroups.AddRange(parentGroups);
            }
            if (childGroups != null)
            {
                allGroups.AddRange(childGroups);
            }

            _cachedCanvasGroups = allGroups.ToArray();
        }

        private void OnDisable()
        {
            if (_uiOverlapCount > 0)
            {
                _uiOverlapCount--;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (IsUIVisible(gameObject))
            {
                _uiOverlapCount++;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_uiOverlapCount > 0)
            {
                _uiOverlapCount--;
            }
        }

        public void OnPointerClick(PointerEventData eventData) { }

        private void Update()
        {
            if (EventSystem.current == null)
            {
                return;
            }

            if (_uiOverlapCount > 0 && !IsUIVisible(gameObject))
            {
                _uiOverlapCount = 0;
                return;
            }

            bool isPointerOverAnyUI = EventSystem.current.IsPointerOverGameObject();

            if (isPointerOverAnyUI)
            {
                if (_uiOverlapCount == 0)
                {
                    Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

                    var pointerData = new PointerEventData(EventSystem.current)
                    {
                        position = mousePos
                    };

                    var results = new System.Collections.Generic.List<RaycastResult>();
                    EventSystem.current.RaycastAll(pointerData, results);

                    foreach (var result in results)
                    {
                        if (result.gameObject == gameObject || result.gameObject.transform.IsChildOf(transform))
                        {
                            if (IsUIVisible(result.gameObject))
                            {
                                _uiOverlapCount = 1;
                                break;
                            }
                        }
                    }
                }
            }
            else if (_uiOverlapCount > 0)
            {
                _uiOverlapCount = 0;
            }
        }

        private bool IsUIVisible(GameObject uiObject)
        {
            if (_cachedCanvasGroups != null && _cachedCanvasGroups.Length > 0)
            {
                foreach (var cg in _cachedCanvasGroups)
                {
                    if (cg == null)
                    {
                        CacheCanvasGroups();
                        return IsUIVisible(uiObject);
                    }

                    if (cg.alpha < 1f)
                    {
                        return false;
                    }
                }

                return true;
            }

            return true;
        }
    }
}