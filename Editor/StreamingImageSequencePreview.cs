using System;

namespace UnityEngine.StreamingImageSequence {
    internal class StreamingImageSequencePreview : IDisposable {

    public void Dispose() {
        if (m_disposed) return;

        m_disposed= true;
        if (m_texture != null) {
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(m_texture);
            else
                UnityEngine.Object.DestroyImmediate(m_texture);
        }
        m_texture = null;
    }

//----------------------------------------------------------------------------------------------------------------------
    public void SetTexture(Texture2D tex) {
        m_texture = tex;
    }

//----------------------------------------------------------------------------------------------------------------------
    public void Render(Rect rect) {
        if (null== m_texture) {
            return;
        }
        Graphics.DrawTexture(rect, m_texture);
    }

//----------------------------------------------------------------------------------------------------------------------
    private bool m_disposed;
    Texture2D m_texture;
}

} //end namespace

