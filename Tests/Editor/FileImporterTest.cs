using NUnit.Framework;

namespace UnityEditor.StreamingImageSequence.Tests {
    public class FileImporterTest {
        [Test]
        public void EstimateAssetNameTest() {
            Assert.AreEqual("A12B",PictureFileImportWindow.EstimateAssetName(@"E:\A\png\A12B00000"));
            Assert.AreEqual("A12B",PictureFileImportWindow.EstimateAssetName(@"E:\A\png\A12B00000.png"));
            Assert.AreEqual("A",PictureFileImportWindow.EstimateAssetName(@"E:\A\png\A_00000"));
            Assert.AreEqual("AB",PictureFileImportWindow.EstimateAssetName(@"E:\A\png\AB_00000.png"));
            Assert.AreEqual("ABC",PictureFileImportWindow.EstimateAssetName(@"E:\A\png\ABC_00000.png"));
            Assert.AreEqual("A",PictureFileImportWindow.EstimateAssetName(@"E:\A\png\A_00000.png"));
            Assert.AreEqual("A",PictureFileImportWindow.EstimateAssetName(@"E:\A\png\A__00000.png"));
            Assert.AreEqual("A",PictureFileImportWindow.EstimateAssetName(@"E:\A\png\A___00000.png"));
            Assert.AreEqual("png",PictureFileImportWindow.EstimateAssetName(@"E:\A\png\00000"));
            Assert.AreEqual("png",PictureFileImportWindow.EstimateAssetName(@"E:\A\png\00000.png"));

            Assert.AreEqual("A12B",PictureFileImportWindow.EstimateAssetName(@"A12B00000"));
            Assert.AreEqual("A12B",PictureFileImportWindow.EstimateAssetName(@"A12B00000.png"));
            Assert.AreEqual("A",PictureFileImportWindow.EstimateAssetName(@"A_00000"));
            Assert.AreEqual("AB",PictureFileImportWindow.EstimateAssetName(@"AB_00000.png"));
            Assert.AreEqual("ABC",PictureFileImportWindow.EstimateAssetName(@"ABC_00000.png"));
            Assert.AreEqual("A",PictureFileImportWindow.EstimateAssetName(@"A_00000.png"));
            Assert.AreEqual("A",PictureFileImportWindow.EstimateAssetName(@"A__00000.png"));
            Assert.AreEqual("A",PictureFileImportWindow.EstimateAssetName(@"A___00000.png"));
            Assert.AreEqual("",PictureFileImportWindow.EstimateAssetName(@"00000"));
            Assert.AreEqual("",PictureFileImportWindow.EstimateAssetName(@"00000.png"));
        }

    }
}
