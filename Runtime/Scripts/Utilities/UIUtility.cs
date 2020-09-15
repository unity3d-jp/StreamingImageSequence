using UnityEngine;
using UnityEngine.UI;

namespace Unity.StreamingImageSequence {

internal static class UIUtility {

    internal static Transform CreateCanvas(string gameObjectName= "Canvas") {
        GameObject canvasObj = new GameObject(gameObjectName);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        return canvasObj.transform;
    }

}

} //end namespace
