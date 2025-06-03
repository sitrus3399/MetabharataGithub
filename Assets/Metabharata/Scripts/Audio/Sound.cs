using UnityEngine;

namespace MetabharataAudio
{
    [System.Serializable]
    public class Sound
    {
        public string soundName;
        public SoundType soundType = SoundType.BGM;
        public AudioClip klip;

        [Range(0, 1)] public float vol;
        public float pitch;
        public bool isLoop;

        [HideInInspector]
        public AudioSource source;
    }

    public enum SoundType
    {
        MASTER,
        BGM,
        SFX,
        VOICE,
        VideoVO
    }
}