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
        [MenuItem(StreamingImageSequenceConstants.MENU_PATH + "Convert MovieProxy to SpriteAnimation", false, 5)]
        private static void ConvertToSpriteAnimation()
        {
            if (Selection.gameObjects == null || 0 == Selection.gameObjects.Length)
            {
                return;
            }
            foreach (var orgGo in Selection.gameObjects)
            {
                StreamingImageSequenceToSpriteAnimation.ConvertIt(orgGo);
            }
        }

//----------------------------------------------------------------------------------------------------------------------

        [MenuItem(StreamingImageSequenceConstants.MENU_PATH + "Import AE Timeline", false, 10)]
        static void ImportAETimeline() {
            string strPath = EditorUtility.OpenFilePanel("Open File", "", "jstimeline");
            if (strPath.Length != 0) {
                JstimelineImporter.ImportTimeline(strPath);
            }
        }

//----------------------------------------------------------------------------------------------------------------------

        /*

        [MenuItem(StreamingImageSequenceConstants.MENU_PATH + "Create MovieProxy/Register files", false, 6)]
        static void ImportAndCreateSpriteAnimation()
        {
            importPictureFiles(PictureFileImporterParam.Mode.SpriteAnimation);

        }
        */

        [MenuItem(StreamingImageSequenceConstants.MENU_PATH + "Reset",false,50)]
        static void Reset()
        {
            UpdateManager.ResetPlugin();
            PreviewTextureFactory.Reset();
        }


//----------------------------------------------------------------------------------------------------------------------
        [MenuItem(StreamingImageSequenceConstants.MENU_PATH + "Show Loaded Images",false,52)]
        static void ShowLoadedImages() {
            StringBuilder sb = new StringBuilder();

            for (int imageType = 0; imageType < StreamingImageSequenceConstants.MAX_IMAGE_TYPES; ++imageType) {
                sb.AppendLine("TEXTURE_TYPE: " + imageType.ToString());

                List<string> loadedTextures = new List<string>();
                StreamingImageSequencePlugin.ListLoadedImages(imageType, (fileName) => {
                    loadedTextures.Add(fileName);
                });

                foreach (var fileName in loadedTextures) {
                    StreamingImageSequencePlugin.GetImageData(fileName,imageType, Time.frameCount, 
                        out ImageData readResult);
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

//----------------------------------------------------------------------------------------------------------------------

    }


}