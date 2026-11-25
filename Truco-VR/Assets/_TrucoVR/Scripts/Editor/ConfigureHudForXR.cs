using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace TrucoVR.Editor
{
    public static class ConfigureHudForXR
    {
        [MenuItem("TrucoVR/Configuración/Preparar HUD XR")]
        public static void ConfigureSelectedOrAllHud()
        {
            GameObject selectedObject = Selection.activeGameObject;
            Canvas[] canvases;

            if (selectedObject != null)
            {
                Canvas selectedCanvas = selectedObject.GetComponentInParent<Canvas>(true);
                canvases = selectedCanvas != null
                    ? new[] { selectedCanvas }
                    : Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            }
            else
            {
                canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            }

            int raycastersAdded = 0;

            foreach (Canvas canvas in canvases)
            {
                canvas.gameObject.SetActive(true);

                if (canvas.renderMode != RenderMode.WorldSpace)
                    canvas.renderMode = RenderMode.WorldSpace;

                if (canvas.worldCamera == null)
                    canvas.worldCamera = Camera.main;

                if (canvas.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
                {
                    canvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
                    raycastersAdded++;
                }

                GraphicRaycaster[] standardRaycasters = canvas.GetComponents<GraphicRaycaster>();
                foreach (GraphicRaycaster raycaster in standardRaycasters)
                    Object.DestroyImmediate(raycaster);

                ConfigureButtonRaycastTargets(canvas.transform);
                EditorUtility.SetDirty(canvas);
            }

            EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (eventSystems.Length == 0)
            {
                GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
                eventSystemObject.AddComponent<XRUIInputModule>();
                Undo.RegisterCreatedObjectUndo(eventSystemObject, "Crear EventSystem XR");
                eventSystems = new[] { eventSystemObject.GetComponent<EventSystem>() };
            }
            else
            {
                foreach (EventSystem eventSystem in eventSystems)
                {
                    StandaloneInputModule standaloneModule = eventSystem.GetComponent<StandaloneInputModule>();
                    if (standaloneModule != null)
                        Object.DestroyImmediate(standaloneModule);

                    if (eventSystem.GetComponent<XRUIInputModule>() == null)
                        eventSystem.gameObject.AddComponent<XRUIInputModule>();

                    EditorUtility.SetDirty(eventSystem);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[TrucoVR] HUD XR preparado: {canvases.Length} Canvas, {eventSystems.Length} EventSystem, {raycastersAdded} TrackedDeviceGraphicRaycaster agregado(s).");
        }

        private static void ConfigureButtonRaycastTargets(Transform root)
        {
            foreach (Graphic graphic in root.GetComponentsInChildren<Graphic>(true))
            {
                graphic.raycastTarget = true;
                EditorUtility.SetDirty(graphic);
            }
        }
    }
}
