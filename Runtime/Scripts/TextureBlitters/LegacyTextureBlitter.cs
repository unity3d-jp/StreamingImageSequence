using UnityEngine;

namespace Unity.StreamingImageSequence {

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
internal class LegacyTextureBlitter : BaseTextureBlitter {

    protected override void AwakeInternalV() { }

//----------------------------------------------------------------------------------------------------------------------    
    
    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        BlitToDest(destination);
    }    
    
}

} //end namespace