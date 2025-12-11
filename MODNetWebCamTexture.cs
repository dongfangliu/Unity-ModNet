using Sirenix.OdinInspector;
using UnityEngine;

public class MODNetWebCamTexture : MonoBehaviour
{
    public MODNetCompositor compositor;
    public int downSample = 2;
    private WebCamTexture webCamTexture;

    [Button]
    public void Open()
    {
        compositor.enabled = true;
        compositor.PreviewUI.gameObject.SetActive(true);
        if (webCamTexture == null)
        {
            webCamTexture = new WebCamTexture(
                (int)compositor.PreviewUI.rectTransform.rect.width / downSample,
                (int)compositor.PreviewUI.rectTransform.rect.height / downSample);
            compositor.InputTexture = webCamTexture;
        }
        webCamTexture.Play();
    }

    public void Close()
    {
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
        }
        compositor.enabled = false;
        compositor.PreviewUI.gameObject.SetActive(false);
    }

    [Button]
    public void SetThreshold(float value)
    {
        compositor.SetThreshold(value);
    }

    public void EnablePreviewImage()
    {
        compositor.PreviewUI.gameObject.SetActive(true);
    }

    public void DisablePreviewImage()
    {
        compositor.PreviewUI.gameObject.SetActive(false);
    }
}
