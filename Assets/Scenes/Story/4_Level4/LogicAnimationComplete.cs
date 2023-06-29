using UnityEngine;

namespace Scenes.Story._4_Level4
{
    public class LogicAnimationComplete : MonoBehaviour
    {

        public void AnimationComplete()
        {
            var obj = GetComponentInParent<LowerGateLogicAnd>();
            obj.DoObservation();
            obj.gameObject.SetActive(false);
        }
    }
}
