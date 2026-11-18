using UnityEngine;

namespace MementoMori.Audio
{
    /// <summary>
    /// Turns a voiced recording into a whisper: the voice's own loudness contour drives breath noise
    /// (a whisper has no pitch), and only the airy top of the original is kept on top for articulation.
    /// Runs on the audio thread after the AudioSource on the same object.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class WhisperFilter : MonoBehaviour
    {
        [Range(0f, 1f)] public float breath = .78f;
        float envelope, hpPrev, hpOut, bandLow, bandBand;
        uint seed = 22222;
        int sampleRate = 48000;

        void Awake() => sampleRate = AudioSettings.outputSampleRate;

        void OnAudioFilterRead(float[] data, int channels)
        {
            // Envelope follower: fast attack, slower release, like breath behind syllables.
            var attack = 1f - Mathf.Exp(-1f / (.004f * sampleRate));
            var release = 1f - Mathf.Exp(-1f / (.06f * sampleRate));
            var hpA = Mathf.Exp(-2f * Mathf.PI * 1600f / sampleRate);
            // State-variable band-pass around the breathy "hh" region.
            var f = 2f * Mathf.Sin(Mathf.PI * 2600f / sampleRate);
            const float q = .7f;
            for (var i = 0; i < data.Length; i += channels)
            {
                var dry = 0f;
                for (var c = 0; c < channels; c++) dry += data[i + c];
                dry /= channels;
                var level = Mathf.Abs(dry);
                envelope += (level > envelope ? attack : release) * (level - envelope);

                seed ^= seed << 13; seed ^= seed >> 17; seed ^= seed << 5;
                var noise = (seed / (float)uint.MaxValue) * 2f - 1f;
                bandLow += f * bandBand;
                var high = noise - bandLow - q * bandBand;
                bandBand += f * high;
                var air = bandBand * envelope * 2.6f;

                // Keep only the hiss of the original (consonants), not its voiced pitch.
                hpOut = hpA * (hpOut + dry - hpPrev); hpPrev = dry;
                var mixed = Mathf.Lerp(hpOut, air, breath);
                for (var c = 0; c < channels; c++) data[i + c] = mixed;
            }
        }
    }
}
