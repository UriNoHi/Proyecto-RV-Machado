using System.Collections;
using System.Collections.Generic;
using TrucoVR.Domain;
using UnityEngine;

namespace TrucoVR.Presentation
{
    [DisallowMultipleComponent]
    public sealed class OpponentView : MonoBehaviour
    {
        [SerializeField] private Vector3 cardSize = new Vector3(0.075f, 0.119f, 0.002f);
        [SerializeField] private bool hideAtStart = true;
        [SerializeField] private Transform opponentTableTarget;
        [SerializeField] private Vector3 tableRotationOffset = new Vector3(90f, 0f, 0f);
        [SerializeField] private SpriteRenderer[] handCardRenderers;
        [SerializeField] private SpriteRenderer frontRenderer;
        [SerializeField] private SpriteRenderer backRenderer;
        [SerializeField] private SpriteRenderer[] playedCardRenderers;
        [SerializeField] private float cardMoveDuration = 0.55f;
        [SerializeField] private float cardMoveArcHeight = 0.08f;

        private SpriteRenderer spriteRenderer;
        private int playedCardIndex;

        private void Awake()
        {
            if (handCardRenderers == null)
                handCardRenderers = new SpriteRenderer[0];

            if (opponentTableTarget != null)
            {
                transform.position = opponentTableTarget.position;
                transform.rotation = opponentTableTarget.rotation * Quaternion.Euler(tableRotationOffset);
            }

            if (frontRenderer == null)
                frontRenderer = GetComponent<SpriteRenderer>();

            if (frontRenderer == null)
                frontRenderer = gameObject.AddComponent<SpriteRenderer>();

            spriteRenderer = frontRenderer;

            List<SpriteRenderer> childRenderers = new List<SpriteRenderer>();
            SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);

            foreach (SpriteRenderer childRenderer in allRenderers)
            {
                if (childRenderer != frontRenderer && childRenderer != backRenderer)
                    childRenderers.Add(childRenderer);
            }

            if (childRenderers.Count > 0)
            {
                playedCardRenderers = childRenderers.ToArray();
                transform.localScale = Vector3.one;

                foreach (SpriteRenderer childRenderer in playedCardRenderers)
                {
                    if (childRenderer != null)
                        childRenderer.transform.localScale = cardSize;
                }
            }
            else if (playedCardRenderers == null || playedCardRenderers.Length == 0)
            {
                playedCardRenderers = new[] { frontRenderer };
                transform.localScale = cardSize;
            }

            if (childRenderers.Count == 0)
                Debug.LogWarning("OpponentView no encontró SpriteRenderer hijos. Se usará el renderer raíz como respaldo.", this);

            if (childRenderers.Count == 0)
                transform.localScale = cardSize;

            spriteRenderer.sortingOrder = 10;
            frontRenderer.enabled = !hideAtStart;
            if (backRenderer != null)
                backRenderer.enabled = !hideAtStart;

            if (!hideAtStart)
                ShowBacks();
            else
                HideCard();
        }

        public void ShowBacks()
        {
            Sprite backSprite = Resources.Load<Sprite>("SpanishCards/card_back");

            if (backSprite == null)
            {
                Debug.LogError("No se encontró Resources/SpanishCards/card_back.jpg", this);
                return;
            }

            playedCardIndex = 0;

            foreach (SpriteRenderer handCardRenderer in handCardRenderers)
            {
                if (handCardRenderer == null)
                    continue;

                handCardRenderer.sprite = backSprite;
                handCardRenderer.enabled = true;
            }

            foreach (SpriteRenderer playedCardRenderer in playedCardRenderers)
            {
                if (playedCardRenderer == null)
                    continue;

                playedCardRenderer.sprite = handCardRenderers.Length == 0 ? backSprite : playedCardRenderer.sprite;
                playedCardRenderer.enabled = handCardRenderers.Length == 0;
            }
        }

        public void ShowCard(SpanishCard card)
        {
            Sprite sprite = Resources.Load<Sprite>($"SpanishCards/{card.FileName}");

            if (sprite == null)
            {
                Debug.LogError($"No se encontró el Sprite de la carta de la IA: {card.FileName}", this);
                return;
            }

            SpriteRenderer targetRenderer = playedCardIndex < playedCardRenderers.Length
                ? playedCardRenderers[playedCardIndex]
                : spriteRenderer;

            if (targetRenderer == null)
                targetRenderer = spriteRenderer;

            int cardIndex = playedCardIndex;
            playedCardIndex++;
            gameObject.name = $"OpponentCard_{card.FileName}";

            if (cardIndex < handCardRenderers.Length && handCardRenderers[cardIndex] != null)
                StartCoroutine(AnimateCardToTable(handCardRenderers[cardIndex], targetRenderer, sprite, cardIndex));
            else
            {
                targetRenderer.sprite = sprite;
                targetRenderer.enabled = true;
                targetRenderer.sortingOrder = 10 + cardIndex;
            }
        }

        public void HideCard()
        {
            playedCardIndex = 0;

            foreach (SpriteRenderer playedCardRenderer in playedCardRenderers)
            {
                if (playedCardRenderer != null)
                    playedCardRenderer.enabled = false;
            }

            if (backRenderer != null)
                backRenderer.enabled = false;

            foreach (SpriteRenderer handCardRenderer in handCardRenderers)
            {
                if (handCardRenderer != null)
                    handCardRenderer.enabled = false;
            }
        }

        private IEnumerator AnimateCardToTable(SpriteRenderer handRenderer, SpriteRenderer tableRenderer, Sprite frontSprite, int cardIndex)
        {
            Vector3 startPosition = handRenderer.transform.position;
            Quaternion startRotation = handRenderer.transform.rotation;
            Vector3 targetPosition = tableRenderer.transform.position;
            Quaternion targetRotation = tableRenderer.transform.rotation;
            float elapsed = 0f;

            tableRenderer.enabled = false;

            while (elapsed < cardMoveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / cardMoveDuration);
                float smooth = Mathf.SmoothStep(0f, 1f, t);
                Vector3 position = Vector3.Lerp(startPosition, targetPosition, smooth);
                position.y += Mathf.Sin(t * Mathf.PI) * cardMoveArcHeight;

                handRenderer.transform.position = position;
                handRenderer.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, smooth);
                yield return null;
            }

            handRenderer.transform.position = startPosition;
            handRenderer.transform.rotation = startRotation;
            handRenderer.enabled = false;

            tableRenderer.sprite = frontSprite;
            tableRenderer.sortingOrder = 10 + cardIndex;
            tableRenderer.enabled = true;
        }
    }
}
