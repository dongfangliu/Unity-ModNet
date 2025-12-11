using UnityEngine;
using UnityEngine.UI;
using MODNet;

public sealed class MODNetCompositor : MonoBehaviour
{
    [SerializeField] RawImage _previewUI = null;
    [SerializeField] MODNetResourceSet _resources = null;
    [SerializeField] Vector2Int detectDimension = new Vector2Int(512, 384);

    MODNetDetector _detector;

    public Texture InputTexture { get; set; }
    public RawImage PreviewUI => _previewUI;

    public void SetThreshold(float value)
      => _previewUI.material.SetFloat("_Threshold", value);

    void Start()
    {
        _detector = new MODNetDetector(_resources, detectDimension.x, detectDimension.y);
    }

    void OnDestroy()
    {
        _detector?.Dispose();
        _detector = null;
    }

    void Update()
    {
        if (InputTexture != null)
        {
            _detector.ProcessImage(InputTexture);
            _previewUI.materialForRendering.SetTexture("_Input", InputTexture);
            _previewUI.materialForRendering.SetTexture("_Mask", _detector.MatteTexture);
        }
        else
        {
            Debug.LogWarning("[MODNetCompositor] InputTexture is null!");
        }
    }
}
