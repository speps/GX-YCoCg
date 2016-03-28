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

        var bufferTemp = RenderTexture.GetTemporary(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.ARGB32);
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

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<"))
        {
            filterType = (FilterType)(((int)filterType + (int)FilterType.Count - 1) % (int)FilterType.Count);
        }
        if (GUILayout.Button(">"))
        {
            filterType = (FilterType)(((int)filterType + 1) % (int)FilterType.Count);
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
