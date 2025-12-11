using Unity.InferenceEngine;
using UnityEngine;
using Klak.NNUtils;
using Klak.NNUtils.Extensions;

namespace MODNet
{
    public sealed class MODNetDetector : System.IDisposable
    {
        #region Public methods/properties

        public MODNetDetector(MODNetResourceSet resources, int width, int height)
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

            // GPU worker
            _worker = new Worker(model, BackendType.GPUCompute);

            // Preprocessing buffers
            // MODNet requires NCHW format (N, C, H, W)
            _preprocess = new ImagePreprocess(_config.InputWidth, _config.InputHeight, nchwFix: true)
            {
                ColorCoeffs = _config.PreprocessCoeffs
            };

            // Output buffer (alpha matte)
            _matte = RTUtil.NewArgbUav(_config.InputWidth, _config.InputHeight);
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
            // Preprocessing
            _preprocess.Dispatch(source, _resources.preprocess);

            // NN worker invocation
            _worker.Schedule(_preprocess.Tensor);

            // Postprocessing (matte)
            var post = _resources.matte;
            post.SetBuffer(0, "MatteBuffer", _worker.PeekOutputBuffer());
            post.SetTexture(0, "Output", _matte);
            post.SetInts("InputSize", _config.InputWidth, _config.InputHeight);
            post.DispatchThreads(0, _config.InputWidth, _config.InputHeight, 1);
        }

        #endregion
    }
}
