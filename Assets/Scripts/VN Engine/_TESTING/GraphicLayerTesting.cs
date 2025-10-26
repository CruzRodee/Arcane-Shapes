using System.Collections;
using CHARACTERS;
using UnityEngine;


public class GraphicLayerTesting : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(Running());

        StartCoroutine(RunningLayers());
    }

    IEnumerator Running()
    {
        GraphicPanel panel = GraphicPanelManager.instance.GetPanel("Background");
        GraphicLayer layer = panel.GetLayer(0, true);

        yield return new WaitForSeconds(1f);

        layer.SetTexture("Graphics/BG Images/2");

        Texture blendTex = Resources.Load<Texture>("Graphics/Transition Effects/hurricane");
        //layer.SetTexture("Graphics/BG Images/2", blendingTexture: blendTex);

        yield return new WaitForSeconds(1);

        //layer.SetVideo("Graphics/BG Videos/Fantasy Landscape", transitionSpeed: 0.01f, useAudio: true);
        layer.SetVideo("Graphics/BG Videos/Fantasy Landscape", blendingTexture: blendTex);
        //layer.currentGraphic.renderer.material.SetColor("_Color", Color.red);

        yield return new WaitForSeconds(3);

        layer.currentGraphic.FadeOut();

        yield return new WaitForSeconds(2);

        Debug.Log(layer.currentGraphic);
    }

    IEnumerator RunningLayers()
    {
        GraphicPanel panel = GraphicPanelManager.instance.GetPanel("Background");
        GraphicLayer layer0 = panel.GetLayer(0, true);
        GraphicLayer layer1 = panel.GetLayer(1, true);

        layer0.SetVideo("Graphics/BG Videos/Nebula");
        layer1.SetTexture("Graphics/BG Images/Spaceshipinterior");

        yield return new WaitForSeconds(2);

        GraphicPanel cinematic = GraphicPanelManager.instance.GetPanel("Cinematic");
        GraphicLayer cinLayer = cinematic.GetLayer(0, true);

        Character Raelin = CharacterManager.instance.CreateCharacter("Raelin", true);

        yield return Raelin.Say("Let's take a look at a picture on the cinematic layer.");

        cinLayer.SetTexture("Graphics/Gallery/pup");

        yield return VNDialogueSystem.instance.Say("Narrator", "We truly don't deserve dogs.");

        cinLayer.Clear();

        yield return new WaitForSeconds(1f);

        panel.Clear();
    }


}
