using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSFXHandler : MonoBehaviour {

    [Header("Target")]
    [SerializeField] private CharControl targetController;

    [Header("Audio Files")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip runSound;


    //component cache
    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        //"subscribe" to relevant events from controller.
        targetController.OnJumpEvent += PlayJumpSound;
        targetController.OnLandEvent += PlayLandSound;
        targetController.OnRunStartEvent += StartRunSound;
        targetController.OnRunEndEvent += StopRunSound;
    }

    public void PlayJumpSound()
    {
        _audioSource.clip = jumpSound;
        _audioSource.loop = false;
        _audioSource.Play();
    }

    public void PlayLandSound()
    {
        _audioSource.clip = landSound;
        _audioSource.loop = false;
        _audioSource.Play();
    }

    public void StartRunSound()
    {
        _audioSource.clip = runSound;
        _audioSource.loop = true;
        _audioSource.Play();
    }

    public void StopRunSound()
    {
        _audioSource.clip = null;
        _audioSource.loop = false;
        _audioSource.Stop();
    }


}
