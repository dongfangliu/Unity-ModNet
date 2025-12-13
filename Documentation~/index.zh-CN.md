# MODNet 文档导航

## 📚 文档列表

### 快速开始
- **[README.md](./README.md)** - 主文档，包含完整使用指南
- **[QUICK_REFERENCE.md](./QUICK_REFERENCE.md)** ⭐ - Width/Height 设置速查表

### 详细配置
- **[CONFIGURATION_GUIDE.md](./CONFIGURATION_GUIDE.md)** - 详细的配置策略和性能优化指南

### 故障排查
- **[TROUBLESHOOTING.md](./TROUBLESHOOTING.md)** - 常见问题和解决方案
- **[QUICK_DEBUG.txt](./QUICK_DEBUG.txt)** - 调试检查清单

### 代码示例
- **[Examples/MODNetConfigExamples.cs](./Examples/MODNetConfigExamples.cs)** - 9 个实用配置示例

---

## 🎯 我应该看哪个文档？

### 我想快速上手
→ 阅读 [README.md](./README.md) 的"使用步骤"部分

### 我不知道如何设置 width 和 height
→ 查看 **[QUICK_REFERENCE.md](./QUICK_REFERENCE.md)** ⚡

### 我想了解所有配置选项
→ 阅读 [CONFIGURATION_GUIDE.md](./CONFIGURATION_GUIDE.md)

### MatteTexture 显示有问题
→ 查看 [TROUBLESHOOTING.md](./TROUBLESHOOTING.md)

### 我想看代码示例
→ 打开 [Examples/MODNetConfigExamples.cs](./Examples/MODNetConfigExamples.cs)

---

## 📋 常见任务速查

### ✅ 创建 ResourceSet
```
1. 右键 Project → Create → ScriptableObjects → MODNet Resource Set
2. 配置字段：
   - Model: Assets/modnet.onnx
   - Reference Size: 512
   - Preprocess: Packages/Klak NNUtils/Shaders/Preprocess.compute
   - Matte: Assets/AddOns/MODNet/Shaders/Matte.compute
```

### ✅ 基础使用
```csharp
// 方法 1: 使用 MODNetCompositor 组件
// Inspector → 配置 detectDimension = (512, 384)

// 方法 2: 直接使用 MODNetDetector
MODNetDetector detector = new MODNetDetector(resources, 512, 384);
detector.ProcessImage(inputTexture);
RenderTexture matte = detector.MatteTexture;
```

### ✅ 推荐配置
```csharp
// 标准配置（平衡）
new MODNetDetector(resources, 512, 384);

// 性能优先
new MODNetDetector(resources, 320, 240);

// 质量优先
new MODNetDetector(resources, 768, 576);
```

---

## 🔧 关键技术参数

| 参数 | 值 | 说明 |
|------|---|------|
| **ColorCoeffs** | `(-1, -1, -1, 2)` | 预处理归一化系数 |
| **NCHW 格式** | `true` | Tensor 格式 |
| **对齐要求** | 32 的倍数 | 自动对齐 |
| **垂直翻转** | 需要 | Unity 坐标系 |
| **Reference Size** | 512 | 默认缩放基准 |

---

## 📊 性能参考

| 分辨率 | 像素数 | 相对速度 | 推荐场景 |
|--------|--------|----------|----------|
| 256×256 | 65K | 3.0× | 移动端 |
| 320×256 | 82K | 2.4× | 低端设备 |
| **512×384** | **197K** | **1.0×** | **标准** |
| 640×480 | 307K | 0.64× | 高质量实时 |
| 768×576 | 443K | 0.45× | 离线处理 |

---

## 📁 文件结构

```
MODNet/
├── Runtime/
│   ├── MODNetDetector.cs          # 核心检测器
│   ├── MODNetDetectorDebug.cs     # 调试版本
│   ├── MODNetConfig.cs            # 配置计算
│   └── MODNetResourceSet.cs       # ScriptableObject
├── Shaders/
│   ├── Matte.compute              # 后处理 shader
│   └── MatteDebug.compute         # 调试 shader
├── Examples/
│   └── MODNetConfigExamples.cs    # 示例代码
├── MODNetCompositor.cs            # UI 组件
├── MODNetWebCamTexture.cs         # WebCam 辅助
├── Compositor.shadergraph         # 显示 shader
├── MODNet.asset                   # ResourceSet 实例
├── modnet.onnx                    # 模型文件（25.9MB）
└── 文档/
    ├── README.md
    ├── QUICK_REFERENCE.md
    ├── CONFIGURATION_GUIDE.md
    ├── TROUBLESHOOTING.md
    ├── QUICK_DEBUG.txt
    └── INDEX.md (本文件)
```

---

## 💡 提示

1. **首次使用？** 从 [README.md](./README.md) 开始
2. **配置疑问？** 查看 [QUICK_REFERENCE.md](./QUICK_REFERENCE.md)
3. **遇到问题？** 参考 [TROUBLESHOOTING.md](./TROUBLESHOOTING.md)
4. **需要示例？** 打开 [Examples/MODNetConfigExamples.cs](./Examples/MODNetConfigExamples.cs)

**所有文档都在 `Assets/AddOns/MODNet/` 目录下！**
