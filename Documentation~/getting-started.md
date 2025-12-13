# Getting Started with MODNet

[English](#) | [中文](./getting-started.zh-CN.md)

## Installation

### Method 1: Install via Git URL (Recommended)

1. Open your Unity project (Unity 6000.0 or later)
2. Open **Window → Package Manager**
3. Click **+ → Add package from git URL**
4. Enter the package URL:
   ```
   https://github.com/your-username/unity-modnet.git#v1.0.0
   ```
5. Click **Add**

### Method 2: Install from Local Path

1. Clone or download this repository
2. Open **Window → Package Manager**
3. Click **+ → Add package from disk**
4. Navigate to the package folder and select `package.json`
5. Click **Open**

### Method 3: Add to manifest.json

Add to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.modnet.unity": "https://github.com/your-username/unity-modnet.git#v1.0.0",
    ...
  },
  "scopedRegistries": [
    {
      "name": "Keijiro",
      "url": "https://registry.npmjs.com",
      "scopes": ["jp.keijiro"]
    }
  ]
}
```

**Note:** The `scopedRegistries` entry is required for the Klak.NNUtils dependency.

---

## Dependencies

This package requires:
- **Unity 6000.0** or later
- **Unity Sentis** 2.3.0 (will be installed automatically)
- **Klak.NNUtils** 2.2.1 (requires scoped registry)
- **Unity Burst** 1.8.17
- **Unity Collections** 2.4.0

All dependencies except Klak.NNUtils will be installed automatically via UPM.

---

## Quick Start (5 Minutes)

### Step 1: Import the WebCam Demo Sample

1. Open **Window → Package Manager**
2. Find **MODNet Background Removal** in the list
3. Expand **Samples**
4. Click **Import** next to "WebCam Demo"

This will import the demo scene to your project's `Assets/Samples/` folder.

### Step 2: Open the Demo Scene

1. Navigate to `Assets/Samples/MODNet Background Removal/1.0.0/WebCam Demo/Scenes/`
2. Open **Demo.unity**

### Step 3: Run the Scene

1. Click **Play**
2. The webcam should start automatically
3. You should see real-time background removal

**That's it!** You now have MODNet working in your project.

---

## Manual Setup (For Your Own Scene)

### Option A: Using the Compositor Component (Easiest)

**Step 1: Create UI Canvas**
```
1. Create Canvas: Hierarchy → UI → Canvas
2. Add RawImage: Right-click Canvas → UI → Raw Image
3. Scale the RawImage to desired size
```

**Step 2: Add MODNetCompositor**
```csharp
1. Select the RawImage in Hierarchy
2. Add Component → MODNetCompositor
3. Configure in Inspector:
   - Preview UI: Drag the RawImage itself
   - Resources: Drag MODNet.asset from Samples
   - Detect Dimension: (512, 384)
```

**Step 3: Set Input Texture**
```csharp
public class WebCamController : MonoBehaviour
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

### Option B: Using MODNetDetector Directly (Advanced)

```csharp
using MODNet;
using UnityEngine;

public class MyMODNetController : MonoBehaviour
{
    public MODNetResourceSet resources;
    MODNetDetector detector;
    WebCamTexture webCam;

    void Start()
    {
        // Create detector
        detector = new MODNetDetector(resources, 512, 384);

        // Start webcam
        webCam = new WebCamTexture(640, 480);
        webCam.Play();
    }

    void Update()
    {
        // Process frame
        detector.ProcessImage(webCam);

        // Get result
        RenderTexture matte = detector.MatteTexture;

        // Use the matte texture in your custom shader/material
        // ...
    }

    void OnDestroy()
    {
        detector?.Dispose();
    }
}
```

---

## Creating a MODNetResourceSet

If you need to create your own resource configuration:

1. **Create the asset:**
   - Right-click in Project window
   - Create → ScriptableObjects → MODNet Resource Set

2. **Configure the asset:**
   - **Model:** `Runtime/Resources/modnet.onnx`
   - **Reference Size:** `512`
   - **Preprocess:** `Packages/Klak NNUtils/Shaders/Preprocess.compute`
   - **Matte:** `Shaders/Matte.compute`

3. **Save** the asset

---

## Configuration Basics

### Understanding Width & Height Parameters

```csharp
new MODNetDetector(resources, width, height);
//                            ^^^^^  ^^^^^^
//                            Inference resolution
```

**Key Points:**
- These define the **inference resolution**, not input resolution
- MODNet automatically scales and aligns dimensions
- Aspect ratio matters more than exact values

**Recommended starting configuration:**
```csharp
// For 4:3 aspect ratio (most webcams)
new MODNetDetector(resources, 512, 384);

// For 16:9 aspect ratio (HD video)
new MODNetDetector(resources, 512, 288);

// Or match your input texture
new MODNetDetector(resources, inputTexture.width, inputTexture.height);
```

For detailed configuration options, see the [Configuration Guide](./configuration-guide.md).

---

## Performance Tips

### Standard Configuration (Recommended)
```csharp
detectDimension = new Vector2Int(512, 384);  // Balanced quality and performance
```

### Increase Performance (Lower Quality)
```csharp
detectDimension = new Vector2Int(320, 240);  // 2-3x faster
```

### Increase Quality (Lower Performance)
```csharp
detectDimension = new Vector2Int(768, 576);  // Better quality, slower
```

Use **Unity Profiler** to monitor GPU performance and adjust accordingly.

---

## Next Steps

- **Configure for your use case:** Read the [Configuration Guide](./configuration-guide.md)
- **Optimize performance:** See the [Quick Reference](./quick-reference.md)
- **Troubleshoot issues:** Check the [Troubleshooting Guide](./troubleshooting.md)
- **Learn the API:** Browse the [API Reference](./api-reference.md)
- **See code examples:** Open `Samples~/WebCamDemo/Scripts/MODNetConfigExamples.cs`

---

## Common Questions

**Q: Can I use this with recorded video files?**
A: Yes! Just pass any `Texture` to `detector.ProcessImage()` or set `compositor.InputTexture`.

**Q: Does it work on mobile?**
A: Yes, but you may need to lower the inference resolution (e.g., 320×240) for acceptable performance.

**Q: Can I customize the background?**
A: Yes! The `MatteTexture` is an alpha matte. Combine it with your input texture using a custom shader.

**Q: How accurate is the segmentation?**
A: MODNet provides high-quality portrait matting with fine edge details, better than simpler segmentation models.

---

## Support

- **Documentation:** [Documentation~/index.md](./index.md)
- **Issues:** Report bugs on GitHub
- **Examples:** Check `Samples~/WebCamDemo/Scripts/MODNetConfigExamples.cs`
