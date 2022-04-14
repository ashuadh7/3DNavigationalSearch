using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class InitState : ManagerState
{
    // Settings Canvas
    private Canvas settingsCanvas;
    // Participant input field on Settings Canvas
    private InputField participantInput;
    // Trial input field on Settings Canvas
    private InputField trialInput;
    // Condition text
    private Text conditionText;
    // Start button on Settings Canvas
    private Button startButton;
    // Check if Start button on Settings Canvas pressed
    private bool isStartPressed;
    // Check if all components are initialized
    private bool areComponentsInitialized;

    // Constructor with context of central manager
    public InitState(GameManager.Context context) : base(context) { }

    // Initializer for the state
    protected override void Init()
    {
        base.Init();
        Debug.Log("Entering state 0 - Init ...");

        // Initialize
        areComponentsInitialized = false;
        isStartPressed = false;
        settingsCanvas = GameObject.Find("Settings Canvas").GetComponent<Canvas>();
        participantInput = GameObject.Find("Participant Input").GetComponent<InputField>();
        trialInput = GameObject.Find("Trial Input").GetComponent<InputField>();
        conditionText = GameObject.Find("Condition Text").GetComponent<Text>();
        startButton = settingsCanvas.GetComponentInChildren<Button>();

        // Add events
        startButton.onClick.AddListener(delegate () 
        {
            isStartPressed = true;
            context.ParticipantID = participantInput.text;
            context.TrialNumber = System.Convert.ToInt32(trialInput.text);
            // Save current settings to text file
            SaveCurrentSettingsToTextFile();
            // Set conditions
            SetConditionsBasedOnInputSettings();
        });

        participantInput.onValueChanged.AddListener(delegate (string text) 
        {
            CheckPredefinedCondition();
        });

        trialInput.onValueChanged.AddListener(delegate (string text)
        {
            CheckPredefinedCondition();
        });

        // Load predefined conditions
        LoadPredefinedConditions();

        // Load previous settings
        LoadPreviousSettingsToTextFile();
    }

    private void CheckPredefinedCondition()
    {
        // Check if (PID, trial) exists in predefined condition
        startButton.interactable = predefinedConditionMapping.ContainsKey(new StrPair(participantInput.text, trialInput.text));
        // Show condition
        conditionText.text = "";
        if (startButton.interactable)
        {
            StrPair condPair = predefinedConditionMapping[new StrPair(participantInput.text, trialInput.text)];
            conditionText.text = string.Format("Invalid: {0} - {1}", condPair.first, condPair.second);
            if (condPair.first == "Walking" && condPair.second == "Standing")
            {
                conditionText.text = "Walking";
            }
            if (condPair.first == "Joystick" && condPair.second == "Sitting")
            {
                conditionText.text = "Joystick";
            }
            if (condPair.first == "Leaning" && condPair.second == "Standing")
            {
                conditionText.text = "NaviBoard";
            }
            if (condPair.first == "Leaning" && condPair.second == "Sitting")
            {
                conditionText.text = "NaviChair";
            }
            if (condPair.first == "LeanWalking" && condPair.second == "Standing")
            {
                conditionText.text = "LeanWalking";
            }
            if (condPair.first == "LeanWalking" && condPair.second == "Standing")
            {
                conditionText.text = "LeanWalking";
            }
            if (condPair.first == "DroneBody" && condPair.second == "Sitting")
            {
                conditionText.text = "HeadJoystick";
            }
            if (condPair.first == "DroneController" && condPair.second == "Sitting")
            {
                conditionText.text = "Controller";
            }
        } 
    }

    // Internal execution called once per frame
    protected override void Execute()
    {
        base.Execute();
        
        // Press escape to use Anonymous as participant ID
        if (Input.GetKeyUp(KeyCode.Escape) || context.DoNotShowLoadingScreen)
        {
            context.ParticipantID = "Anonymous";
            context.TrialNumber = 1;
            isStartPressed = true;
        }

        // Check if start button on settings canvas is pressed
        if (isStartPressed)
        {
            // Destruy settings canvas
            settingsCanvas.gameObject.SetActive(false);

            // Init camera
            InitCamera();

            // Init locomotion
            InitLocomotion();

            // Init interaction
            InitCollector();

            // Init target field
            InitTargetField();

            InitDrone();

            // Done init
            areComponentsInitialized = true;
        }
    }

    // Deinitializer for the state
    protected override void Deinit()
    {
        base.Deinit();
    }

    // Condition to stop the state
    protected override bool ShouldBeEnded()
    {
        return areComponentsInitialized;
    }

    // ------------- Init components --------------

    // Choose camera to enable
    private void InitCamera()
    {
        // Remove dummy camera
        GameObject.Destroy(GameObject.Find("Dummy Camera"));

        // Get folder path of camera prefabs
        string prefabFolderPath = Path.Combine("Camera", "Prefabs");
        Object cameraPrefab = null;
        // Choose correct prefab
        switch (context.CameraOption)
        {
            case CameraOption.Desktop:
                cameraPrefab = Resources.Load(Path.Combine(prefabFolderPath, "Desktop Player System"));
                break;
            case CameraOption.Vive:
                cameraPrefab = Resources.Load(Path.Combine(prefabFolderPath, "Vive Player System"));
                break;
            case CameraOption.Vive2:
                 cameraPrefab = Resources.Load(Path.Combine(prefabFolderPath, "Vive Player System 2"));
                 break;
            default:
                break;
        }

        // Instantiate camera object from prefab
        if (cameraPrefab != null)
        {
            GameObject camera = Object.Instantiate(cameraPrefab) as GameObject;
            camera.name = camera.name.Replace("(Clone)", "");
        }
    }

    // Choose locomotion interface
    private void InitLocomotion ()
    {
        // Get folder path of locomotion prefabs
        string prefabFolderPath = Path.Combine("Locomotion", "Prefabs");
        Object locomotionPrefab = null;
        // Choose correct prefab
        switch (context.LocomotionOption)
        {
            case LocomotionOption.Keyboard:
                locomotionPrefab = Resources.Load(Path.Combine(prefabFolderPath, "Keyboard Locomotion"));
                break;
            case LocomotionOption.WiiBoard:
                locomotionPrefab = Resources.Load(Path.Combine(prefabFolderPath, "Wii Board Locomotion"));
                break;
            case LocomotionOption.Joystick:
                locomotionPrefab = Resources.Load(Path.Combine(prefabFolderPath, "Joystick Locomotion"));
                break;
            case LocomotionOption.Joystick2DOF:
                locomotionPrefab = Resources.Load(Path.Combine(prefabFolderPath, "Joystick 2DOF Locomotion"));
                break;
            /*
            case LocomotionOption.TrackerHead:
                locomotionPrefab = Resources.Load(Path.Combine(prefabFolderPath, "Tracker Head Locomotion"));
                break;
                */
            case LocomotionOption.Head:
                locomotionPrefab = Resources.Load(Path.Combine(prefabFolderPath, "Head Locomotion"));
                break;
            case LocomotionOption.Walking:
                locomotionPrefab = Resources.Load(Path.Combine(prefabFolderPath, "Walking Locomotion"));
                break;
            case LocomotionOption.LeanWalking:
                locomotionPrefab = Resources.Load(Path.Combine(prefabFolderPath, "New Locomotion"));
                break;
            case LocomotionOption.DroneBody:
                locomotionPrefab = Resources.Load(Path.Combine(prefabFolderPath, "Drone Locomotion"));
                break;
            case LocomotionOption.DroneController:
                locomotionPrefab = Resources.Load(Path.Combine(prefabFolderPath, "DroneGamepad Locomotion"));
                break;
            default:
                break;
        }

        // Instantiate camera object from prefab
        if (locomotionPrefab != null)
        {
            GameObject locomotion = Object.Instantiate(locomotionPrefab) as GameObject;
            locomotion.name = locomotion.name.Replace("(Clone)", "");
        }
    }

    // Choose interaction method
    private void InitCollector()
    {
        // Get folder path of interaction prefabs
        string prefabFolderPath = Path.Combine("Collector", "Prefabs");
        Object collectorPrefab = null;
        // Choose correct prefab
        switch (context.CollectorOption)
        {
            case CollectorOption.SpaceBar:
                collectorPrefab = Resources.Load(Path.Combine(prefabFolderPath, "Space Bar Collector"));
                break;
            case CollectorOption.ViveController:
                collectorPrefab = Resources.Load(Path.Combine(prefabFolderPath, "Vive Controller Collector"));
                break;
            case CollectorOption.HandFree:
                collectorPrefab = Resources.Load(Path.Combine(prefabFolderPath, "Hand Free Collector"));
                break;
            default:
                break;
        }

        // Instantiate camera object from prefab
        if (collectorPrefab != null)
        {
            GameObject collector = Object.Instantiate(collectorPrefab) as GameObject;
            collector.name = collector.name.Replace("(Clone)", "");
        }
    }

    // Choose target field
    private void InitTargetField()
    {
        // Get folder path of target prefabs
        string targetPrefabFolderPath = Path.Combine("Target", "Prefabs");
        //Object targetPrefab = Resources.Load(Path.Combine(targetPrefabFolderPath, "Standing Box Target"));
        Object targetPrefab = Resources.Load(Path.Combine(targetPrefabFolderPath, "Cube"));

        // Get folder path of target prefabs
        string fieldPrefabFolderPath = Path.Combine("Field", "Prefabs");
        Object fieldPrefab = Resources.Load(Path.Combine(fieldPrefabFolderPath, "Circular Field"));
        GameObject fieldObject = Object.Instantiate(fieldPrefab) as GameObject;
        BaseField field = fieldObject.GetComponent<BaseField>();
        // Set target prefab
        fieldObject.name = "Target Field";
        field.TargetPrefab = targetPrefab;
        field.NumberOfTargets = 16;
        field.NumberOfSpecialTargets = 8;
    }

    private void InitDrone()
    {
       // Get folder path of drone prefabs
        string dronePrefabFolderPath = Path.Combine("Drone", "Prefabs");
        //Object targetPrefab = Resources.Load(Path.Combine(targetPrefabFolderPath, "Standing Box Target"));
        Object dronePrefab = Resources.Load(Path.Combine(dronePrefabFolderPath, "Drone"));
        GameObject droneObject = Object.Instantiate(dronePrefab) as GameObject;
        droneObject.name = droneObject.name.Replace("(Clone)", "");
        GameManager gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        droneObject.transform.localScale += new Vector3((gameManager.scaleSize-1)/3,(gameManager.scaleSize-1)/3,(gameManager.scaleSize-1)/3);
        GameObject cockpit = GameObject.FindGameObjectWithTag("cockpit");
        if(gameManager.scaleSize != 1)
        {
            cockpit.transform.localScale = new Vector3(3/(gameManager.scaleSize-1),3/(gameManager.scaleSize-1),3/(gameManager.scaleSize-1));
        }
        
    }

    // ---------------------- Supports -------------------------

    struct StrPair
    {
        public string first;
        public string second;
        public StrPair(string first, string second)
        {
            this.first = first;
            this.second = second;
        }
    }

    // Predefined condition mapping (PID, Trial) -> (Locomotion, Posture)
    private Dictionary<StrPair, StrPair> predefinedConditionMapping;

    private void LoadPredefinedConditions()
    {
        if (!File.Exists(context.PredefinedConditionFilePath))
        {
            using (StreamWriter writer = new StreamWriter(context.PredefinedConditionFilePath))
            {
                // Write header
                writer.WriteLine("PID,Trial,Locomotion (Leaning/Walking/Joystick/LeanWalking/DroneBody/DroneController),Posture (Sitting/Standing)");
                writer.Close();
            }
        }
        // Read from file
        predefinedConditionMapping = new Dictionary<StrPair, StrPair>();
        using (FileStream stream = File.Open(context.PredefinedConditionFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                // Read headers
                reader.ReadLine();
                // Read mappings
                int cnt = 1;
                while (!reader.EndOfStream)
                {
                    ++cnt;
                    string line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    if (parts.Length < 4)
                    {
                        Debug.LogError(string.Format("Line {0}: Not enough parameters in \"{1}\"", cnt, line));
                        continue;
                    }
                    if (parts[2] != "Leaning" && parts[2] != "Walking" && parts[2] != "Joystick" && parts[2] != "LeanWalking" && parts[2] != "DroneBody" && parts[2] != "DroneController")
                    {
                        Debug.LogError(string.Format("Line {0}: Unsupported locomotion interface \"{1}\" in \"{2}\"", cnt, parts[2], line));
                        continue;
                    }
                    if (parts[3] != "Sitting" && parts[3] != "Standing")
                    {
                        Debug.LogError(string.Format("Line {0}: Unsupported posture \"{1}\" in \"{2}\"", cnt, parts[3], line));
                        continue;
                    }
                    if (parts[2] == "Walking" && parts[3] == "Sitting")
                    {
                        Debug.LogError(string.Format("Line {0}: \"Walking\" interface cannot be combined with \"Sitting\" posture in \"{1}\"", cnt, line));
                        continue;
                    }
                    if (!predefinedConditionMapping.ContainsKey(new StrPair(parts[0], parts[1])))
                    {
                        predefinedConditionMapping.Add(new StrPair(parts[0], parts[1]), new StrPair(parts[2], parts[3]));
                        Debug.Log(string.Format("Line {0}: Accepted predefined mapping ({1}, {2}) => ({3}, {4})", cnt, parts[0], parts[1], parts[2], parts[3]));
                    }
                    else
                    {
                        Debug.LogError(string.Format("Line {0}: Duplicated PID-Trial pair ({1}, {2})", cnt, parts[0], parts[1]));
                    }
                }
                reader.Close();
            }
        }
    }

    private void SetConditionsBasedOnInputSettings()
    {
        // Set camera and collector options
        context.CameraOption = CameraOption.Vive;
        context.CollectorOption = CollectorOption.HandFree;
        // Set locomotion and posture
        StrPair condPair = predefinedConditionMapping[new StrPair(participantInput.text, trialInput.text)];
        switch (condPair.first)
        {
            case "Leaning":
                context.LocomotionOption = LocomotionOption.Head;
                break;
            case "Walking":
                context.LocomotionOption = LocomotionOption.Walking;
                break;
            case "Joystick":
                context.LocomotionOption = LocomotionOption.Joystick2DOF;
                break;
            case "DroneBody":
                context.LocomotionOption = LocomotionOption.DroneBody;
                context.IsController = false;
                break;
            case "DroneController":
                context.LocomotionOption = LocomotionOption.DroneController;
                context.IsController = true;
                break;
            case "LeanWalking":
                context.LocomotionOption = LocomotionOption.LeanWalking;
                context.CameraOption = CameraOption.Vive2;
                break;              
            default:
                break;
        }
        switch (condPair.second)
        {
            case "Sitting":
                context.IsStanding = false;
                break;
            case "Standing":
                context.IsStanding = true;
                break;
            default:
                break;
        }
    }

    private void LoadPreviousSettingsToTextFile()
    {
        if (!File.Exists("settings.sts"))
        {
            return;
        }
        using (StreamReader reader = new StreamReader("settings.sts"))
        {
            participantInput.text = reader.ReadLine();
            trialInput.text = reader.ReadLine();
            reader.Close();
        }
    }

    private void SaveCurrentSettingsToTextFile()
    {
        using (StreamWriter writer = new StreamWriter("settings.sts"))
        {
            writer.WriteLine(participantInput.text);
            writer.WriteLine(trialInput.text);
            writer.Close();
        }
    }
}
