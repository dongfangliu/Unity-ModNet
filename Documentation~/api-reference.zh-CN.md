# MODNet API 参考

[English](./api-reference.md) | [中文](#)

## 命名空间

所有 MODNet 类都在 `MODNet` 命名空间中：

```csharp
using MODNet;
```

---

## 核心类

### MODNetDetector

运行 MODNet 推理的主类。

#### 构造函数

```csharp
public MODNetDetector(MODNetResourceSet resources, int width, int height)
```

**参数：**
- `resources`: 包含模型和shader引用的MODNetResourceSet
- `width`: 期望的推理宽度（会自动缩放和对齐）
- `height`: 期望的推理高度（会自动缩放和对齐）

**示例：**
```csharp
var detector = new MODNetDetector(resources, 512, 384);
```

#### 方法

##### ProcessImage

```csharp
public void ProcessImage(Texture input)
```

处理输入纹理并生成 alpha matte。

**参数：**
- `input`: 输入纹理（WebCamTexture、Texture2D、RenderTexture等）

**示例：**
```csharp
detector.ProcessImage(webCamTexture);
```

##### Dispose

```csharp
public void Dispose()
```

释放所有资源。使用完检测器后**必须**调用此方法。

**示例：**
```csharp
void OnDestroy() {
    detector?.Dispose();
}
```

#### 属性

##### MatteTexture

```csharp
public RenderTexture MatteTexture { get; }
```

返回生成的 alpha matte 作为 RenderTexture。

**范围：** 值在 [0, 1] 之间，其中0表示透明，1表示不透明。

**示例：**
```csharp
RenderTexture matte = detector.MatteTexture;
myMaterial.SetTexture("_Matte", matte);
```

---

### MODNetDetectorDebug

带详细日志的 MODNetDetector 调试版本。

#### 构造函数

```csharp
public MODNetDetectorDebug(MODNetResourceSet resources, int width, int height)
```

与 MODNetDetector 相同，但会在控制台输出详细调试信息。

**用途：**
- 调试模型加载问题
- 检查输入/输出 tensor 形状
- 验证 buffer 大小
- 排查预处理问题

**示例：**
```csharp
// 将常规检测器替换为调试版本
var detector = new MODNetDetectorDebug(resources, 512, 384);
```

---

### MODNetCompositor

用于与 Unity UI 轻松集成的 MonoBehaviour 组件。

#### Inspector 字段

```csharp
[SerializeField] RawImage _previewUI
```
用于显示结果的 RawImage 组件。

```csharp
[SerializeField] MODNetResourceSet _resources
```
对 MODNetResourceSet 资源的引用。

```csharp
[SerializeField] Vector2Int detectDimension = new Vector2Int(512, 384)
```
推理分辨率（宽度，高度）。

#### 属性

##### InputTexture

```csharp
public Texture InputTexture { get; set; }
```

要处理的输入纹理（通常是 WebCamTexture）。

**示例：**
```csharp
compositor.InputTexture = webCamTexture;
```

##### PreviewUI

```csharp
public RawImage PreviewUI { get; }
```

返回配置的预览 RawImage 组件。

#### 方法

##### SetThreshold

```csharp
public void SetThreshold(float value)
```

为合成器材质设置阈值。

**参数：**
- `value`: 阈值（通常为 0-1）

**示例：**
```csharp
compositor.SetThreshold(0.5f);
```

---

### MODNetWebCamTexture

用于管理摄像头输入的辅助组件。

#### Inspector 字段

```csharp
public MODNetCompositor compositor
```
对 MODNetCompositor 组件的引用。

```csharp
public int downSample = 2
```
摄像头分辨率的降采样系数。

#### 方法

##### Open

```csharp
public void Open()
```

打开摄像头并开始处理。

**示例：**
```csharp
webCamController.Open();
```

##### Close

```csharp
public void Close()
```

停止摄像头并禁用处理。

##### SetThreshold

```csharp
public void SetThreshold(float value)
```

将阈值传递给合成器。

**参数：**
- `value`: 阈值

##### EnablePreviewImage / DisablePreviewImage

```csharp
public void EnablePreviewImage()
public void DisablePreviewImage()
```

显示或隐藏预览 UI。

---

### MODNetResourceSet

用于配置 MODNet 资源的 ScriptableObject。

#### Inspector 字段

```csharp
public NNModel model
```
对 MODNet ONNX 模型文件的引用。

```csharp
public int referenceSize = 512
```
用于自动分辨率缩放的参考大小。

```csharp
public ComputeShader preprocess
```
预处理 compute shader（来自 Klak.NNUtils）。

```csharp
public ComputeShader matte
```
Matte 后处理 compute shader。

#### 创建 ResourceSet

1. 在 Project 窗口右键
2. Create → ScriptableObjects → MODNet Resource Set
3. 在 Inspector 中配置字段
4. 保存资源

**典型配置：**
- Model: `Runtime/Resources/modnet.onnx`
- Reference Size: `512`
- Preprocess: `Packages/Klak NNUtils/Shaders/Preprocess.compute`
- Matte: `Shaders/Matte.compute`

---

### MODNetConfig

内部配置类（自动生成，无需直接使用）。

处理：
- 基于 referenceSize 的分辨率缩放
- 对齐到32的倍数
- 长宽比保持

---

## 使用模式

### 模式 1: 直接使用检测器

最大控制：

```csharp
using MODNet;
using UnityEngine;

public class MyController : MonoBehaviour
{
    public MODNetResourceSet resources;
    MODNetDetector detector;
    WebCamTexture webCam;

    void Start()
    {
        detector = new MODNetDetector(resources, 512, 384);
        webCam = new WebCamTexture(640, 480);
        webCam.Play();
    }

    void Update()
    {
        detector.ProcessImage(webCam);
        var matte = detector.MatteTexture;
        // 在自定义 shader 中使用 matte
    }

    void OnDestroy()
    {
        detector?.Dispose();
    }
}
```

### 模式 2: 使用 Compositor 组件

快速 UI 集成：

```csharp
using MODNet;
using UnityEngine;

public class WebCamSetup : MonoBehaviour
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

### 模式 3: 使用辅助组件

最简单的设置：

```csharp
// 将 MODNetWebCamTexture 附加到 GameObject
// 在 Inspector 中配置 compositor 引用
// 调用 Open() 开始

public class UIController : MonoBehaviour
{
    public MODNetWebCamTexture webCamController;

    public void StartWebcam()
    {
        webCamController.Open();
    }

    public void StopWebcam()
    {
        webCamController.Close();
    }
}
```

---

## Shader 集成

### 在自定义 Shader 中使用 Matte

MatteTexture 可以在任何 Unity shader 中使用：

```hlsl
// 在自定义 shader 中
sampler2D _Matte;

fixed4 frag(v2f i) : SV_Target
{
    fixed4 color = tex2D(_MainTex, i.uv);
    float alpha = tex2D(_Matte, i.uv).r;  // Matte 在 R 通道
    color.a = alpha;
    return color;
}
```

### Compositor Shader Graph

包中包含预构建的 Compositor ShaderGraph (`Shaders/Compositor.shadergraph`)：
- 将输入纹理与 matte 组合
- 支持阈值调整
- 提供平滑 alpha 混合

---

## 最佳实践

### 资源管理

始终释放检测器：

```csharp
void OnDestroy()
{
    detector?.Dispose();
    detector = null;
}
```

### 性能优化

从标准分辨率开始并调整：

```csharp
// 从这里开始
new MODNetDetector(resources, 512, 384);

// 太慢？降低：
new MODNetDetector(resources, 320, 240);

// 太快且质量不足？提高：
new MODNetDetector(resources, 640, 480);
```

### 错误处理

检查空纹理：

```csharp
void Update()
{
    if (inputTexture != null)
    {
        detector.ProcessImage(inputTexture);
    }
    else
    {
        Debug.LogWarning("输入纹理为空");
    }
}
```

### Inspector 配置

对可调参数使用 SerializeField：

```csharp
[SerializeField] Vector2Int detectDimension = new Vector2Int(512, 384);
[SerializeField] MODNetResourceSet resources;
```

---

## 另请参阅

- [快速开始](./getting-started.zh-CN.md) - 设置和安装
- [配置指南](./configuration-guide.zh-CN.md) - 详细配置选项
- [快速参考](./quick-reference.zh-CN.md) - 快速配置速查表
- [故障排查](./troubleshooting.zh-CN.md) - 常见问题和解决方案
