using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace UnityEngine.InputSystem.Samples.RebindUI
{
    public class RebindActionUI : MonoBehaviour
    {
        public InputActionReference actionReference
        {
            get => m_Action;
            set
            {
                m_Action = value;
                UpdateActionLabel();
                UpdateBindingDisplay();
            }
        }

        public string bindingId
        {
            get => m_BindingId;
            set
            {
                m_BindingId = value;
                UpdateBindingDisplay();
            }
        }

        public InputBinding.DisplayStringOptions displayStringOptions
        {
            get => m_DisplayStringOptions;
            set
            {
                m_DisplayStringOptions = value;
                UpdateBindingDisplay();
            }
        }

        public Text actionLabel
        {
            get => m_ActionLabel;
            set
            {
                m_ActionLabel = value;
                UpdateActionLabel();
            }
        }

        public Text bindingText
        {
            get => m_BindingText;
            set
            {
                m_BindingText = value;
                UpdateBindingDisplay();
            }
        }

        public Text rebindPrompt
        {
            get => m_RebindText;
            set => m_RebindText = value;
        }

        public Text rebindInfo
        {
            get => m_RebindInfo;
            set => m_RebindInfo = value;
        }

        public Button rebindCancelButton
        {
            get => m_RebindCancelButton;
            set => m_RebindCancelButton = value;
        }

        public GameObject rebindOverlay
        {
            get => m_RebindOverlay;
            set => m_RebindOverlay = value;
        }

        public UpdateBindingUIEvent updateBindingUIEvent
        {
            get
            {
                if (m_UpdateBindingUIEvent == null)
                    m_UpdateBindingUIEvent = new UpdateBindingUIEvent();
                return m_UpdateBindingUIEvent;
            }
        }

        public InteractiveRebindEvent startRebindEvent
        {
            get
            {
                if (m_RebindStartEvent == null)
                    m_RebindStartEvent = new InteractiveRebindEvent();
                return m_RebindStartEvent;
            }
        }

        public InteractiveRebindEvent stopRebindEvent
        {
            get
            {
                if (m_RebindStopEvent == null)
                    m_RebindStopEvent = new InteractiveRebindEvent();
                return m_RebindStopEvent;
            }
        }

        public InputActionRebindingExtensions.RebindingOperation ongoingRebind => m_RebindOperation;

        [Header("Input")]
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string gameplayMapName = "Player";

        public bool ResolveActionAndBinding(out InputAction action, out int bindingIndex)
        {
            action = m_Action?.action;

            if (action == null)
            {
                bindingIndex = -1;
                return false;
            }

            bindingIndex = action.bindings.IndexOf(x => x.id.ToString() == m_BindingId);

            if (bindingIndex != -1)
                return true;

            Debug.LogError($"Cannot find binding with ID '{m_BindingId}' on '{action}'", this);
            return false;
        }

        public void UpdateBindingDisplay()
        {
            var displayString = string.Empty;
            var deviceLayoutName = default(string);
            var controlPath = default(string);

            var action = m_Action?.action;

            if (action != null)
            {
                var bindingIndex = action.bindings.IndexOf(x => x.id.ToString() == m_BindingId);

                if (bindingIndex != -1)
                {
                    displayString = action.GetBindingDisplayString(
                        bindingIndex,
                        out deviceLayoutName,
                        out controlPath,
                        displayStringOptions);
                }
            }

            if (m_BindingText != null)
                m_BindingText.text = displayString;

            m_UpdateBindingUIEvent?.Invoke(this, displayString, deviceLayoutName, controlPath);
        }

        public void ResetToDefault()
        {
            if (!ResolveActionAndBinding(out var action, out var bindingIndex))
                return;

            if (action.bindings[bindingIndex].isComposite)
            {
                for (var i = bindingIndex + 1;
                     i < action.bindings.Count && action.bindings[i].isPartOfComposite;
                     ++i)
                {
                    action.RemoveBindingOverride(i);
                }
            }
            else
            {
                action.RemoveBindingOverride(bindingIndex);
            }

            UpdateBindingDisplay();
        }

        public void StartInteractiveRebind()
        {
            m_Action.action.Disable();
            if (!ResolveActionAndBinding(out var action, out var bindingIndex))
                return;

            Debug.Log($"Starting rebind for {action.name}");

            if (action.bindings[bindingIndex].isComposite)
            {
                var firstPartIndex = bindingIndex + 1;

                if (firstPartIndex < action.bindings.Count &&
                    action.bindings[firstPartIndex].isPartOfComposite)
                {
                    PerformInteractiveRebind(action, firstPartIndex, true);
                }
            }
            else
            {
                PerformInteractiveRebind(action, bindingIndex);
            }
        }

        private void PerformInteractiveRebind(
            InputAction action,
            int bindingIndex,
            bool allCompositeParts = false)
        {
            m_RebindOperation?.Cancel();

            InputActionMap gameplayMap = null;

            if (playerInput != null)
            {
                gameplayMap = playerInput.actions.FindActionMap(gameplayMapName);

                if (gameplayMap != null)
                {
                    Debug.Log($"Disabling action map: {gameplayMap.name}");
                    gameplayMap.Disable();
                }
            }

            void CleanUp()
            {
                if (m_RebindCancelButton != null)
                    m_RebindCancelButton.onClick.RemoveListener(CancelRebind);

                m_RebindOperation?.Dispose();
                m_RebindOperation = null;

                if (gameplayMap != null)
                {
                    Debug.Log($"Re-enabling action map: {gameplayMap.name}");
                    gameplayMap.Enable();
                }

                UpdateBindingDisplay();
                m_Action.action.Enable();
            }

            Debug.Log($"Rebinding {action.name} binding index {bindingIndex}");

            m_RebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("<Mouse>/position")
                .WithControlsExcluding("<Mouse>/delta")
                .WithControlsExcluding("<Keyboard>/escape")
                .WithCancelingThrough("<Keyboard>/escape")
                .WithActionEventNotificationsBeingSuppressed()
                .WithTimeout(m_RebindTimeout)

                .OnCancel(operation =>
                {
                    Debug.Log("Rebind canceled");

                    m_RebindStopEvent?.Invoke(this, operation);

                    if (m_RebindOverlay != null)
                        m_RebindOverlay.SetActive(false);

                    CleanUp();
                })

                .OnComplete(operation =>
                {
                    Debug.Log("Rebind complete: " +
                        action.bindings[bindingIndex].effectivePath);

                    if (m_RebindOverlay != null)
                        m_RebindOverlay.SetActive(false);

                    m_RebindStopEvent?.Invoke(this, operation);

                    CleanUp();

                    if (allCompositeParts)
                    {
                        var nextBindingIndex = bindingIndex + 1;

                        if (nextBindingIndex < action.bindings.Count &&
                            action.bindings[nextBindingIndex].isPartOfComposite)
                        {
                            PerformInteractiveRebind(action, nextBindingIndex, true);
                        }
                    }
                });

            string partName = null;

            if (action.bindings[bindingIndex].isPartOfComposite)
                partName = $"Binding '{action.bindings[bindingIndex].name}'. ";

            m_RebindOverlay?.SetActive(true);

            if (m_RebindText != null)
            {
                var text = !string.IsNullOrEmpty(m_RebindOperation.expectedControlType)
                    ? $"{partName}Waiting for {m_RebindOperation.expectedControlType} input..."
                    : $"{partName}Waiting for input...";

                m_RebindText.text = text;
            }

            if (m_RebindCancelButton != null)
                m_RebindCancelButton.onClick.AddListener(CancelRebind);

            m_RebindStartEvent?.Invoke(this, m_RebindOperation);

            m_RebindOperation.Start();
        }

        private void CancelRebind()
        {
            m_RebindOperation?.Cancel();
        }

        protected void OnEnable()
        {
            if (s_RebindActionUIs == null)
                s_RebindActionUIs = new List<RebindActionUI>();

            s_RebindActionUIs.Add(this);

            if (s_RebindActionUIs.Count == 1)
                InputSystem.onActionChange += OnActionChange;

            UpdateBindingDisplay();
        }

        protected void OnDisable()
        {
            m_RebindOperation?.Dispose();
            m_RebindOperation = null;

            s_RebindActionUIs.Remove(this);

            if (s_RebindActionUIs.Count == 0)
            {
                s_RebindActionUIs = null;
                InputSystem.onActionChange -= OnActionChange;
            }
        }

        private static void OnActionChange(object obj, InputActionChange change)
        {
            if (change != InputActionChange.BoundControlsChanged)
                return;

            for (var i = 0; i < s_RebindActionUIs.Count; ++i)
                s_RebindActionUIs[i].UpdateBindingDisplay();
        }

        private void UpdateActionLabel()
        {
            if (m_ActionLabel != null)
            {
                var action = m_Action?.action;
                m_ActionLabel.text = action != null ? action.name : string.Empty;
            }
        }

        [Serializable]
        public class UpdateBindingUIEvent :
            UnityEvent<RebindActionUI, string, string, string>
        {
        }

        [Serializable]
        public class InteractiveRebindEvent :
            UnityEvent<RebindActionUI, InputActionRebindingExtensions.RebindingOperation>
        {
        }

        [SerializeField] private InputActionReference m_Action;
        [SerializeField] private string m_BindingId;
        [SerializeField] private InputBinding.DisplayStringOptions m_DisplayStringOptions;
        [SerializeField] private Text m_ActionLabel;
        [SerializeField] private Text m_BindingText;
        [SerializeField] private GameObject m_RebindOverlay;
        [SerializeField] private Text m_RebindText;
        [SerializeField] private Text m_RebindInfo;
        [SerializeField] private Button m_RebindCancelButton;
        [SerializeField] private float m_RebindTimeout = 5f;
        [SerializeField] private UpdateBindingUIEvent m_UpdateBindingUIEvent;
        [SerializeField] private InteractiveRebindEvent m_RebindStartEvent;
        [SerializeField] private InteractiveRebindEvent m_RebindStopEvent;

        private InputActionRebindingExtensions.RebindingOperation m_RebindOperation;
        private static List<RebindActionUI> s_RebindActionUIs;
    }
}