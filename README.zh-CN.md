# MODNet 人像抠图集成

**[English](./README.md)** | **中文**

基于 Unity Sentis 的 MODNet 实时人像抠图系统。

## 简介

本包为 Unity 提供了高质量的实时人像抠图功能，使用 MODNet 神经网络模型和 Unity Sentis 进行 GPU 加速推理。适用于虚拟背景、AR 应用、直播系统等场景。

## 主要特性

- ✨ **高质量抠图** - 精细的边缘细节，优于传统分割算法
- ⚡ **实时性能** - GPU Compute 加速，支持实时处理
- 🎮 **易于集成** - 简单的 API 和现成的 UI 组件
- 🎯 **灵活配置** - 可根据需求调整质量和性能平衡
- 📱 **跨平台支持** - 支持 Windows、macOS、Linux 和移动平台
- 🔧 **完全可定制** - 可自定义 shader、后处理等

## 安装

### 方法 1: Git URL 安装（推荐）

1. 打开 Unity Package Manager（Window → Package Manager）
2. 点击 **+ → Add package from git URL**
3. 输入：
   ```
   https://github.com/your-username/unity-modnet.git#v1.0.0
   ```

### 方法 2: 手动添加到 manifest.json

在项目的 `Packages/manifest.json` 中添加：

```json
{
  "dependencies": {
    "com.modnet.unity": "https://github.com/your-username/unity-modnet.git#v1.0.0"
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

**重要：** `scopedRegistries` 配置是必需的，用于安装 Klak.NNUtils 依赖。

## 快速开始

### 1. 导入示例场景

1. 打开 Package Manager
2. 找到 **MODNet Background Removal** 包
3. 展开 **Samples**
4. 导入 **WebCam Demo**

### 2. 运行演示

1. 打开场景：`Assets/Samples/MODNet Background Removal/1.0.0/WebCam Demo/Scenes/Demo.unity`
2. 点击播放按钮
3. 摄像头会自动打开并显示实时抠图效果

### 3. 基本代码示例

```csharp
using MODNet;
using UnityEngine;

public class SimpleExample : MonoBehaviour
{
    public MODNetResourceSet resources;
    MODNetDetector detector;
    WebCamTexture webCam;

    void Start()
    {
        // 创建检测器（推理分辨率 512x384）
        detector = new MODNetDetector(resources, 512, 384);

        // 启动摄像头
        webCam = new WebCamTexture(640, 480);
        webCam.Play();
    }

    void Update()
    {
        // 处理当前帧
        detector.ProcessImage(webCam);

        // 获取结果（alpha matte）
        RenderTexture matte = detector.MatteTexture;

        // 在您的 shader/material 中使用 matte
        // ...
    }

    void OnDestroy()
    {
        // 必须释放资源
        detector?.Dispose();
    }
}
```

## 文件结构

```
com.modnet.unity/
├── Runtime/
│   ├── MODNetDetector.cs          # 核心检测器类
│   ├── MODNetDetectorDebug.cs     # 调试版本
│   ├── MODNetConfig.cs            # 配置和参数计算
│   ├── MODNetResourceSet.cs       # ScriptableObject 资源配置
│   ├── MODNetCompositor.cs        # UI 组件
│   ├── MODNetWebCamTexture.cs     # 摄像头辅助组件
│   └── Resources/
│       └── modnet.onnx            # MODNet 模型（25.9 MB）
├── Shaders/
│   ├── Compositor.shadergraph     # 合成 shader
│   ├── MODNetCompositor.mat       # 合成材质
│   ├── Matte.compute              # Alpha matte 后处理
│   ├── MatteDebug.compute         # 调试 shader
│   └── Preprocess.compute         # 预处理 shader
├── Samples~/
│   └── WebCamDemo/
│       ├── Scenes/Demo.unity
│       ├── Prefabs/ModNetWebCam.prefab
│       ├── Scripts/MODNetConfigExamples.cs
│       └── Resources/MODNet.asset
├── Documentation~/                # 完整文档（中英双语）
├── package.json                   # UPM 包配置
├── LICENSE.md                     # MIT 许可证
└── README.md                      # 本文档
```

## 配置说明

### Width 和 Height 参数

```csharp
new MODNetDetector(resources, width, height);
//                            ^^^^^  ^^^^^^
//                            推理分辨率
```

这两个参数定义**推理分辨率**，MODNet 会自动进行缩放和对齐。

### 推荐配置

```csharp
// 标准配置（平衡质量和性能）- 4:3 比例
new MODNetDetector(resources, 512, 384);

// 标准配置 - 16:9 比例
new MODNetDetector(resources, 512, 288);

// 性能优先（移动端）
new MODNetDetector(resources, 320, 240);

// 质量优先（离线处理）
new MODNetDetector(resources, 768, 576);

// 或者直接匹配输入纹理
new MODNetDetector(resources, inputTexture.width, inputTexture.height);
```

### 配置对照表

| 配置 | 实际分辨率 | 性能 | 质量 | 适用场景 |
|------|-----------|------|------|----------|
| `(256, 256)` | 256×256 | ⚡⚡⚡⚡⚡ | ⭐⭐ | 移动端 |
| `(320, 240)` | 320×256 | ⚡⚡⚡⚡ | ⭐⭐⭐ | 低端设备 |
| `(512, 384)` | **512×384** | ⚡⚡⚡ | ⭐⭐⭐⭐ | **标准（推荐）** |
| `(640, 480)` | 640×480 | ⚡⚡ | ⭐⭐⭐⭐ | 高质量实时 |
| `(768, 576)` | 768×576 | ⚡ | ⭐⭐⭐⭐⭐ | 离线处理 |

## 使用方式

### 方式 1: 使用 MODNetCompositor 组件（最简单）

适合快速集成到 UI：

1. 创建 Canvas 和 RawImage
2. 为 RawImage 添加 `MODNetCompositor` 组件
3. 在 Inspector 中配置：
   - Preview UI: 拖入 RawImage
   - Resources: 拖入 MODNet.asset
   - Detect Dimension: (512, 384)
4. 在代码中设置输入纹理：

```csharp
compositor.InputTexture = webCamTexture;
```

### 方式 2: 直接使用 MODNetDetector（最灵活）

适合自定义处理流程：

```csharp
using MODNet;
using UnityEngine;

