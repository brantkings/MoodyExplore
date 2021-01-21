using UnityEngine;
using UnityEngine.Video;

namespace Code.MoodGame.Events
{
    [CreateAssetMenu(fileName = "Event_Video_", menuName = "Mood/Event/Video", order = 0)]
    public class VideoMoodEvent : MoodEvent, MoodCheckHUD.IVideoAsset
    {
        public VideoClip clip;


        protected override void Effect()
        {
            MoodCheckHUD.Instance.ShowVideo(this);
        }

        public VideoClip GetClip()
        {
            return clip;
        }
    }
}