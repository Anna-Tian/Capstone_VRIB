using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using SickscoreGames.HUDNavigationSystem;
using System.Linq;

public class PlayRandomAudio : MonoBehaviour
{
    public PairingTraining pTraining;
    public ExperimentController expctrl;

    AudioSource audioSource;
    AudioClip[] clipAudio = new AudioClip[3];
    int clipNum = 0;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        InitAudioClip();
    }

    void Update()
    {
        if (pTraining)
        {
            if (pTraining.qSymbol.name == gameObject.name && pTraining.isTrialStart)
            {
                pTraining.isTrialStart = false;
                clipNum = clipNum < 3 ? clipNum + 1 : Random.Range(1, 4);
                switch (clipNum)
                {
                    case 1:
                        audioSource.PlayOneShot(clipAudio[0]);
                        break;
                    case 2:
                        audioSource.PlayOneShot(clipAudio[1]);
                        break;
                    case 3:
                        audioSource.PlayOneShot(clipAudio[2]);
                        break;
                    default:
                        throw new System.Exception("Could not select audio clip");
                }
                audioSource.Play();
                audioSource.volume = 0.1f;
            }
        }
        if (expctrl && expctrl.EventRunning)
        {
            if (expctrl.HUDIcon.name == gameObject.name && expctrl.isAudioTrialStart)
            {
                int clipNumRandom = Random.Range(0, 3);
                expctrl.isAudioTrialStart = false;
                audioSource.PlayOneShot(clipAudio[clipNumRandom]);
                audioSource.Play();
                audioSource.volume = 0.1f;
                expctrl.currentTrial.audioName = clipAudio[clipNumRandom].name;
                transform.parent.parent.Find("ExperimentObjects").Find("NoticeScreen").Find("AudioName").GetComponent<Text>().text = expctrl.currentTrial.audioName;
            }
        }
    }



    private void InitAudioClip()
    {
        switch (gameObject.name)
        {
            case "Animal":
                clipAudio[0] = Resources.Load<AudioClip>("Sounds/300ms/animal_dog_cut300");
                clipAudio[1] = Resources.Load<AudioClip>("Sounds/300ms/animal_goat_cut300");
                clipAudio[2] = Resources.Load<AudioClip>("Sounds/300ms/animal_cat_cut300");
                break;
            case "Explosion":
                clipAudio[0] = Resources.Load<AudioClip>("Sounds/300ms/explosion_bomb_cut300");
                clipAudio[1] = Resources.Load<AudioClip>("Sounds/300ms/explosion_bomb_small_cut300");
                clipAudio[2] = Resources.Load<AudioClip>("Sounds/300ms/explosion_gunshot4_cut300");
                break;
            case "Ground Vehicle":
                clipAudio[0] = Resources.Load<AudioClip>("Sounds/300ms/ground_vehicle_alarma_de_carro_cut300");
                clipAudio[1] = Resources.Load<AudioClip>("Sounds/300ms/ground_vehicle_car_horn_cut300");
                clipAudio[2] = Resources.Load<AudioClip>("Sounds/300ms/ground_vehicle_truck_horn_cut300");
                break;
            case "Human Speech":
                clipAudio[0] = Resources.Load<AudioClip>("Sounds/300ms/human_speech_oh_male_cut300");
                clipAudio[1] = Resources.Load<AudioClip>("Sounds/300ms/human_speech_speech_female_cut300");
                clipAudio[2] = Resources.Load<AudioClip>("Sounds/300ms/human_speech_you_start_to_go_out_of_control_cut300");
                break;
            case "Unidentified":
                clipAudio[0] = Resources.Load<AudioClip>("Sounds/300ms/unidentified_chime_cut300");
                clipAudio[1] = Resources.Load<AudioClip>("Sounds/300ms/unidentified_noise_cut300");
                clipAudio[2] = Resources.Load<AudioClip>("Sounds/300ms/unidentified_ringtone_cut300");
                break;
            default:
                throw new System.Exception("Could not set audio clip");
        }
    }
}
