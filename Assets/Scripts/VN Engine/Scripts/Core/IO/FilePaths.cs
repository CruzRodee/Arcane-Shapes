using UnityEngine;


public class FilePaths
{
    public const string HOME_DIRECTORY_SYMBOL = "~/";
    public static readonly string root = $"{Application.dataPath}/gameData/";

    //Resources Paths
    public static readonly string resources_graphics = "Graphics/";
    public static readonly string resources_backgroundImages = $"{resources_graphics}BG Images/";
    public static readonly string resources_backgroundVideos = $"{resources_graphics}BG Videos/";
    public static readonly string resources_blendTextures = $"{resources_graphics}Transition Effects/";

    public static readonly string resources_audio = "Audio/";
    public static readonly string resources_audio_sfx = $"{resources_audio}SFX/";
    public static readonly string resources_audio_voices = $"{resources_audio}Voices/";
    public static readonly string resources_audio_music = $"{resources_audio}Music/";
    public static readonly string resources_audio_ambience = $"{resources_audio}Ambience/";

    public static readonly string resources_dialogueFiles = $"Dialogue Files/";

    public static string GetPathToResource(string defaultPath, string resourceName)
    {
        if (resourceName.StartsWith(HOME_DIRECTORY_SYMBOL))
            return resourceName.Replace(HOME_DIRECTORY_SYMBOL, root);

        return defaultPath + resourceName;
    }
}
