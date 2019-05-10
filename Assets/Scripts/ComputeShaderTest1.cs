using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ComputeShaderTest1 : MonoBehaviour
{
    public TextAsset inputTextureData;
    private SerializableTextureData deserializedInputTextureData;
    private ComputeShader computeShader;
    private ComputeBuffer inputDataBuffer;
    private float[] outputValuesData;
    private ComputeBuffer outputDataBuffer;
    private string saveAssetName = "out";

    // Use this for initialization
    void Start()
    {
        deserializedInputTextureData = JsonUtility.FromJson<SerializableTextureData>(inputTextureData.text);

        computeShader = Resources.Load<ComputeShader>("Shaders/ComputeShader1");

        if (computeShader == null)
            Debug.LogError("computeShader not found in the specified path");
        else
            compute();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void compute()
    {
        saveAssetName += deserializedInputTextureData.width + "x" + deserializedInputTextureData.height;

        int inputDataSize = deserializedInputTextureData.width * deserializedInputTextureData.height;

        int csMain = computeShader.FindKernel("CSMain");

        if (csMain < 0)
        {
            Debug.Log("Initialization failed.");
            return;
        }

        uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
        int offsetX, offsetY;
        int groupsX, groupsY, groupsZ;

        computeShader.GetKernelThreadGroupSizes(csMain, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
        offsetX = (int)threadGroupSizeX - 1;
        offsetY = (int)threadGroupSizeY - 1;

        groupsX = (deserializedInputTextureData.width + offsetX) / (int)threadGroupSizeX;
        groupsY = (deserializedInputTextureData.height + offsetY) / (int)threadGroupSizeY;
        groupsZ = 1;

        inputDataBuffer = new ComputeBuffer(inputDataSize, sizeof(float));
        inputDataBuffer.SetData(deserializedInputTextureData.data);
        computeShader.SetBuffer(csMain, "InputDataBuffer", inputDataBuffer);
        computeShader.SetInt("InputDataWidth", deserializedInputTextureData.width);
        computeShader.SetInt("InputDataHeight", deserializedInputTextureData.height);

        outputDataBuffer = new ComputeBuffer(inputDataSize, sizeof(float));
        computeShader.SetBuffer(csMain, "OutputDataBuffer", outputDataBuffer);

        Debug.Log("Dispatching [" + groupsX + "," + groupsY + "," + groupsZ + "] groups");

        var watch = System.Diagnostics.Stopwatch.StartNew();

        computeShader.Dispatch(csMain, groupsX, groupsY, groupsZ);

        watch.Stop();

        outputValuesData = new float[inputDataSize];
        outputDataBuffer.GetData(outputValuesData);

        Debug.Log("Compute Shader Execution Completed. Time elapsed (ns): " + watch.Elapsed.TotalMilliseconds * 1000000);

        saveOutDataAsJSON();
        saveOutDataAsTexture2D();
    }

    public void saveOutDataAsJSON()
    {
        string path = null;

#if UNITY_EDITOR
        path = "Assets/Resources/OutputData/";
#endif
#if !UNITY_STANDALONE
        path = "MyGame_Data/Resources/";
#endif

        path += saveAssetName + ".json";

        SerializableTextureData data = new SerializableTextureData();
        data.width = deserializedInputTextureData.width;
        data.height = deserializedInputTextureData.height;
        data.data = outputValuesData;
        string json = JsonUtility.ToJson(data);

        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.Write(json);
            }
        }
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    private void saveOutDataAsTexture2D()
    {
        string path = null;

#if UNITY_EDITOR
        path = "Assets/Resources/OutputData/";
#endif
#if !UNITY_STANDALONE
        path = "MyGame_Data/Resources/";
#endif

        path += saveAssetName + ".png";

        int inputDataSize = deserializedInputTextureData.width * deserializedInputTextureData.height;

        Texture2D tex = new Texture2D(deserializedInputTextureData.width, deserializedInputTextureData.height);
        Color[] cols = new Color[inputDataSize];

        int n = inputDataSize;
        for (int i = 0; i < n; i++)
        {
            float v = outputValuesData[i];
            cols[i] = new Color(v, v, v, 1);
        }

        tex.SetPixels(cols);
        tex.Apply();

        File.WriteAllBytes(path, tex.EncodeToPNG());
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    void OnDestroy()
    {
        if (inputDataBuffer != null)
            inputDataBuffer.Dispose();
        if (outputDataBuffer != null)
            outputDataBuffer.Dispose();
    }
}
