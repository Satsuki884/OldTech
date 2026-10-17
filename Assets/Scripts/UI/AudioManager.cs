using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    AudioClip click;
    void Awake()
    {
        if (!GetComponent<AudioSource>()) gameObject.AddComponent<AudioSource>();
        int count = 2205; var samples = new float[count];
        for (int i = 0; i < count; i++) samples[i] = Mathf.Sin(i * 2 * Mathf.PI * 660 / 44100) * .12f * (1f - (float)i / count);
        click = AudioClip.Create("Repair click", count, 1, 44100, false); click.SetData(samples, 0);
        GetComponent<AudioSource>().playOnAwake = false;
    }
    public void PlayClick() { GetComponent<AudioSource>().PlayOneShot(click); }
    void OnDestroy() { if (click) Destroy(click); }
}
