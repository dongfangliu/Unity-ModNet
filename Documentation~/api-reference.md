# MODNet API Reference

[English](#) | [中文](./api-reference.zh-CN.md)

## Namespace

All MODNet classes are in the `MODNet` namespace:

```csharp
using MODNet;
```

---

## Core Classes

### MODNetDetector

Main class for running MODNet inference.

#### Constructor

```csharp
public MODNetDetector(MODNetResourceSet resources, int width, int height)
```

**Parameters:**
- `resources`: MODNetResourceSet containing model and shader references
- `width`: Desired inference width (will be auto-scaled and aligned)
- `height`: Desired inference height (will be auto-scaled and aligned)

**Example:**
```csharp
var detector = new MODNetDetector(resources, 512, 384);
```

#### Methods

##### ProcessImage

```csharp
public void ProcessImage(Texture input)
```

Processes an input texture and generates an alpha matte.

**Parameters:**
- `input`: Input texture (WebCamTexture, Texture2D, RenderTexture, etc.)

**Example:**
```csharp
detector.ProcessImage(webCamTexture);
```

##### Dispose

```csharp
public void Dispose()
```

Releases all resources. **Must** be called when done using the detector.

**Example:**
```csharp
void OnDestroy() {
    detector?.Dispose();
}
```

#### Properties

##### MatteTexture

```csharp
public RenderTexture MatteTexture { get; }
```

Returns the generated alpha matte as a RenderTexture.

**Range:** Values are in [0, 1] where 0 is transparent and 1 is opaque.

**Example:**
```csharp
RenderTexture matte = detector.MatteTexture;
myMaterial.SetTexture("_Matte", matte);
```

---

### MODNetDetectorDebug

Debug version of MODNetDetector with detailed logging.

#### Constructor

```csharp
public MODNetDetectorDebug(MODNetResourceSet resources, int width, int height)
```

Same as MODNetDetector, but outputs detailed debug information to Console.

**Use for:**
- Debugging model loading issues
- Checking input/output tensor shapes
- Verifying buffer sizes
- Troubleshooting preprocessing

**Example:**
```csharp
// Replace regular detector with debug version
var detector = new MODNetDetectorDebug(resources, 512, 384);
```

---

### MODNetCompositor

MonoBehaviour component for easy integration with Unity UI.

#### Inspector Fields

```csharp
[SerializeField] RawImage _previewUI
```
RawImage component to display the result.

```csharp
[SerializeField] MODNetResourceSet _resources
```
Reference to MODNetResourceSet asset.

```csharp
[SerializeField] Vector2Int detectDimension = new Vector2Int(512, 384)
```
Inference resolution (width, height).

#### Properties

##### InputTexture

```csharp
public Texture InputTexture { get; set; }
```

Input texture to process (typically a WebCamTexture).

**Example:**
```csharp
compositor.InputTexture = webCamTexture;
```

##### PreviewUI

```csharp
public RawImage PreviewUI { get; }
```

Returns the configured preview RawImage component.

#### Methods

##### SetThreshold

```csharp
public void SetThreshold(float value)
```

Sets the threshold value for the compositor material.

**Parameters:**
- `value`: Threshold value (typically 0-1)

**Example:**
```csharp
compositor.SetThreshold(0.5f);
```

---

### MODNetWebCamTexture

Helper component for managing webcam input.

#### Inspector Fields

```csharp
public MODNetCompositor compositor
```
Reference to MODNetCompositor component.

```csharp
public int downSample = 2
```
Downsampling factor for webcam resolution.

#### Methods

##### Open

```csharp
public void Open()
```

Opens the webcam and starts processing.

**Example:**
```csharp
webCamController.Open();
```

##### Close

```csharp
public void Close()
```

Stops the webcam and disables processing.

##### SetThreshold

```csharp
public void SetThreshold(float value)
```

Passes threshold value to the compositor.

**Parameters:**
- `value`: Threshold value

##### EnablePreviewImage / DisablePreviewImage

```csharp
public void EnablePreviewImage()
public void DisablePreviewImage()
```

Shows or hides the preview UI.

---

### MODNetResourceSet

ScriptableObject for configuring MODNet resources.

#### Inspector Fields

```csharp
public NNModel model
```
Reference to the MODNet ONNX model file.

```csharp
public int referenceSize = 512
```
Reference size for automatic resolution scaling.

```csharp
public ComputeShader preprocess
```
Preprocessing compute shader (from Klak.NNUtils).

```csharp
public ComputeShader matte
```
Matte post-processing compute shader.

#### Creating a ResourceSet

1. Right-click in Project window
2. Create → ScriptableObjects → MODNet Resource Set
3. Configure the fields in Inspector
4. Save the asset

**Typical configuration:**
- Model: `Runtime/Resources/modnet.onnx`
- Reference Size: `512`
- Preprocess: `Packages/Klak NNUtils/Shaders/Preprocess.compute`
- Matte: `Shaders/Matte.compute`

---

### MODNetConfig

Internal configuration class (auto-generated, no direct usage needed).

Handles:
- Resolution scaling based on referenceSize
- Alignment to multiples of 32
- Aspect ratio preservation

---

## Usage Patterns

### Pattern 1: Direct Detector Usage

For maximum control:

```csharp
using MODNet;
using UnityEngine;

public class MyController : MonoBehaviour
{
    public MODNetResourceSet resources;
    MODNetDetector detector;
    WebCamTexture webCam;

    void Start()
    {
        detector = new MODNetDetector(resources, 512, 384);
        webCam = new WebCamTexture(640, 480);
        webCam.Play();
    }

    void Update()
    {
        detector.ProcessImage(webCam);
        var matte = detector.MatteTexture;
        // Use matte in custom shader
    }

    void OnDestroy()
    {
        detector?.Dispose();
    }
}
```

### Pattern 2: Using Compositor Component

For quick UI integration:

```csharp
using MODNet;
using UnityEngine;

public class WebCamSetup : MonoBehaviour
{
    public MODNetCompositor compositor;
    WebCamTexture webCam;

    void Start()
    {
        webCam = new WebCamTexture(640, 480);
        compositor.InputTexture = webCam;
        webCam.Play();
    }
}
```

### Pattern 3: Using Helper Component

For the simplest setup:

```csharp
// Attach MODNetWebCamTexture to a GameObject
// Configure compositor reference in Inspector
// Call Open() to start

public class UIController : MonoBehaviour
{
    public MODNetWebCamTexture webCamController;

    public void StartWebcam()
    {
        webCamController.Open();
    }

    public void StopWebcam()
    {
        webCamController.Close();
    }
}
```

---

## Shader Integration

### Using Matte in Custom Shaders

The MatteTexture can be used in any Unity shader:

```hlsl
// In your custom shader
sampler2D _Matte;

fixed4 frag(v2f i) : SV_Target
{
    fixed4 color = tex2D(_MainTex, i.uv);
    float alpha = tex2D(_Matte, i.uv).r;  // Matte is in R channel
    color.a = alpha;
    return color;
}
```

### Compositor Shader Graph

The package includes a pre-built Compositor ShaderGraph (`Shaders/Compositor.shadergraph`) that:
- Combines input texture with matte
- Supports threshold adjustment
- Provides smooth alpha blending

---

## Best Practices

### Resource Management

Always dispose of detectors:

```csharp
void OnDestroy()
{
    detector?.Dispose();
    detector = null;
}
```

### Performance Optimization

Start with standard resolution and adjust:

```csharp
// Start here
new MODNetDetector(resources, 512, 384);

// Too slow? Reduce:
new MODNetDetector(resources, 320, 240);

// Too fast and quality insufficient? Increase:
new MODNetDetector(resources, 640, 480);
```

### Error Handling

Check for null textures:

```csharp
void Update()
{
    if (inputTexture != null)
    {
        detector.ProcessImage(inputTexture);
    }
    else
    {
        Debug.LogWarning("Input texture is null");
    }
}
```

### Inspector Configuration

Use SerializeField for tunable parameters:

```csharp
[SerializeField] Vector2Int detectDimension = new Vector2Int(512, 384);
[SerializeField] MODNetResourceSet resources;
```

---

## See Also

- [Getting Started](./getting-started.md) - Setup and installation
- [Configuration Guide](./configuration-guide.md) - Detailed configuration options
- [Quick Reference](./quick-reference.md) - Quick configuration cheat sheet
- [Troubleshooting](./troubleshooting.md) - Common issues and solutions
