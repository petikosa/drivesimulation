using System;
using System.Collections;
using System.Diagnostics;
using OpenCVForUnity.CoreModule;
using OpenCvSharp;
using UnityEngine;
using UnityEngine.UI;
using Rect = UnityEngine.Rect;
using Mat = OpenCvSharp.Mat;
using Debug = UnityEngine.Debug;
using Size = OpenCvSharp.Size;

public class CamVision : MonoBehaviour
{
    public RenderTexture leftRenderTexture;
    public RenderTexture rightRenderTexture;
    public RawImage depthImage;
    public GameObject linePrefab;
    
    private Mat matLeft;
    private Mat matRight;
    private Texture2D texture2D;


    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    private void Start()
    {
    }

    private void FixedUpdate()
    {
        CastRays();
    }

    private void showStereo()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var leftTexture2D = toTexture(leftRenderTexture);
        var rightTexture2D = toTexture(rightRenderTexture);
        var matOut = new Mat(leftTexture2D.height, leftTexture2D.width, CvType.CV_8UC1);
        var matOutFinal = new Mat(leftTexture2D.height, leftTexture2D.width, CvType.CV_8UC1);
        var matLeft = OpenCvSharp.Unity.TextureToMat(leftTexture2D);
        var matRight = OpenCvSharp.Unity.TextureToMat(rightTexture2D);
        var matLeft8U = new Mat(leftTexture2D.height, leftTexture2D.width, CvType.CV_8UC1);
        var matRight8U = new Mat(leftTexture2D.height, leftTexture2D.width, CvType.CV_8UC1);
        matLeft.ConvertTo(matLeft8U, CvType.CV_8UC1);
        matRight.ConvertTo(matRight8U, CvType.CV_8UC1);
        Cv2.CvtColor(matLeft8U, matLeft8U, ColorConversionCodes.BGR2GRAY);
        Cv2.CvtColor(matRight8U, matRight8U, ColorConversionCodes.BGR2GRAY);
        var stereoBm = StereoBM.Create(16, 15);
        stereoBm.Compute(matLeft8U, matRight8U, matOut);
        Cv2.GaussianBlur(matOut, matOut, new Size(5, 5), 2);
        matOut.ConvertTo(matOutFinal, CvType.CV_8UC1);
        texture2D = OpenCvSharp.Unity.MatToTexture(matOutFinal);
        depthImage.texture = texture2D;
        stopwatch.Stop();
        Debug.Log(stopwatch.ElapsedMilliseconds);
    }

    private void CastRays()
    {
        int raysCount = 10;
        
        for (var x = 1; x <= raysCount; x++) 
        {
            for (var y = 1; y <= raysCount; y++)
            {
                for (var z = 1; z <= raysCount; z++)
                {
                    GameObject newLine = Instantiate(linePrefab, transform);

                    // Get the LineRenderer component
                    LineRenderer lineRenderer = newLine.GetComponent<LineRenderer>();

                    GameObject childObj = new GameObject("name" + y);
                    childObj.transform.parent = gameObject.transform;
                    lineRenderer.transform.parent = childObj.transform;
                    childObj.transform.Translate(Vector3.up * 1);
                    RaycastHit hit;
                    Vector3 shift = Vector3.up * 1.5f;
                    Vector3 vector = Quaternion.Euler((360 / (raysCount / 3)) * x, (360 / (raysCount / 3)) * y, (360 / (raysCount / 3)) * z) * Vector3.forward;
                    if (Physics.Raycast(transform.position + shift, vector, out hit))
                    {
                        lineRenderer.SetPosition(0, transform.position + shift);
                        lineRenderer.SetPosition(1, hit.point);
                    }

                    StartCoroutine(DestructLine(childObj));
                }
            }
        }
    }
    
    IEnumerator DestructLine(GameObject obj)
    {
        yield return new WaitForSeconds(0.02f);
        Destroy(obj);
    }

    private Texture2D toTexture(RenderTexture myRenderTexture)
    {
        RenderTexture.active = myRenderTexture;
        var myTexture2D = new Texture2D(256, 256);
        myTexture2D.ReadPixels(new Rect(0, 0, myRenderTexture.width, myRenderTexture.height), 0, 0);
        myTexture2D.Apply();
        return myTexture2D;
    }
}