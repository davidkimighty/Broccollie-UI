using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Broccollie.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Broccollie.UI
{
    [DisallowMultipleComponent]
    public class ColorUIFeature : BaseUIFeature
    {
        [SerializeField] private Element[] _elements = null;

        #region Public Functions
        public override List<Task> GetFeatures(UIStates state, bool instantChange, bool playAudio, CancellationToken ct)
        {
            if (_elements == null) return default;

            List<Task> features = new List<Task>();
            for (int i = 0; i < _elements.Length; i++)
            {
                if (!_elements[i].IsEnabled || _elements[i].Preset == null) continue;

                ColorUIFeaturePreset.Setting setting = Array.Find(_elements[i].Preset.Settings, x => x.ExecutionState == state);
                if (!setting.IsEnabled) continue;

                if (instantChange)
                    features.Add(InstantColorChange(_elements[i].Graphic, setting.TargetColor, ct));
                else
                    features.Add(_elements[i].Graphic.LerpColorAsync(setting.TargetColor, setting.Duration, ct, setting.Curve));
            }
            return features;
        }

        #endregion

        private async Task InstantColorChange(MaskableGraphic graphic, Color color, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();
            graphic.color = color;
            await Task.Yield();
        }

        [Serializable]
        public struct Element
        {
            public bool IsEnabled;
            public MaskableGraphic Graphic;
            public ColorUIFeaturePreset Preset;
        }
    }
}
