using UnityEngine;
using Unity.InferenceEngine;

namespace MODNet
{
    [CreateAssetMenu(fileName = "MODNet",
                     menuName = "ScriptableObjects/MODNet Resource Set")]
    public sealed class MODNetResourceSet : ScriptableObject
    {
        public ModelAsset model;
        public int referenceSize = 512;
        public ComputeShader preprocess;
        public ComputeShader matte;
    }
}
