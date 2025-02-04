using System.Collections.Generic;
using UnityEngine.Serialization;
namespace Takato.AnimationRiggingHelperTools.Data
{
    [System.Serializable]
    public class PoseData
    {
        public string poseName;
        [FormerlySerializedAs("boneTransforms")]
        public List<BoneTransform> otherBoneTransforms = new List<BoneTransform>();
        public List<BoneTransform> HumanBoneTransforms = new List<BoneTransform>();
    }
}
