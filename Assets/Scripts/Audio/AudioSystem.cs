using GunSprint.Data;
using UnityEngine;

namespace GunSprint.Audio
{
    /// <summary>
    /// Plays every <see cref="AudioCueSO"/> through a small pool of 2D voices and applies the
    /// player's sound setting. One instance lives in each scene.
    /// </summary>
    [DisallowMultipleComponent]
    public class AudioSystem : MonoBehaviour
    {
        [SerializeField] private PlayerProgressSO _progress;

        [Tooltip("How many sound effects can overlap before the oldest one is cut.")]
        [SerializeField, Range(1, 16)] private int _voiceCount = 8;

        private AudioSource[] _voices;
        private int _nextVoice;

        private void Awake()
        {
            _voices = new AudioSource[_voiceCount];
            for (int i = 0; i < _voiceCount; i++)
            {
                AudioSource voice = gameObject.AddComponent<AudioSource>();
                voice.playOnAwake = false;
                voice.loop = false;
                voice.spatialBlend = 0f;
                _voices[i] = voice;
            }
        }

        private void OnEnable()
        {
            AudioCueSO.PlayRequested += PlayCue;
            _progress.SoundChanged += ApplySoundSetting;
            ApplySoundSetting(_progress.IsSoundOn);
        }

        private void OnDisable()
        {
            AudioCueSO.PlayRequested -= PlayCue;
            _progress.SoundChanged -= ApplySoundSetting;
        }

        private void PlayCue(AudioCueSO cue)
        {
            AudioSource voice = _voices[_nextVoice];
            _nextVoice = (_nextVoice + 1) % _voices.Length;
            cue.PlayOn(voice);
        }

        private static void ApplySoundSetting(bool isOn)
        {
            // The listener volume mutes music and effects together, so no source can get out of sync.
            AudioListener.volume = isOn ? 1f : 0f;
        }
    }
}
