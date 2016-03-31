using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using System;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class YCoCgCameraScript : MonoBehaviour
{
    public FilterType filterType = FilterType.Nearest;
    private RenderTexture bufferYCoCg;
    private Material matYCoCgEncode;
    private Material matYCoCgDecode;

    private float psnr;

    private enum ScreenshotState
    {
        Idle,
        Start,
        Taking,
        Stop,
    };
    private ScreenshotState screenshotState = ScreenshotState.Idle;

    public enum FilterType
    {
        None,
        Nearest,
        Bilinear,
        EdgeDirected,
        Count,
    }

    void OnRenderImage(RenderTexture source, RenderTexture dest)
    {
        var camera = GetComponent<Camera>();

        var bufferTemp = RenderTexture.GetTemporary(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.ARGBFloat);
        if (filterType == FilterType.None)
        {
            Graphics.Blit(source, bufferTemp);
            Graphics.Blit(bufferTemp, dest);
        }
        else
        {
            if (bufferYCoCg == null || camera.pixelWidth != bufferYCoCg.width || camera.pixelHeight != bufferYCoCg.height)
            {
                bufferYCoCg = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.RGFloat);
            }
            if (matYCoCgEncode == null)
            {
                matYCoCgEncode = (Material)Resources.Load("YCoCgEncode", typeof(Material));
            }
            if (matYCoCgDecode == null)
            {
                matYCoCgDecode = (Material)Resources.Load("YCoCgDecode", typeof(Material));
            }
            matYCoCgDecode.SetInt("_FilterType", (int)filterType);
            Graphics.Blit(source, bufferYCoCg, matYCoCgEncode);
            Graphics.Blit(bufferYCoCg, bufferTemp, matYCoCgDecode);
            Graphics.Blit(bufferTemp, dest);
        }

        psnr = PSNR.Compute(source, bufferTemp);

        RenderTexture.ReleaseTemporary(bufferTemp);
    }

    void TakeScreenshot()
    {
        filterType = (FilterType)(((int)filterType + 1) % (int)FilterType.Count);
        string name = "YCoCg-Screenshot-" + filterType.ToString() + ".png";
        Application.CaptureScreenshot(name);
        Debug.Log(name);
        if (filterType == FilterType.None)
        {
            screenshotState = ScreenshotState.Stop;
        }
    }

    void OnPostRender()
    {
        if (screenshotState == ScreenshotState.Stop)
        {
            screenshotState = ScreenshotState.Idle;
        }
        if (screenshotState == ScreenshotState.Taking)
        {
            TakeScreenshot();
        }
        if (screenshotState == ScreenshotState.Start)
        {
            filterType = FilterType.None;
            screenshotState = ScreenshotState.Taking;
        }
    }

    void OnGUI()
    {
        if (screenshotState == ScreenshotState.Idle)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Screenshot"))
            {
                screenshotState = ScreenshotState.Start;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        GUILayout.BeginHorizontal();
        if (screenshotState == ScreenshotState.Idle)
        {
            if (GUILayout.Button("<"))
            {
                filterType = (FilterType)(((int)filterType + (int)FilterType.Count - 1) % (int)FilterType.Count);
            }
            if (GUILayout.Button(">"))
            {
                filterType = (FilterType)(((int)filterType + 1) % (int)FilterType.Count);
            }
        }
        GUILayout.Box(filterType.ToString());
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if (!float.IsInfinity(psnr))
        {
            GUILayout.BeginHorizontal();
            GUILayout.Box(string.Format("PSNR {0:F2}dB", psnr));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}
