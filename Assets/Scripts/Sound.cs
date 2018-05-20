using System.Collections;
using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound {
    public string name;

    public AudioClip clip;

    public GameObject soundEmitter;

    [Range(0f, 1f)]
    public float volume;
    [Range(.1f, 3f)]
    public float pitch;
    public bool modularPitch;
    public float modularPitchRange;

    public bool loop;

    [HideInInspector]
    public AudioSource source;

    public void PlaySound()
    {
        source = soundEmitter.AddComponent<AudioSource>();

        source.playOnAwake = false;

        source.clip = clip;
        source.volume = volume;

        if (modularPitch == true)
            source.pitch = ModularPitch(pitch, modularPitchRange);
        else
            source.pitch = pitch;

        if(!source.isPlaying)
            source.Play();
    }

    public float ModularPitch(float _pitch, float _modularPitchRange)
    {
        float updatedPitch = Random.Range(_pitch - _modularPitchRange/2, _pitch + _modularPitchRange/2);

        if (updatedPitch < 0.1f)
            updatedPitch = 0.1f;
        else if (updatedPitch > 1)
            updatedPitch = 1;

        return updatedPitch;
    }

    ///////////////////////// 
    /*
    for (int i = 0; i<sounds.Length; i++)
    {
        if (sounds[i].name == "ha")
            sounds[i].PlaySound();
    }
    */
    ///////////////////////// 
}

