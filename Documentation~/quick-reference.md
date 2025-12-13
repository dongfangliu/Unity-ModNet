# MODNet Quick Reference

[English](#) | [中文](./quick-reference.zh-CN.md)

## Width & Height Configuration Cheat Sheet

### 🎯 Recommended Configurations

```csharp
// Standard configuration (4:3 aspect ratio)
new MODNetDetector(resources, 512, 384);  // ← Use this for most cases

// Standard configuration (16:9 aspect ratio)
new MODNetDetector(resources, 512, 288);

// Or match input texture automatically
new MODNetDetector(resources, inputTexture.width, inputTexture.height);
```

### 📊 Configuration Table

| Setting | Actual Resolution | Performance | Quality | Use Case |
|---------|------------------|-------------|---------|----------|
| `(256, 256)` | 256×256 | ⚡⚡⚡⚡⚡ | ⭐⭐ | Mobile |
| `(320, 240)` | 320×256 | ⚡⚡⚡⚡ | ⭐⭐⭐ | Low-end devices |
| `(512, 384)` | **512×384** | ⚡⚡⚡ | ⭐⭐⭐⭐ | **Standard (Recommended)** |
| `(640, 480)` | 640×480 | ⚡⚡ | ⭐⭐⭐⭐ | High-quality real-time |
| `(768, 576)` | 768×576 | ⚡ | ⭐⭐⭐⭐⭐ | Offline processing |

### 🔑 Key Points

1. **Only aspect ratio matters** - MODNet automatically scales
2. **No need for multiples of 32** - Automatic alignment
3. **Same aspect ratio = same inference resolution**
   - `(640, 480)` and `(800, 600)` → both become `512×384`
   - `(1280, 720)` and `(1920, 1080)` → both become `512×288`

### 📝 Usage Examples

#### Configure in Inspector
```csharp
public class MyScript : MonoBehaviour {
    [SerializeField] Vector2Int detectDimension = new Vector2Int(512, 384);

    void Start() {
        _detector = new MODNetDetector(resources, detectDimension.x, detectDimension.y);
    }
}
```

#### WebCam Real-time Matting
```csharp
WebCamTexture webCam = new WebCamTexture(640, 480);
MODNetDetector detector = new MODNetDetector(resources, 512, 384);
```

#### Adaptive Quality
```csharp
// Adjust based on FPS
if (fps < 30) {
    detector = new MODNetDetector(resources, 320, 240);  // Lower quality
} else if (fps > 60) {
    detector = new MODNetDetector(resources, 640, 480);  // Higher quality
}
```

### 🛠️ Tuning Tips

1. **Start with standard configuration**: `(512, 384)`
2. **Low FPS?** Reduce resolution → `(320, 240)`
3. **High FPS?** Increase resolution → `(640, 480)`
4. **Use Unity Profiler** to monitor GPU time

### 📐 Calculation Formula

```
actual_width = round((input_width * min(512/input_width, 512/input_height)) / 32) * 32
actual_height = round((input_height * min(512/input_width, 512/input_height)) / 32) * 32
```

**Examples:**
- Input `(640, 480)` → Scale `(512, 384)` → Align **`512×384`** ✅
- Input `(1920, 1080)` → Scale `(512, 288)` → Align **`512×288`** ✅

---

## Complete Documentation

- [index.md](./index.md) - Documentation hub
- [getting-started.md](./getting-started.md) - Installation and setup
- [configuration-guide.md](./configuration-guide.md) - Detailed configuration
- [troubleshooting.md](./troubleshooting.md) - Problem solving
- [api-reference.md](./api-reference.md) - API documentation
