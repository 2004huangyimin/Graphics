using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Experimental.VFX.Utility
{
    [RequireComponent(typeof(Renderer))]
    public class VFXVisibilityEventBinder : VFXEventBinderBase
    {
        public enum Activation
        {
            OnBecameVisible,
            OnBecameInvisible
        }

        public Activation activation = Activation.OnBecameVisible;

        protected override void SetEventAttribute(Object[] parameters) {}

        private void OnBecameVisible()
        {
            if (activation != Activation.OnBecameVisible) return;
            SendEventToVisualEffect();
        }

        private void OnBecameInvisible()
        {
            if (activation != Activation.OnBecameInvisible) return;
            SendEventToVisualEffect();
        }
    }
}
