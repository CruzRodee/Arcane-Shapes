using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class GraphicLayer
{
    public const string LAYER_OBJECT_NAME_FORMAT = "Layer_{0}";
    public int layerDepth = 0;
    public Transform Panel;

    public GraphicObject currentGraphic = null;
    private List<GraphicObject> oldGraphics = new List<GraphicObject>();

    public Coroutine SetTexture(string filePath, float transitionSpeed = 1f, Texture blendingTexture = null, bool immediate = false)
    {
        Texture tex = Resources.Load<Texture>(filePath);

        if (tex == null)
        {
            Debug.LogWarning($"GraphicLayer: SetTexture: Texture not found at path: {filePath}. Please make sure the texture is located in a Resources folder.");
            return null;
        }

        return SetTexture(tex, transitionSpeed, blendingTexture, filePath);
    }

    public Coroutine SetTexture(Texture tex, float transitionSpeed = 1f, Texture blendingTexture = null, string filePath = "", bool immediate = false)
    {
        return CreateGraphic(tex, transitionSpeed, filePath, blendingTexture: blendingTexture, immediate: immediate);
    }

    public Coroutine SetVideo(string filePath, float transitionSpeed = 1f, bool useAudio = true, Texture blendingTexture = null, bool immediate = false)
    {
        VideoClip clip = Resources.Load<VideoClip>(filePath);

        if (clip == null)
        {
            Debug.LogWarning($"GraphicLayer: SetVideo: VideoClip not found at path: {filePath}. Please make sure the video is located in a Resources folder.");
            return null;
        }

        return SetVideo(clip, transitionSpeed, useAudio, blendingTexture, filePath);
    }

    public Coroutine SetVideo(VideoClip video, float transitionSpeed = 1f, bool useAudio = true, Texture blendingTexture = null, string filePath = "", bool immediate = false)
    {
        return CreateGraphic(video, transitionSpeed, filePath, useAudioForVideo: useAudio, blendingTexture, immediate);
    }

    private Coroutine CreateGraphic<T>(T graphicData, float transitionSpeed, string filePath, bool useAudioForVideo = true, Texture blendingTexture = null, bool immediate = false)
    {
        GraphicObject newGraphic = null;

        if (graphicData is Texture)
            newGraphic = new GraphicObject(this, filePath, graphicData as Texture, immediate);
        else if (graphicData is VideoClip)
            newGraphic = new GraphicObject(this, filePath, graphicData as VideoClip, useAudioForVideo, immediate);

        if (currentGraphic != null && !oldGraphics.Contains(currentGraphic))
            oldGraphics.Add(currentGraphic);

        currentGraphic = newGraphic;

        if (!immediate)
            return currentGraphic.FadeIn(transitionSpeed, blendingTexture);
        //Otherwise this is tan immediate effect
        DestroyOldGraphics();
        return null;
    }


    public void DestroyOldGraphics()
    {
        foreach (var g in oldGraphics)
            Object.Destroy(g.renderer.gameObject);

        oldGraphics.Clear();
    }

    public void Clear()
    {
        if (currentGraphic != null)
            currentGraphic.FadeOut();

        foreach (var g in oldGraphics)
            g.FadeOut();
    }

}
