using System.Collections;
using TMPro;
using UnityEngine;

namespace TrucoVR.Presentation
{
    [DisallowMultipleComponent]
    public sealed class RoundResultView : MonoBehaviour
    {
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private float visibleDuration = 1.8f;

        private Coroutine hideRoutine;

        public void ShowHandResult(string message)
        {
            Show(message);
        }

        public void ShowRoundResult(string message)
        {
            Show(message);
        }

        public void ShowStatus(string message)
        {
            Show(message);
        }

        private void Show(string message)
        {
            if (resultText == null)
                return;

            resultText.text = message;
            resultText.gameObject.SetActive(true);

            if (hideRoutine != null)
                StopCoroutine(hideRoutine);

            hideRoutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(visibleDuration);
            resultText.gameObject.SetActive(false);
            hideRoutine = null;
        }
    }
}
