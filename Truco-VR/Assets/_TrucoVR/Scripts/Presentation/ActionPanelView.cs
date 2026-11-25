using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TrucoVR.Presentation
{
    [DisallowMultipleComponent]
    public sealed class ActionPanelView : MonoBehaviour
    {
        [SerializeField] private Button trucoButton;
        [SerializeField] private Button retrucoButton;
        [SerializeField] private Button valeCuatroButton;
        [SerializeField] private Button quieroButton;
        [SerializeField] private Button noQuieroButton;
        [SerializeField] private Button envidoButton;

        public event Action TrucoRequested;
        public event Action RetrucoRequested;
        public event Action ValeCuatroRequested;
        public event Action RealEnvidoRequested;
        public event Action FaltaEnvidoRequested;
        public event Action QuieroRequested;
        public event Action NoQuieroRequested;
        public event Action EnvidoRequested;

        private string retrucoButtonLabel;
        private string valeCuatroButtonLabel;
        private bool envidoResponseMode;

        private void Awake()
        {
            retrucoButtonLabel = GetButtonLabel(retrucoButton);
            valeCuatroButtonLabel = GetButtonLabel(valeCuatroButton);

            if (trucoButton != null)
                trucoButton.onClick.AddListener(OnTrucoClicked);

            if (retrucoButton != null)
                retrucoButton.onClick.AddListener(OnRetrucoClicked);

            if (valeCuatroButton != null)
                valeCuatroButton.onClick.AddListener(OnValeCuatroClicked);

            if (quieroButton != null)
                quieroButton.onClick.AddListener(OnQuieroClicked);

            if (noQuieroButton != null)
                noQuieroButton.onClick.AddListener(OnNoQuieroClicked);

            if (envidoButton != null)
                envidoButton.onClick.AddListener(OnEnvidoClicked);
        }

        private void OnDestroy()
        {
            if (trucoButton != null)
                trucoButton.onClick.RemoveListener(OnTrucoClicked);

            if (retrucoButton != null)
                retrucoButton.onClick.RemoveListener(OnRetrucoClicked);

            if (valeCuatroButton != null)
                valeCuatroButton.onClick.RemoveListener(OnValeCuatroClicked);

            if (quieroButton != null)
                quieroButton.onClick.RemoveListener(OnQuieroClicked);

            if (noQuieroButton != null)
                noQuieroButton.onClick.RemoveListener(OnNoQuieroClicked);

            if (envidoButton != null)
                envidoButton.onClick.RemoveListener(OnEnvidoClicked);
        }

        public void SetActionsAvailable(bool trucoAvailable, bool envidoAvailable)
        {
            if (trucoButton != null)
                trucoButton.interactable = trucoAvailable;

            if (envidoButton != null)
                envidoButton.interactable = envidoAvailable;
        }

        public void SetTrucoResponsesAvailable(bool available)
        {
            if (quieroButton != null)
                quieroButton.interactable = available;

            if (noQuieroButton != null)
                noQuieroButton.interactable = available;
        }

        public void SetEnvidoAvailable(bool available)
        {
            if (envidoButton != null)
                envidoButton.interactable = available;
        }

        public void SetEnvidoResponsesAvailable(bool available, bool canRaiseToReal, bool canRaiseToFalta)
        {
            envidoResponseMode = available;

            if (retrucoButton != null)
            {
                retrucoButton.interactable = available && canRaiseToReal;
                SetButtonLabel(retrucoButton, available ? "Real Envido" : retrucoButtonLabel);
            }

            if (valeCuatroButton != null)
            {
                valeCuatroButton.interactable = available && canRaiseToFalta;
                SetButtonLabel(valeCuatroButton, available ? "Falta Envido" : valeCuatroButtonLabel);
            }

            SetTrucoResponsesAvailable(available);
        }

        public void SetRaiseActionsAvailable(bool trucoAvailable, bool retrucoAvailable, bool valeCuatroAvailable)
        {
            SetEnvidoResponsesAvailable(false, false, false);

            if (trucoButton != null)
                trucoButton.interactable = trucoAvailable;

            if (retrucoButton != null)
                retrucoButton.interactable = retrucoAvailable;

            if (valeCuatroButton != null)
                valeCuatroButton.interactable = valeCuatroAvailable;
        }

        private void OnTrucoClicked()
        {
            TrucoRequested?.Invoke();
        }

        private void OnRetrucoClicked()
        {
            if (envidoResponseMode)
                RealEnvidoRequested?.Invoke();
            else
                RetrucoRequested?.Invoke();
        }

        private void OnValeCuatroClicked()
        {
            if (envidoResponseMode)
                FaltaEnvidoRequested?.Invoke();
            else
                ValeCuatroRequested?.Invoke();
        }

        private void OnQuieroClicked()
        {
            QuieroRequested?.Invoke();
        }

        private void OnNoQuieroClicked()
        {
            NoQuieroRequested?.Invoke();
        }

        private void OnEnvidoClicked()
        {
            EnvidoRequested?.Invoke();
        }

        private static string GetButtonLabel(Button button)
        {
            TMP_Text label = button == null ? null : button.GetComponentInChildren<TMP_Text>();
            return label == null ? string.Empty : label.text;
        }

        private static void SetButtonLabel(Button button, string label)
        {
            if (button == null)
                return;

            TMP_Text text = button.GetComponentInChildren<TMP_Text>();
            if (text != null)
                text.text = label;
        }
    }
}
