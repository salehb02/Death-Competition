﻿//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

//Dev, not yet ready. Buoyancy API is set to change in 2.1.9+
#define USE_LEGACY_FUNCTIONS

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace StylizedWater2
{
    [ExecuteInEditMode]
    [AddComponentMenu("Stylized Water 2/Floating Transform")]
    public class FloatingTransform : MonoBehaviour
    {
        [Tooltip("This reference is required to grab the wave distance and height values")]
        public WaterObject waterObject;
        [Tooltip("Automatically find the Water Object below of above the Transform's position. This is slower than assigning a specific Water Object directly.")]
        public bool autoFind;
        [Tooltip("Only enable if the material's wave parameters are being changed in realtime, this has some performance overhead.\n\nIn edit-mode, the wave parameters are always fetched, so changes are directly visible")]
        public bool dynamicMaterial;

        public bool lerp;
        public float lerpSpeed = 2f;

        public enum WaterLevelSource
        {
            FixedValue,
            WaterObject
        }
        [Tooltip("Configure what should be used to set the base water level. Relative wave height is added to this value")]
        public WaterLevelSource waterLevelSource = WaterLevelSource.WaterObject;
        public float waterLevel;
        [Tooltip("You can assign a child mesh object here. When assigned, the sample points will rotate/scale with the transform, instead of transform the component is attached to.")]
        public Transform childTransform;

        public float heightOffset;
        [Min(0)]
        [Tooltip("Controls how strongly the transform should rotate to align with the wave curvature")]
        public float rollAmount = 0.1f;

#if !USE_LEGACY_FUNCTIONS
        private Buoyancy.BuoyancySample sampler;
#endif
        public List<Vector3> samples = new List<Vector3>();

        private Vector3 normal;
        private float height;
        private float m_waterLevel = 0f;

        /// <summary>
        /// Global toggle to disable the animations. This is used to temporarily disable all instances when editing a prefab, or sample positions in the editor
        /// </summary>
        public static bool Disable;

#if UNITY_EDITOR
        public static bool EnableInEditor
        {
            get { return UnityEditor.EditorPrefs.GetBool("SWS2_BUOYANCY_EDITOR_ENABLED", true); }
            set { UnityEditor.EditorPrefs.SetBool("SWS2_BUOYANCY_EDITOR_ENABLED", value); }
        }
#endif

#if UNITY_EDITOR
        private void OnEnable()
        {
            UnityEditor.EditorApplication.update += FixedUpdate;

#if !USE_LEGACY_FUNCTIONS
            sampler.SetSamplePositions(samples.ToArray());
#endif
        }

        private void Reset()
        {
            //Auto-assign water object if there is only one
            if (waterObject == null && WaterObject.Instances.Count > 0)
            {
                waterObject = WaterObject.Instances[0];
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }

        private void OnDisable()
        {
            UnityEditor.EditorApplication.update -= FixedUpdate;
        }
#endif

        private void OnValidate()
        {
#if !USE_LEGACY_FUNCTIONS
            sampler.SetSamplePositions(samples.ToArray());
#endif
        }

        public void FixedUpdate()
        {
            if (!this || !this.enabled || Disable) return;

#if UNITY_EDITOR
            if (!EnableInEditor && Application.isPlaying == false) return;
#endif

            if (autoFind) waterObject = WaterObject.Find(this.transform.position, false);

            if (!waterObject || !waterObject.material) return;

            m_waterLevel = waterObject && waterLevelSource == WaterLevelSource.WaterObject ? waterObject.transform.position.y : waterLevel;

#if USE_LEGACY_FUNCTIONS
            normal = Vector3.up;
            height = 0f;
            if (samples.Count == 0)
            {
                height = Buoyancy.SampleWaves(this.transform.position, waterObject.material, m_waterLevel, rollAmount, dynamicMaterial, out normal);
            }
            else
            {
                Vector3 avgNormal = Vector3.zero;
                for (int i = 0; i < samples.Count; i++)
                {
                    height += Buoyancy.SampleWaves(ConvertToWorldSpace(samples[i]), waterObject.material, m_waterLevel, rollAmount, dynamicMaterial, out normal);
                    avgNormal += normal;
                }

                height /= samples.Count;
                normal = (avgNormal / samples.Count).normalized;
            }

            height += heightOffset;
#else
            Buoyancy.SampleWaves(ref sampler, waterObject.material, m_waterLevel, dynamicMaterial);
            height = 0f;
            for (int i = 0; i < sampler.inputPositions.Length; i++)
            {
                height += sampler.outputOffset[i].y;
                normal += sampler.outputNormal[i];
            }
            
            height /= sampler.inputPositions.Length;
            height += heightOffset;

            normal /= sampler.inputPositions.Length;
            normal = Vector3.Lerp(Vector3.up, normal.normalized, rollAmount);
#endif

            ApplyTransform();
        }

        private void ApplyTransform()
        {
            //if (rollAmount > 0) this.transform.up = this.transform.up + normal;

            if (!lerp)
                this.transform.position = new Vector3(this.transform.position.x, height, this.transform.position.z);
            else
                transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, height, transform.position.z), Time.deltaTime * lerpSpeed);
        }

        public Vector3 ConvertToWorldSpace(Vector3 position)
        {
            if (childTransform) return childTransform.TransformPoint(position);

            return this.transform.TransformPoint(position);
        }

        public Vector3 ConvertToLocalSpace(Vector3 position)
        {
            if (childTransform) return childTransform.InverseTransformPoint(position);

            return this.transform.InverseTransformPoint(position);
        }

    }
}