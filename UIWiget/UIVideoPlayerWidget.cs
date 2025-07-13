// Dateiname: UIVideoPlayerWidget.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using YourGame.UI.Widgets; // Annahme, dass sich UIWidget hier befindet

namespace YourGame.UI.Widgets
{
    // Erfordert eine VideoPlayer-Komponente auf demselben GameObject
    [RequireComponent(typeof(VideoPlayer))]
    [RequireComponent(typeof(RawImage))] // Benötigt RawImage, um den Videostream anzuzeigen
    public class UIVideoPlayerWidget : UIWidget
    {
        [Header("Video Player Components")]
        [SerializeField] private VideoPlayer _videoPlayer;
        [SerializeField] private RawImage _videoDisplay;
        [SerializeField] private AudioSource _audioSource; // Optional: für Video-Audio

        [Header("Video Player Settings")]
        [SerializeField] private bool _playOnAwake = false;
        [SerializeField] private bool _loop = false;
        [SerializeField] private bool _prepareOnStart = true;

        public VideoPlayer VideoPlayerInstance => _videoPlayer;

        protected override void Awake()
        {
            base.Awake();
            // Referenzen holen
            _videoPlayer = GetComponent<VideoPlayer>();
            _videoDisplay = GetComponent<RawImage>();
            _audioSource = GetComponent<AudioSource>(); // Versuche, eine AudioSource zu bekommen

            // Sicherstellen, dass VideoPlayer und RawImage korrekt konfiguriert sind
            _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            // Erstellen oder Zuweisen einer RenderTexture, wenn nicht vorhanden
            if (_videoPlayer.targetTexture == null)
            {
                _videoPlayer.targetTexture = new RenderTexture(1920, 1080, 24); // Beispielauflösung
            }
            _videoDisplay.texture = _videoPlayer.targetTexture;

            // Audio-Setup
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            _videoPlayer.SetTargetAudioSource(0, _audioSource);
            _audioSource.playOnAwake = false;
            _audioSource.loop = _loop; // Audio-Looping an Video-Looping anpassen

            _videoPlayer.playOnAwake = _playOnAwake;
            _videoPlayer.isLooping = _loop;

            // Events abonnieren
            _videoPlayer.prepareCompleted += OnVideoPrepared;
            _videoPlayer.loopPointReached += OnVideoLoopPointReached;
            _videoPlayer.errorReceived += OnVideoErrorReceived;

            // Optional: Startet die Vorbereitung direkt, wenn das Widget aktiv wird
            if (_prepareOnStart && _videoPlayer.clip != null)
            {
                PrepareVideo();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            // Events deabonnieren, um Memory Leaks zu vermeiden
            if (_videoPlayer != null)
            {
                _videoPlayer.prepareCompleted -= OnVideoPrepared;
                _videoPlayer.loopPointReached -= OnVideoLoopPointReached;
                _videoPlayer.errorReceived -= OnVideoErrorReceived;
            }
            // RenderTexture aufräumen, wenn sie nicht mehr benötigt wird
            if (_videoPlayer != null && _videoPlayer.targetTexture != null)
            {
                _videoPlayer.targetTexture.Release();
                _videoPlayer.targetTexture = null;
            }
        }

        /// <summary>
        /// Lädt ein Videoclip.
        /// </summary>
        /// <param name="clip">Der abzuspielende Videoclip.</param>
        /// <param name="playImmediately">Gibt an, ob das Video nach dem Laden sofort abgespielt werden soll.</param>
        /// <param name="loopVideo">Gibt an, ob das Video in einer Schleife abgespielt werden soll.</param>
        public void LoadVideo(VideoClip clip, bool playImmediately = false, bool loopVideo = false)
        {
            if (_videoPlayer == null)
            {
                Debug.LogError("[UIVideoPlayerWidget] VideoPlayer ist nicht zugewiesen oder gefunden!");
                return;
            }

            _videoPlayer.clip = clip;
            _videoPlayer.isLooping = loopVideo;
            if (_audioSource != null) _audioSource.loop = loopVideo;

            if (clip == null)
            {
                Debug.Log("[UIVideoPlayerWidget] Videoclip ist NULL. Stoppe Wiedergabe und deaktiviere Anzeige.");
                StopVideo();
                _videoDisplay.enabled = false;
                return;
            }

            _videoDisplay.enabled = true;
            PrepareVideo(playImmediately);
        }

        /// <summary>
        /// Bereitet das Video vor. Nach Abschluss wird OnVideoPrepared aufgerufen.
        /// </summary>
        /// <param name="playAfterPrepare">Ob das Video nach der Vorbereitung abgespielt werden soll.</param>
        public void PrepareVideo(bool playAfterPrepare = false)
        {
            if (_videoPlayer == null || _videoPlayer.clip == null) return;

            _videoPlayer.Prepare();
            if (playAfterPrepare)
            {
                // Füge einen einmaligen Listener hinzu, der nach der Vorbereitung abspielt
                _videoPlayer.prepareCompleted += (source) =>
                {
                    PlayVideo();
                    source.prepareCompleted -= OnVideoPrepared; // Entferne den Listener nach Gebrauch
                };
            }
            Debug.Log($"[UIVideoPlayerWidget] Bereite Video '{_videoPlayer.clip.name}' vor...");
        }

        /// <summary>
        /// Startet die Wiedergabe des Videos.
        /// </summary>
        public void PlayVideo()
        {
            if (_videoPlayer == null || _videoPlayer.clip == null) return;
            if (!_videoPlayer.isPrepared)
            {
                Debug.LogWarning("[UIVideoPlayerWidget] Video ist noch nicht vorbereitet. Starte Vorbereitung und spiele danach ab.");
                PrepareVideo(true); // Spielt ab, sobald vorbereitet
                return;
            }
            _videoPlayer.Play();
            if (_audioSource != null) _audioSource.Play();
            Debug.Log($"[UIVideoPlayerWidget] Spiele Video '{_videoPlayer.clip.name}' ab.");
        }

        /// <summary>
        /// Pausiert die Wiedergabe des Videos.
        /// </summary>
        public void PauseVideo()
        {
            if (_videoPlayer == null) return;
            _videoPlayer.Pause();
            if (_audioSource != null) _audioSource.Pause();
            Debug.Log($"[UIVideoPlayerWidget] Video '{_videoPlayer.clip.name}' pausiert.");
        }

        /// <summary>
        /// Stoppt die Wiedergabe des Videos und setzt die Zeit auf den Anfang zurück.
        /// </summary>
        public void StopVideo()
        {
            if (_videoPlayer == null) return;
            _videoPlayer.Stop();
            if (_audioSource != null) _audioSource.Stop();
            Debug.Log($"[UIVideoPlayerWidget] Video '{_videoPlayer.clip.name}' gestoppt.");
        }

        // --- VideoPlayer Callbacks ---
        private void OnVideoPrepared(VideoPlayer source)
        {
            Debug.Log($"[UIVideoPlayerWidget] Video '{source.clip.name}' vorbereitet. Dauer: {source.length} Sekunden.");
        }

        private void OnVideoLoopPointReached(VideoPlayer source)
        {
            Debug.Log($"[UIVideoPlayerWidget] Video '{source.clip.name}' Schleifenpunkt erreicht.");
            // Hier könnte man Logik hinzufügen, wenn ein Video einmalig abgespielt wurde (z.B. Widget ausblenden)
            if (!source.isLooping)
            {
                Hide(); // Beispiel: Widget ausblenden, wenn Video nicht looped
            }
        }

        private void OnVideoErrorReceived(VideoPlayer source, string message)
        {
            Debug.LogError($"[UIVideoPlayerWidget] Video-Fehler bei '{source.clip?.name ?? "Unbekanntes Video"}': {message}");
            // Hier könnte man ein Fehler-Overlay anzeigen
        }

        // Optional: Steuerung der Lautstärke des Videos (über die AudioSource)
        public void SetVolume(float volume)
        {
            if (_audioSource != null)
            {
                _audioSource.volume = Mathf.Clamp01(volume);
            }
        }
    }
}