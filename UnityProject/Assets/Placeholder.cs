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
            this.textMesh.text = result?.Text ?? "not found";
          }, 
          false);
        },
        TimeSpan.FromSeconds(30));
#endif
  }
  public void OnReset()
  {
    this.textMesh.text = "say scan to start";
  }
  TextMesh textMesh;
}
