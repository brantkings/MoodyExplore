using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Video;

public class MoodCheckHUD : Singleton<MoodCheckHUD>
{
    
    private static readonly char[] PassableChars = new char[] {' ', ',', '.', ';', ':'};
    
    private class WaitForNextInput : CustomYieldInstruction
    {
        private bool _nextPressed;
        private MoodCheckHUD _hud;
        
        public WaitForNextInput(MoodCheckHUD hud)
        {
            _hud = hud;
            _hud.OnPressNext += OnPressNext;
        }

        private void OnPressNext()
        {
            _nextPressed = true;
            _hud.OnPressNext -= OnPressNext;
            _hud = null;

        }

        public override bool keepWaiting
        {
            get
            {
                return !_nextPressed;
            }
        }
    }

    public delegate void DelCheckHUDEvent();

    public event DelCheckHUDEvent OnPressNext;


    public interface IVideoAsset
    {
        public VideoClip GetClip();
    }
    
    public interface ITalkAsset
    {
        public List<string> GetDialogue();
        public float GetTimeBetweenChars();
    }

    public interface IImageAsset
    {
        public Sprite GetImage();
    }

    public VideoPlayer videoPlayer;
    public GameObject videoBG;
    public Image image;
    public GameObject imageBG;
    public Text text;
    public GameObject pressNextObject;
    public GameObject textBG;

    private void Start()
    {
        HideAll();
    }

    public void PressNext()
    {
        if (OnPressNext != null)
        {
            OnPressNext.Invoke();
            return;
        }
        HideAll();
    }
    
    public void HideAll()
    {
        HideVideo();
        HideImage();
        HideText();
    }

    private void SetImageBGVisible(bool set)
    {
        imageBG.SetActive(set);
    }

    private void SetVideoBGVisible(bool set)
    {
        videoBG.SetActive(set);
    }

    private void SetTextVisible(bool set)
    {
        pressNextObject.SetActive(false);
        textBG.SetActive(set);
        text.gameObject.SetActive(set);
    }

    public bool IsShowingVideo()
    {
        return videoPlayer.gameObject.activeSelf;
    }

    public bool IsShowingImage()
    {
        return image.gameObject.activeSelf;
    }

    public bool IsShowingText()
    {
        return textBG.gameObject.activeSelf;
    }

    public bool IsShowing()
    {
        return IsShowingVideo() || IsShowingText() || IsShowingImage();
    }
    
    public void ShowVideo(IVideoAsset asset)
    {
        SetVideoBGVisible(true);
        videoPlayer.clip = asset.GetClip();
        videoPlayer.Play();
        videoPlayer.gameObject.SetActive(true);
    }

    public void HideVideo()
    {
        SetVideoBGVisible(false);
        videoPlayer.Stop();
        videoPlayer.gameObject.SetActive(false);
    }
    
    public void ShowText(ITalkAsset asset)
    {
        text.text = string.Empty;
        SetTextVisible(true);
        StartCoroutine(Write(asset));
    }
    
    public void HideText()
    {
        StopAllCoroutines();
        OnPressNext -= SkipText;
        SetTextVisible(false);
    }
    
    public void ShowImage(IImageAsset asset)
    {
        SetImageBGVisible(true);
        image.sprite = asset.GetImage();
        image.gameObject.SetActive(true);
    }

    public void HideImage()
    {
        SetImageBGVisible(false);
        image.gameObject.SetActive(false);
    }


    private IEnumerator Write(ITalkAsset asset)
    {
        _skipNextText = false;
        List<string> dialogue = asset.GetDialogue();
        for(int i = 0, len = dialogue.Count;i<len;i++)
        {
            string str = dialogue[i];
            yield return Write(str, asset.GetTimeBetweenChars(), i == (len - 1));
        }
        
    }

    private IEnumerator Write(string dialogue, float timeBetweenChars, bool lastDialogue)
    {
        OnPressNext += SkipText; //Careful with interruptions in text.
        string written = "";
        string toWrite = dialogue.Substring(0);
        int i = 0;
        while (toWrite.Length > 0)
        {
            char newC = toWrite[0];
            toWrite = toWrite.Substring(1);
            written += newC;
            Write(written, toWrite, "#ffffff");
            if (PassableChars.Contains(newC)) continue;
            if (!_skipNextText)
            {
                yield return new WaitForSecondsRealtime(timeBetweenChars);
            }
        }
        OnPressNext -= SkipText;
        _skipNextText = false;
        pressNextObject.SetActive(true);
        if (!lastDialogue)
        {
            yield return new WaitForNextInput(this);
            pressNextObject.SetActive(false);
        }
    }

    private bool _skipNextText;

    public void SkipText()
    {
        _skipNextText = true;
    }
    
    private void Write(string written, string notWritten, string color)
    {
        text.text = $"<color={color}>{written}</color><color='#00000000'>{notWritten}</color>";
    }
}
