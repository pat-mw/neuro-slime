using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using ScriptableObjectArchitecture;
using RetinaNetworking.Server;
using Cysharp.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing;
using Gif.Components;
using System.IO;

public class ScreenRecord : MonoBehaviour
{
    public SlimeSettings slimeSettings;

    public GameEvent onStartRecording;

    public GameEvent onStopRecording;

    //public string imageFolder = "NeuroprintFramesTemp";

    public float captureEveryXSeconds = 2f;

    public int gifFrameRate = 12;

    private int frameCounter = 0;

    private Camera screenshotCamera;

    private bool takeScreenshotOnNextFrame = false;

    public TemplateEmailWithAttachment email;

    private bool isRecording;

    private void Awake()
    {
        frameCounter = 0;
        email = GetComponent<TemplateEmailWithAttachment>();
        screenshotCamera = GetComponent<Camera>();
        ResetTargetTexture();
        onStartRecording.AddListener(StartRecording);
        onStopRecording.AddListener(StopRecording);
    }


    void TakeScreenshot()
    {
        ResetTargetTexture();
        takeScreenshotOnNextFrame = true;
    }


    private void OnPostRender()
    {
        if (takeScreenshotOnNextFrame)
        {
            takeScreenshotOnNextFrame = false;

            Wenzil.Console.Console.Log($"RECORDING: Taking Screenshot {frameCounter}");
            RenderTexture renderTexture = screenshotCamera.targetTexture;
            
            // read image
            Texture2D renderResult = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            renderResult.ReadPixels(rect, 0, 0);

            // save image to png
            byte[] byteArray = renderResult.EncodeToPNG();

            // TODO: add user name and timestamp here for unique naming
            string filepath = Application.persistentDataPath + "/neuroprint_frame_" + frameCounter + ".png";
            System.IO.File.WriteAllBytes(filepath, byteArray);
            Wenzil.Console.Console.Log($"Saved screenshot to: {filepath}");

            // reset/clean-up
            RenderTexture.ReleaseTemporary(renderTexture);
            ResetTargetTexture();
        }
    }

    private void ResetTargetTexture()
    {
        screenshotCamera.targetTexture = RenderTexture.GetTemporary(slimeSettings.width, slimeSettings.height, 16);
    }


    private void StartRecording()
    {

        Debug.Log($"START RECORDING");
        isRecording = true;

        RecordingLoop();
    }


    private void StopRecording()
    {
        isRecording = false;
        Debug.Log($"STOP RECORDING");

        ExportGif();
    }

    async private void RecordingLoop()
    {
        if (isRecording)
        {
            TakeScreenshot();

            await UniTask.Delay(System.TimeSpan.FromSeconds(captureEveryXSeconds), ignoreTimeScale: false);

            frameCounter += 1;

            RecordingLoop();
        }
        else
        {
            return;
        }
    }

    private void ExportGif()
    {
        if (frameCounter == 0)
        {
            Debug.LogError($"No frames found to convert to gif");
            return;
        }

        int totalFrames = frameCounter;

        Debug.Log($"TOTAL FRAMES: {totalFrames}");
        frameCounter = 0;
        string[] imageFilePaths = new string[totalFrames];
        for (int i = 0; i < totalFrames; i++)
        {
            Debug.Log($"index: {i}");
            string filepath = Application.persistentDataPath + "/neuroprint_frame_" + frameCounter + ".png";
            imageFilePaths[i] = filepath;
            frameCounter += 1;
        }

        string outputFilePath = Application.persistentDataPath + "/neurogif.gif";
        AnimatedGifEncoder gif = new AnimatedGifEncoder();
        gif.Start(outputFilePath);
        gif.SetDelay((int)(1000 / gifFrameRate));

        //-1:no repeat,0:always repeat
        gif.SetRepeat(0);

        for (int i = 0; i < imageFilePaths.Length-1; i++)
        {
            Debug.Log($"Attempting to load png image from: {imageFilePaths[i]}");

            if (File.Exists(imageFilePaths[i]))
            {
                try
                {
                    string path = "c:\\Users\\pmass\\AppData\\LocalLow\\kouo\\neuroprint\\neuroprint_frame_0.png";
                    //Image bob = Image.FromFile(imageFilePaths[i]);
                    Image bob = Image.FromFile(path);
                    Debug.Log($"Image loaded [{i}]: {bob}");
                    gif.AddFrame(bob);
                }
                catch (System.OutOfMemoryException e)
                {
                    Debug.LogError($"OUT OF MEMORY: {e}");
                    return;
                }
                catch (System.IO.FileNotFoundException e)
                {
                    Debug.LogError($"FILE NOT FOUND: {e}");
                    return;
                }
                catch (System.ArgumentException e)
                {
                    Debug.LogError($"ARGUMENT EXCEPTION: {e}");
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogError($"EXCEPTION: {e}");
                }
            }
        }
        gif.Finish();

        Debug.Log($"FINISHED PROCESSING GIF. Saved To: {outputFilePath}");

    }
}
