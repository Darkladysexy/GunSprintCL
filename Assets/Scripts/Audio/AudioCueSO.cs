using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GunSprint.Audio
{
    /// <summary>
    /// A playable sound effect: one or more clips plus volume and pitch variation (delegate object pattern).
    /// Gameplay code only calls <see cref="Play"/>; the scene's <see cref="AudioSystem"/> decides which
    /// AudioSource plays it, so prefabs never need a reference to a scene audio manager.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioCue", menuName = "GunSprint/Audio/Audio Cue")]
    public class AudioCueSO : ScriptableObject
    {
        [Tooltip("One clip is picked at random each time the cue plays.")]
        [SerializeField] private AudioClip[] _clips = Array.Empty<AudioClip>();

        [Tooltip("Random volume range (x = min, y = max).")]
        [SerializeField] private Vector2 _volume = new Vector2(1f, 1f);

        [Tooltip("Random pitch range (x = min, y = max). A small range keeps repeated sounds from feeling robotic.")]
        [SerializeField] private Vector2 _pitch = new Vector2(1f, 1f);

        /// <summary>
        /// Raised when any cue asks to be played. Listened to by the active <see cref="AudioSystem"/>.
        /// </summary>
        public static event Action<AudioCueSO> PlayRequested;

        public void Play()
        {
            PlayRequested?.Invoke(this);
        }

        /// <summary>
        /// Configures the given source with a random clip, volume and pitch, then plays it.
        /// </summary>
        public void PlayOn(AudioSource source)
        {
            if (source == null || _clips.Length == 0)
            {
                return;
            }

            AudioClip clip = _clips[Random.Range(0, _clips.Length)];
            if (clip == null)
            {
                return;
            }

            source.clip = clip;
            source.volume = Random.Range(_volume.x, _volume.y);
            source.pitch = Random.Range(_pitch.x, _pitch.y);
            source.Play();
        }

        private void OnValidate()
        {
            _volume = new Vector2(Mathf.Clamp01(_volume.x), Mathf.Clamp(_volume.y, _volume.x, 1f));
            _pitch = new Vector2(Mathf.Clamp(_pitch.x, 0.1f, 3f), Mathf.Clamp(_pitch.y, _pitch.x, 3f));
        }
    }
}
