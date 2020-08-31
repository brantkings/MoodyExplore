using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MoodCheckHUD : MonoBehaviour
{
    
    private static readonly char[] PassableChars = new char[] {' ', ',', '.', ';', ':'};
    
    private class WaitForNextInput : CustomYieldInstruction
    {
        private bool _nextPressed;
        
        public WaitForNextInput(MoodCheckHUD hud)
        {
            
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

    public void PressNext()
    {
        if (OnPressNext != null)
        {
            OnPressNext.Invoke();
            return;
        }



    }
    

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
    public Image image;
    public Text text;

    public void Next()
    {
        
    }

    public void Show(IVideoAsset asset)
    {
        videoPlayer.clip = asset.GetClip();
        videoPlayer.Play();
        videoPlayer.gameObject.SetActive(true);
    }
    
    public void Show(ITalkAsset asset)
    {
    }

    
    private IEnumerator Write(ITalkAsset asset)
    {
        _skipNextText = false;
        foreach (string str in asset.GetDialogue())
        {
            yield return Write(str, asset.GetTimeBetweenChars());
        }
    }

    private IEnumerator Write(string dialogue, float timeBetweenChars)
    {
        OnPressNext += SkipText;
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
            yield return new WaitForSecondsRealtime(timeBetweenChars);
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
    
    public void Show(IImageAsset asset)
    {
        image.sprite = asset.GetImage();
        image.gameObject.SetActive(true);
    }

    public void HideAll()
    {
        image.gameObject.SetActive(false);
        text.gameObject.SetActive(false);
        videoPlayer.gameObject.SetActive(false);
    }
}
