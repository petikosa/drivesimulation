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
    public GameObject newLine;

    private readonly Vector3 shift = Vector3.up * 1.5f;
    private int i = 1;
    private int j = 1;
    private int k = 1;

    private LineRenderer lineRenderer;
    private Mat matLeft;
    private Mat matRight;
    private readonly int raysCount = 10;
    private Texture2D texture2D;

    private void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        InvokeRepeating(nameof(CastRays), .1f, 0.1f);
    }

    private void FixedUpdate()
    {
    }

    private void OnRenderObject()
    {
        Graphics.DrawProceduralNow(MeshTopology.Points, 1, 10);
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
        var count = 0;

        if (j <= raysCount)
        {
            j++;
        } 
        
        if (j > raysCount)
        {
            j = 0;
        }

        RaycastHit hit;
        var degree = 360 / raysCount;
        var vector = Quaternion.Euler(0, j * degree, 0) * Vector3.forward;
        if (Physics.Raycast(transform.position + shift, vector, out hit))
        {
            lineRenderer.SetPosition(0, transform.position + shift);
            lineRenderer.SetPosition(1, hit.point);
        }
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