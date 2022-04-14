using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour {

    // ------------ Public-Editor variables --------------------

    public bool doNotShowLoadingScreen = false;

    [Space(4f), Header("Participant")]
    public string participantID = "Anonymous";
    public int trialNumber = 1;
    public bool isStanding = false;
    public bool isController = false;

    [Space(4f), Header("Specialized fields")]
    public string predefinedConditionFilePath = "PredefinedConditions.csv";

    [Space(4f), Header("Options")]
    public CameraOption cameraOption;
    public LocomotionOption locomotionOption;
    public CollectorOption collectorOption;

    [Space(4f), Header("Player-Target Interaction Settings")]
    public float minDistanceToOpenTarget = 0.1f;
    public float maxDistanceToOpenTarget = 0.9f;
    public float angularFieldToOpenTarget = 90f;
    public float maxDistanceToSeeTarget = 3.5f;
    public float scaleSize = 1f;

    [Space(4f), Header("Target Field")]
    public float safeRadius = 4f;

    [Space(4f), Header("Saving Settings")]
    public string saveFolderPath = "Results";

    // ------------ Public-Accessed properties -----------------
    public string ParticipantID { get { return participantID; } }
    public CameraOption CameraOption { get { return cameraOption; } }
    public LocomotionOption LocomotionOption { get { return locomotionOption; } }
    public CollectorOption CollectorOption { get { return collectorOption; } }
    public string SaveFolderPath { get { return saveFolderPath; } }
    public string FileNamePrefix
    {
        get
        {
            // Prefix of saving file name
            return string.Format("{0}_{1}_{2}_{3}_{4}",
            startingDateTime.ToString("yyyyMMdd_HHmmss"),
            participantID,
            cameraOption.ToString(),
            locomotionOption.ToString(),
            collectorOption.ToString()); ;
        }
    }

    // ------------ Internal-use variables --------------

    // States
    private ManagerState currentState;
    // Context for states
    private Context context;
    // Starting time
    private DateTime startingDateTime;

    // Use this for initialization
    void Start() {
        context = new Context(this);
        SetCurrentState(new InitState(context));
        startingDateTime = DateTime.Now;
    }

    // Update is called once per frame
    void Update() {
        // Update current state if it is not null
        if (currentState != null)
        {
            currentState.Update();
        }

    }

    // Set current state
    private void SetCurrentState(ManagerState state)
    {
        currentState = state;
    }

    // -------------------------- Extra components -----------------

    // Extra components except BaseCamera, BaseLocomotion, BaseCollector, BaseTarget, BaseField
    private List<BaseComponent> extras = new List<BaseComponent>();

    // This method adds a new component to this game manager. I should be called in Start() of the components.
    public void AddExtra(BaseComponent component)
    {
        if (component is BaseCamera || component is BaseLocomotion || component is BaseCollector 
            || component is BaseTarget || component is BaseField)
        {
            return;
        }
        extras.Add(component);
    }

    // -------------------------- Game Events ---------------------------

    // Player event
    public enum PlayerAction
    {
        Undefined = -1,
        Finding = 0,
        Approaching = 1,
        Visiting = 2,
        Collecting = 3,
        Leaving = 4
    }

    // Event args of player-target events
    public class GamePlayEventArgs
    {
        // Frame time
        public float time;
        // Player
        public GameObject player;
        // Player event
        public PlayerAction action;
        // Target
        public BaseTarget target;

        // Constructor 
        public GamePlayEventArgs(float time, GameObject player, PlayerAction action, BaseTarget target)
        {
            this.time = time;
            this.player = player;
            this.action = action;
            this.target = target;
        }

        // Constructor without target
        public GamePlayEventArgs(float time, GameObject player, PlayerAction action) : this(time, player, action, null) { }

        // Constructor with time only
        public GamePlayEventArgs(float time) : this(time, null, PlayerAction.Undefined) { }
    }

    // Game play event handler
    public delegate void GamePlayEventHandler(GamePlayEventArgs e);

    // Default handler
    private static GamePlayEventHandler GamePlayDefaultEventHandler = delegate (GamePlayEventArgs e) {};

    // Beginning and ending events of play state
    public event GamePlayEventHandler OnPlayStateBeginning = GamePlayDefaultEventHandler;
    public event GamePlayEventHandler OnPlayStateFinishing = GamePlayDefaultEventHandler;

    // Player-target events: these events NEVER happen in the same frame
    public event GamePlayEventHandler OnPlayerFindingTarget = GamePlayDefaultEventHandler;
    public event GamePlayEventHandler OnPlayerApproachingTarget = GamePlayDefaultEventHandler;
    public event GamePlayEventHandler OnPlayerVisitingTarget = GamePlayDefaultEventHandler;
    public event GamePlayEventHandler OnPlayerCollectingTarget = GamePlayDefaultEventHandler;
    public event GamePlayEventHandler OnPlayerLeavingTarget = GamePlayDefaultEventHandler;

    // Compound event: Approaching + Visiting + Collecting + Leaving
    public event GamePlayEventHandler OnPlayerInteractingWithTarget = GamePlayDefaultEventHandler;
    // Compound event: Finding + Interacting
    public event GamePlayEventHandler OnPlaying = GamePlayDefaultEventHandler;

    // Player in safe area
    public event GamePlayEventHandler OnPlayerInSafeArea = GamePlayDefaultEventHandler;
    // Player in unsafe area 
    public event GamePlayEventHandler OnPlayerInUnsafeArea = GamePlayDefaultEventHandler;

    // ----------------- Context for manager states ----------------
    public class Context
    {
        // ------------- Public variables ----------------

        public bool DoNotShowLoadingScreen { get { return gameManager.doNotShowLoadingScreen; } }

        public string ParticipantID
        {
            get { return gameManager.participantID; }
            set
            {
                // Only allow modifying participant ID in initial state
                if (gameManager.currentState is InitState)
                {
                    gameManager.participantID = value;
                }
            }
        }
        public int TrialNumber
        {
            get { return gameManager.trialNumber; }
            set
            {
                // Only allow modifying trial number in initial state
                if (gameManager.currentState is InitState)
                {
                    gameManager.trialNumber = value;
                }
            }
        }

        public bool IsStanding
        {
            get { return gameManager.isStanding; }
            set
            {
                // Only allow modifying posture in initial state
                if (gameManager.currentState is InitState)
                {
                    gameManager.isStanding = value;
                }
            }
        }

        public bool IsController
        {
            get {return gameManager.isController; }
            set
            {
                // Only allow modifying Control type in intial state
                if (gameManager.currentState is InitState)
                {
                    gameManager.isController = value;
                }
            }
        }

        public string PredefinedConditionFilePath { get { return gameManager.predefinedConditionFilePath; } }

        public string SaveFolderPath { get { return gameManager.saveFolderPath; } }

        public CameraOption CameraOption
        {
            get { return gameManager.cameraOption; }
            set
            {
                // Only allow modifying in initial state
                if (gameManager.currentState is InitState)
                {
                    gameManager.cameraOption = value;
                }
            }
        }
        public LocomotionOption LocomotionOption
        {
            get { return gameManager.locomotionOption; }
            set
            {
                // Only allow modifying in initial state
                if (gameManager.currentState is InitState)
                {
                    gameManager.locomotionOption = value;
                }
            }
        }
        public CollectorOption CollectorOption
        {
            get { return gameManager.collectorOption; }
            set
            {
                // Only allow modifying in initial state
                if (gameManager.currentState is InitState)
                {
                    gameManager.collectorOption = value;
                }
            }
        }

        public float MinDistanceToOpenTarget { get { return gameManager.minDistanceToOpenTarget; } }
        public float MaxDistanceToOpenTarget { get { return gameManager.maxDistanceToOpenTarget; } }
        public float AngularFieldToOpenTarget { get { return gameManager.angularFieldToOpenTarget; } }
        public float MaxDistanceToSeeTarget { get { return gameManager.maxDistanceToSeeTarget; } }
        public float ScaleSize {get {return gameManager.scaleSize;}}

        public float SafeRadius { get { return gameManager.safeRadius; } }

        public List<BaseComponent> Extras { get { return gameManager.extras; } }

        // ------------- Public methods --------------

        // Central Manager instance
        private GameManager gameManager;

        // Initializer with central manager
        public Context(GameManager gameManager)
        {
            this.gameManager = gameManager;
        }

        // Get next state from current state
        public ManagerState GetNextState(ManagerState currentState)
        {
            if (currentState is InitState)
            {
                return new SetupState(this);
            }
            if (currentState is SetupState)
            {
                return new PlayState(this);
            }
            if (currentState is PlayState)
            {
                return new SaveState(this);
            }
            if (currentState is SaveState)
            {
                return new EndState(this);
            }
            return null;
        }

        // Switch to next state
        public void SwitchToNextState(ManagerState nextState)
        {
            gameManager.SetCurrentState(nextState);
        }

        // Start coroutine
        public void StartCoroutine(IEnumerator coroutine)
        {
            gameManager.StartCoroutine(coroutine);
        }

        // ----------------- Player-Target events ---------------------

        // When play state begins
        public void OnPlayStateBeginning(GameObject player)
        {
            gameManager.OnPlayStateBeginning(new GamePlayEventArgs(Time.time, player, PlayerAction.Undefined));
        }

        // When play state finishes
        public void OnPlayStateFinishing(GameObject player)
        {
            gameManager.OnPlayStateFinishing(new GamePlayEventArgs(Time.time, player, PlayerAction.Undefined));
        }

        // When player finds a target
        public void OnPlayerFindingTarget(GameObject player)
        {
            GamePlayEventArgs e = new GamePlayEventArgs(Time.time, player, PlayerAction.Finding);
            gameManager.OnPlayerFindingTarget(e);
            gameManager.OnPlaying(e);
        }

        // When player approaches a target
        public void OnPlayerApproachingTarget(GameObject player, BaseTarget target)
        {
            GamePlayEventArgs e = new GamePlayEventArgs(Time.time, player, PlayerAction.Approaching, target);
            gameManager.OnPlayerApproachingTarget(e);
            gameManager.OnPlayerInteractingWithTarget(e);
            gameManager.OnPlaying(e);
        }

        // When player visits a target
        public void OnPlayerVisitingTarget(GameObject player, BaseTarget target)
        {
            GamePlayEventArgs e = new GamePlayEventArgs(Time.time, player, PlayerAction.Visiting, target);
            gameManager.OnPlayerVisitingTarget(e);
            gameManager.OnPlayerInteractingWithTarget(e);
            gameManager.OnPlaying(e);
        }

        // When player collects a target's core
        public void OnPlayerCollectingTarget(GameObject player, BaseTarget target)
        {
            GamePlayEventArgs e = new GamePlayEventArgs(Time.time, player, PlayerAction.Collecting, target);
            gameManager.OnPlayerCollectingTarget(e);
            gameManager.OnPlayerInteractingWithTarget(e);
            gameManager.OnPlaying(e);
        }

        // When player leaves a target
        public void OnPlayerLeavingTarget(GameObject player, BaseTarget target)
        {
            GamePlayEventArgs e = new GamePlayEventArgs(Time.time, player, PlayerAction.Leaving, target);
            gameManager.OnPlayerLeavingTarget(e);
            gameManager.OnPlayerInteractingWithTarget(e);
            gameManager.OnPlaying(e);
        }

        // When player is in safe area
        public void OnPlayerInSafeArea(GameObject player)
        {
            GamePlayEventArgs e = new GamePlayEventArgs(Time.time, player, PlayerAction.Undefined);
            gameManager.OnPlayerInSafeArea(e);
        }

        // When player goes out of safe area
        public void OnPlayerInUnsafeArea(GameObject player)
        {
            GamePlayEventArgs e = new GamePlayEventArgs(Time.time, player, PlayerAction.Undefined);
            gameManager.OnPlayerInUnsafeArea(e);
        }
    }

    // ------------------------ Save Data in Standalone when app is closed --------------------------------------------------

    // Check if application is quitted by accidentally CLOSING button on GAME WINDOWS
    // This method does not work in Unity Editor. It is only invoked in Standalone Binary.
    void OnApplicationQuit()
    {
        if (this.currentState is SaveState)
        {
            Application.CancelQuit();
        }
        if (this.currentState is PlayState)
        {
            Application.CancelQuit();
            this.currentState.ForceStop();
            this.currentState = new SaveState(context);
        }
    }

    // -------------------------- Save Data in Unity Editor when switching from Play Mode to Edit Mode -----------------------

#if UNITY_EDITOR

    // This method should only be invoked by Unity Editor in case STOP button is pressed in the Editor to save data.
    // It returns true if central manager is currently in PlayState and forcibly switched to SaveState.
    // Otherwise it returns false.
    public bool ForceSaveDataInEditorMode()
    {
        if (currentState is SaveState)
        {
            return true;
        }
        if (this.currentState is PlayState)
        {
            this.currentState.ForceStop();
            this.currentState = new SaveState(context);
            return true;
        }
        return false;
    }

    // This static class listens to Play-Edit mode changes in Unity Editor
    [UnityEditor.InitializeOnLoadAttribute]
    public static class PlayModeStateChangedListener
    {

        // Static constructor
        static PlayModeStateChangedListener()
        {
            UnityEditor.EditorApplication.playModeStateChanged += (state) => {
                if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                {
                    // Look for central manager and force save data
                    GameManager gameManager = GameObject.FindObjectOfType<GameManager>();
                    if (gameManager != null)
                    {
                        if (gameManager.ForceSaveDataInEditorMode())
                        {
                            // If there are data needed to be saved, cancel quitting Play Mode.
                            UnityEditor.EditorApplication.isPlaying = true;
                        }
                    }
                }
            };
        }

    }

#endif

}
