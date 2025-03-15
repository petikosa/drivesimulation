using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCvSharp;
using UnityEngine;
using Rect = UnityEngine.Rect;
using UnityEngine.UI;
using Mat = OpenCvSharp.Mat;
using OpenCvSharp;
using Size = OpenCvSharp.Size;

public class CamVision : MonoBehaviour
{
    public RenderTexture leftRenderTexture;
    public RenderTexture rightRenderTexture;
    public RenderTexture depthRenderTexture;
    public RawImage leftImage;
    public RawImage rightImage;
    public RawImage depthImage;
    private Mat matLeft;
    private Mat matRight;
    private Texture2D texture2D;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    { 
        Texture2D leftTexture2D = toTexture(leftRenderTexture);
        Texture2D rightTexture2D = toTexture(rightRenderTexture);
        Mat matOut = new Mat(leftTexture2D.height, leftTexture2D.width, CvType.CV_8UC1);
        Mat matOutFinal = new Mat(leftTexture2D.height, leftTexture2D.width, CvType.CV_8UC1);
        Mat matLeft = OpenCvSharp.Unity.TextureToMat(leftTexture2D);
        Mat matRight = OpenCvSharp.Unity.TextureToMat(rightTexture2D);
        Mat matLeft8U = new Mat(leftTexture2D.height, leftTexture2D.width, CvType.CV_8UC1);
        Mat matRight8U = new Mat(leftTexture2D.height, leftTexture2D.width, CvType.CV_8UC1);
        matLeft.ConvertTo(matLeft8U, CvType.CV_8UC1);
        matRight.ConvertTo(matRight8U, CvType.CV_8UC1);
        Cv2.CvtColor(matLeft8U, matLeft8U, ColorConversionCodes.BGR2GRAY);
        Cv2.CvtColor(matRight8U, matRight8U, ColorConversionCodes.BGR2GRAY);
        StereoBM stereoBm = StereoBM.Create(16, 15);
        stereoBm.Compute(matLeft8U, matRight8U, matOut);
        Cv2.GaussianBlur(matOut, matOut, new Size(5,5), 2);
        matOut.ConvertTo(matOutFinal, CvType.CV_8UC1);
        texture2D = OpenCvSharp.Unity.MatToTexture(matOutFinal);
        depthImage.texture = texture2D;
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