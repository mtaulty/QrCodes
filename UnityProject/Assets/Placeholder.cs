using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Placeholder : MonoBehaviour
{
  public Transform textMeshObject;

  private void Start()
  {
    this.textMesh = this.textMeshObject.GetComponent<TextMesh>();
    this.OnReset();
  }
  public void OnScan()
  {
    this.textMesh.text = "scanning for 30s";

#if !UNITY_EDITOR
    MediaFrameQrProcessing.Wrappers.ZXingQrCodeScanner.ScanFirstCameraForQrCode(
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
    MediaFrameQrProcessing.Wrappers.ZXingQrCodeScanner.ScanFirstCameraForQrCode(
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
