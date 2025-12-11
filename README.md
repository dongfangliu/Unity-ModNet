# MODNet 人像抠图集成

基于 Unity Sentis 的 MODNet 实时人像抠图系统，仿照 BodyPix 架构实现。

## 文件结构

```
MODNet/
├── Runtime/
│   ├── MODNetDetector.cs      # 核心检测器类
│   ├── MODNetConfig.cs         # 配置和参数计算
│   └── MODNetResourceSet.cs    # ScriptableObject 资源配置
├── Shaders/
│   └── Matte.compute           # Alpha matte 后处理
├── MODNetCompositor.cs         # MonoBehaviour UI 组件
└── README.md                   # 本文档
```

## 快速配置

### Width 和 Height 设置

MODNetDetector 的 width 和 height 参数定义**推理分辨率**：

```csharp
// 最简单：直接传入输入图像尺寸（自动缩放到最优分辨率）
new MODNetDetector(resources, inputTexture.width, inputTexture.height);

// 标准配置（推荐）- 4:3 比例
new MODNetDetector(resources, 512, 384);

// 标准配置（推荐）- 16:9 比例
new MODNetDetector(resources, 512, 288);

// 性能优先（移动端）
new MODNetDetector(resources, 320, 240);

// 质量优先（离线处理）
new MODNetDetector(resources, 768, 576);
```

**重要：**只需保持长宽比正确，MODNet 会自动：
1. 缩放到合适尺寸（基于 referenceSize=512）
2. 对齐到 32 的倍数

详细配置说明请查看 [CONFIGURATION_GUIDE.md](./CONFIGURATION_GUIDE.md)

---

## 使用步骤

### 1. 创建 MODNetResourceSet

在 Unity 编辑器中：
1. 右键 Project 窗口 → Create → ScriptableObjects → MODNet Resource Set
2. 配置资源：
   - **Model**: 拖入 `Assets/modnet.onnx`
   - **Reference Size**: 保持默认 512
   - **Preprocess**: 从 Packages 中找到 `Klak.NNUtils/Shaders/Preprocess.compute`
   - **Matte**: 选择 `Assets/AddOns/MODNet/Shaders/Matte.compute`
3. 保存为 `MODNetResourceSet.asset`

### 2. 在场景中使用

#### 方法 A: 使用 MODNetCompositor 组件
1. 在场景中创建 Canvas（如果没有）
2. 添加 RawImage 组件
3. 为 RawImage 添加 MODNetCompositor 脚本
4. 配置 MODNetCompositor：
   - **Preview UI**: 拖入 RawImage 自身
   - **Resources**: 拖入上面创建的 MODNetResourceSet
   - **Detect Dimension**: 设置推理分辨率（默认 512x384）
5. 在代码中设置 `InputTexture`（如 WebCamTexture）

示例代码：
```csharp
public MODNetCompositor modnetCompositor;
WebCamTexture webCam;

void Start() {
    webCam = new WebCamTexture(640, 480);
    modnetCompositor.InputTexture = webCam;
    webCam.Play();
}
```

#### 方法 B: 直接使用 MODNetDetector
```csharp
using MODNet;

MODNetResourceSet resources;
MODNetDetector detector;

void Start() {
    detector = new MODNetDetector(resources, 512, 384);
}

void Update() {
    detector.ProcessImage(inputTexture);
    RenderTexture matte = detector.MatteTexture;
    // 使用 matte...
}

void OnDestroy() {
    detector?.Dispose();
}
```

### 3. 配置显示材质

MODNetCompositor 需要一个材质来混合输入图像和 matte。
可以参考 `Assets/AddOns/Compositor/` 中的材质设置，或创建自定义 Shader。

材质需要接收两个纹理：
- `_Input`: 原始输入图像
- `_Mask`: MODNet 输出的 matte

## 技术细节

### 预处理
- 输入格式: NCHW (1, 3, H, W)
- 归一化: `(pixel - 127.5) / 127.5` → [-1, 1]
  - Unity 纹理已在 [0,1]，所以实际公式: `rgb * 2 - 1`
  - ColorCoeffs: `(-1, -1, -1, 2)` ✅
- 维度对齐: 自动对齐到 32 的倍数
- 长宽比保持: 根据 referenceSize 自动缩放

### 后处理
- 垂直翻转: ✅ 需要（Unity 纹理坐标系）
- 输出范围: [0, 1]（已用 saturate 限制）

### 推理
- 后端: GPU Compute (BackendType.GPUCompute)
- 模型: modnet.onnx (25.9 MB)
- 输出: 单通道 alpha matte，范围 [0, 1]

### 性能
- 推荐分辨率: 512x384
- 可根据需求调整 detectDimension
- 更高分辨率 = 更好质量，但性能开销更大

## 与 BodyPix 对比

| 特性 | BodyPix | MODNet |
|------|---------|---------|
| 用途 | 人体分割+关键点 | 人像抠图 |
| 输出质量 | 粗糙分割 | 高质量 matte |
| 边缘细节 | 一般 | 优秀 |
| 性能 | 较快 | 中等 |
| 模型大小 | 较小 | 25.9 MB |

## 调试

如果遇到问题：
1. 检查 Console 是否有错误信息
2. 确认 modnet.onnx 已正确引用
3. 验证 Preprocess.compute 路径正确
4. 检查 InputTexture 是否为 null
5. 使用 Unity Profiler 查看性能

## 依赖

- Unity 6000.0+
- Unity Sentis (com.unity.ai.inference v2.3.0)
- Klak.NNUtils (jp.keijiro.klak.nnutils)

## 许可

本代码基于 BodyPix 实现模式，遵循相同的代码结构。
MODNet 模型遵循其原始仓库的许可证。
