# MODNet 快速参考

## Width & Height 设置速查表

### 🎯 推荐配置

```csharp
// 标准配置（4:3 比例）
new MODNetDetector(resources, 512, 384);  // ← 大多数情况使用这个

// 标准配置（16:9 比例）
new MODNetDetector(resources, 512, 288);

// 或者直接匹配输入图像
new MODNetDetector(resources, inputTexture.width, inputTexture.height);
```

### 📊 配置对照表

| 设置 | 实际分辨率 | 性能 | 质量 | 适用场景 |
|------|-----------|------|------|----------|
| `(256, 256)` | 256×256 | ⚡⚡⚡⚡⚡ | ⭐⭐ | 移动端 |
| `(320, 240)` | 320×256 | ⚡⚡⚡⚡ | ⭐⭐⭐ | 低端设备 |
| `(512, 384)` | **512×384** | ⚡⚡⚡ | ⭐⭐⭐⭐ | **标准（推荐）** |
| `(640, 480)` | 640×480 | ⚡⚡ | ⭐⭐⭐⭐ | 高质量实时 |
| `(768, 576)` | 768×576 | ⚡ | ⭐⭐⭐⭐⭐ | 离线处理 |

### 🔑 关键要点

1. **只需保持长宽比正确** - MODNet 会自动缩放
2. **不需要是 32 的倍数** - 会自动对齐
3. **长宽比相同 = 推理分辨率相同**
   - `(640, 480)` 和 `(800, 600)` → 都是 `512×384`
   - `(1280, 720)` 和 `(1920, 1080)` → 都是 `512×288`

### 📝 使用示例

#### 在 Inspector 中配置
```csharp
public class MyScript : MonoBehaviour {
    [SerializeField] Vector2Int detectDimension = new Vector2Int(512, 384);

    void Start() {
        _detector = new MODNetDetector(resources, detectDimension.x, detectDimension.y);
    }
}
```

#### WebCam 实时抠图
```csharp
WebCamTexture webCam = new WebCamTexture(640, 480);
MODNetDetector detector = new MODNetDetector(resources, 512, 384);
```

#### 自适应质量
```csharp
// 根据 FPS 调整
if (fps < 30) {
    detector = new MODNetDetector(resources, 320, 240);  // 降低质量
} else if (fps > 60) {
    detector = new MODNetDetector(resources, 640, 480);  // 提高质量
}
```

### 🛠️ 调优技巧

1. **从标准配置开始**: `(512, 384)`
2. **FPS 不足**: 降低分辨率 → `(320, 240)`
3. **FPS 充足**: 提高分辨率 → `(640, 480)`
4. **使用 Unity Profiler** 监控 GPU 时间

### 📐 计算公式

```
实际宽度 = round((输入宽度 * min(512/输入宽度, 512/输入高度)) / 32) * 32
实际高度 = round((输入高度 * min(512/输入宽度, 512/输入高度)) / 32) * 32
```

**示例：**
- 输入 `(640, 480)` → 缩放 `(512, 384)` → 对齐 **`512×384`** ✅
- 输入 `(1920, 1080)` → 缩放 `(512, 288)` → 对齐 **`512×288`** ✅

---

**完整文档：**
- [README.md](./README.md) - 使用指南
- [CONFIGURATION_GUIDE.md](./CONFIGURATION_GUIDE.md) - 详细配置说明
- [TROUBLESHOOTING.md](./TROUBLESHOOTING.md) - 故障排查
- [Examples/MODNetConfigExamples.cs](./Examples/MODNetConfigExamples.cs) - 代码示例
