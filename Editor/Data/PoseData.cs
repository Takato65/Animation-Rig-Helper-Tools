using System.Collections.Generic;
namespace Takato.AnimationRiggingHelperTools.Data
{
    [System.Serializable]
    public class PoseData
    {
        public string poseName;
        public List<BoneTransform> boneTransforms = new List<BoneTransform>();
    }
}
