using System;
using UnityEngine;

namespace CorrentesDaNoite.UI
{
    [CreateAssetMenu(menuName = "CorrentesDaNoite/Cutscene Data")]
    public class CutsceneData : ScriptableObject
    {
        [Serializable]
        public class CutsceneSlide
        {
            public Sprite image;
            [TextArea(2, 4)] public string text;
            public float minDisplayTime = 0.5f;
            public float autoAdvanceTime = 0f;
        }

        public CutsceneSlide[] slides = Array.Empty<CutsceneSlide>();
        public float defaultMinDisplayTime = 0.5f;
        public float defaultAutoAdvanceTime = 4f;

        public int SlideCount => slides?.Length ?? 0;

        public CutsceneSlide GetSlide(int index)
        {
            if (slides == null || index < 0 || index >= slides.Length)
                return null;

            return slides[index];
        }

        public float GetMinDisplayTime(int index)
        {
            CutsceneSlide slide = GetSlide(index);
            float slideMin = slide != null ? slide.minDisplayTime : 0f;
            return Mathf.Max(defaultMinDisplayTime, slideMin);
        }

        public float GetAutoAdvanceTime(int index)
        {
            CutsceneSlide slide = GetSlide(index);
            float slideAuto = slide != null ? slide.autoAdvanceTime : 0f;
            float timeToUse = slideAuto > 0f ? slideAuto : defaultAutoAdvanceTime;
            return timeToUse > 0f ? timeToUse : 0f;
        }
    }
}