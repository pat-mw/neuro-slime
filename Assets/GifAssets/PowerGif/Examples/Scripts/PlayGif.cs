using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.GifAssets.PowerGif;
using Sirenix.OdinInspector;
using System.IO;

[RequireComponent(typeof(AnimatedImage))]
public class PlayGif : MonoBehaviour
{
    private AnimatedImage animatedImage;

    private void Start()
    {
        animatedImage = GetComponent<AnimatedImage>();
    }


    [GUIColor(1, 0, 0)]
    [Button(ButtonSizes.Large)]
    public void PlayTestGif()
    {
        //var path = UnityEditor.EditorUtility.SaveFilePanel("Save", "Assets/GifAssets/PowerGif/Examples/Samples", "EncodeExample2", "gif");

        //if (path == "") return;

        //var bytes = File.ReadAllBytes(path);
        //var gif = Gif.Decode(bytes);

        //animatedImage.Play(gif);
    }

}
