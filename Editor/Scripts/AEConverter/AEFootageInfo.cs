using System.Collections.Generic;
using Unity.StreamingImageSequence;

namespace Unity.StreamingImageSequence.Editor {

[System.Serializable]
internal class AEFootageInfo {
    public int               Version;
    public ImageDimensionInt Resolution;
    public string            Folder;
    public List<string>      Pictures;
}

} //end namespace