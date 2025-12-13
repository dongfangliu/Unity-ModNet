# MODNet Troubleshooting Guide

[English](#) | [中文](./troubleshooting.zh-CN.md)

## Issue: MatteTexture is Always Black

### Fixed Issues

#### 1. ✅ Incorrect Preprocessing Coefficients (Fixed)
**Problem:** Previously used incorrect ColorCoeffs
- Incorrect value: `new Vector4(-127.5f, -127.5f, -127.5f, 1.0f/127.5f)`
- Correct value: `new Vector4(-1, -1, -1, 2)`

**Reason:** Unity textures are already in [0,1] range, no need to divide by 255

#### 2. ✅ Removed Unnecessary Vertical Flip
Matte.compute now reads directly without Y-axis flipping

### Diagnostic Steps

#### Step 1: Check Model with Debug Detector

Replace `MODNetDetector` with `MODNetDetectorDebug` in your code:

```csharp
// Change this
_detector = new MODNetDetector(_resources, detectDimension.x, detectDimension.y);

// To this
_detector = new MODNetDetectorDebug(_resources, detectDimension.x, detectDimension.y);
```

Run and check Console for detailed logs:
- Is the model loaded successfully?
- What are the input/output shapes?
- What is the output value range? (should be [0,1])

#### Step 2: Verify Model Output Name

MODNet model output might not use the default name. Check Console for output list:

```
[MODNet] Model outputs:
  - output_name: shape=1,1,H,W
```

If the output name is not default, modify `MODNetDetector.cs` line 82:

```csharp
// Current code (uses default output)
post.SetBuffer(0, "MatteBuffer", _worker.PeekOutputBuffer());

// If you need to specify output name
post.SetBuffer(0, "MatteBuffer", _worker.PeekOutputBuffer("output_name"));
```

### Common Issues

#### Q1: Output is all zeros

**Possible causes:**
- Incorrect model input format (NCHW vs NHWC)
- Incorrect preprocessing coefficients
- Input image is all black or unsupported format

**Solutions:**
1. Confirm `ImagePreprocess` uses `nchwFix: true` (already set)
2. Check if InputTexture is valid
3. Test the model itself with debug detector

#### Q2: Output values outside [0,1] range

**Possible causes:**
- Model output hasn't gone through sigmoid activation
- Additional post-processing needed

**Solution:**
Modify `Matte.compute`, add sigmoid:

```hlsl
float sigmoid(float x) {
    return 1.0 / (1.0 + exp(-x));
}

// In PostprocessMatte
float matte = sigmoid(MatteBuffer[offset]);
```

#### Q3: Image is upside down

**Solution:**
Modify `Matte.compute` line 16, enable vertical flip:

```hlsl
uint offset = (InputSize.y - 1 - id.y) * InputSize.x + id.x;
```

#### Q4: ResourceSet Configuration Issues

Ensure MODNetResourceSet.asset is correctly configured:
- ✅ Model: `Runtime/Resources/modnet.onnx`
- ✅ Reference Size: `512`
- ✅ Preprocess: `Packages/Klak NNUtils/.../Preprocess.compute`
- ✅ Matte: `Shaders/Matte.compute`

### Advanced Debugging

#### Using MatteDebug.compute

1. In MODNetResourceSet, temporarily change `matte` to `MatteDebug.compute`
2. Run and observe output:
   - R channel: Raw values
   - G channel: Enhanced values (2x)
   - B channel: Vertically flipped version
   - A channel: Raw values

#### Check Buffer Size

In `MODNetDetectorDebug` Console output:

```
[MODNet] Output buffer count: 262144
[MODNet] Expected buffer size: 262144
```

These two values should be equal. If not, the model output size doesn't match expectations.

#### Verify Preprocessing

Add to `MODNetDetectorDebug.cs` in `AllocateObjects`:

```csharp
// Test preprocessing output
var testTexture = new Texture2D(64, 64);
_preprocess.Dispatch(testTexture, _resources.preprocess);
Debug.Log($"Preprocess tensor shape: {_preprocess.Tensor.shape}");
```

Should output something like: `shape=1,3,H,W` (NCHW format)

---

## Issue: Low FPS / Poor Performance

### Quick Fixes

1. **Reduce inference resolution:**
   ```csharp
   // From
   new MODNetDetector(resources, 512, 384);

   // To
   new MODNetDetector(resources, 320, 240);
   ```

2. **Check GPU usage:**
   - Open Unity Profiler
   - Look at GPU section
   - MODNet should use compute shaders efficiently

3. **Optimize WebCam resolution:**
   ```csharp
   // Don't use excessive webcam resolution
   webCam = new WebCamTexture(640, 480);  // Good
   // Instead of
   webCam = new WebCamTexture(1920, 1080);  // Overkill
   ```

---

## Issue: Poor Matting Quality

### Increase Inference Resolution

```csharp
// From
new MODNetDetector(resources, 320, 240);

// To
new MODNetDetector(resources, 640, 480);
// Or even
new MODNetDetector(resources, 768, 576);
```

### Check Input Image Quality

- Ensure good lighting
- Avoid motion blur
- Use higher quality webcam if possible

---

## Issue: Compilation Errors

### Missing Namespace

If you see errors like:
```
error CS0246: The type or namespace name 'MODNetDetector' could not be found
```

**Solution:** Add namespace to your script:
```csharp
using MODNet;
```

### Missing Assembly Reference

If MODNet types are not recognized:

1. Check that `MODNet.Runtime.asmdef` exists in Runtime folder
2. If you're using assembly definitions in your code, add reference to `MODNet.Runtime`

---

## Issue: Package Installation Problems

### Klak.NNUtils Not Found

**Solution:** Add scoped registry to `Packages/manifest.json`:

```json
{
  "scopedRegistries": [
    {
      "name": "Keijiro",
      "url": "https://registry.npmjs.com",
      "scopes": ["jp.keijiro"]
    }
  ]
}
```

### Unity Sentis Not Found

**Solution:** Ensure Unity version is 6000.0 or later. Sentis is included in Unity 6000.0+.

---

## Issue: Webcam Not Working

### Permission Issues

On some platforms, webcam access requires permissions:
- **macOS:** Grant permission in System Preferences
- **Windows:** Grant permission in Windows Settings
- **WebGL:** Browser will prompt for permission

### No Webcam Detected

```csharp
// Check available devices
WebCamDevice[] devices = WebCamTexture.devices;
Debug.Log($"Found {devices.Length} webcam devices");
foreach (var device in devices) {
    Debug.Log($"  - {device.name}");
}
```

---

## Still Having Issues?

1. **Check Console for errors** - Most issues show error messages
2. **Use Unity Profiler** - Identify performance bottlenecks
3. **Enable debug detector** - Get detailed runtime information
4. **Review sample code** - Compare with working example in `Samples~/WebCamDemo/`
5. **Check dependencies** - Ensure all required packages are installed

---

## Reporting Bugs

When reporting issues, please include:

1. Unity version
2. MODNet package version
3. Platform (Windows/macOS/Linux/Mobile)
4. Console error messages (full stack trace)
5. MODNetResourceSet configuration screenshot
6. Steps to reproduce

---

## Performance Checklist

- [ ] Using appropriate inference resolution (start with 512×384)
- [ ] WebCam resolution not excessive (640×480 is usually sufficient)
- [ ] GPU Compute backend enabled
- [ ] No unnecessary processing in Update()
- [ ] Detector disposed properly in OnDestroy()
- [ ] Using release build (not development build) for testing final performance

---

For more information, see:
- [Getting Started](./getting-started.md)
- [Configuration Guide](./configuration-guide.md)
- [Quick Reference](./quick-reference.md)
- [API Reference](./api-reference.md)