public class CustomController : MonoBehaviour
{
    public MODNetResourceSet resources;
    MODNetDetector detector;

    void Start()
    {
        detector = new MODNetDetector(resources, 512, 384);
    }

    void Update()
    {
        detector.ProcessImage(inputTexture);

        // 在自定义 shader 中使用 MatteTexture
        myMaterial.SetTexture("_Matte", detector.MatteTexture);
    }

    void OnDestroy()
    {
        detector?.Dispose();
    }
}
```

## 技术细节

### 预处理

- **输入格式**: NCHW (1, 3, H, W)
- **归一化**: `rgb * 2 - 1` → [-1, 1]
- **ColorCoeffs**: `(-1, -1, -1, 2)`
- **维度对齐**: 自动对齐到 32 的倍数
- **长宽比保持**: 根据 referenceSize 自动缩放

### 推理

- **后端**: GPU Compute (BackendType.GPUCompute)
- **模型**: modnet.onnx (25.9 MB)
- **输出**: 单通道 alpha matte，范围 [0, 1]

### 后处理

- **垂直翻转**: 需要（Unity 纹理坐标系）
- **输出范围**: [0, 1]（使用 saturate 限制）

## 性能优化

### 调优流程

1. 从标准配置开始: `(512, 384)`
2. 运行游戏，使用 Unity Profiler 查看 GPU 时间
3. 如果 FPS < 30: 降低分辨率 → `(416, 416)` 或 `(320, 240)`
4. 如果 FPS > 60 且质量不满意: 提高分辨率 → `(640, 480)`
5. 微调到满意的平衡点

### GPU 消耗估算

- 推理时间与像素数成正比
- `512x384` ≈ 197K 像素（基准 1.0×）
- `1024x768` ≈ 786K 像素（~4倍计算量）
- `320x256` ≈ 82K 像素（~2.4倍速度）

## 系统要求

- **Unity**: 6000.0 或更高版本
- **Unity Sentis**: 2.3.0（自动安装）
- **Klak.NNUtils**: 2.2.1
- **Unity Burst**: 1.8.17
- **Unity Collections**: 2.4.0

## 文档

完整的中英双语文档位于 `Documentation~/` 文件夹：

- **[文档导航](./Documentation~/index.zh-CN.md)** - 文档中心
- **[快速开始](./Documentation~/getting-started.zh-CN.md)** - 安装和设置指南
- **[配置指南](./Documentation~/configuration-guide.zh-CN.md)** - 详细配置策略和性能优化
- **[快速参考](./Documentation~/quick-reference.zh-CN.md)** - Width/Height 设置速查表
- **[故障排查](./Documentation~/troubleshooting.zh-CN.md)** - 常见问题和解决方案
- **[API 参考](./Documentation~/api-reference.zh-CN.md)** - 完整 API 文档

## 示例代码

查看 `Samples~/WebCamDemo/Scripts/MODNetConfigExamples.cs` 获取 9 个实用配置示例。

## 常见问题

### Q: 为什么 MatteTexture 是黑色的？

A: 请检查：
1. MODNetResourceSet 配置是否正确
2. 模型文件 (modnet.onnx) 是否存在
3. InputTexture 是否有效
4. 使用 MODNetDetectorDebug 查看详细日志

详见[故障排查指南](./Documentation~/troubleshooting.zh-CN.md)。

### Q: 如何提高性能？

A: 降低推理分辨率：
```csharp
new MODNetDetector(resources, 320, 240);  // 更快
```

### Q: 如何提高质量？

A: 提高推理分辨率：
```csharp
new MODNetDetector(resources, 768, 576);  // 更高质量
```

### Q: 可以用于视频文件吗？

A: 可以！只需将视频帧作为 Texture 传递给 `ProcessImage()`。

### Q: 支持移动平台吗？

A: 支持，但建议使用较低的推理分辨率（如 320×240）以获得更好的性能。

## 许可证

本项目采用 MIT License - 详见 [LICENSE.md](./LICENSE.md)。

### 第三方许可

本包包含 MODNet 神经网络模型，受原始 MODNet 许可证约束。请参阅：
https://github.com/ZHKKKe/MODNet

本包的实现模式参考了 Keijiro Takahashi 的 BodyPix 实现。

## 致谢

- **MODNet 模型**: [ZHKKKe/MODNet](https://github.com/ZHKKKe/MODNet)
- **Unity Sentis**: Unity Technologies
- **Klak.NNUtils**: Keijiro Takahashi
- **架构参考**: BodyPix for Unity by Keijiro Takahashi

## 支持

- **问题反馈**: 在 GitHub 上提交 Issue
- **文档**: 查看 `Documentation~/` 文件夹
- **示例**: 导入 WebCam Demo 示例

## 更新日志

详见 [CHANGELOG.md](./CHANGELOG.md)。

---

**享受使用 MODNet for Unity！** 🎉
