using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace ArchNet.Service.Tests
{
    public class PopupServiceTests
    {
        GameObject _object;
        PopupService _popups;
        PopupView _view;

        [SetUp]
        public void SetUp()
        {
            _object = new GameObject("Popups");
            _view = _object.AddComponent<PopupView>(); // brings UIPanel and CanvasGroup
            _popups = _object.AddComponent<PopupService>();
            _popups.View = _view;
        }

        [TearDown]
        public void TearDown()
        {
            if (_object != null) Object.DestroyImmediate(_object);
        }

        [Test]
        public void ConfirmGivesThePlayersAnswer()
        {
            Task<bool> answer = _popups.Confirm("Quit", "Leave the game?");
            Assert.IsTrue(_popups.IsOpen);
            Assert.IsFalse(answer.IsCompleted);

            _view.Answer(true);

            Assert.IsTrue(answer.IsCompleted);
            Assert.IsTrue(answer.Result);
            Assert.IsFalse(_popups.IsOpen);
        }

        [Test]
        public void PopupsWaitInLineAndOpenInOrder()
        {
            Task<bool> first = _popups.Confirm("First", "?");
            Task<bool> second = _popups.Confirm("Second", "?");
            Assert.AreEqual(1, _popups.Waiting);

            _view.Answer(false);
            Assert.IsFalse(first.Result);
            Assert.IsTrue(_popups.IsOpen, "the second popup opens as soon as the first closes");
            Assert.IsFalse(second.IsCompleted);

            _view.Answer(true);
            Assert.IsTrue(second.Result);
            Assert.AreEqual(0, _popups.Waiting);
        }

        [Test]
        public void ACancelledPromptGivesNull()
        {
            Task<string> answer = _popups.Prompt("Name", "Your name?");
            _view.Answer(false);
            Assert.IsNull(answer.Result);
        }

        [Test]
        public void AnAlertCompletesWhenClosed()
        {
            Task closed = _popups.Alert("Saved", "Done.");
            Assert.IsFalse(closed.IsCompleted);
            _view.Answer(true);
            Assert.IsTrue(closed.IsCompleted);
        }

        [Test]
        public void WaitingPopupsAreCancelledWhenTheServiceGoes()
        {
            Task<bool> shown = _popups.Confirm("A", "?");
            Task<bool> waiting = _popups.Confirm("B", "?");
            _popups.CancelAll(); // what OnDestroy does in the game

            Assert.IsFalse(shown.Result);
            Assert.IsFalse(waiting.Result);
        }
    }
}
