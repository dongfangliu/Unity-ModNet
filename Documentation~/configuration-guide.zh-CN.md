# MODNet 配置指南

## Width 和 Height 参数详解

### 参数含义

```csharp
new MODNetDetector(resources, width, height)
                              ^^^^^  ^^^^^^
                              目标推理分辨率
```

这两个参数定义了**期望的推理分辨率**（不是最终实际使用的分辨率）。

### 自动处理流程

MODNetConfig 会对输入的 width 和 height 进行以下处理：

```csharp
// 步骤 1: 根据 referenceSize (默认 512) 缩放，保持长宽比
float scale = Min(512 / width, 512 / height);
scaledWidth = width * scale;
scaledHeight = height * scale;

// 步骤 2: 对齐到 32 的倍数
InputWidth = (scaledWidth + 31) / 32 * 32;
InputHeight = (scaledHeight + 31) / 32 * 32;
```

**示例计算：**

| 输入 (width, height) | 缩放后 | 对齐后 (实际使用) |
|---------------------|--------|------------------|
| (640, 480) → 4:3 | (512, 384) | **(512, 384)** ✅ |
| (1920, 1080) → 16:9 | (512, 288) | **(512, 288)** |
| (800, 600) → 4:3 | (512, 384) | **(512, 384)** ✅ |
| (1280, 720) → 16:9 | (512, 288) | **(512, 288)** |
| (512, 512) → 1:1 | (512, 512) | **(512, 512)** |

**关键发现：**只要长宽比相同，最终推理分辨率就相同！

---

## 推荐设置策略

### 策略 1: 匹配输入图像的长宽比（推荐）✅

**原理：**传入与输入图像相同的长宽比，让 MODNet 自动缩放到合适尺寸。

```csharp
// 如果你的 WebCam 是 640x480 (4:3)
new MODNetDetector(resources, 640, 480);
// 实际推理: 512x384

// 如果你的 WebCam 是 1280x720 (16:9)
new MODNetDetector(resources, 1280, 720);
// 实际推理: 512x288

// 如果你的图像是 1920x1080 (16:9)
new MODNetDetector(resources, 1920, 1080);
// 实际推理: 512x288
```

**优点：**
- 自动保持长宽比，不会拉伸变形
- 简单直观，传入实际分辨率即可
- MODNet 会自动缩放到最优推理尺寸

### 策略 2: 直接指定目标推理分辨率

**原理：**如果你已知想要的推理分辨率，直接传入。

```csharp
// 标准配置（平衡质量和性能）
new MODNetDetector(resources, 512, 384);  // 4:3 比例
new MODNetDetector(resources, 512, 288);  // 16:9 比例

// 高质量配置（需要更强 GPU）
new MODNetDetector(resources, 768, 576);  // 4:3，实际: 768x576
new MODNetDetector(resources, 1024, 768); // 4:3，实际: 1024x768

// 性能优先配置（速度快但质量降低）
new MODNetDetector(resources, 320, 240);  // 4:3，实际: 320x256
new MODNetDetector(resources, 416, 416);  // 1:1，实际: 416x416
```

### 策略 3: 使用 Inspector 可调节配置（最灵活）✅

在 MODNetCompositor 中使用 `detectDimension`：

```csharp
[SerializeField] Vector2Int detectDimension = new Vector2Int(512, 384);

void Start() {
    _detector = new MODNetDetector(_resources, detectDimension.x, detectDimension.y);
}
```

**在 Unity Inspector 中调整：**
- 运行前在 Inspector 修改 `detectDimension`
- 无需重新编译，快速测试不同配置
- 可以为不同场景使用不同配置

---

## 常见配置场景

### 场景 1: WebCam 实时抠图

```csharp
// BodyPix 使用 320x240
public class MODNetWebCamTexture : MonoBehaviour
{
    public MODNetCompositor compositor;
    WebCamTexture webCam;

    void Start() {
        // 方案 A: 低分辨率 WebCam + 高质量推理（推荐）
        webCam = new WebCamTexture(640, 480);
        // MODNetDetector 配置: (512, 384) - 推理质量高

        // 方案 B: 高分辨率 WebCam + 降采样
        webCam = new WebCamTexture(1280, 720);
        // MODNetDetector 配置: (512, 288) - 平衡

        // 方案 C: 性能优先
        webCam = new WebCamTexture(640, 480);
        // MODNetDetector 配置: (320, 240) - 推理快
    }
}
```

### 场景 2: 静态图片抠图

```csharp
// 高质量抠图（离线处理）
new MODNetDetector(resources, 1024, 768);  // 实际: 1024x768
```

### 场景 3: 移动端或低端设备

