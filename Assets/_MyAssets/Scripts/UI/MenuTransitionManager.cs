using CorrentesDaNoite.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
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
            public string backMenuName;
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

        public bool IsTransitioning => _isTransitioning;
        public string CurrentMenuName => _currentMenu?.menuName;

        private void Awake()
        {
            foreach (MenuScreen menu in menuScreens) _menuDictionary[menu.menuName] = menu;

            MusicManager.GetOrCreate().PlayMusic("Menu", 1.5f);
            FindFirstObjectByType<AudioStateController>()?.SetMenuState();
        }

        private void Start()
        {
            if (transitionBlocker != null)
            {
                transitionBlocker.blocksRaycasts = false;
                transitionBlocker.gameObject.SetActive(false);
            }

            if (!_menuDictionary.TryGetValue(initialMenuName, out MenuScreen initialMenu))
            {
                if (hideAllOnStart) HideAllMenusImmediately();
                return;
            }

            if (hideAllOnStart)
            {
                foreach (MenuScreen menu in menuScreens)
                {
                    if (menu != initialMenu) SetCanvasGroupState(menu.canvasGroup, false, true);
                    else if (menu.controlsOwnVisibility) SetCanvasGroupState(menu.canvasGroup, true, false);
                }
            }

            _currentMenu = initialMenu;
            if (!initialMenu.controlsOwnVisibility) ShowMenu(initialMenuName, false);
        }

        private void Update()
        {
            HandleNavigationInput();
            if (Time.unscaledTime < _nextAutoFixTime) return;

            _nextAutoFixTime = Time.unscaledTime + 0.5f;
            FixStuckRaycasts();
        }

        // clicar no vazio com o mouse tira a selecao entao a seta ou o controle seleciona de novo e o cancel volta pra tela de tras
        void HandleNavigationInput()
        {
            if (_isTransitioning || _currentMenu == null || !IsInteractable(_currentMenu) || !(EventSystem.current?.currentInputModule is InputSystemUIInputModule module)) return;

            if (!string.IsNullOrEmpty(_currentMenu.backMenuName) && module.cancel.action.WasPressedThisFrame()) ShowMenu(_currentMenu.backMenuName);
            else if (EventSystem.current.currentSelectedGameObject == null && module.move.action.WasPressedThisFrame()) SelectFirstButtonInMenu(_currentMenu);
        }

        bool IsInteractable(MenuScreen menu) => menu.canvasGroup != null && menu.canvasGroup.gameObject.activeInHierarchy && menu.canvasGroup.interactable;

        void FixStuckRaycasts()
        {
            if (transitionBlocker != null && (!transitionBlocker.gameObject.activeInHierarchy || transitionBlocker.alpha < 0.1f)) transitionBlocker.blocksRaycasts = false;

            foreach (MenuScreen menu in menuScreens)
                if (menu.canvasGroup != null && menu.canvasGroup.alpha < 0.1f) menu.canvasGroup.blocksRaycasts = false;
        }

        public void HideAllMenusImmediately()
        {
            foreach (MenuScreen menu in menuScreens)
                if (!menu.controlsOwnVisibility) SetCanvasGroupState(menu.canvasGroup, false, true);
        }

        public void ShowMenu(string menuName, bool useTransition = true)
        {
            if (!_menuDictionary.TryGetValue(menuName, out MenuScreen targetMenu))
            {
                Debug.LogWarning("menu nao encontrado " + menuName);
                return;
            }

            if (_isTransitioning) return;
            if (_currentMenu?.canvasGroup != null) _currentMenu.canvasGroup.interactable = false;

            if (useTransition)
            {
                _isTransitioning = true;
                _currentTransition = StartCoroutine(_currentMenu != null ? TransitionBetweenMenus(_currentMenu, targetMenu) : FadeInMenu(targetMenu));
                if (_currentMenu == null) _currentMenu = targetMenu;
                return;
            }

            if (_currentMenu != null)
            {
                SetCanvasGroupState(_currentMenu.canvasGroup, false, true);
                _currentMenu.OnHideComplete?.Invoke();
            }

            SetCanvasGroupState(targetMenu.canvasGroup, true, true);
            targetMenu.OnShowComplete?.Invoke();
            SelectFirstButtonInMenu(targetMenu);
            _currentMenu = targetMenu;
        }

        public void HideCurrentMenu(bool useTransition = true)
        {
            if (_currentMenu == null) return;

            if (useTransition)
            {
                if (_currentTransition != null) StopCoroutine(_currentTransition);
                _currentTransition = StartCoroutine(FadeOutMenu(_currentMenu));
                return;
            }

            SetCanvasGroupState(_currentMenu.canvasGroup, false, true);
            _currentMenu.OnHideComplete?.Invoke();
            _currentMenu = null;
        }

        IEnumerator TransitionBetweenMenus(MenuScreen currentMenu, MenuScreen targetMenu)
        {
            currentMenu.OnHideBegin?.Invoke();

            // sem o blocker ligado o fundo e o logo ficam invisiveis entao nao espera por eles
            bool useBlocker = useTransitionBlocker && transitionBlocker != null;
            if (useBlocker)
            {
                transitionBlocker.gameObject.SetActive(true);
                transitionBlocker.blocksRaycasts = true;
                if (blockerLogo != null) blockerLogo.alpha = 1f;
                if (blockerBackground != null) blockerBackground.alpha = 0f;
            }

            for (float elapsed = 0f; elapsed < fadeTime; elapsed += Time.unscaledDeltaTime)
            {
                currentMenu.canvasGroup.alpha = fadeCurve.Evaluate(1 - elapsed / fadeTime);
                if (currentMenu.canvasGroup.alpha < 0.5f) currentMenu.canvasGroup.blocksRaycasts = false;
                yield return null;
            }

            SetCanvasGroupState(currentMenu.canvasGroup, false, false);
            currentMenu.OnHideComplete?.Invoke();

            if (useBlocker && blockerBackground != null)
            {
                yield return FadeCanvasGroup(blockerBackground, 0f, 1f, fadeTime / logoFadeSpeed);
                yield return new WaitForSecondsRealtime(logoHoldTime);
                yield return FadeCanvasGroup(blockerBackground, 1f, 0f, fadeTime / logoFadeSpeed);
            }

            targetMenu.canvasGroup.gameObject.SetActive(true);
            targetMenu.canvasGroup.alpha = 0f;
            targetMenu.canvasGroup.blocksRaycasts = false;
            targetMenu.canvasGroup.transform.SetAsLastSibling();
            targetMenu.OnShowBegin?.Invoke();

            yield return FadeInCanvasGroup(targetMenu.canvasGroup);
            SetCanvasGroupState(targetMenu.canvasGroup, true, true);
            targetMenu.OnShowComplete?.Invoke();

            if (useBlocker && blockerLogo != null) yield return FadeCanvasGroup(blockerLogo, 1f, 0f, fadeTime);

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

            yield return FadeInCanvasGroup(menu.canvasGroup);
            SetCanvasGroupState(menu.canvasGroup, true, true);
            menu.OnShowComplete?.Invoke();

            SelectFirstButtonInMenu(menu);
            _isTransitioning = false;
            _currentTransition = null;
        }

        IEnumerator FadeOutMenu(MenuScreen menu)
        {
            menu.OnHideBegin?.Invoke();
            yield return FadeCanvasGroup(menu.canvasGroup, 1f, 0f, fadeTime);

            SetCanvasGroupState(menu.canvasGroup, false, true);
            menu.OnHideComplete?.Invoke();
            if (_currentMenu == menu) _currentMenu = null;

            _isTransitioning = false;
            _currentTransition = null;
        }

        // libera o clique na metade do fade pra quem clica rapido
        IEnumerator FadeInCanvasGroup(CanvasGroup canvasGroup)
        {
            for (float elapsed = 0f; elapsed < fadeTime; elapsed += Time.unscaledDeltaTime)
            {
                canvasGroup.alpha = fadeCurve.Evaluate(elapsed / fadeTime);
                if (canvasGroup.alpha > 0.5f) canvasGroup.blocksRaycasts = canvasGroup.interactable = true;
                yield return null;
            }
        }

        IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration)
        {
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                canvasGroup.alpha = fadeCurve.Evaluate(Mathf.Lerp(from, to, elapsed / duration));
                yield return null;
            }

            canvasGroup.alpha = to;
        }

        void SetCanvasGroupState(CanvasGroup canvasGroup, bool visible, bool interactive)
        {
            if (canvasGroup == null) return;

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible && interactive;
            canvasGroup.blocksRaycasts = visible && interactive;
        }

        void SelectFirstButtonInMenu(MenuScreen menu)
        {
            if (menu?.canvasGroup != null && menu.hasSelectableButtons) StartCoroutine(SelectFirstButtonDelayed(menu.canvasGroup.gameObject));
        }

        IEnumerator SelectFirstButtonDelayed(GameObject menuObject)
        {
            yield return new WaitForEndOfFrame();
            if (EventSystem.current == null) yield break;

            foreach (Selectable selectable in menuObject.GetComponentsInChildren<Selectable>())
            {
                if (!selectable.gameObject.activeInHierarchy || !selectable.interactable) continue;
                EventSystem.current.SetSelectedGameObject(selectable.gameObject);
                yield break;
            }
        }
    }
}