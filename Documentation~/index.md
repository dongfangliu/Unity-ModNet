# MODNet Documentation

[English](#english) | [中文](./index.zh-CN.md)

## English

### Documentation Index

#### Quick Start
- **[Getting Started](./getting-started.md)** - Installation and quick setup guide
- **[Quick Reference](./quick-reference.md)** ⭐ - Width/Height configuration cheat sheet

#### Detailed Configuration
- **[Configuration Guide](./configuration-guide.md)** - Detailed configuration strategies and performance optimization

#### Troubleshooting
- **[Troubleshooting](./troubleshooting.md)** - Common issues and solutions

#### API Reference
- **[API Reference](./api-reference.md)** - Complete API documentation

#### Code Examples
- **[Samples~/WebCamDemo/Scripts/MODNetConfigExamples.cs](../Samples~/WebCamDemo/Scripts/MODNetConfigExamples.cs)** - 9 practical configuration examples

---

## Which Document Should I Read?

### I want to get started quickly
→ Read [Getting Started](./getting-started.md)

### I don't know how to set width and height
→ Check **[Quick Reference](./quick-reference.md)** ⚡

### I want to understand all configuration options
→ Read [Configuration Guide](./configuration-guide.md)

### MatteTexture display issues
→ See [Troubleshooting](./troubleshooting.md)

### I want to see code examples
→ Open [Samples~/WebCamDemo/Scripts/MODNetConfigExamples.cs](../Samples~/WebCamDemo/Scripts/MODNetConfigExamples.cs)

---

## Common Tasks Quick Reference

### ✅ Create ResourceSet
```
1. Right-click Project → Create → ScriptableObjects → MODNet Resource Set
2. Configure fields:
   - Model: Runtime/Resources/modnet.onnx
   - Reference Size: 512
   - Preprocess: Packages/Klak NNUtils/Shaders/Preprocess.compute
   - Matte: Shaders/Matte.compute
```

### ✅ Basic Usage
```csharp
// Method 1: Using MODNetCompositor component
// Inspector → Configure detectDimension = (512, 384)

// Method 2: Using MODNetDetector directly
MODNetDetector detector = new MODNetDetector(resources, 512, 384);
detector.ProcessImage(inputTexture);
RenderTexture matte = detector.MatteTexture;
```

### ✅ Recommended Configurations
```csharp
// Standard configuration (balanced)
new MODNetDetector(resources, 512, 384);

// Performance priority
new MODNetDetector(resources, 320, 240);

// Quality priority
new MODNetDetector(resources, 768, 576);
```

---

## Key Technical Parameters

| Parameter | Value | Description |
|-----------|-------|-------------|
| **ColorCoeffs** | `(-1, -1, -1, 2)` | Preprocessing normalization coefficients |
| **NCHW Format** | `true` | Tensor format |
| **Alignment** | Multiple of 32 | Automatic alignment |
| **Vertical Flip** | Required | Unity coordinate system |
| **Reference Size** | 512 | Default scaling baseline |

---

## Performance Reference

| Resolution | Pixels | Relative Speed | Recommended Use |
|------------|--------|----------------|-----------------|
| 256×256 | 65K | 3.0× | Mobile |
| 320×256 | 82K | 2.4× | Low-end devices |
| **512×384** | **197K** | **1.0×** | **Standard** |
| 640×480 | 307K | 0.64× | High-quality real-time |
| 768×576 | 443K | 0.45× | Offline processing |

---

## Package Structure

```
com.modnet.unity/
├── Runtime/
│   ├── MODNetDetector.cs          # Core detector
│   ├── MODNetDetectorDebug.cs     # Debug version
│   ├── MODNetConfig.cs            # Configuration calculations
│   ├── MODNetResourceSet.cs       # ScriptableObject
│   ├── MODNetCompositor.cs        # UI component
│   ├── MODNetWebCamTexture.cs     # WebCam helper
│   └── Resources/
│       └── modnet.onnx            # Model file (25.9MB)
├── Shaders/
│   ├── Compositor.shadergraph     # Display shader
│   ├── MODNetCompositor.mat       # Compositor material
│   ├── Matte.compute              # Post-processing shader
│   ├── MatteDebug.compute         # Debug shader
│   └── Preprocess.compute         # Preprocessing shader
├── Samples~/
│   └── WebCamDemo/
│       ├── Scenes/Demo.unity
│       ├── Prefabs/ModNetWebCam.prefab
│       ├── Scripts/MODNetConfigExamples.cs
│       └── Resources/MODNet.asset
└── Documentation~/
    ├── index.md (this file)
    ├── getting-started.md
    ├── configuration-guide.md
    ├── quick-reference.md
    ├── troubleshooting.md
    └── api-reference.md
```

---

## Tips

1. **First time using?** Start with [Getting Started](./getting-started.md)
2. **Configuration questions?** Check [Quick Reference](./quick-reference.md)
3. **Encountered issues?** See [Troubleshooting](./troubleshooting.md)
4. **Need examples?** Open [Samples~/WebCamDemo/Scripts/MODNetConfigExamples.cs](../Samples~/WebCamDemo/Scripts/MODNetConfigExamples.cs)
