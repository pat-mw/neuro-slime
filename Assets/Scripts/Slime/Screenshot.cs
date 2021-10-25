using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjectArchitecture;
using RetinaNetworking.Server;

[RequireComponent(typeof(EmailWithAttachment))]
[RequireComponent(typeof(Camera))]
public class Screenshot : MonoBehaviour
{
    public SlimeSettings slimeSettings;

    public GameEvent onTakeScreenshot;

    private Camera screenshotCamera;

    private bool takeScreenshotOnNextFrame = false;

    private EmailWithAttachment email;

    private void Awake()
    {
        email = GetComponent<EmailWithAttachment>();
        screenshotCamera = GetComponent<Camera>();
        ResetTargetTexture();
        onTakeScreenshot.AddListener(TakeScreenshot);

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
            Debug.Log($"Taking Screenshot");
            RenderTexture renderTexture = screenshotCamera.targetTexture;
            
            // read image
            Texture2D renderResult = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            renderResult.ReadPixels(rect, 0, 0);

            // save image to png
            byte[] byteArray = renderResult.EncodeToPNG();
            string filepath = Application.dataPath + "/CameraScreenshot.png";
            System.IO.File.WriteAllBytes(filepath, byteArray);
            Debug.Log($"Saved screenshot to: {filepath}");

            // send email
            email.SendEmail(filepath);

            // reset/clean-up
            RenderTexture.ReleaseTemporary(renderTexture);
            takeScreenshotOnNextFrame = false;
            ResetTargetTexture();

            
        }
    }

    private void ResetTargetTexture()
    {
        screenshotCamera.targetTexture = RenderTexture.GetTemporary(slimeSettings.width, slimeSettings.height, 16);
    }
}
