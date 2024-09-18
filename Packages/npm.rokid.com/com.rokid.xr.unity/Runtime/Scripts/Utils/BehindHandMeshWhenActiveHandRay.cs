using UnityEngine;
using Rokid.UXR.Interaction;
using System;
using System.Reflection.Emit;

namespace Rokid.UXR.Utility
{
    [Obsolete("Use CustomHandVisualState Instead")]
    public class BehindHandMeshWhenActiveHandRay : MonoBehaviour
    {
        [SerializeField]
        private ActiveHandType hand;
        [SerializeField]
        private GameObject handRay;

        [SerializeField]
        private GameObject handVisual;

        private bool handRayActive = false;
        private bool handVisualActive = false;

        private void Start()
        {
            InputModuleManager.OnObjectActive += OnObjectActive;
        }

        private void OnDestroy()
        {
            InputModuleManager.OnObjectActive -= OnObjectActive;
        }

        private void OnObjectActive(IInputModuleActive moduleActive, bool enable)
        {
            if (moduleActive.ActiveHandType == this.hand)
            {
                if (moduleActive.Go == handRay)
                {
                    handRayActive = enable;
                }
                else if (moduleActive.Go == handVisual)
                {
                    handVisualActive = enable;
                }
            }
        }

        private void LateUpdate()
        {
            if (handVisualActive && handRayActive && enabled)
            {
                RKLog.KeyInfo($"====OnObjectActive==== ray & hand enable disable hand");
                handVisualActive = false;
                handRayActive = false;
                handVisual.gameObject.SetActive(false);
            }
        }
    }
}
