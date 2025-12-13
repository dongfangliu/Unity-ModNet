# MODNet Background Removal for Unity

[English](#english) | [中文](#中文)

---

## English

Real-time portrait matting using MODNet neural network with Unity Sentis. High-quality background removal for Unity projects.

### Features

- ✨ **High-Quality Matting** - MODNet provides superior portrait segmentation with fine edge details
- ⚡ **Real-time Performance** - GPU-accelerated inference using Unity Sentis
- 🎮 **Easy Integration** - Simple API and ready-to-use components
- 🎯 **Flexible Configuration** - Adjustable quality/performance trade-offs
- 📱 **Cross-Platform** - Works on desktop and mobile platforms
- 🔧 **Customizable** - Full control over matting pipeline

### Quick Start

#### Installation

Add via Git URL in Unity Package Manager:

```
https://github.com/your-username/unity-modnet.git#v1.0.0
```

Or add to `Packages/manifest.json`:

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

**Note:** The scoped registry is required for the Klak.NNUtils dependency.

#### Basic Usage

```csharp
using MODNet;
using UnityEngine;

public class QuickStart : MonoBehaviour
{
    public MODNetResourceSet resources;
    MODNetDetector detector;
    WebCamTexture webCam;

    void Start()
    {
        // Create detector (512x384 inference resolution)
        detector = new MODNetDetector(resources, 512, 384);

        // Start webcam
        webCam = new WebCamTexture(640, 480);
        webCam.Play();
    }

    void Update()
    {
        // Process frame
        detector.ProcessImage(webCam);

        // Get alpha matte
        RenderTexture matte = detector.MatteTexture;
        // Use matte in your shader...
    }

    void OnDestroy()
    {
        detector?.Dispose();
    }
}
```

### Documentation

- 📚 **[Getting Started](./Documentation~/getting-started.md)** - Installation and setup
- ⚙️ **[Configuration Guide](./Documentation~/configuration-guide.md)** - Detailed configuration options
- ⚡ **[Quick Reference](./Documentation~/quick-reference.md)** - Configuration cheat sheet
- 🔧 **[Troubleshooting](./Documentation~/troubleshooting.md)** - Common issues and solutions
- 📖 **[API Reference](./Documentation~/api-reference.md)** - Complete API documentation
- 📂 **[Full Documentation](./Documentation~/index.md)** - Documentation hub

### Requirements

- **Unity 6000.0** or later
- **Unity Sentis** 2.3.0 (installed automatically)
- **Klak.NNUtils** 2.2.1 (requires scoped registry)
- **Unity Burst** 1.8.17
- **Unity Collections** 2.4.0

### Sample

Import the **WebCam Demo** sample from Package Manager to see a working example of real-time background removal.

### Performance Tips

| Configuration | Resolution | Use Case |
|--------------|-----------|----------|
| `(320, 240)` | 320×256 | Mobile, low-end devices |
| `(512, 384)` | **512×384** | **Standard (Recommended)** |
| `(640, 480)` | 640×480 | High-quality real-time |
| `(768, 576)` | 768×576 | Offline processing |

Start with `(512, 384)` and adjust based on your performance requirements.

### License

MIT License - See [LICENSE.md](./LICENSE.md) for details.

This package includes the MODNet neural network model. See the [original MODNet repository](https://github.com/ZHKKKe/MODNet) for model license information.

---

## 中文

基于 Unity Sentis 的 MODNet 实时人像抠图系统。为 Unity 项目提供高质量背景移除功能。

### 特性

- ✨ **高质量抠图** - MODNet 提供卓越的人像分割效果，边缘细节精细
- ⚡ **实时性能** - 使用 Unity Sentis 的 GPU 加速推理
- 🎮 **易于集成** - 简单的 API 和开箱即用的组件
- 🎯 **灵活配置** - 可调节质量/性能权衡
- 📱 **跨平台** - 支持桌面和移动平台
- 🔧 **可定制** - 完全控制抠图流程

### 快速开始

#### 安装

在 Unity Package Manager 中通过 Git URL 添加：

```
https://github.com/your-username/unity-modnet.git#v1.0.0
```

或添加到 `Packages/manifest.json`：

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

**注意：** 需要 scoped registry 来安装 Klak.NNUtils 依赖。

#### 基本用法

```csharp
using MODNet;
using UnityEngine;

public class QuickStart : MonoBehaviour
{
    public MODNetResourceSet resources;
    MODNetDetector detector;
    WebCamTexture webCam;

    void Start()
    {
        // 创建检测器（512x384 推理分辨率）
        detector = new MODNetDetector(resources, 512, 384);

        // 启动摄像头
        webCam = new WebCamTexture(640, 480);
        webCam.Play();
    }

    void Update()
    {
        // 处理帧
        detector.ProcessImage(webCam);

        // 获取 alpha matte
        RenderTexture matte = detector.MatteTexture;
        // 在 shader 中使用 matte...
    }

    void OnDestroy()
    {
        detector?.Dispose();
    }
}
```

### 文档

- 📚 **[快速开始](./Documentation~/getting-started.zh-CN.md)** - 安装和设置
- ⚙️ **[配置指南](./Documentation~/configuration-guide.zh-CN.md)** - 详细配置选项
- ⚡ **[快速参考](./Documentation~/quick-reference.zh-CN.md)** - 配置速查表
- 🔧 **[故障排查](./Documentation~/troubleshooting.zh-CN.md)** - 常见问题和解决方案
- 📖 **[API 参考](./Documentation~/api-reference.zh-CN.md)** - 完整 API 文档
- 📂 **[完整文档](./Documentation~/index.zh-CN.md)** - 文档中心

也可查看 **[完整中文文档](./README.zh-CN.md)**。

### 系统要求

- **Unity 6000.0** 或更高版本
- **Unity Sentis** 2.3.0（自动安装）
- **Klak.NNUtils** 2.2.1（需要 scoped registry）
- **Unity Burst** 1.8.17
- **Unity Collections** 2.4.0

### 示例

从 Package Manager 导入 **WebCam Demo** 示例，查看实时背景移除的工作示例。

### 性能建议

| 配置 | 分辨率 | 适用场景 |
|------|-------|----------|
| `(320, 240)` | 320×256 | 移动端、低端设备 |
| `(512, 384)` | **512×384** | **标准配置（推荐）** |
| `(640, 480)` | 640×480 | 高质量实时 |
| `(768, 576)` | 768×576 | 离线处理 |

从 `(512, 384)` 开始，根据性能要求调整。

### 许可证

MIT License - 详见 [LICENSE.md](./LICENSE.md)。

此包包含 MODNet 神经网络模型。模型许可信息请参见 [MODNet 原始仓库](https://github.com/ZHKKKe/MODNet)。
