using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace COMMANDS
{
    public class CMD_DatabaseExtension_GraphicPanels : CMD_DatabaseExtension
    {
        private static string[] PARAM_PANEL = new string[] { "-p", "-panel" };
        private static string[] PARAM_LAYER = new string[] { "-l", "-layer" };
        private static string[] PARAM_MEDIA = new string[] { "-m", "-media" };
        private static string[] PARAM_SPEED = new string[] { "-spd", "-speed" };
        private static string[] PARAM_IMMEDIATE = new string[] { "-i", "-immediate" };
        private static string[] PARAM_BLENDTEX = new string[] { "-b", "-blend", "-blendtex" };
        private static string[] PARAM_USEVIDEOAUDIO = new string[] { "-aud", "-audio" };

        private const string HOMEDIRECTORY_SYMBOL = "~/";
        new public static void Extend(CommandDatabase database)
        {
            database.AddCommand("setlayermedia", new Func<string[], IEnumerator>(SetLayerMedia));
            database.AddCommand("clearlayermedia", new Func<string[], IEnumerator>(ClearLayerMedia));
        }

        private static IEnumerator SetLayerMedia(string[] data)
        {
            //Parameters available to function
            string panelName = "";
            int layer = 0;
            string mediaName = "";
            float transitionSpeed = 0;
            bool immediate = false;
            string blendTexName = "";
            bool useAudio = false;

            string pathToGraphic = "";
            UnityEngine.Object graphic = null;
            Texture blendTex = null;

            //Now get the parameters
            var parameters = ConvertDataToParameters(data);

            // Try to get the panel that this media is applied to
            parameters.TryGetValue(PARAM_PANEL, out panelName);
            GraphicPanel panel = GraphicPanelManager.instance.GetPanel(panelName);
            if (panel == null)
            {
                Debug.LogError($"[CMD_DatabaseExtension_GraphicPanels] SetLayerMedia: GraphicPanel '{panelName}' not found. Check panel name.");
                yield break;
            }

            // Get the layer to apply the media to
            parameters.TryGetValue(PARAM_LAYER, out layer, defaultValue: 0);

            //Try to get the media
            parameters.TryGetValue(PARAM_MEDIA, out mediaName);

            //Try to get if this is an immediate effect or not
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            //Try to get the transition speed if not immediate
            if (!immediate)
                parameters.TryGetValue(PARAM_SPEED, out transitionSpeed, defaultValue: GraphicPanelManager.DEFAULT_TRANSITION_SPEED);

            //Try to get the blending texture if any
            parameters.TryGetValue(PARAM_BLENDTEX, out blendTexName);

            //If this is a video, try to get if audio is used
            parameters.TryGetValue(PARAM_USEVIDEOAUDIO, out useAudio, defaultValue: false);

            //Now run the logic
            pathToGraphic = GetPathToGraphic(FilePaths.resources_backgroundImages, mediaName);
            graphic = Resources.Load<Texture>(pathToGraphic);

            // If texture not found, try to get video
            if (graphic == null)
            {
                pathToGraphic = GetPathToGraphic(FilePaths.resources_backgroundVideos, mediaName);
                graphic = Resources.Load<VideoClip>(pathToGraphic);
            }

            if (graphic == null)
            {
                Debug.LogError($"[CMD_DatabaseExtension_GraphicPanels] SetLayerMedia: Media '{mediaName}' not found at path: {pathToGraphic}. Check media name and ensure it is in a Resources folder.");
                yield break;
            }

            if (!immediate && blendTexName != string.Empty)
                blendTex = Resources.Load<Texture>(FilePaths.resources_blendTextures + blendTexName);

            // Lets try to get the layer to apply the media to
            GraphicLayer graphicLayer = panel.GetLayer(layer, createIfDoesNotExist: true);

            if (graphic is Texture)
            {
                yield return graphicLayer.SetTexture(graphic as Texture, transitionSpeed, blendTex, pathToGraphic, immediate);
            }
            else
            {
                yield return graphicLayer.SetVideo(graphic as VideoClip, transitionSpeed, useAudio, blendTex, pathToGraphic, immediate);
            }

        }

        private static IEnumerator ClearLayerMedia(string[] data)
        {
            //Parameters available to function
            string panelName = "";
            int layer = 0;
            float transitionSpeed = 0;
            bool immediate = false;
            string blendTexName = "";

            Texture blendTex = null;

            //Now get the parameters
            var parameters = ConvertDataToParameters(data);

            // Try to get the panel that this media is applied to
            parameters.TryGetValue(PARAM_PANEL, out panelName);
            GraphicPanel panel = GraphicPanelManager.instance.GetPanel(panelName);
            if (panel == null)
            {
                Debug.LogError($"[CMD_DatabaseExtension_GraphicPanels] ClearLayerMedia: GraphicPanel '{panelName}' not found. Check panel name.");
                yield break;
            }

            // Get the layer to apply the media to
            parameters.TryGetValue(PARAM_LAYER, out layer, defaultValue: -1);

            //Try to get if this is an immediate effect or not
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            //Try to get the transition speed if not immediate
            if (!immediate)
                parameters.TryGetValue(PARAM_SPEED, out transitionSpeed, defaultValue: GraphicPanelManager.DEFAULT_TRANSITION_SPEED);

            //Try to get the blending texture if any
            parameters.TryGetValue(PARAM_BLENDTEX, out blendTexName);

            if (!immediate && blendTexName != string.Empty)
                blendTex = Resources.Load<Texture>(FilePaths.resources_blendTextures + blendTexName);


            if (layer == -1)
                panel.Clear(transitionSpeed, blendTex, immediate);
            else
            {
                GraphicLayer graphiclayer = panel.GetLayer(layer);
                if (graphiclayer == null)
                {
                    Debug.LogError($"[CMD_DatabaseExtension_GraphicPanels] ClearLayerMedia: GraphicLayer '{layer}' not found in panel '{panelName}'. Check layer number.");
                    yield break;
                }

                graphiclayer.Clear(transitionSpeed, blendTex, immediate);
            }
        }
        private static string GetPathToGraphic(string defaultPath, string graphicName)
        {
            if (graphicName.StartsWith(HOMEDIRECTORY_SYMBOL))
                return graphicName.Substring(HOMEDIRECTORY_SYMBOL.Length);



            return defaultPath + graphicName;
        }


    }
}
