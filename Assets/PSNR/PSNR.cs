using System;
using UnityEngine;
using UnityEngine.Rendering;

public static class PSNR
{
    private static Material matPSNR;

    public static float Compute(Texture reference, Texture subject)
    {
        var idTemp0 = Shader.PropertyToID("Temp0");
        var idTemp1 = Shader.PropertyToID("Temp1");
        int currentW = reference.width;
        int currentH = reference.height;
        int reduceW = currentW;
        int reduceH = currentH;

        if (matPSNR == null)
        {
            matPSNR = (Material)Resources.Load("PSNR", typeof(Material));
        }
        var bufferMSE = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGBFloat);

        matPSNR.SetTexture("_SubjTex", subject);

        // Build command buffer for MSE and then reduce with add
        var cmd = new CommandBuffer();
        cmd.name = "PSNR";
        cmd.GetTemporaryRT(idTemp0, currentW, currentH, 0, FilterMode.Point, RenderTextureFormat.ARGBFloat);
        cmd.Blit((Texture)reference, idTemp0, matPSNR, 0); // MSE
        while (reduceW > 1 || reduceH > 1)
        {
            reduceW = Mathf.Max(1, reduceW >> 1);
            reduceH = Mathf.Max(1, reduceH >> 1);

            cmd.GetTemporaryRT(idTemp1, reduceW, reduceH, 0, FilterMode.Point, RenderTextureFormat.ARGBFloat);
            cmd.Blit(idTemp0, idTemp1, matPSNR, 1); // reduce
            cmd.ReleaseTemporaryRT(idTemp0);
            cmd.GetTemporaryRT(idTemp0, reduceW, reduceH, 0, FilterMode.Point, RenderTextureFormat.ARGBFloat);
            cmd.Blit(idTemp1, idTemp0);
            cmd.ReleaseTemporaryRT(idTemp1);

            currentW = reduceW;
            currentH = reduceH;
        }
        cmd.Blit(idTemp0, bufferMSE);
        cmd.ReleaseTemporaryRT(idTemp0);
        Graphics.ExecuteCommandBuffer(cmd);

        // Get into Texture2D (Texture doesn't have ReadPixels)
        var texMSE = new Texture2D(1, 1, TextureFormat.RGBAFloat, false, true);
        RenderTexture.active = bufferMSE;
        texMSE.ReadPixels(new Rect(0, 0, 1, 1), 0, 0);
        RenderTexture.active = null;

        // Get data from texture and convert to floats then MSE
        byte[] mseData = texMSE.GetRawTextureData();
        float mseR = BitConverter.ToSingle(mseData, 0);
        float mseG = BitConverter.ToSingle(mseData, 4);
        float mseB = BitConverter.ToSingle(mseData, 8);
        float mse = (mseR + mseG + mseB) / (reference.width * reference.height * 3);

        return 10.0f * Mathf.Log10(255.0f * 255.0f / mse);
    }
}
