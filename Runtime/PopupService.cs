// ArchNet · Popup Service
// Alerts, yes / no questions and text prompts you await from code. One popup at a time: the
// others wait in line and open in order. The Hub creates GamePopups (your class) deriving from
// it; reach it from anywhere with Services.Get<GamePopups>().
//
//   await popups.Alert("Saved", "Your progress is safe.");
//   if (await popups.Confirm("Quit", "Leave the game?")) Application.Quit();
//   string name = await popups.Prompt("New save", "Name it:");   // null when cancelled
//
// Docs: https://archnet-plateform.fr/#ServicePopup

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ArchNet.Core;
using UnityEngine;

[assembly: InternalsVisibleTo("ArchNet.Service.Popup.Tests")]

namespace ArchNet.Service
{
    public enum PopupKind { Alert, Confirm, Prompt }

    /// <summary>One popup waiting to be shown, or on screen.</summary>
    public sealed class PopupRequest
    {
        public PopupKind Kind { get; internal set; }
        public string Title { get; internal set; }
        public string Message { get; internal set; }
        public string ConfirmLabel { get; internal set; }
        public string CancelLabel { get; internal set; }
        public string Placeholder { get; internal set; }

        internal readonly TaskCompletionSource<(bool confirmed, string text)> Answer =
            new TaskCompletionSource<(bool confirmed, string text)>();
    }

    public class PopupService : GameService
    {
        [SerializeField] PopupView _view;

        readonly Queue<PopupRequest> _queue = new Queue<PopupRequest>();
        PopupRequest _current;

        /// <summary>True while a popup is on screen.</summary>
        public bool IsOpen => _current != null;

        /// <summary>Popups waiting behind the one on screen.</summary>
        public int Waiting => _queue.Count;

        internal PopupView View
        {
            get => _view;
            set => _view = value;
        }

        /// <summary>A message with one button. Completes when the player closes it.</summary>
        public Task Alert(string title, string message, string ok = "OK") =>
            Open(new PopupRequest { Kind = PopupKind.Alert, Title = title, Message = message, ConfirmLabel = ok });

        /// <summary>A question with two buttons: true for yes, false for no.</summary>
        public Task<bool> Confirm(string title, string message, string yes = "Yes", string no = "No") =>
            Open(new PopupRequest { Kind = PopupKind.Confirm, Title = title, Message = message, ConfirmLabel = yes, CancelLabel = no })
                .ContinueWith(answer => answer.Result.confirmed, TaskContinuationOptions.ExecuteSynchronously);

        /// <summary>A question with a text field: the text typed, or null when cancelled.</summary>
        public Task<string> Prompt(string title, string message, string placeholder = "", string ok = "OK", string cancel = "Cancel") =>
            Open(new PopupRequest { Kind = PopupKind.Prompt, Title = title, Message = message, Placeholder = placeholder, ConfirmLabel = ok, CancelLabel = cancel })
                .ContinueWith(answer => answer.Result.confirmed ? answer.Result.text ?? "" : null, TaskContinuationOptions.ExecuteSynchronously);

        Task<(bool confirmed, string text)> Open(PopupRequest request)
        {
            _queue.Enqueue(request);
            ShowNext();
            return request.Answer.Task;
        }

        void ShowNext()
        {
            if (_current != null || _queue.Count == 0) return;
            _current = _queue.Dequeue();
            if (_view == null)
            {
                Debug.LogError("[ArchNet] The popup service has no PopupView assigned; the popup is answered as cancelled.", this);
                OnAnswered(false, null);
                return;
            }
            _view.Show(_current, OnAnswered);
        }

        void OnAnswered(bool confirmed, string text)
        {
            PopupRequest answered = _current;
            _current = null;
            answered?.Answer.TrySetResult((confirmed, text));
            ShowNext();
        }

        protected override void OnDestroy()
        {
            CancelAll();
            base.OnDestroy();
        }

        /// <summary>Answers every open or waiting popup as cancelled, so nobody waits forever.</summary>
        internal void CancelAll()
        {
            PopupRequest current = _current;
            _current = null;
            current?.Answer.TrySetResult((false, null));
            while (_queue.Count > 0) _queue.Dequeue().Answer.TrySetResult((false, null));
        }
    }
}
