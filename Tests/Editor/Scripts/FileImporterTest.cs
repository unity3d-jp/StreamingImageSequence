using NUnit.Framework;
using System.IO;
using Unity.StreamingImageSequence;
using Unity.StreamingImageSequence.Editor;

namespace Unity.StreamingImageSequence.EditorTests {
    public class FileImporterTest {
        [Test]
        public void EstimateAssetNameTest() {
            Assert.AreEqual("A12B",ImageSequenceImporter.EstimateAssetName(Path.Combine("E:", "A","png","A12B00000")));
            Assert.AreEqual("A12B",ImageSequenceImporter.EstimateAssetName(Path.Combine("E:","A","png","A12B00000")));
            Assert.AreEqual("A12B",ImageSequenceImporter.EstimateAssetName(Path.Combine("E:","A","png","A12B00000.png")));
            Assert.AreEqual("A",ImageSequenceImporter.EstimateAssetName(Path.Combine("E:","A","png","A_00000")));
            Assert.AreEqual("AB",ImageSequenceImporter.EstimateAssetName(Path.Combine("E:","A","png","AB_00000.png")));
            Assert.AreEqual("ABC",ImageSequenceImporter.EstimateAssetName(Path.Combine("E:","A","png","ABC_00000.png")));
            Assert.AreEqual("A",ImageSequenceImporter.EstimateAssetName(Path.Combine("E:","A","png","A_00000.png")));
            Assert.AreEqual("A",ImageSequenceImporter.EstimateAssetName(Path.Combine("E:","A","png","A__00000.png")));
            Assert.AreEqual("A",ImageSequenceImporter.EstimateAssetName(Path.Combine("E:","A","png","A___00000.png")));
            Assert.AreEqual("png",ImageSequenceImporter.EstimateAssetName(Path.Combine("E:","A","png","00000")));
            Assert.AreEqual("png",ImageSequenceImporter.EstimateAssetName(Path.Combine("E:","A","png","00000.png")));

            Assert.AreEqual("A12B",ImageSequenceImporter.EstimateAssetName(Path.Combine("A12B00000")));
            Assert.AreEqual("A12B",ImageSequenceImporter.EstimateAssetName(Path.Combine("A12B00000.png")));
            Assert.AreEqual("A",ImageSequenceImporter.EstimateAssetName(Path.Combine("A_00000")));
            Assert.AreEqual("AB",ImageSequenceImporter.EstimateAssetName(Path.Combine("AB_00000.png")));
            Assert.AreEqual("ABC",ImageSequenceImporter.EstimateAssetName(Path.Combine("ABC_00000.png")));
            Assert.AreEqual("A",ImageSequenceImporter.EstimateAssetName(Path.Combine("A_00000.png")));
            Assert.AreEqual("A",ImageSequenceImporter.EstimateAssetName(Path.Combine("A__00000.png")));
            Assert.AreEqual("A",ImageSequenceImporter.EstimateAssetName(Path.Combine("A___00000.png")));
            Assert.AreEqual("",ImageSequenceImporter.EstimateAssetName(Path.Combine("00000")));
            Assert.AreEqual("",ImageSequenceImporter.EstimateAssetName(Path.Combine("00000.png")));
        }

    }
}
