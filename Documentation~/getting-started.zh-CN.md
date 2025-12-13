# MODNet 快速开始

[English](./getting-started.md) | [中文](#)

## 安装

### 方法 1: 通过 Git URL 安装（推荐）

1. 打开您的 Unity 项目（Unity 6000.0 或更高版本）
2. 打开 **Window → Package Manager**
3. 点击 **+ → Add package from git URL**
4. 输入包 URL：
   ```
   https://github.com/your-username/unity-modnet.git#v1.0.0
   ```
5. 点击 **Add**

### 方法 2: 从本地路径安装

1. 克隆或下载此仓库
2. 打开 **Window → Package Manager**
3. 点击 **+ → Add package from disk**
4. 导航到包文件夹并选择 `package.json`
5. 点击 **Open**

### 方法 3: 添加到 manifest.json

添加到项目的 `Packages/manifest.json`：

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

**注意：** 需要 `scopedRegistries` 条目才能安装 Klak.NNUtils 依赖。

---

## 依赖项

此包需要：
- **Unity 6000.0** 或更高版本
- **Unity Sentis** 2.3.0（将自动安装）
- **Klak.NNUtils** 2.2.1（需要 scoped registry）
- **Unity Burst** 1.8.17
- **Unity Collections** 2.4.0

除 Klak.NNUtils 外，所有依赖项都将通过 UPM 自动安装。

---

## 快速开始（5分钟）

### 步骤 1: 导入 WebCam Demo 示例

1. 打开 **Window → Package Manager**
2. 在列表中找到 **MODNet Background Removal**
3. 展开 **Samples**
4. 点击 "WebCam Demo" 旁边的 **Import**

这将把示例场景导入到项目的 `Assets/Samples/` 文件夹中。

### 步骤 2: 打开演示场景

1. 导航到 `Assets/Samples/MODNet Background Removal/1.0.0/WebCam Demo/Scenes/`
2. 打开 **Demo.unity**

### 步骤 3: 运行场景

1. 点击 **Play**
2. 摄像头应该会自动启动
3. 您应该看到实时背景移除效果

**就这样！** 您现在已经在项目中运行 MODNet 了。

---

## 手动设置（用于您自己的场景）

### 选项 A: 使用 Compositor 组件（最简单）

**步骤 1: 创建 UI Canvas**
```
1. 创建 Canvas: Hierarchy → UI → Canvas
2. 添加 RawImage: 右键 Canvas → UI → Raw Image
3. 将 RawImage 缩放到所需大小
```

**步骤 2: 添加 MODNetCompositor**
```csharp
1. 在 Hierarchy 中选择 RawImage
2. Add Component → MODNetCompositor
3. 在 Inspector 中配置：
   - Preview UI: 拖入 RawImage 本身
   - Resources: 从 Samples 拖入 MODNet.asset
   - Detect Dimension: (512, 384)
```

**步骤 3: 设置输入纹理**
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

### 选项 B: 直接使用 MODNetDetector（高级）

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
        // 创建检测器
        detector = new MODNetDetector(resources, 512, 384);

        // 启动摄像头
        webCam = new WebCamTexture(640, 480);
        webCam.Play();
    }

    void Update()
    {
        // 处理帧
        detector.ProcessImage(webCam);

        // 获取结果
        RenderTexture matte = detector.MatteTexture;

        // 在自定义 shader/material 中使用 matte 纹理
        // ...
    }

    void OnDestroy()
    {
        detector?.Dispose();
    }
}
```

---

## 创建 MODNetResourceSet

如果需要创建自己的资源配置：

1. **创建资源：**
   - 在 Project 窗口右键
   - Create → ScriptableObjects → MODNet Resource Set

2. **配置资源：**
   - **Model:** `Runtime/Resources/modnet.onnx`
   - **Reference Size:** `512`
   - **Preprocess:** `Packages/Klak NNUtils/Shaders/Preprocess.compute`
   - **Matte:** `Shaders/Matte.compute`

3. **保存**资源

---

## 配置基础

### 理解 Width 和 Height 参数

```csharp
new MODNetDetector(resources, width, height);
//                            ^^^^^  ^^^^^^
//                            推理分辨率
```

**要点：**
- 这些定义**推理分辨率**，而不是输入分辨率
- MODNet 自动缩放和对齐维度
- 长宽比比精确值更重要

**推荐的起始配置：**
```csharp
// 对于 4:3 长宽比（大多数摄像头）
new MODNetDetector(resources, 512, 384);

// 对于 16:9 长宽比（高清视频）
new MODNetDetector(resources, 512, 288);

// 或匹配输入纹理
new MODNetDetector(resources, inputTexture.width, inputTexture.height);
```

详细配置选项请参阅[配置指南](./configuration-guide.zh-CN.md)。

---

## 性能提示

### 标准配置（推荐）
```csharp
detectDimension = new Vector2Int(512, 384);  // 平衡质量和性能
```

### 提高性能（降低质量）
```csharp
detectDimension = new Vector2Int(320, 240);  // 快 2-3 倍
```

### 提高质量（降低性能）
```csharp
detectDimension = new Vector2Int(768, 576);  // 更好的质量，更慢
```

使用 **Unity Profiler** 监控 GPU 性能并相应调整。

---

## 下一步

- **为您的用例配置：** 阅读[配置指南](./configuration-guide.zh-CN.md)
- **优化性能：** 查看[快速参考](./quick-reference.zh-CN.md)
- **排查问题：** 查看[故障排查指南](./troubleshooting.zh-CN.md)
- **学习 API：** 浏览 [API 参考](./api-reference.zh-CN.md)
- **查看代码示例：** 打开 `Samples~/WebCamDemo/Scripts/MODNetConfigExamples.cs`

---

## 常见问题

**问：可以用于录制的视频文件吗？**
答：可以！只需将任何 `Texture` 传递给 `detector.ProcessImage()` 或设置 `compositor.InputTexture`。

**问：能在移动设备上工作吗？**
答：可以，但可能需要降低推理分辨率（例如 320×240）以获得可接受的性能。

**问：可以自定义背景吗？**
答：可以！`MatteTexture` 是 alpha matte。使用自定义 shader 将其与输入纹理组合。

**问：分割精度如何？**
答：MODNet 提供高质量的人像抠图，具有精细的边缘细节，优于更简单的分割模型。

---

## 支持

- **文档：** [Documentation~/index.zh-CN.md](./index.zh-CN.md)
- **问题：** 在 GitHub 上报告错误
- **示例：** 查看 `Samples~/WebCamDemo/Scripts/MODNetConfigExamples.cs`
