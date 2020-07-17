using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.StreamingImageSequence;

namespace UnityEditor.StreamingImageSequence {

    internal static class StreamingImageSequenceMenu
    {
        private const string PNG_EXTENSION = "png";
        private const string TGA_EXTENSION = "tga";

        [MenuItem(StreamingImageSequenceConstants.MENU_PATH +  "Create Clip", false, 1)]
        private static void RegisterFilesAndCreateStreamingImageSequence()
        {
            string path = EditorUtility.OpenFilePanel("Open File", "", PNG_EXTENSION + "," + TGA_EXTENSION);
            if (string.IsNullOrEmpty(path)) {
                return;
            }

            ImageSequenceImporter.ImportPictureFiles(ImageFileImporterParam.Mode.StreamingAssets, path, null);
        }

//----------------------------------------------------------------------------------------------------------------------

        [MenuItem(StreamingImageSequenceConstants.MENU_PATH + "Import AE Timeline", false, 10)]
        private static void ImportAETimeline() {
            string strPath = EditorUtility.OpenFilePanel("Open File", "", "jstimeline");
            if (strPath.Length != 0) {
                JstimelineImporter.ImportTimeline(strPath);
            }
        }

//----------------------------------------------------------------------------------------------------------------------

        [MenuItem(StreamingImageSequenceConstants.MENU_PATH + "Reset",false,50)]
        private static void Reset()
        {
            EditorUpdateManager.ResetImageLoading();
            PreviewTextureFactory.Reset();            
        }


//----------------------------------------------------------------------------------------------------------------------
        [MenuItem(StreamingImageSequenceConstants.MENU_PATH + "Debug/Show Loaded Images",false,52)]
        private static void ShowLoadedImages() {
            StringBuilder sb = new StringBuilder();

            for (int imageType = 0; imageType < StreamingImageSequenceConstants.MAX_IMAGE_TYPES; ++imageType) {
                sb.AppendLine("IMAGE_TYPE: " + imageType.ToString());

                List<string> loadedTextures = new List<string>();
                StreamingImageSequencePlugin.ListLoadedImages(imageType, (fileName) => {
                    loadedTextures.Add(fileName);
                });

                foreach (var fileName in loadedTextures) {
                    ImageLoader.GetImageDataInto(fileName,imageType, out ImageData readResult);
                    sb.Append("    ");
                    sb.Append(fileName);
                    sb.Append(". Status: " + readResult.ReadStatus);
                    sb.Append(", Size: (" + readResult.Width + ", " + readResult.Height);
                    sb.AppendLine(") ");
                }

                sb.AppendLine("----------------------------------------------------------------");
                sb.AppendLine();
                sb.AppendLine();
            }
            Debug.Log(sb.ToString());
        }

        [MenuItem(StreamingImageSequenceConstants.MENU_PATH + "Debug/Show Used Image Memory",false,53)]
        private static void ShowUsedImageMemory() {
            Debug.Log($"Used memory for images: {StreamingImageSequencePlugin.GetUsedImagesMemory().ToString()} MB");
        }
        
//----------------------------------------------------------------------------------------------------------------------
        [MenuItem(StreamingImageSequenceConstants.MENU_PATH + "Debug/Show Image Load Order",false,54)]
        private static void ShowImageLoadOrder() {
            StringBuilder sb = new StringBuilder();

            for (int imageType = 0; imageType < StreamingImageSequenceConstants.MAX_IMAGE_TYPES; ++imageType) {
                int latestRequestFrame = StreamingImageSequencePlugin.GetImageLoadOrder(imageType);
                sb.AppendLine($"IMAGE_TYPE: {imageType.ToString()}, order: {latestRequestFrame}");
                sb.AppendLine();
            }
            sb.AppendLine("Current Frame: " + ImageLoader.GetCurrentFrame());
            Debug.Log(sb.ToString());
        }
//----------------------------------------------------------------------------------------------------------------------

    }


}