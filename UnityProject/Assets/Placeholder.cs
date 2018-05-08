using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class Placeholder : MonoBehaviour
{
    public Transform textMeshObject;
    KeywordRecognizer recognizer;

    private void Start()
    {
        this.textMesh = this.textMeshObject.GetComponent<TextMesh>();
        
        this.recognizer = new KeywordRecognizer(
            new string[] { "scan", "run", "reset" });

        this.recognizer.OnPhraseRecognized += OnPhraseRecognized;

        this.recognizer.Start();

        this.OnReset();
    }

    void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        if (args.text == "scan")
        {
            this.OnScan();
        }
        else if (args.text == "run")
        {
            this.OnRun();
        }
        else if (args.text == "reset")
        {
            this.OnReset();
        }
    }

    public void OnScan()
    {
        this.textMesh.text = "scanning for 30s";

#if !UNITY_EDITOR
    MediaFrameQrProcessing.Wrappers.ZXingBarCodeScanner.ScanFirstCameraForBarCode(
        result =>
        {
          UnityEngine.WSA.Application.InvokeOnAppThread(() =>
          {
            this.textMesh.text = result ?? "not found";
          }, 
          false);
        },
        TimeSpan.FromSeconds(30));
#endif
    }
    public void OnRun()
    {
        this.textMesh.text = "running forever";

#if !UNITY_EDITOR
    MediaFrameQrProcessing.Wrappers.ZXingBarCodeScanner.ScanFirstCameraForBarCode(
        result =>
        {
          UnityEngine.WSA.Application.InvokeOnAppThread(() =>
          {
            this.textMesh.text = $"Got result {result} at {DateTime.Now}";
          }, 
          false);
        },
        null);
#endif
    }
    public void OnReset()
    {
        this.textMesh.text = "say scan or run to start";
    }
    TextMesh textMesh;
}
