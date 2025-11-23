using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CorrentesDaNoite.UI
{
    public class MenuTransitionManager : MonoBehaviour
    {
        [System.Serializable]
        public class MenuScreen
        {
            public string menuName;
            public CanvasGroup canvasGroup;
            public bool hasSelectableButtons = true;
            [Tooltip("Se marcado, este menu controla sua propria visibilidade (ex: SplashScreen)")]
            public bool controlsOwnVisibility;
            public UnityEvent OnShowBegin;
            public UnityEvent OnShowComplete;
            public UnityEvent OnHideBegin;
            public UnityEvent OnHideComplete;
        }

        [Header("Configuracoes")]
        [SerializeField] float fadeTime = 0.3f;
        [SerializeField] AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] List<MenuScreen> menuScreens = new List<MenuScreen>();

        [Header("Blocker de Transicao")]
        [SerializeField] CanvasGroup transitionBlocker;
        [SerializeField] bool useTransitionBlocker;

        [Header("Elementos do Blocker")]
        [SerializeField] CanvasGroup blockerBackground;
        [SerializeField] CanvasGroup blockerLogo;

        [Header("Timing da Transicao")]
        [SerializeField] float logoHoldTime = 0.1f;
        [SerializeField] float logoFadeSpeed = 2.5f;

        [Header("Comportamento")]
        [SerializeField] bool hideAllOnStart = true;
        [SerializeField] string initialMenuName = string.Empty;

        MenuScreen _currentMenu;
        Coroutine _currentTransition;
        bool _isTransitioning;
        readonly Dictionary<string, MenuScreen> _menuDictionary = new Dictionary<string, MenuScreen>();
        float _nextAutoFixTime;

        private void Awake()
        {
            foreach (MenuScreen menu in menuScreens)
            {
                _menuDictionary[menu.menuName] = menu;
            }
        }

        private void Update()
        {
            if (Time.time < _nextAutoFixTime)
            {
                return;
            }

            _nextAutoFixTime = Time.time + 0.5f;

            if (transitionBlocker != null && !transitionBlocker.gameObject.activeInHierarchy)
            {
                transitionBlocker.blocksRaycasts = false;
            }

            foreach (MenuScreen menu in menuScreens)
            {
                if (menu.canvasGroup != null && menu.canvasGroup.alpha < 0.1f && menu.canvasGroup.blocksRaycasts)
                {
                    menu.canvasGroup.blocksRaycasts = false;
                }
            }

            if (transitionBlocker != null && transitionBlocker.gameObject.activeInHierarchy && transitionBlocker.alpha < 0.1f && transitionBlocker.blocksRaycasts)
            {
                transitionBlocker.blocksRaycasts = false;
            }
        }

        private void Start()
        {
            if (transitionBlocker != null)
            {
                transitionBlocker.blocksRaycasts = false;
                transitionBlocker.gameObject.SetActive(false);
            }

            if (!string.IsNullOrEmpty(initialMenuName) && _menuDictionary.ContainsKey(initialMenuName))
            {
                MenuScreen initialMenu = _menuDictionary[initialMenuName];

                if (hideAllOnStart)
                {
                    foreach (MenuScreen menu in menuScreens)
                    {
                        if (menu == initialMenu && menu.controlsOwnVisibility)
                        {
                            SetCanvasGroupState(menu.canvasGroup, true, false);
                        }
                        else if (menu != initialMenu)
                        {
                            SetCanvasGroupState(menu.canvasGroup, false, true);
                        }
                    }
                }

                _currentMenu = initialMenu;

                if (!initialMenu.controlsOwnVisibility)
                {
                    ShowMenu(initialMenuName, false);
                }
            }
            else if (hideAllOnStart)
            {
                HideAllMenusImmediately();
            }
        }

        public void HideAllMenusImmediately()
        {
            foreach (MenuScreen menu in menuScreens)
            {
                if (menu.controlsOwnVisibility)
                {
                    continue;
                }

                SetCanvasGroupState(menu.canvasGroup, false, true);
            }
        }

        public void ShowMenu(string menuName, bool useTransition = true)
        {
            if (!_menuDictionary.ContainsKey(menuName))
            {
                Debug.LogWarning($"Menu '{menuName}' nao encontrado!");
                return;
            }

            if (_isTransitioning)
            {
                return;
            }

            if (_currentMenu != null && _currentMenu.canvasGroup != null)
            {
                _currentMenu.canvasGroup.interactable = false;
            }

            MenuScreen targetMenu = _menuDictionary[menuName];

            if (_currentMenu != null)
            {
                if (useTransition)
                {
                    _isTransitioning = true;
                    _currentTransition = StartCoroutine(TransitionBetweenMenus(_currentMenu, targetMenu));
                }
                else
                {
                    SetCanvasGroupState(_currentMenu.canvasGroup, false, true);
                    SetCanvasGroupState(targetMenu.canvasGroup, true, true);

                    _currentMenu.OnHideComplete?.Invoke();
                    targetMenu.OnShowComplete?.Invoke();

                    SelectFirstButtonInMenu(targetMenu);

                    _currentMenu = targetMenu;
                }
            }
            else
            {
                if (useTransition)
                {
                    _isTransitioning = true;
                    _currentTransition = StartCoroutine(FadeInMenu(targetMenu));
                }
                else
                {
                    SetCanvasGroupState(targetMenu.canvasGroup, true, true);
                    targetMenu.OnShowComplete?.Invoke();

                    SelectFirstButtonInMenu(targetMenu);
                }

                _currentMenu = targetMenu;
            }
        }

        public void HideCurrentMenu(bool useTransition = true)
        {
            if (_currentMenu == null)
            {
                return;
            }

            if (useTransition)
            {
                if (_currentTransition != null)
                {
                    StopCoroutine(_currentTransition);
                }

                _currentTransition = StartCoroutine(FadeOutMenu(_currentMenu));
            }
            else
            {
                SetCanvasGroupState(_currentMenu.canvasGroup, false, true);
                _currentMenu.OnHideComplete?.Invoke();
                _currentMenu = null;
            }
        }

        IEnumerator TransitionBetweenMenus(MenuScreen currentMenu, MenuScreen targetMenu)
        {
            currentMenu.OnHideBegin?.Invoke();

            if (useTransitionBlocker && transitionBlocker != null)
            {
                transitionBlocker.gameObject.SetActive(true);
                transitionBlocker.blocksRaycasts = true;
                if (blockerLogo != null) blockerLogo.alpha = 1f;
                if (blockerBackground != null) blockerBackground.alpha = 0f;
            }

            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                currentMenu.canvasGroup.alpha = fadeCurve.Evaluate(1 - (elapsed / fadeTime));

                if (currentMenu.canvasGroup.alpha < 0.5f)
                {
                    currentMenu.canvasGroup.blocksRaycasts = false;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            SetCanvasGroupState(currentMenu.canvasGroup, false, false);
            currentMenu.OnHideComplete?.Invoke();

            if (blockerBackground != null)
            {
                float logoTime = fadeTime / logoFadeSpeed;
                elapsed = 0f;
                while (elapsed < logoTime)
                {
                    blockerBackground.alpha = fadeCurve.Evaluate(elapsed / logoTime);
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                blockerBackground.alpha = 1f;
            }

            yield return new WaitForSeconds(logoHoldTime);

            if (blockerBackground != null)
            {
                float logoTime = fadeTime / logoFadeSpeed;
                elapsed = 0f;
                while (elapsed < logoTime)
                {
                    blockerBackground.alpha = fadeCurve.Evaluate(1 - (elapsed / logoTime));
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                blockerBackground.alpha = 0f;
            }

            targetMenu.canvasGroup.gameObject.SetActive(true);
            targetMenu.canvasGroup.alpha = 0f;
            targetMenu.canvasGroup.blocksRaycasts = false;
            targetMenu.canvasGroup.transform.SetAsLastSibling();

            targetMenu.OnShowBegin?.Invoke();

            elapsed = 0f;
            while (elapsed < fadeTime)
            {
                targetMenu.canvasGroup.alpha = fadeCurve.Evaluate(elapsed / fadeTime);

                if (targetMenu.canvasGroup.alpha > 0.5f)
                {
                    targetMenu.canvasGroup.blocksRaycasts = true;
                    targetMenu.canvasGroup.interactable = true;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            SetCanvasGroupState(targetMenu.canvasGroup, true, true);
            targetMenu.OnShowComplete?.Invoke();

            if (blockerLogo != null)
            {
                elapsed = 0f;
                while (elapsed < fadeTime)
                {
                    blockerLogo.alpha = fadeCurve.Evaluate(1 - (elapsed / fadeTime));
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                blockerLogo.alpha = 0f;
            }

            if (transitionBlocker != null)
            {
                transitionBlocker.blocksRaycasts = false;
                transitionBlocker.gameObject.SetActive(false);
            }

            SelectFirstButtonInMenu(targetMenu);

            _currentMenu = targetMenu;
            _isTransitioning = false;
            _currentTransition = null;
        }

        IEnumerator FadeInMenu(MenuScreen menu)
        {
            menu.canvasGroup.transform.SetAsLastSibling();

            SetCanvasGroupState(menu.canvasGroup, true, false);
            menu.OnShowBegin?.Invoke();

            float elapsedTime = 0f;
            while (elapsedTime < fadeTime)
            {
                float alpha = fadeCurve.Evaluate(elapsedTime / fadeTime);
                SetCanvasGroupAlpha(menu.canvasGroup, alpha);

                if (alpha > 0.5f && menu.canvasGroup != null)
                {
                    menu.canvasGroup.blocksRaycasts = true;
                    menu.canvasGroup.interactable = true;
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            SetCanvasGroupState(menu.canvasGroup, true, true);
            menu.OnShowComplete?.Invoke();

            SelectFirstButtonInMenu(menu);

            _isTransitioning = false;
            _currentTransition = null;
        }

        IEnumerator FadeOutMenu(MenuScreen menu)
        {
            menu.OnHideBegin?.Invoke();

            float elapsedTime = 0f;
            while (elapsedTime < fadeTime)
            {
                float alpha = fadeCurve.Evaluate(1 - (elapsedTime / fadeTime));
                SetCanvasGroupAlpha(menu.canvasGroup, alpha);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            SetCanvasGroupState(menu.canvasGroup, false, true);
            menu.OnHideComplete?.Invoke();

            if (_currentMenu == menu)
            {
                _currentMenu = null;
            }

            _isTransitioning = false;
            _currentTransition = null;
        }

        void SetCanvasGroupAlpha(CanvasGroup canvasGroup, float alpha)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }
        }

        void SetCanvasGroupState(CanvasGroup canvasGroup, bool visible, bool interactive)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible && interactive;
            canvasGroup.blocksRaycasts = visible && interactive;
        }

        void SelectFirstButtonInMenu(MenuScreen menu)
        {
            if (menu == null || menu.canvasGroup == null)
            {
                return;
            }

            if (!menu.hasSelectableButtons)
            {
                return;
            }

            StartCoroutine(SelectFirstButtonDelayed(menu.canvasGroup.gameObject));
        }

        IEnumerator SelectFirstButtonDelayed(GameObject menuObject)
        {
            yield return new WaitForEndOfFrame();

            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                yield break;
            }

            Selectable[] selectables = menuObject.GetComponentsInChildren<Selectable>();

            foreach (Selectable selectable in selectables)
            {
                if (selectable != null && selectable.gameObject.activeInHierarchy && selectable.interactable)
                {
                    eventSystem.SetSelectedGameObject(selectable.gameObject);
                    yield break;
                }
            }
        }
    }
}