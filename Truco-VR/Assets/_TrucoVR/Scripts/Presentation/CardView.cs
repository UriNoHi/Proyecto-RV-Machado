using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TrucoVR.Domain;

namespace TrucoVR.Presentation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class CardView : MonoBehaviour
    {
        [SerializeField] private float hoverLift = 0.022f;
        [SerializeField] private float hoverScale = 1.03f;
        [SerializeField] private float moveDuration = 0.55f;
        [SerializeField] private float moveArcHeight = 0.12f;
        [Header("Tamaño físico de la carta")]
        [SerializeField] private Vector3 cardSize = new Vector3(0.075f, 0.119f, 0.002f);
        [Header("Caras")]
        [SerializeField] private SpriteRenderer frontRenderer;
        [SerializeField] private SpriteRenderer backRenderer;
        [Header("Orientación en mesa")]
        [SerializeField] private Vector3 tableRotationOffset = new Vector3(90f, 0f, 0f);

        private XRSimpleInteractable interactable;
        private CardFanController fanController;
        private Transform tableTarget;
        private Renderer[] renderers;
        private Vector3 baseLocalPosition;
        private Quaternion baseLocalRotation;
        private Vector3 baseLocalScale;
        private bool isHovered;
        private bool isInHand = true;
        private Coroutine moveCoroutine;

        public bool IsInHand => isInHand;
        public SpanishCard Card { get; private set; }

        public void SetCard(SpanishCard card, Sprite sprite)
        {
            Card = card;
            gameObject.name = $"Card_{card.FileName}";

            if (frontRenderer == null)
                frontRenderer = GetComponent<SpriteRenderer>();

            if (frontRenderer == null)
                frontRenderer = gameObject.AddComponent<SpriteRenderer>();

            frontRenderer.sprite = sprite;
            frontRenderer.enabled = true;

            if (backRenderer != null)
                backRenderer.enabled = true;

            Vector3 physicalScale = cardSize;
            transform.localScale = physicalScale;
            baseLocalScale = physicalScale;

            BoxCollider collider = GetComponent<BoxCollider>();
            if (collider != null)
                collider.size = Vector3.one;
        }

        private void Awake()
        {
            if (frontRenderer == null)
                frontRenderer = GetComponent<SpriteRenderer>();

            interactable = GetComponent<XRSimpleInteractable>();
            if (interactable == null)
                interactable = gameObject.AddComponent<XRSimpleInteractable>();

            renderers = GetComponentsInChildren<Renderer>(true);
            baseLocalScale = transform.localScale;
        }

        private void OnEnable()
        {
            if (interactable == null)
                return;

            interactable.hoverEntered.AddListener(OnHoverEntered);
            interactable.hoverExited.AddListener(OnHoverExited);
            interactable.selectEntered.AddListener(OnSelectEntered);
        }

        private void OnDisable()
        {
            if (interactable == null)
                return;

            interactable.hoverEntered.RemoveListener(OnHoverEntered);
            interactable.hoverExited.RemoveListener(OnHoverExited);
            interactable.selectEntered.RemoveListener(OnSelectEntered);
        }

        public void Configure(CardFanController controller, Transform target)
        {
            fanController = controller;
            tableTarget = target;
        }

        public void SetFanPose(Vector3 localPosition, Quaternion localRotation)
        {
            baseLocalPosition = localPosition;
            baseLocalRotation = localRotation;

            if (!isInHand)
                return;

            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
        }

        private void Update()
        {
            if (!isInHand || moveCoroutine != null)
                return;

            Vector3 targetPosition = baseLocalPosition;
            Vector3 targetScale = baseLocalScale;

            if (isHovered)
            {
                targetPosition += transform.parent.up * hoverLift;
                targetScale *= hoverScale;
            }

            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * 12f);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, baseLocalRotation, Time.deltaTime * 12f);
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 12f);
        }

        private void OnHoverEntered(HoverEnterEventArgs args)
        {
            isHovered = true;
            SetHighlight(true);
        }

        private void OnHoverExited(HoverExitEventArgs args)
        {
            isHovered = false;
            SetHighlight(false);
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (!isInHand)
                return;

            if (fanController == null || tableTarget == null)
            {
                Debug.LogWarning("CardView needs a CardFanController and a table target.", this);
                return;
            }

            fanController.TryPlayCard(this);
        }

        private void SetHighlight(bool highlighted)
        {
            foreach (Renderer cardRenderer in renderers)
            {
                if (cardRenderer == null)
                    continue;

                Material material = cardRenderer.material;

                if (highlighted)
                {
                    if (material.HasProperty("_EmissionColor"))
                    {
                        material.EnableKeyword("_EMISSION");
                        material.SetColor("_EmissionColor", new Color(0.25f, 0.25f, 0.25f));
                    }
                }
                else if (material.HasProperty("_EmissionColor"))
                {
                    material.DisableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", Color.black);
                }
            }
        }

        public void MoveToTable(Transform destination = null)
        {
            if (moveCoroutine != null)
                return;

            if (destination != null)
                tableTarget = destination;

            if (tableTarget == null)
            {
                Debug.LogWarning("CardView necesita un destino de mesa.", this);
                return;
            }

            isHovered = false;
            isInHand = false;
            SetHighlight(false);

            if (interactable != null)
                interactable.enabled = false;

            moveCoroutine = StartCoroutine(AnimateToTable());
        }

        public void ResetToHand(Transform handTransform, CardFanController controller, Transform target)
        {
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }

            fanController = controller;
            tableTarget = target;
            isHovered = false;
            isInHand = true;

            transform.SetParent(handTransform, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = baseLocalScale;

            if (interactable != null)
                interactable.enabled = true;

            SetHighlight(false);
        }

        private IEnumerator AnimateToTable()
        {
            Vector3 startPosition = transform.position;
            Quaternion startRotation = transform.rotation;
            Vector3 startScale = transform.lossyScale;
            Vector3 targetPosition = tableTarget.position;
            Quaternion targetRotation = tableTarget.rotation * Quaternion.Euler(tableRotationOffset);
            float elapsed = 0f;

            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / moveDuration);
                float smooth = Mathf.SmoothStep(0f, 1f, t);

                Vector3 position = Vector3.Lerp(startPosition, targetPosition, smooth);
                position.y += Mathf.Sin(t * Mathf.PI) * moveArcHeight;

                transform.position = position;
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, smooth);

                float scaleFactor = 1f;
                transform.localScale = Vector3.Scale(baseLocalScale, Vector3.one * scaleFactor);

                yield return null;
            }

            transform.SetParent(tableTarget, true);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(tableRotationOffset);
            transform.localScale = baseLocalScale;

            moveCoroutine = null;
        }
    }
}
