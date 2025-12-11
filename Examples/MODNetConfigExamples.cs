using UnityEngine;
using MODNet;

/// <summary>
/// MODNet 配置示例
/// 展示不同场景下如何设置 width 和 height 参数
/// </summary>
public class MODNetConfigExamples : MonoBehaviour
{
    [Header("Resources")]
    public MODNetResourceSet resources;

    [Header("Input Source")]
    public Texture inputTexture;
    public WebCamTexture webCamTexture;

    MODNetDetector _detector;

    // ========================================
    // 示例 1: 自动匹配输入图像尺寸（最简单）
    // ========================================
    void Example1_AutoMatchInputSize()
    {
        // 推荐方式：直接传入输入纹理的尺寸
        // MODNet 会自动缩放到最优推理分辨率
        if (inputTexture != null)
        {
            _detector = new MODNetDetector(
                resources,
                inputTexture.width,   // 640
                inputTexture.height   // 480
            );
            // → 实际推理分辨率: 512x384 (自动缩放并对齐)
        }
    }

    // ========================================
    // 示例 2: 标准配置（平衡质量和性能）
    // ========================================
    void Example2_StandardConfig()
    {
        // 4:3 比例（适合大多数 WebCam）
        _detector = new MODNetDetector(resources, 512, 384);

        // 或 16:9 比例（适合 HD 视频）
        // _detector = new MODNetDetector(resources, 512, 288);

        // 或 1:1 比例（正方形）
        // _detector = new MODNetDetector(resources, 512, 512);
    }

    // ========================================
    // 示例 3: WebCam 实时抠图
    // ========================================
    void Example3_WebCamRealtime()
    {
        // 创建 WebCam（低分辨率以提高性能）
        webCamTexture = new WebCamTexture(640, 480);

        // 方案 A: 标准质量（推荐）
        _detector = new MODNetDetector(resources, 512, 384);

        // 方案 B: 高性能（FPS 优先）
        // _detector = new MODNetDetector(resources, 320, 240);

        // 方案 C: 高质量（GPU 充足时）
        // _detector = new MODNetDetector(resources, 640, 480);

        webCamTexture.Play();
    }

    // ========================================
    // 示例 4: 性能优先（移动端/低端设备）
    // ========================================
    void Example4_PerformanceFirst()
    {
        // 最小配置 - 速度最快
        _detector = new MODNetDetector(resources, 256, 256);
        // → 实际: 256x256, ~65K 像素

        // 或稍高质量
        // _detector = new MODNetDetector(resources, 320, 240);
        // → 实际: 320x256, ~82K 像素

        // 或平衡配置
        // _detector = new MODNetDetector(resources, 416, 416);
        // → 实际: 416x416, ~173K 像素
    }

    // ========================================
    // 示例 5: 质量优先（离线处理/高端设备）
    // ========================================
    void Example5_QualityFirst()
    {
        // 高质量
        _detector = new MODNetDetector(resources, 768, 576);
        // → 实际: 768x576, ~443K 像素

        // 或最高质量（需要强大 GPU）
        // _detector = new MODNetDetector(resources, 1024, 768);
        // → 实际: 1024x768, ~786K 像素
    }

    // ========================================
    // 示例 6: 使用 Inspector 可调节配置
    // ========================================
    [Header("Configurable Settings")]
    [Tooltip("推理分辨率（可在 Inspector 调整）")]
    public Vector2Int detectDimension = new Vector2Int(512, 384);

    void Example6_InspectorConfigurable()
    {
        // 优点：无需重新编译，可在编辑器实时调整
        _detector = new MODNetDetector(
            resources,
            detectDimension.x,
            detectDimension.y
        );

        Debug.Log($"MODNet initialized with {detectDimension.x}x{detectDimension.y}");
    }

    // ========================================
    // 示例 7: 竖屏模式
    // ========================================
    void Example7_PortraitMode()
    {
        // 3:4 比例（竖屏）
        _detector = new MODNetDetector(resources, 384, 512);

        // 或 9:16 比例（竖屏 HD）
        // _detector = new MODNetDetector(resources, 288, 512);
    }

    // ========================================
    // 示例 8: 根据设备性能动态选择
    // ========================================
    void Example8_AdaptiveQuality()
    {
        // 根据系统内存选择配置
        if (SystemInfo.systemMemorySize < 4096) // < 4GB RAM
        {
            // 低端设备
            _detector = new MODNetDetector(resources, 320, 240);
            Debug.Log("Low-end device: using 320x240");
        }
        else if (SystemInfo.systemMemorySize < 8192) // 4-8GB RAM
        {
            // 中端设备
            _detector = new MODNetDetector(resources, 512, 384);
            Debug.Log("Mid-range device: using 512x384");
        }
        else
        {
            // 高端设备
            _detector = new MODNetDetector(resources, 640, 480);
            Debug.Log("High-end device: using 640x480");
        }
    }

    // ========================================
    // 示例 9: 运行时切换分辨率
    // ========================================
    public void SwitchToLowQuality()
    {
        _detector?.Dispose();
        _detector = new MODNetDetector(resources, 320, 240);
        Debug.Log("Switched to low quality (320x240)");
    }

    public void SwitchToMediumQuality()
    {
        _detector?.Dispose();
        _detector = new MODNetDetector(resources, 512, 384);
        Debug.Log("Switched to medium quality (512x384)");
    }

    public void SwitchToHighQuality()
    {
        _detector?.Dispose();
        _detector = new MODNetDetector(resources, 768, 576);
        Debug.Log("Switched to high quality (768x576)");
    }

    // ========================================
    // 使用示例
    // ========================================
    void Start()
    {
        // 取消注释你想测试的示例
        // Example1_AutoMatchInputSize();
        // Example2_StandardConfig();
        // Example3_WebCamRealtime();
        // Example4_PerformanceFirst();
        // Example5_QualityFirst();
        Example6_InspectorConfigurable();  // 默认使用可配置方式
        // Example7_PortraitMode();
        // Example8_AdaptiveQuality();
    }

    void Update()
    {
        if (_detector != null && inputTexture != null)
        {
            _detector.ProcessImage(inputTexture);
            // 使用 _detector.MatteTexture
        }
    }

    void OnDestroy()
    {
        _detector?.Dispose();
    }

    // ========================================
    // 辅助方法：计算实际推理分辨率
    // ========================================
    public static Vector2Int CalculateActualResolution(int width, int height, int referenceSize = 512)
    {
        // 模拟 MODNetConfig 的计算过程
        float scale = Mathf.Min((float)referenceSize / width, (float)referenceSize / height);
        int scaledWidth = Mathf.RoundToInt(width * scale);
        int scaledHeight = Mathf.RoundToInt(height * scale);

        // 对齐到 32 的倍数
        int alignedWidth = (scaledWidth + 31) / 32 * 32;
        int alignedHeight = (scaledHeight + 31) / 32 * 32;

        return new Vector2Int(alignedWidth, alignedHeight);
    }

    // 测试方法
    [ContextMenu("Test Resolution Calculation")]
    void TestResolutionCalculation()
    {
        var testCases = new[] {
            new Vector2Int(640, 480),
            new Vector2Int(1280, 720),
            new Vector2Int(1920, 1080),
            new Vector2Int(512, 384),
            new Vector2Int(320, 240),
            new Vector2Int(800, 600)
        };

        Debug.Log("=== Resolution Calculation Test ===");
        foreach (var testCase in testCases)
        {
            var actual = CalculateActualResolution(testCase.x, testCase.y);
            Debug.Log($"{testCase.x}x{testCase.y} → {actual.x}x{actual.y}");
        }
    }
}
