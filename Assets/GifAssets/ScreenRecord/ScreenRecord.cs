using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using ScriptableObjectArchitecture;
using RetinaNetworking.Server;
using Cysharp.Threading.Tasks;
using System.IO;
using Assets.GifAssets.PowerGif;

public class ScreenRecord : MonoBehaviour
{
    public bool isActive = false;

    public SlimeSettings slimeSettings;

    public GameEvent onStartRecording;

    public GameEvent onStopRecording;

    //public string imageFolder = "NeuroprintFramesTemp";

    public float captureEveryXSeconds = 2f;

    public int gifFrameRate = 12;

    private int gifFrameDelay;

    private int frameCounter = 0;

    private Camera screenshotCamera;

    private bool takeScreenshotOnNextFrame = false;

    public TemplateEmailWithAttachment email;

    private bool isRecording;

    private List<Texture2D> frames = new List<Texture2D>();

    private void Awake()
    {
        ResetGif();
        gifFrameDelay = 1000/gifFrameRate;
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
            Texture2D renderResult = new Texture2D(renderTexture.width, renderTexture.height);
            Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            renderResult.ReadPixels(rect, 0, 0);

            frames.Add(renderResult);

            Wenzil.Console.Console.Log($"Added screenshot to frames: {renderResult.width} x {renderResult.height}");

            // reset/clean-up
            RenderTexture.ReleaseTemporary(renderTexture);
            ResetTargetTexture();
        }
    }

    private void ResetTargetTexture()
    {
        screenshotCamera.targetTexture = RenderTexture.GetTemporary(32, 32);
    }


    private void StartRecording()
    {
        if (isActive)
        {
            Debug.Log($"START RECORDING");
            ResetGif();
            isRecording = true;

            RecordingLoop();
        }
    }

    void ResetGif()
    {
        frames.Clear();
        frameCounter = 0;
    }

    private void StopRecording()
    {
        if (isActive)
        {
            isRecording = false;
            Debug.Log($"STOP RECORDING");

            ExportGif();
        }
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
        var totalFrames = frames.Count;
        Debug.Log($"TOTAL FRAMES: {totalFrames}");

        if (totalFrames == 0)
        {
            Debug.LogError($"No frames found to convert to gif");
            return;
        }
        string outputFilePath = Application.persistentDataPath + "/neurogif.gif";

        List<GifFrame> gifFrames = ConvertTexture2DtoGIF(frames);
        var gif = new Gif(gifFrames);
        var bytes = gif.Encode();
        File.WriteAllBytes(outputFilePath, bytes);

        // send email
        List<string> attachments = new List<string>();
        attachments.Add(outputFilePath);
        email.SendEmail(attachments);

        Debug.Log($"FINISHED PROCESSING GIF. Saved To: {outputFilePath}");

    }

    List<GifFrame> ConvertTexture2DtoGIF(List<Texture2D> frames)
    {
        /*ar framesConverted = frames.Select(f => new GifFrame(f, 0.1f)).ToList();*/

        List<GifFrame> outputFrames = new List<GifFrame>();

        foreach (var frame in frames)
        {
            outputFrames.Add(new GifFrame(frame, 0.1f));
        }

        return outputFrames;
    }

    Texture2D CompressSquareTexture(Texture2D texture, int newSize = 256)
    {
        int originalSize = texture.width; // only true considering a 1x1 aspect ratio (otherwise consider height separate

        if (newSize > originalSize)
        {
            Debug.LogError("Cannot compress a texture to a larger resolution");
            return null;
        }

        int stepSize = (int) originalSize / newSize; // compression ratio effectively

        Texture2D outputTexture = new Texture2D(newSize, newSize);
        for (int i = 0; i < originalSize; i += stepSize)
        {
            for(int j = 0; j < originalSize; i += stepSize)
            {
                var x = i / stepSize;
                var y = j / stepSize;
                Color col = texture.GetPixel(i, j);
                outputTexture.SetPixel(x, y, col);
            }
        }

        Debug.Log($"Compressed texture to: {outputTexture.width}x{outputTexture.height}");
        return outputTexture;
    }
}
