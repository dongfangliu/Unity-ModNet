using UnityEngine;

namespace MODNet
{
    struct MODNetConfig
    {
        public int ReferenceSize { get; private set; }
        public int InputWidth { get; private set; }
        public int InputHeight { get; private set; }

        // MODNet preprocessing coefficients: (pixel - 127.5) / 127.5
        // Unity textures are in [0, 1] range, so pixel = rgb * 255
        // (rgb * 255 - 127.5) / 127.5 = rgb * 2 - 1
        // Using Klak.NNUtils formula: rgb = rgb * ColorCoeffs.w + ColorCoeffs.xyz
        // Therefore: ColorCoeffs.w = 2, ColorCoeffs.xyz = -1
        public Vector4 PreprocessCoeffs => new Vector4(-1, -1, -1, 2);

        public MODNetConfig(MODNetResourceSet resources, int width, int height)
        {
            ReferenceSize = resources.referenceSize;

            // Maintain aspect ratio while scaling
            float scale = Mathf.Min((float)ReferenceSize / width, (float)ReferenceSize / height);
            int scaledWidth = Mathf.RoundToInt(width * scale);
            int scaledHeight = Mathf.RoundToInt(height * scale);

            // Align dimensions to nearest multiple of 32
            InputWidth = (scaledWidth + 31) / 32 * 32;
            InputHeight = (scaledHeight + 31) / 32 * 32;
        }
    }
}
