using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSFXHandler : MonoBehaviour {

    [Header("Target")]
    [SerializeField] private CharControl targetController;

    [Header("Audio Files")]
    [SerializeField] private AudioClip jumpSound;


    //component cache
    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        targetController.OnJumpEvent += playJumpSound;
    }


    private void playJumpSound()
    {
        _audioSource.clip = jumpSound;
        _audioSource.loop = false;
        _audioSource.Play();
    }


}
