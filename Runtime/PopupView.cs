// ArchNet · Popup Service — popup screen
// Fills the popup prefab with a request (title, message, text field, button labels), shows it
// and reports the player's answer. Enter in the text field confirms.
// Docs: https://archnet-plateform.fr/#ServicePopup

using System;
using ArchNet.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArchNet.Service
{
    [RequireComponent(typeof(UIPanel))]
    public class PopupView : MonoBehaviour
    {
        [SerializeField] TMP_Text _title;
        [SerializeField] TMP_Text _message;
        [Tooltip("Shown for prompts only.")]
        [SerializeField] TMP_InputField _input;
        [SerializeField] Button _confirm;
        [SerializeField] TMP_Text _confirmLabel;
        [Tooltip("Hidden for alerts.")]
        [SerializeField] Button _cancel;
        [SerializeField] TMP_Text _cancelLabel;

        Action<bool, string> _onAnswer;
        UIPanel _panel;

        UIPanel Panel => _panel != null ? _panel : _panel = GetComponent<UIPanel>();

        void Awake()
        {
            if (_confirm != null) _confirm.onClick.AddListener(() => Answer(true));
            if (_cancel != null) _cancel.onClick.AddListener(() => Answer(false));
            if (_input != null) _input.onSubmit.AddListener(_ => Answer(true));
            Panel.Hidden.AddListener(() => Answer(false)); // the Back key answers "no" / cancels
        }

        public void Show(PopupRequest request, Action<bool, string> onAnswer)
        {
            _onAnswer = onAnswer ?? throw new ArgumentNullException(nameof(onAnswer));
            UISupport.EnsureEventSystem();
            Set(_title, request.Title);
            Set(_message, request.Message);
            Set(_confirmLabel, request.ConfirmLabel);
            Set(_cancelLabel, request.CancelLabel);
            if (_cancel != null) _cancel.gameObject.SetActive(request.Kind != PopupKind.Alert);

            bool prompt = request.Kind == PopupKind.Prompt;
            if (_input != null)
            {
                _input.gameObject.SetActive(prompt);
                _input.text = "";
                if (_input.placeholder is TMP_Text hint) hint.text = request.Placeholder ?? "";
            }
            Panel.Show();
            if (prompt && _input != null && Application.isPlaying) _input.ActivateInputField();
        }

        /// <summary>Closes the popup with this answer (the buttons call it).</summary>
        public void Answer(bool confirmed)
        {
            if (_onAnswer == null) return;
            Action<bool, string> onAnswer = _onAnswer;
            _onAnswer = null;
            string text = _input != null && _input.gameObject.activeSelf ? _input.text : null;
            Panel.Hide();
            onAnswer(confirmed, text);
        }

        static void Set(TMP_Text label, string text)
        {
            if (label == null) return;
            label.text = text ?? "";
            label.gameObject.SetActive(!string.IsNullOrEmpty(text));
        }
    }
}
