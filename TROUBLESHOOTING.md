# MODNet 故障排查指南

## 问题：MatteTexture 总是黑色

### 已修复的问题

#### 1. ✅ 预处理系数错误（已修复）
**问题**：之前使用了错误的 ColorCoeffs
- 错误值：`new Vector4(-127.5f, -127.5f, -127.5f, 1.0f/127.5f)`
- 正确值：`new Vector4(-1, -1, -1, 2)`

**原因**：Unity 纹理已经在 [0,1] 范围，不需要除以 255

#### 2. ✅ 移除了不必要的垂直翻转
Matte.compute 现在直接读取，不进行 Y 轴翻转

### 诊断步骤

#### 步骤 1: 使用诊断脚本检查模型
1. 在场景中创建空 GameObject
2. 添加 `DiagnoseMODNet` 组件
3. 配置 `resources` 引用你的 MODNetResourceSet
4. 运行游戏，查看 Console 输出

**检查点：**
- 模型是否成功加载？
- 输入/输出的 shape 是什么？
- 输出值的范围是多少？（应该在 [0,1] 之间）

#### 步骤 2: 使用调试版本 Detector
替换 `MODNetCompositor.cs` 中的 detector：

```csharp
// 将这行
_detector = new MODNetDetector(_resources, detectDimension.x, detectDimension.y);

// 替换为
_detector = new MODNetDetectorDebug(_resources, detectDimension.x, detectDimension.y);
```

运行后查看 Console 的详细日志

#### 步骤 3: 检查模型输出名称

MODNet 模型的输出可能不是默认名称。检查 Console 中的输出列表：

```
[MODNet] Model outputs:
  - output_name: shape=1,1,H,W
```

如果输出名称不是默认的，需要修改 `MODNetDetector.cs` 第 82 行：

```csharp
// 当前代码（使用默认输出）
post.SetBuffer(0, "MatteBuffer", _worker.PeekOutputBuffer());

// 如果需要指定输出名称
post.SetBuffer(0, "MatteBuffer", _worker.PeekOutputBuffer("output_name"));
```

### 常见问题

#### Q1: 输出全是 0
**可能原因：**
- 模型输入格式不正确（NCHW vs NHWC）
- 预处理系数错误
- 输入图像全黑或格式不支持

**解决方案：**
1. 确认 `ImagePreprocess` 使用 `nchwFix: true`（已设置）
2. 检查 InputTexture 是否有效
3. 使用 DiagnoseMODNet 测试模型本身

#### Q2: 输出值超出 [0,1] 范围
**可能原因：**
- 模型输出未经过 sigmoid 激活
- 需要额外的后处理

**解决方案：**
修改 `Matte.compute`，添加 sigmoid：

```hlsl
float sigmoid(float x) {
    return 1.0 / (1.0 + exp(-x));
}

// 在 PostprocessMatte 中
float matte = sigmoid(MatteBuffer[offset]);
```

#### Q3: 图像上下颠倒
**解决方案：**
修改 `Matte.compute` 第 16 行，启用垂直翻转：

```hlsl
uint offset = (InputSize.y - 1 - id.y) * InputSize.x + id.x;
```

#### Q4: ResourceSet 配置问题

确保 MODNetResourceSet.asset 正确配置：
- ✅ Model: `Assets/modnet.onnx`
- ✅ Reference Size: `512`
- ✅ Preprocess: `Packages/Klak NNUtils/.../Preprocess.compute`
- ✅ Matte: `Assets/AddOns/MODNet/Shaders/Matte.compute`

### 高级调试

#### 使用 MatteDebug.compute
1. 在 MODNetResourceSet 中，临时将 `matte` 改为 `MatteDebug.compute`
2. 运行后观察输出：
   - R 通道：原始值
   - G 通道：增强值（2倍）
   - B 通道：垂直翻转版本
   - A 通道：原始值

#### 检查 Buffer 大小
在 `MODNetDetectorDebug` 的 Console 输出中：

```
[MODNet] Output buffer count: 262144
[MODNet] Expected buffer size: 262144
```

这两个值应该相等。如果不等，说明模型输出尺寸与预期不符。

#### 验证预处理
在 `MODNetDetectorDebug.cs` 的 `AllocateObjects` 中添加：

```csharp
// 测试预处理输出
var testTexture = new Texture2D(64, 64);
_preprocess.Dispatch(testTexture, _resources.preprocess);
Debug.Log($"Preprocess tensor shape: {_preprocess.Tensor.shape}");
```

应该输出类似：`shape=1,3,H,W` (NCHW 格式)

### 参考：BodyPix 对比

如果 MODNet 仍然不工作，可以对比 BodyPix 的实现：

| 组件 | BodyPix | MODNet |
|------|---------|--------|
| ColorCoeffs | `(-1,-1,-1,2)` | `(-1,-1,-1,2)` ✅ |
| NCHW | `false` | `true` ❓ |
| 输出处理 | sigmoid + argmax | 直接使用 ❓ |

### 下一步

如果上述所有方法都无效：
1. 导出 Console 完整日志
2. 截图 MODNetResourceSet Inspector
3. 使用 Netron 查看 modnet.onnx 的实际结构
4. 对比 Python 推理代码的预处理步骤
