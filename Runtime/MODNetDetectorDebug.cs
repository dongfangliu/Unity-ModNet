using Unity.InferenceEngine;
using UnityEngine;
using Klak.NNUtils;
using Klak.NNUtils.Extensions;

namespace MODNet
{
    public sealed class MODNetDetectorDebug : System.IDisposable
    {
        #region Public methods/properties

        public MODNetDetectorDebug(MODNetResourceSet resources, int width, int height)
          => AllocateObjects(resources, width, height);

        public void Dispose()
          => DeallocateObjects();

        public void ProcessImage(Texture sourceTexture)
          => RunModel(sourceTexture);

        public RenderTexture MatteTexture
          => _matte;

        #endregion

        #region Private objects

        MODNetResourceSet _resources;
        MODNetConfig _config;
        Worker _worker;
        ImagePreprocess _preprocess;
        RenderTexture _matte;

        void AllocateObjects(MODNetResourceSet resources, int width, int height)
        {
            _resources = resources;

            // NN model
            var model = ModelLoader.Load(_resources.model);
            _config = new MODNetConfig(_resources, width, height);

            Debug.Log($"[MODNet Debug] Model loaded successfully");
            Debug.Log($"[MODNet Debug] Input dimensions: {_config.InputWidth}x{_config.InputHeight}");
            Debug.Log($"[MODNet Debug] ColorCoeffs: {_config.PreprocessCoeffs}");

            // GPU worker
            _worker = new Worker(model, BackendType.GPUCompute);

            // Preprocessing buffers
            // MODNet requires NCHW format (N, C, H, W)
            _preprocess = new ImagePreprocess(_config.InputWidth, _config.InputHeight, nchwFix: true)
            {
                ColorCoeffs = _config.PreprocessCoeffs
            };

            Debug.Log($"[MODNet Debug] Preprocessing initialized (NCHW=true)");

            // Output buffer (alpha matte)
            _matte = RTUtil.NewArgbUav(_config.InputWidth, _config.InputHeight);

            Debug.Log($"[MODNet Debug] Initialization complete");
        }

        void DeallocateObjects()
        {
            _worker?.Dispose();
            _worker = null;

            _preprocess?.Dispose();
            _preprocess = null;

            RTUtil.Destroy(_matte);
            _matte = null;
        }

        #endregion

        #region Main inference function

        void RunModel(Texture source)
        {
            if (source == null)
            {
                Debug.LogWarning("[MODNet Debug] Input texture is null!");
                return;
            }

            // Preprocessing
            _preprocess.Dispatch(source, _resources.preprocess);

            // NN worker invocation
            _worker.Schedule(_preprocess.Tensor);

            // Get output buffer
            ComputeBuffer outputBuffer = null;
            try
            {
                outputBuffer = _worker.PeekOutputBuffer();
                Debug.Log($"[MODNet Debug] Output buffer count: {outputBuffer.count}, stride: {outputBuffer.stride}");

                // Read first few values for debugging
                float[] testData = new float[Mathf.Min(20, outputBuffer.count)];
                outputBuffer.GetData(testData, 0, 0, testData.Length);

                float min = float.MaxValue, max = float.MinValue, sum = 0;
                for (int i = 0; i < testData.Length; i++)
                {
                    min = Mathf.Min(min, testData[i]);
                    max = Mathf.Max(max, testData[i]);
                    sum += testData[i];
                }
                Debug.Log($"[MODNet Debug] Output sample: min={min:F4}, max={max:F4}, avg={sum/testData.Length:F4}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MODNet Debug] Failed to peek output buffer: {e.Message}");
            }

            // Postprocessing (matte)
            var post = _resources.matte;
            post.SetBuffer(0, "MatteBuffer", outputBuffer);
            post.SetTexture(0, "Output", _matte);
            post.SetInts("InputSize", _config.InputWidth, _config.InputHeight);
            post.DispatchThreads(0, _config.InputWidth, _config.InputHeight, 1);
        }

        #endregion
    }
}
