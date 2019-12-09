using NUnit.Framework;
using System.IO;

namespace UnityEditor.StreamingImageSequence.Tests {
    public class FileImporterTest {
        [Test]
        public void EstimateAssetNameTest() {
            Assert.AreEqual("A12B",PictureFileImporter.EstimateAssetName(Path.Combine("E:", "A","png","A12B00000")));
            Assert.AreEqual("A12B",PictureFileImporter.EstimateAssetName(Path.Combine("E:","A","png","A12B00000")));
            Assert.AreEqual("A12B",PictureFileImporter.EstimateAssetName(Path.Combine("E:","A","png","A12B00000.png")));
            Assert.AreEqual("A",PictureFileImporter.EstimateAssetName(Path.Combine("E:","A","png","A_00000")));
            Assert.AreEqual("AB",PictureFileImporter.EstimateAssetName(Path.Combine("E:","A","png","AB_00000.png")));
            Assert.AreEqual("ABC",PictureFileImporter.EstimateAssetName(Path.Combine("E:","A","png","ABC_00000.png")));
            Assert.AreEqual("A",PictureFileImporter.EstimateAssetName(Path.Combine("E:","A","png","A_00000.png")));
            Assert.AreEqual("A",PictureFileImporter.EstimateAssetName(Path.Combine("E:","A","png","A__00000.png")));
            Assert.AreEqual("A",PictureFileImporter.EstimateAssetName(Path.Combine("E:","A","png","A___00000.png")));
            Assert.AreEqual("png",PictureFileImporter.EstimateAssetName(Path.Combine("E:","A","png","00000")));
            Assert.AreEqual("png",PictureFileImporter.EstimateAssetName(Path.Combine("E:","A","png","00000.png")));

            Assert.AreEqual("A12B",PictureFileImporter.EstimateAssetName(Path.Combine("A12B00000")));
            Assert.AreEqual("A12B",PictureFileImporter.EstimateAssetName(Path.Combine("A12B00000.png")));
            Assert.AreEqual("A",PictureFileImporter.EstimateAssetName(Path.Combine("A_00000")));
            Assert.AreEqual("AB",PictureFileImporter.EstimateAssetName(Path.Combine("AB_00000.png")));
            Assert.AreEqual("ABC",PictureFileImporter.EstimateAssetName(Path.Combine("ABC_00000.png")));
            Assert.AreEqual("A",PictureFileImporter.EstimateAssetName(Path.Combine("A_00000.png")));
            Assert.AreEqual("A",PictureFileImporter.EstimateAssetName(Path.Combine("A__00000.png")));
            Assert.AreEqual("A",PictureFileImporter.EstimateAssetName(Path.Combine("A___00000.png")));
            Assert.AreEqual("",PictureFileImporter.EstimateAssetName(Path.Combine("00000")));
            Assert.AreEqual("",PictureFileImporter.EstimateAssetName(Path.Combine("00000.png")));
        }

    }
}
