using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLoopController : MonoBehaviour
{
    private enum GameState
    {
        Start,
        Playing,
        Paused,
        HitPaused,
        GameOver,
        Won
    }

    [SerializeField] private Player player;
    [SerializeField] private Invaders invaders;
    [SerializeField] private Text stateMessageText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text livesText;
    [SerializeField] private AudioClip winSound;
    [SerializeField, Range(0f, 1f)] private float winVolume = 1f;

    private const string HighScoreKey = "HighScore";
    private const int PointsPerInvader = 10;

    private Controls controls;
    private AudioSource audioSource;
    private GameState state;
    private int score;
    private int highScore;
    private bool highScoreDirty;
    private MobileTouchControls touchControls;

    private void Awake()
    {
        state = GameState.Start;
        Time.timeScale = 0f;
        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        controls = new Controls();
        controls.Player.Attack.performed += HandleStartOrRestart;
        controls.UI.Cancel.performed += HandlePause;
        touchControls = gameObject.AddComponent<MobileTouchControls>();
    }

    private void Start()
    {
        if (player == null)
            player = FindFirstObjectByType<Player>();

        touchControls.Initialize(this, player);

        if (player != null)
        {
            player.OnLivesChanged += HandleLivesChanged;
            UpdateLivesText(player.LivesRemaining);
        }
        else
            Debug.LogError("GameLoopController could not find a Player.");

        if (invaders == null)
            invaders = FindFirstObjectByType<Invaders>();

        if (invaders != null)
        {
            invaders.OnInvaderKilled += HandleInvaderKilled;
            invaders.OnAllInvadersKilled += HandleAllInvadersKilled;
        }
        else
            Debug.LogError("GameLoopController could not find the invader swarm.");

        UpdateScoreText();

        SetState(GameState.Start);
        controls.Player.Attack.Enable();
        controls.UI.Cancel.Enable();
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;

        if (player != null)
            player.OnLivesChanged -= HandleLivesChanged;
        if (invaders != null)
        {
            invaders.OnInvaderKilled -= HandleInvaderKilled;
            invaders.OnAllInvadersKilled -= HandleAllInvadersKilled;
        }
        if (highScoreDirty)
            PlayerPrefs.Save();

        if (controls != null)
        {
            controls.Player.Attack.performed -= HandleStartOrRestart;
            controls.UI.Cancel.performed -= HandlePause;
            controls.Dispose();
        }
    }

    private void HandleStartOrRestart(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (state == GameState.Start)
            SetState(GameState.Playing);
        else if (state == GameState.HitPaused)
            SetState(GameState.Playing);
        else if (state == GameState.GameOver || state == GameState.Won)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void HandlePause(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (state == GameState.Playing)
            SetState(GameState.Paused);
        else if (state == GameState.Paused)
            SetState(GameState.Playing);
    }

    private void HandleLivesChanged(int livesRemaining)
    {
        UpdateLivesText(livesRemaining);

        if (livesRemaining <= 0)
            SetState(GameState.GameOver);
        else if (state == GameState.Playing)
            SetState(GameState.HitPaused);
    }

    private void HandleInvaderKilled(Invader killed)
    {
        score += PointsPerInvader;
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(HighScoreKey, highScore);
            highScoreDirty = true;
        }

        UpdateScoreText();
    }

    private void HandleAllInvadersKilled()
    {
        SetState(GameState.Won);
        if (winSound != null)
            audioSource.PlayOneShot(winSound, winVolume);
    }

    private void SetState(GameState newState)
    {
        state = newState;
        Time.timeScale = state == GameState.Playing ? 1f : 0f;

        if (player != null)
            player.SetGameActive(state == GameState.Playing);

        if (touchControls != null)
            touchControls.SetState(state == GameState.Playing, state == GameState.Paused);

        if (stateMessageText != null)
        {
            bool touch = touchControls != null && touchControls.IsAvailable;
            stateMessageText.text = state switch
            {
                GameState.Start => touch ? "START GAME\nTap PLAY to begin" : "START GAME\nPress Space to play",
                GameState.Playing => string.Empty,
                GameState.Paused => touch ? "PAUSED\nTap RESUME" : "PAUSED\nPress Esc to resume",
                GameState.HitPaused => touch ? "HIT!\nTap PLAY to continue" : "HIT!\nPress Space to continue",
                GameState.GameOver => touch ? "GAME OVER\nTap PLAY to restart" : "GAME OVER\nPress Space to restart",
                GameState.Won => touch ? "YOU'RE A WINNER!\nTap PLAY to restart" : "YOU'RE WINNER!\nPress Space to restart",
                _ => string.Empty
            };
        }
    }

    public void TouchPrimaryAction()
    {
        if (state == GameState.Playing)
            player?.FireFromTouch();
        else if (state == GameState.Start || state == GameState.HitPaused)
            SetState(GameState.Playing);
        else if (state == GameState.GameOver || state == GameState.Won)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void TouchPause()
    {
        if (state == GameState.Playing)
            SetState(GameState.Paused);
        else if (state == GameState.Paused)
            SetState(GameState.Playing);
    }

    public void TouchHeldFire() => player?.FireFromTouch();

    private void UpdateLivesText(int livesRemaining)
    {
        if (livesText != null)
            livesText.text = $"Lives: {livesRemaining}";
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = $"SCORE {score:0000}    HIGH SCORE {highScore:0000}";
    }
}
