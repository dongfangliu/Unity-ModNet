# MODNet Configuration Guide

[English](#) | [中文](./configuration-guide.zh-CN.md)

## Understanding Width and Height Parameters

### Parameter Meaning

```csharp
new MODNetDetector(resources, width, height)
                              ^^^^^  ^^^^^^
                              Desired inference resolution
```

These parameters define the **desired inference resolution** (not the final actual resolution).

### Automatic Processing

MODNetConfig processes the input width and height as follows:

```csharp
// Step 1: Scale based on referenceSize (default 512), maintaining aspect ratio
float scale = Min(512 / width, 512 / height);
scaledWidth = width * scale;
scaledHeight = height * scale;

// Step 2: Align to multiples of 32
InputWidth = (scaledWidth + 31) / 32 * 32;
InputHeight = (scaledHeight + 31) / 32 * 32;
```

**Example calculations:**

| Input (width, height) | After Scaling | After Alignment (Actual) |
|----------------------|---------------|-------------------------|
| (640, 480) → 4:3 | (512, 384) | **(512, 384)** ✅ |
| (1920, 1080) → 16:9 | (512, 288) | **(512, 288)** |
| (800, 600) → 4:3 | (512, 384) | **(512, 384)** ✅ |
| (1280, 720) → 16:9 | (512, 288) | **(512, 288)** |
| (512, 512) → 1:1 | (512, 512) | **(512, 512)** |

**Key finding:** Same aspect ratio = same final inference resolution!

---

## Recommended Configuration Strategies

### Strategy 1: Match Input Aspect Ratio (Recommended) ✅

**Principle:** Pass the same aspect ratio as your input image, let MODNet auto-scale to optimal size.

```csharp
// If your WebCam is 640x480 (4:3)
new MODNetDetector(resources, 640, 480);
// Actual inference: 512x384

// If your WebCam is 1280x720 (16:9)
new MODNetDetector(resources, 1280, 720);
// Actual inference: 512x288

// If your image is 1920x1080 (16:9)
new MODNetDetector(resources, 1920, 1080);
// Actual inference: 512x288
```

**Advantages:**
- Automatically maintains aspect ratio, no distortion
- Simple and intuitive, just pass actual resolution
- MODNet automatically scales to optimal inference size

### Strategy 2: Directly Specify Target Resolution

**Principle:** If you know the desired inference resolution, pass it directly.

```csharp
// Standard configuration (balanced quality and performance)
new MODNetDetector(resources, 512, 384);  // 4:3 ratio
new MODNetDetector(resources, 512, 288);  // 16:9 ratio

// High quality configuration (requires stronger GPU)
new MODNetDetector(resources, 768, 576);  // 4:3, actual: 768x576
new MODNetDetector(resources, 1024, 768); // 4:3, actual: 1024x768

// Performance priority configuration (faster but lower quality)
new MODNetDetector(resources, 320, 240);  // 4:3, actual: 320x256
new MODNetDetector(resources, 416, 416);  // 1:1, actual: 416x416
```

### Strategy 3: Inspector-Adjustable Configuration (Most Flexible) ✅

Use `detectDimension` in MODNetCompositor:

```csharp
[SerializeField] Vector2Int detectDimension = new Vector2Int(512, 384);

void Start() {
    _detector = new MODNetDetector(_resources, detectDimension.x, detectDimension.y);
}
```

**Adjust in Unity Inspector:**
- Modify `detectDimension` in Inspector before running
- No recompilation needed, quick testing of different configurations
- Different configurations for different scenes

---

## Common Configuration Scenarios

### Scenario 1: WebCam Real-time Matting

```csharp
public class MODNetWebCamTexture : MonoBehaviour
{
    public MODNetCompositor compositor;
    WebCamTexture webCam;

    void Start() {
        // Option A: Low-res WebCam + High-quality inference (Recommended)
        webCam = new WebCamTexture(640, 480);
        // MODNetDetector config: (512, 384) - High inference quality

        // Option B: High-res WebCam + Downsampling
        webCam = new WebCamTexture(1280, 720);
        // MODNetDetector config: (512, 288) - Balanced

        // Option C: Performance priority
        webCam = new WebCamTexture(640, 480);
        // MODNetDetector config: (320, 240) - Fast inference
    }
}
```

### Scenario 2: Static Image Matting

```csharp
// High-quality matting (offline processing)
new MODNetDetector(resources, 1024, 768);  // Actual: 1024x768
```

### Scenario 3: Mobile or Low-end Devices

```csharp
// Performance priority configuration
new MODNetDetector(resources, 320, 240);   // Actual: 320x256
new MODNetDetector(resources, 256, 256);   // Actual: 256x256
```

---

## Performance vs Quality Trade-offs

| Configuration | Actual Resolution | Quality | Performance | Use Case |
|--------------|------------------|---------|-------------|----------|
| `(320, 240)` | 320x256 | ⭐⭐ | ⚡⚡⚡⚡⚡ | Mobile, low-end GPU |
| `(416, 416)` | 416x416 | ⭐⭐⭐ | ⚡⚡⚡⚡ | Real-time apps |
| `(512, 384)` | 512x384 | ⭐⭐⭐⭐ | ⚡⚡⚡ | **Standard (Recommended)** |
| `(640, 480)` | 640x480 | ⭐⭐⭐⭐ | ⚡⚡ | High-quality real-time |
| `(768, 576)` | 768x576 | ⭐⭐⭐⭐⭐ | ⚡ | High-quality offline |
| `(1024, 768)` | 1024x768 | ⭐⭐⭐⭐⭐ | ⚡ | Highest quality |

**GPU Cost Estimation:**
- Inference time proportional to pixel count
- `512x384` ≈ 197K pixels (baseline)
- `1024x768` ≈ 786K pixels (~4x computation)
- `320x256` ≈ 82K pixels (~2.4x speed)

---

## Tuning Workflow

```
1. Start with standard configuration: (512, 384)
2. Run game, use Unity Profiler to check GPU time
3. If FPS < 30: Reduce resolution → (416, 416) or (320, 240)
4. If FPS > 60 and quality unsatisfactory: Increase resolution → (640, 480)
5. Fine-tune to desired balance
```

---

## Advanced: Dynamic Quality Adjustment

```csharp
public class AdaptiveMODNet : MonoBehaviour
{
    MODNetDetector _detector;
    MODNetResourceSet _resources;

    // Dynamically adjust resolution based on FPS
    void AdjustQuality(float targetFPS)
    {
        float currentFPS = 1f / Time.deltaTime;

        if (currentFPS < targetFPS * 0.8f)
        {
            // Lower quality
            _detector?.Dispose();
            _detector = new MODNetDetector(_resources, 320, 240);
        }
        else if (currentFPS > targetFPS * 1.2f)
        {
            // Increase quality
            _detector?.Dispose();
            _detector = new MODNetDetector(_resources, 640, 480);
        }
    }
}
```

---

## Frequently Asked Questions

### Q: Why do (640, 480) and (800, 600) give the same result?

**A:** Because they have the same aspect ratio (both 4:3), after scaling and alignment they both become `512x384`.

### Q: How to get the highest quality?

**A:** Set a larger `referenceSize`:

```csharp
// In MODNetResourceSet
public int referenceSize = 1024;  // Change from 512 to 1024

// Then use
new MODNetDetector(resources, 1024, 768);  // Actual: 1024x768
```

### Q: Must Width and Height be multiples of 32?

**A:** No! MODNetConfig automatically aligns. You can pass any values:
- `(640, 480)` → Aligns to `(640, 480)` ✅
- `(641, 481)` → Aligns to `(640, 480)` ✅
- `(1920, 1080)` → Scales then aligns to `(512, 288)` ✅

### Q: Can I use portrait orientation?

**A:** Yes!

```csharp
new MODNetDetector(resources, 480, 640);   // Portrait 3:4
new MODNetDetector(resources, 720, 1280);  // Portrait 9:16
```

---

## Summary

**Simplest method:**
```csharp
// Just pass your input image dimensions, MODNet handles everything
new MODNetDetector(resources, inputTexture.width, inputTexture.height);
```

**Recommended standard configuration:**
```csharp
// 4:3 ratio (common for webcams)
[SerializeField] Vector2Int detectDimension = new Vector2Int(512, 384);

// 16:9 ratio (HD video)
[SerializeField] Vector2Int detectDimension = new Vector2Int(512, 288);
```

**Performance priority:**
```csharp
new MODNetDetector(resources, 320, 240);
```

**Quality priority:**
```csharp
new MODNetDetector(resources, 768, 576);
```
