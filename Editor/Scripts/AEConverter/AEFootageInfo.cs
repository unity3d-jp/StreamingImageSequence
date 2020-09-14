using System.Collections.Generic;
using UnityEngine.StreamingImageSequence;

namespace UnityEditor.StreamingImageSequence {

[System.Serializable]
internal class AEFootageInfo {
    public int               Version;
    public ImageDimensionInt Resolution;
    public string            Folder;
    public List<string>      Pictures;
}

} //end namespace