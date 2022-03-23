using System.IO;
using UnityEngine;

public class CameraCapture : MonoBehaviour
{
    public void CamCapture()
    {
        Camera cam = GetComponent<Camera>();

        cam.targetTexture = new RenderTexture(256, 256, 24, RenderTextureFormat.ARGB32);

        RenderTexture activeRenderTexture = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;

        cam.Render();

        Texture2D image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height, TextureFormat.ARGB32, false, true);
        image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
        image.Apply();
        RenderTexture.active = activeRenderTexture;

        byte[] bytes = image.EncodeToPNG();
        Destroy(image);

        File.WriteAllBytes(Application.dataPath + "/NeuralLF/Captures/" + gameObject.name + ".png", bytes);
    }
}
