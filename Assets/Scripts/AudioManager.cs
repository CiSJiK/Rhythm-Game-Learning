using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Instance")]
    public static AudioManager Instance;
    
    [Header("SFX")]
    FMOD.ChannelGroup sfxChannelGroup;
    FMOD.Sound[] sfxs;
    FMOD.Channel[] sfxChannels;
    
    [Header("Music")]
    FMOD.Channel musicChannel;

    private float fixedScrollSpeed = 1;
    private float tickTime = 0;
    private float scrollSpeed;
    private float originalSpeed;
    private List<double> scrollPos = new List<double>();

    void LoadSFX()
    {
        int count = System.Enum.GetNames(typeof(Enums.SFX)).Length;
        sfxChannelGroup = new FMOD.ChannelGroup();
        sfxChannels = new FMOD.Channel[count];
        sfxs = new FMOD.Sound[count];

        for (int i = 0; i < count; i++)
        {
            string sfxFile = System.Enum.GetName(typeof(Enums.SFX), i) + ".ogg";

            FMODUnity.RuntimeManager.CoreSystem.createSound(
                Path.Combine(Application.streamingAssetsPath, "SFXS", sfxFile), 
                FMOD.MODE.CREATESAMPLE, out sfxs[i]
            );
        }

        for (int i = 0; i < count; i++)
        {
            sfxChannels[i].setChannelGroup(sfxChannelGroup);
        }
    }

    void PlaySFX(Enums.SFX _sfx, float _volume = 1)
    {
        int index = (int)_sfx;
        sfxChannels[index].stop();
        
        FMODUnity.RuntimeManager.CoreSystem.playSound(
            sfxs[index], sfxChannelGroup, false, out sfxChannels[index]);
        sfxChannels[index].setPaused(true);
        //sfxChannels[index].setVolume(_volume)
        sfxChannels[index].setPaused(false);
        
    }
    
    uint GetTiming()
    {
        AudioManager.Instance.musicChannel.getPosition(out uint position, FMOD.TIMEUNIT.MS);
        return position;
    }

    public void SetMetronome(int _bpm, int _offset = 0)
    {
        int preBars = 10;
        
        uint time = GetTiming();
        uint startTime = time - (uint)_offset;

        tickTime = (60 / _bpm) * 1000;
        int tickCount = 0;

        if (startTime >= tickTime * (tickCount - preBars))
        {
            AudioManager.Instance.PlaySFX(Enums.SFX.Metronome);
            tickCount++;
            
        }
    }

    void SetFixedSpeed(float pivot)
    {
        fixedScrollSpeed = pivot / tickTime;
    }

    void SetSpeed(float speed)
    {
        scrollSpeed = tickTime * fixedScrollSpeed / speed;
        originalSpeed = fixedScrollSpeed / speed;
    }

    void GetScrollPosition(float noteStartTime)
    {
        for (float i = 0.1f; i <= 100f; i++)
        {
            double _time = noteStartTime - (tickTime * i) + tickTime;
            double yPos = (_time - noteStartTime) / tickTime * i;
            scrollPos.Add(yPos);
        }
    }
}
