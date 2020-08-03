using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Experimental.Rendering.Universal
{
    internal struct LightStats
    {
        public int totalLights;
        public int totalNormalMapUsage;
        public int totalVolumetricUsage;
        public uint blendStylesUsed;
    }

    internal interface ILight2DCullResult
    {
        List<Light2D> visibleLights { get; }
        LightStats GetLightStatsByLayer(int layer);
        bool IsSceneLit();
    }

    internal class Light2DCullResult : ILight2DCullResult
    {
        private List<Light2D> m_VisibleLights = new List<Light2D>();
        public List<Light2D> visibleLights => m_VisibleLights;

        public bool IsSceneLit()
        {
            if (visibleLights.Count > 0)
                return true;

            foreach (var light in Light2DManager.lights)
            {
                if (light.lightType == Light2D.LightType.Global)
                    return true;
            }

            return false;
        }
        public LightStats GetLightStatsByLayer(int layer)
        {
            var returnStats = new LightStats();
            foreach (var light in visibleLights)
            {
                if (!light.IsLitLayer(layer))
                    continue;

                returnStats.totalLights++;
                if (light.useNormalMap)
                    returnStats.totalNormalMapUsage++;
                if (light.volumeOpacity > 0)
                    returnStats.totalVolumetricUsage++;

                returnStats.blendStylesUsed |= (uint)(1 << light.blendStyleIndex);;
            }
            return returnStats;
        }

        public void SetupCulling(ref ScriptableCullingParameters cullingParameters, ref CameraData cameraData)
        {
            Profiler.BeginSample("Cull 2D Lights");
            m_VisibleLights.Clear();
            foreach (var light in Light2DManager.lights)
            {
                if ((cameraData.camera.cullingMask & (1 << light.gameObject.layer)) == 0)
                    continue;

#if UNITY_EDITOR
                if (!UnityEditor.SceneManagement.StageUtility.IsGameObjectRenderedByCamera(light.gameObject, cameraData.camera))
                    continue;
#endif

                Profiler.BeginSample("Test Planes");
                var culled = false;
                for (var i = 0; i < cullingParameters.cullingPlaneCount; ++i)
                {
                    var plane = cullingParameters.GetCullingPlane(i);
                    // most of the time is spent getting world position
                    var distance = math.dot(light.transform.position, plane.normal) + plane.distance;
                    if (distance < -light.boundingSphereRadius)
                    {
                        culled = true;
                        break;
                    }
                }
                Profiler.EndSample();
                if (culled)
                    continue;

                m_VisibleLights.Add(light);
            }

            // must be sorted here because light order could change
            m_VisibleLights.Sort((l1, l2) => l1.lightOrder - l2.lightOrder);
            Profiler.EndSample();
        }
    }
}
