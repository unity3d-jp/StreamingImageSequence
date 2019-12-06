using NUnit.Framework;

namespace UnityEditor.StreamingImageSequence.Tests {
    public class FileImporterTest {
        [Test]
        public void EstimateAssetNameTest() {
            Assert.AreEqual("A12B",PictureFileImporter.EstimateAssetName(@"E:\A\png\A12B00000"));
            Assert.AreEqual("A12B",PictureFileImporter.EstimateAssetName(@"E:\A\png\A12B00000.png"));
            Assert.AreEqual("A",PictureFileImporter.EstimateAssetName(@"E:\A\png\A_00000"));
            Assert.AreEqual("AB",PictureFileImporter.EstimateAssetName(@"E:\A\png\AB_00000.png"));
            Assert.AreEqual("ABC",PictureFileImporter.EstimateAssetName(@"E:\A\png\ABC_00000.png"));
            Assert.AreEqual("A",PictureFileImporter.EstimateAssetName(@"E:\A\png\A_00000.png"));
            Assert.AreEqual("A",PictureFileImporter.EstimateAssetName(@"E:\A\png\A__00000.png"));
            Assert.AreEqual("A",PictureFileImporter.EstimateAssetName(@"E:\A\png\A___00000.png"));
            Assert.AreEqual("png",PictureFileImporter.EstimateAssetName(@"E:\A\png\00000"));
            Assert.AreEqual("png",PictureFileImporter.EstimateAssetName(@"E:\A\png\00000.png"));

            Assert.AreEqual("A12B",PictureFileImporter.EstimateAssetName(@"A12B00000"));
            Assert.AreEqual("A12B",PictureFileImporter.EstimateAssetName(@"A12B00000.png"));
            Assert.AreEqual("A",PictureFileImporter.EstimateAssetName(@"A_00000"));
            Assert.AreEqual("AB",PictureFileImporter.EstimateAssetName(@"AB_00000.png"));
            Assert.AreEqual("ABC",PictureFileImporter.EstimateAssetName(@"ABC_00000.png"));
            Assert.AreEqual("A",PictureFileImporter.EstimateAssetName(@"A_00000.png"));
            Assert.AreEqual("A",PictureFileImporter.EstimateAssetName(@"A__00000.png"));
            Assert.AreEqual("A",PictureFileImporter.EstimateAssetName(@"A___00000.png"));
            Assert.AreEqual("",PictureFileImporter.EstimateAssetName(@"00000"));
            Assert.AreEqual("",PictureFileImporter.EstimateAssetName(@"00000.png"));
        }

    }
}
