using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Linq;
#if ENABLE_WINMD_SUPPORT
using MediaFrameQrProcessing.Processors;
using Windows.Graphics.Imaging;
using ZXing;
#endif 

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

#if ENABLE_WINMD_SUPPORT
    MediaFrameQrProcessing.Wrappers.ZXingBarCodeScanner.ScanFirstCameraForBarCode(
        (finder, device, subtype) => new MultiBarCodeCaptureFrameProcessor(finder, device, subtype),
        this.HandleResults,
        TimeSpan.FromSeconds(30));
#endif
    }
    public void OnRun()
    {
        this.textMesh.text = "running forever";

#if ENABLE_WINMD_SUPPORT
        MediaFrameQrProcessing.Wrappers.ZXingBarCodeScanner.ScanFirstCameraForBarCode(
            (finder, device, subtype) => new MultiBarCodeCaptureFrameProcessor(finder, device, subtype),
            this.HandleResults,
            null);
#endif
    }
#if ENABLE_WINMD_SUPPORT
    void HandleResults(object result, SoftwareBitmap bitmap)
    {
        bitmap.Dispose();
        UnityEngine.WSA.Application.InvokeOnAppThread(
            () =>
            {
                var results = result as Result[];
                var textPieces = string.Join(
                    ",",
                    results?.Select(r => r.Text));

                this.textMesh.text = $"Got results [{textPieces}] at {DateTime.Now}";
            },
            false);
    }
#endif
    public void OnReset()
    {
        this.textMesh.text = "say scan or run to start";
    }
    TextMesh textMesh;
}