```csharp
// 性能优先配置
new MODNetDetector(resources, 320, 240);   // 实际: 320x256
new MODNetDetector(resources, 256, 256);   // 实际: 256x256
```

---

## 性能与质量权衡

| 配置 | 实际分辨率 | 质量 | 性能 | 适用场景 |
|------|-----------|------|------|----------|
| `(320, 240)` | 320x256 | ⭐⭐ | ⚡⚡⚡⚡⚡ | 移动端、低端 GPU |
| `(416, 416)` | 416x416 | ⭐⭐⭐ | ⚡⚡⚡⚡ | 实时应用 |
| `(512, 384)` | 512x384 | ⭐⭐⭐⭐ | ⚡⚡⚡ | **标准配置（推荐）** |
| `(640, 480)` | 640x480 | ⭐⭐⭐⭐ | ⚡⚡ | 高质量实时 |
| `(768, 576)` | 768x576 | ⭐⭐⭐⭐⭐ | ⚡ | 高质量离线 |
| `(1024, 768)` | 1024x768 | ⭐⭐⭐⭐⭐ | ⚡ | 最高质量 |

**GPU 消耗估算：**
- 推理时间与像素数成正比
- `512x384` ≈ 197K 像素（基准）
- `1024x768` ≈ 786K 像素（~4倍计算量）
- `320x256` ≈ 82K 像素（~2.4倍速度）

---

## 实际应用建议

### 1. 对于你的 MIDI-Draw 项目

根据现有的 BodyPix 配置：

```csharp
// BodyPix 使用
new BodyDetector(resources, 320, 240);

// MODNet 推荐配置（更高质量，相近性能）
new MODNetDetector(resources, 512, 384);  // ← 推荐

// 或者保持相同分辨率
new MODNetDetector(resources, 320, 240);
```

### 2. 调优流程

```
1. 从标准配置开始: (512, 384)
2. 运行游戏，使用 Unity Profiler 查看 GPU 时间
3. 如果 FPS < 30: 降低分辨率 → (416, 416) 或 (320, 240)
4. 如果 FPS > 60 且质量不满意: 提高分辨率 → (640, 480)
5. 微调到满意的平衡点
```

### 3. 动态调整（高级）

如果需要运行时调整：

```csharp
public class AdaptiveMODNet : MonoBehaviour
{
    MODNetDetector _detector;
    MODNetResourceSet _resources;

    // 根据 FPS 动态调整分辨率
    void AdjustQuality(float targetFPS)
    {
        float currentFPS = 1f / Time.deltaTime;

        if (currentFPS < targetFPS * 0.8f)
        {
            // 降低质量
            _detector?.Dispose();
            _detector = new MODNetDetector(_resources, 320, 240);
        }
        else if (currentFPS > targetFPS * 1.2f)
        {
            // 提高质量
            _detector?.Dispose();
            _detector = new MODNetDetector(_resources, 640, 480);
        }
    }
}
```

---

## 常见问题

### Q: 为什么我设置 (640, 480) 和 (800, 600) 结果一样？

**A:** 因为它们的长宽比相同（都是 4:3），经过缩放和对齐后都变成 `512x384`。

### Q: 如何获得最高质量？

**A:** 设置更大的 `referenceSize`：

```csharp
// 在 MODNetResourceSet 中
public int referenceSize = 1024;  // 从 512 改为 1024

// 然后使用
new MODNetDetector(resources, 1024, 768);  // 实际: 1024x768
```

### Q: Width 和 Height 必须是 32 的倍数吗？

**A:** 不需要！MODNetConfig 会自动对齐。你可以传入任意值，如：
- `(640, 480)` → 对齐为 `(640, 480)` ✅
- `(641, 481)` → 对齐为 `(640, 480)` ✅
- `(1920, 1080)` → 缩放后对齐为 `(512, 288)` ✅

### Q: 可以使用竖屏比例吗？

**A:** 可以！

```csharp
new MODNetDetector(resources, 480, 640);   // 竖屏 3:4
new MODNetDetector(resources, 720, 1280);  // 竖屏 9:16
```

---

## 总结

**最简单的方法：**
```csharp
// 直接传入你的输入图像尺寸，MODNet 会自动处理
new MODNetDetector(resources, inputTexture.width, inputTexture.height);
```

**推荐标准配置：**
```csharp
// 4:3 比例（WebCam 常用）
[SerializeField] Vector2Int detectDimension = new Vector2Int(512, 384);

// 16:9 比例（HD 视频）
[SerializeField] Vector2Int detectDimension = new Vector2Int(512, 288);
```

**性能优先：**
```csharp
new MODNetDetector(resources, 320, 240);
```

**质量优先：**
```csharp
new MODNetDetector(resources, 768, 576);
```
