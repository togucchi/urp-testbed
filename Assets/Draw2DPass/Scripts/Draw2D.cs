using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Toguchi.Rendering
{
    public class Draw2D : VolumeComponent
    {
        public ClampedFloatParameter FocusDistance = new ClampedFloatParameter(0f, 0f, 1f);
        public ClampedFloatParameter FocusRange = new ClampedFloatParameter(0f, 0f, 1f);
    }
}