using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace RemoveBackgroundVideo
{
    [RequireComponent(typeof(VideoPlayer))]
    [RequireComponent(typeof(RawImage))]
    public class UIVideoPlayer : MonoBehaviour
    {
        private VideoPlayer videoPlayer;
        private RawImage rawImage;

        void Awake()
        {
            videoPlayer = GetComponent<VideoPlayer>();
            rawImage = GetComponent<RawImage>();
            
            videoPlayer.renderMode = VideoRenderMode.APIOnly;
        }

        void Update()
        {
            if (videoPlayer.texture != null)
            {
                rawImage.texture = videoPlayer.texture;
            }
        }
    }
}
