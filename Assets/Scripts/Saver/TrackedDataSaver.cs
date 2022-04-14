using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TrackedDataSaver : BaseSaver
{
    private static HashSet<LocomotionOption> designatedLocomotion =
        new HashSet<LocomotionOption> { LocomotionOption.Walking, LocomotionOption.Head, LocomotionOption.Joystick2DOF };

    // Public variables
    public int SavedRecordsInOneFrame = 50;

    // Game manager
    private GameManager gameManager;
    // Vive tracker
    private ViveTracker viveTracker;
    // Target field
    private BaseField baseField;
    // Target list
    private List<BaseTarget> targetList;
    // Starting time of game play (play state)
    private float startingTime;

    // Series of time
    private List<float> timeList;
    // Series of player position
    private List<Vector3> headPositionList;
    // Series of player rotation
    private List<Vector3> headRotationList;
    // Series of body position
    private List<Vector3> bodyPositionList;
    // Series of body rotation
    private List<Vector3> bodyRotationList;
    // Series of target event
    private List<GameManager.PlayerAction> targetEventList;
    // Series of target index
    private List<int> targetIndexList;

    // Series of head's travel distance
    private List<float> totalHeadDistance;
    // Series of head's total rotation
    private List<float> totalHeadRotation;
    // Series of body's travel distance
    private List<float> totalBodyDistance;
    // Series of body's total rotation
    private List<float> totalBodyRotation;
    // Series of cores collected
    private List<int> totalCores;
    // Series of total visits
    private List<int> totalVisits;
    // Series of total revisit
    private List<int> totalRevisits;

    // FOR SUMMARY
    // Time to ball #n
    private List<float> timeToBallN;
    // Head's travel distance to ball #n
    private List<float> headDistanceToBallN;
    // Head's rotation to ball #n
    private List<float> headRotationToBallN;
    // Body's travel distance to ball #n
    private List<float> bodyDistanceToBallN;
    // Body's rotation to ball #n
    private List<float> bodyRotationToBallN;
    // Total visits to ball #n
    private List<int> totalVisitsToBallN;
    // Total revisits to ball #n
    private List<int> totalRevisitsToBallN;

    // Set of visited target
    private HashSet<int> visitedTargetIndices;
    // Last visited target's index
    private int lastVisitedTargetIndex;
    // Time to first revisit
    private float timeToFirstRevisit;

    // Use this for initialization
    void Start()
    {
        // Get game manager
        gameManager = GameObject.FindObjectOfType<GameManager>();

        // Get starting time and target list
        gameManager.OnPlayStateBeginning +=
            delegate (GameManager.GamePlayEventArgs e)
            {
                startingTime = e.time;
                baseField = GameObject.FindObjectOfType<BaseField>();
                viveTracker = GameObject.FindObjectOfType<ViveTracker>();
                targetList = baseField.TargetList;
            };

        // Show score when play state ended
        gameManager.OnPlayStateFinishing +=
            delegate (GameManager.GamePlayEventArgs e)
            {
                BaseNotifier.ShowInAnyNotifier(string.Format("Score = {0:F0}", 600000f / timeList[timeList.Count - 1] / totalHeadDistance[totalHeadDistance.Count - 1]));
            };

        // Assign events
        gameManager.OnPlaying += AddEvent;

        // Initialize
        timeList = new List<float>();
        headPositionList = new List<Vector3>();
        headRotationList = new List<Vector3>();
        bodyPositionList = new List<Vector3>();
        bodyRotationList = new List<Vector3>();
        targetEventList = new List<GameManager.PlayerAction>();
        targetIndexList = new List<int>();

        // Additional
        totalHeadDistance = new List<float>();
        totalHeadRotation = new List<float>();
        totalBodyDistance = new List<float>();
        totalBodyRotation = new List<float>();
        totalCores = new List<int>();
        totalVisits = new List<int>();
        totalRevisits = new List<int>();

        // Collection stats
        timeToBallN = new List<float>();
        headDistanceToBallN = new List<float>();
        headRotationToBallN = new List<float>();
        bodyDistanceToBallN = new List<float>();
        bodyRotationToBallN = new List<float>();
        totalVisitsToBallN = new List<int>();
        totalRevisitsToBallN = new List<int>();

        // Support
        visitedTargetIndices = new HashSet<int>();
        lastVisitedTargetIndex = -1;
        timeToFirstRevisit = -1f;
    }

    // Add information to event lists
    void AddEvent(GameManager.GamePlayEventArgs e)
    {
        // Get current count
        int count = timeList.Count;

        // Basic information
        timeList.Add(e.time - startingTime);
        headPositionList.Add(e.player.transform.position);
        headRotationList.Add(e.player.transform.rotation.eulerAngles);
        bodyPositionList.Add(viveTracker.transform.position);
        bodyRotationList.Add(viveTracker.transform.rotation.eulerAngles);
        targetEventList.Add(e.action);
        targetIndexList.Add(e.target != null ? e.target.Index : -1);

        // Additional information
        totalHeadDistance.Add(count == 0 ? 0f : (totalHeadDistance[count - 1] + PairDistance(headPositionList[count - 1], headPositionList[count])));
        totalHeadRotation.Add(count == 0 ? 0f : (totalHeadRotation[count - 1] + PairRotation(headRotationList[count - 1], headRotationList[count])));
        totalBodyDistance.Add(count == 0 ? 0f : (totalBodyDistance[count - 1] + PairDistance(bodyPositionList[count - 1], bodyPositionList[count])));
        totalBodyRotation.Add(count == 0 ? 0f : (totalBodyRotation[count - 1] + PairRotation(bodyRotationList[count - 1], bodyRotationList[count])));
        totalCores.Add((count == 0 ? 0 : totalCores[count - 1]) + (e.action == GameManager.PlayerAction.Collecting ? 1 : 0));

        // Add to visisted target set
        if (e.action == GameManager.PlayerAction.Approaching && lastVisitedTargetIndex != e.target.Index)
        {
            totalVisits.Add((count == 0 ? 0 : totalVisits[count - 1]) + 1);
            totalRevisits.Add((count == 0 ? 0 : totalRevisits[count - 1]) + (visitedTargetIndices.Contains(e.target.Index) ? 1 : 0));
            visitedTargetIndices.Add(e.target.Index);
            lastVisitedTargetIndex = e.target.Index;
        }
        else
        {
            totalVisits.Add(count == 0 ? 0 : totalVisits[count - 1]);
            totalRevisits.Add((count == 0 ? 0 : totalRevisits[count - 1]));
        }

        // Collecting event
        if (e.action == GameManager.PlayerAction.Collecting)
        {
            timeToBallN.Add(timeList[count]);
            headDistanceToBallN.Add(totalHeadDistance[count]);
            headRotationToBallN.Add(totalHeadRotation[count]);
            bodyDistanceToBallN.Add(totalBodyDistance[count]);
            bodyRotationToBallN.Add(totalBodyRotation[count]);
            totalVisitsToBallN.Add(totalVisits[count]);
            totalRevisitsToBallN.Add(totalRevisits[count]);
        }

        // Time to first revisit
        if (totalRevisits[count] == 1 && totalRevisits[count - 1] == 0)
        {
            timeToFirstRevisit = timeList[count];
        }
    }

    // Save
    public override IEnumerator Save()
    {
        if (timeList.Count > 0)
        {
            // Prepare for saving
            Prepare();
            // Save individual trial
            yield return SaveTrial();
            // Save summary
            yield return SaveSummary();

            // Save designated summary when conditions meet requirements
            if (gameManager.CameraOption == CameraOption.Vive
                && gameManager.CollectorOption == CollectorOption.ViveController
                && designatedLocomotion.Contains(gameManager.LocomotionOption)
                && (gameManager.LocomotionOption != LocomotionOption.Walking || gameManager.isStanding))
            {
                yield return SaveDesignatedSummary();
            }
        }
    }

    // Prepare for saving
    private void Prepare()
    {
        // Add the last leaving event if needed
        GameManager.PlayerAction lastEvent = targetEventList[targetEventList.Count - 1];
        if (lastEvent != GameManager.PlayerAction.Finding && lastEvent != GameManager.PlayerAction.Leaving)
        {
            // Get the last count
            int count = timeList.Count - 1;

            // Basic information
            timeList.Add(timeList[count] + 0.016f);
            headPositionList.Add(headPositionList[count]);
            headRotationList.Add(headRotationList[count]);
            bodyPositionList.Add(bodyPositionList[count]);
            bodyRotationList.Add(bodyRotationList[count]);
            targetEventList.Add(GameManager.PlayerAction.Leaving);
            targetIndexList.Add(targetIndexList[count]);

            // Additional information
            totalHeadDistance.Add(totalHeadDistance[count]);
            totalHeadRotation.Add(totalHeadRotation[count]);
            totalBodyDistance.Add(totalBodyDistance[count]);
            totalBodyRotation.Add(totalBodyRotation[count]);
            totalCores.Add(totalCores[count]);
            totalVisits.Add(totalVisits[count]);
            totalRevisits.Add(totalRevisits[count]);
        }

        // Fill collection stats with -1
        while (timeToBallN.Count < baseField.NumberOfSpecialTargets)
        {
            timeToBallN.Add(-1);
            headDistanceToBallN.Add(-1);
            headRotationToBallN.Add(-1);
            bodyDistanceToBallN.Add(-1);
            bodyRotationToBallN.Add(-1);
            totalVisitsToBallN.Add(-1);
            totalRevisitsToBallN.Add(-1);
        }
    }

    // Save this trial
    private IEnumerator SaveTrial()
    {
        // Get prefix of file name
        string prefix = gameManager.FileNamePrefix;

        // Save target field
        string targetFieldFileName = string.Format("{0}_TargetField.csv", prefix);
        string targetFieldFilePath = Path.Combine(gameManager.SaveFolderPath, targetFieldFileName);
        SaveTargetField(targetFieldFilePath);

        // Skip one frame
        yield return null;

        // Save tracked data
        string trackedDataFileName = string.Format("{0}_TrackedData.csv", prefix);
        string trackedDataFilePath = Path.Combine(gameManager.SaveFolderPath, trackedDataFileName);
        yield return SaveTrackedData(trackedDataFilePath);
    }

    // Save target field
    private void SaveTargetField(string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // Headers: index of target, position of target (X,Y,Z), Euler angle of target (A,B,C), having core or not (0 or 1), position of default core
            writer.WriteLine("Index,Target_X,Target_Y,Target_Z,Target_A,Target_B,Target_C,Having_Ball,Ball_X,Ball_Y,Ball_Z");
            // Information of each target
            foreach (BaseTarget target in targetList)
            {
                writer.WriteLine("{0},{1},{2},{3},{4}",
                    target.Index,
                    ToCSVString(target.transform.position),
                    ToCSVString(target.transform.rotation.eulerAngles),
                    target.IsSpecial ? 1 : 0,
                    ToCSVString(target.GetDefaultCorePosition()));
            }
            // Close file
            writer.Close();
        }
    }

    // Save tracked data
    private IEnumerator SaveTrackedData(string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // Headers: time since beginning of Play State in ms, player's position (XYZ), player's Euler angles (ABC), event ID, target ID
            writer.WriteLine("Time,Head_X,Head_Y,Head_Z,Head_A,Head_B,Head_C,Body_X,Body_Y,Body_Z,Body_A,Body_B,Body_C," + 
                "Event,Event_ID,Target_ID,Total_Head_Distance,Total_Head_Rotation,Total_Body_Distance,Total_Body_Rotation," +
                "Total_Balls,Total_Visits,Total_Revisits");

            // Information
            for (int i = 0; i < timeList.Count; ++i)
            {
                writer.WriteLine("{0:F2},{1},{2},{3},{4},{5},{6},{7},{8:F3},{9:F3},{10:F3},{11:F3},{12},{13},{14}",
                    timeList[i] * 1000f,                    // 0
                    ToCSVString(headPositionList[i]),       // 1
                    ToCSVString(headRotationList[i]),       // 2
                    ToCSVString(bodyPositionList[i]),       // 3
                    ToCSVString(bodyRotationList[i]),       // 4
                    targetEventList[i].ToString(),          // 5
                    (int)targetEventList[i],                // 6
                    targetIndexList[i],                     // 7
                    totalHeadDistance[i],                   // 8
                    totalHeadRotation[i],                   // 9
                    totalBodyDistance[i],                   // 10
                    totalBodyRotation[i],                   // 11
                    totalCores[i],                          // 12
                    totalVisits[i],                         // 13
                    totalRevisits[i]);                      // 14

                // Skip one frame
                if ((i + 1) % SavedRecordsInOneFrame == 0)
                {
                    yield return null;
                }
            }

            // Close file
            writer.Close();
        }
    }

    // Save summary 
    private IEnumerator SaveSummary()
    {
        // Generate file names
        string mainFilePath = Path.Combine(gameManager.SaveFolderPath, "SUMMARY.csv");
        string tmpFilePath = Path.Combine(gameManager.SaveFolderPath, "SUMMARY.tmp");
        string bakFilePath = Path.Combine(gameManager.SaveFolderPath, "SUMMARY.bak");

        // Check if bak exists
        if (!File.Exists(mainFilePath) && !File.Exists(tmpFilePath))
        {
            // Create main summary file
            using (StreamWriter writer = new StreamWriter(mainFilePath))
            {
                // Write headers
                writer.Write("Date,Time,ID,Trial,Display,Locomotion Interface,Interaction Method,Completion Time,Number of ball collected,Time to first revisit");
                for (int i = 0; i < baseField.NumberOfSpecialTargets; ++i)
                {
                    writer.Write(",Time to ball #{0}" +
                        ",Travel distance to ball #{0}" +
                        ",Accumulated head yaw rotation to ball #{0}" +
                        ",Accumulated body yaw rotation to ball #{0}" +
                        ",Total target visits to ball #{0}" +
                        ",Total target revisits to ball #{0}", i + 1);
                }
                writer.WriteLine();
                writer.Close();
            }

            // Skip one frame
            yield return null;

        }

        if (!File.Exists(tmpFilePath))
        {
            // Copy to tmp
            File.Copy(mainFilePath, tmpFilePath);

            // Skip one frame
            yield return null;
        }

        // Copy tmp file to bak file if needed
        File.Copy(tmpFilePath, bakFilePath, true);

        // Skip one frame
        yield return null;

        DateTime now = DateTime.Now;

        // Append to bak file
        using (StreamWriter writer = File.AppendText(bakFilePath))
        {
            int lastIndex = timeList.Count - 1;
            writer.Write("{0},{1},{2},{3},{4},{5},{6},{7:F3},{8},{9:F3}",
                now.ToString("yyyyMMdd"), // 0
                now.ToString("HHmmss"),   // 1
                gameManager.ParticipantID,  // 2
                gameManager.trialNumber, // 3
                gameManager.CameraOption.ToString(),    // 4
                gameManager.LocomotionOption.ToString(),    // 5
                gameManager.CollectorOption.ToString(),     // 6
                timeList[lastIndex],        // 7
                totalCores[lastIndex],      // 8
                timeToFirstRevisit          // 9
            );
            for (int i = 0; i < baseField.NumberOfSpecialTargets; ++i)
            {
                writer.Write(",{0:F3},{1:F3},{2:F3},{3:F3},{4},{5}",
                    timeToBallN[i],
                    headDistanceToBallN[i],
                    headRotationToBallN[i],
                    bodyRotationToBallN[i],
                    totalVisitsToBallN[i],
                    totalRevisitsToBallN[i]);
            }
            writer.WriteLine();
            writer.Close();
        }

        // Skip one frame
        yield return null;

        // Copy to tmp file
        File.Copy(bakFilePath, tmpFilePath, true);

        // Skip one frame
        yield return null;

        // Copy to main file
        File.Copy(tmpFilePath, mainFilePath, true);
    }

    // -------------------- Designated summary ---------------

    // Save summary 
    private IEnumerator SaveDesignatedSummary()
    {
        // Generate file names
        string mainFilePath = Path.Combine(gameManager.SaveFolderPath, "DESIGNATED_SUMMARY.csv");
        string tmpFilePath = Path.Combine(gameManager.SaveFolderPath, "DESIGNATED_SUMMARY.tmp");
        string bakFilePath = Path.Combine(gameManager.SaveFolderPath, "DESIGNATED_SUMMARY.bak");

        // Check if bak exists
        if (!File.Exists(mainFilePath) && !File.Exists(tmpFilePath))
        {
            // Create main summary file
            using (StreamWriter writer = new StreamWriter(mainFilePath))
            {
                // Write headers
                writer.Write("Date,Time,ID,Trial,Locomotion Interface,Completion Time,Number of ball collected,Time to first revisit,Score");
                for (int i = 0; i < baseField.NumberOfSpecialTargets; ++i)
                {
                    writer.Write(",Time to ball #{0}" +
                        ",Travel distance to ball #{0}" +
                        ",Accumulated head yaw rotation to ball #{0}" +
                        ",Accumulated body yaw rotation to ball #{0}" +
                        ",Total target visits to ball #{0}" +
                        ",Total target revisits to ball #{0}", i + 1);
                }
                writer.WriteLine();
                writer.Close();
            }

            // Skip one frame
            yield return null;

        }

        if (!File.Exists(tmpFilePath))
        {
            // Copy to tmp
            File.Copy(mainFilePath, tmpFilePath);

            // Skip one frame
            yield return null;
        }

        // Copy tmp file to bak file if needed
        File.Copy(tmpFilePath, bakFilePath, true);

        // Skip one frame
        yield return null;

        // Append to bak file
        using (StreamWriter writer = File.AppendText(bakFilePath))
        {
            DateTime now = DateTime.Now;
            int lastIndex = timeList.Count - 1;
            writer.Write("{0},{1},{2},{3},{4},{5:F3},{6},{7:F3},{8:F0}",
                now.ToString("yyyyMMdd"), // 0
                now.ToString("HHmmss"),   // 1
                gameManager.ParticipantID,  // 2
                gameManager.trialNumber, // 3
                gameManager.LocomotionOption == LocomotionOption.Head && !gameManager.isStanding ? "NaviChair" : (
                    gameManager.LocomotionOption == LocomotionOption.Head && gameManager.isStanding ? "NaviBoard" : (
                    gameManager.LocomotionOption == LocomotionOption.Joystick2DOF && !gameManager.isStanding ? "Joystick" : (
                    gameManager.LocomotionOption == LocomotionOption.Walking && gameManager.isStanding ? "Walking" : (
                    gameManager.LocomotionOption == LocomotionOption.LeanWalking && gameManager.isStanding ? "LeanWalking" :
                        gameManager.LocomotionOption.ToString() + "-" + (gameManager.isStanding ? "Standing" : "Sitting"))))),    // 4
                timeList[lastIndex],        // 5
                totalCores[lastIndex],      // 6
                timeToFirstRevisit,          // 7
                600000f / timeList[lastIndex] / totalHeadDistance[lastIndex] // 8
            );
            for (int i = 0; i < baseField.NumberOfSpecialTargets; ++i)
            {
                writer.Write(",{0:F3},{1:F3},{2:F3},{3:F3},{4},{5}",
                    timeToBallN[i],
                    headDistanceToBallN[i],
                    headRotationToBallN[i],
                    bodyRotationToBallN[i],
                    totalVisitsToBallN[i],
                    totalRevisitsToBallN[i]);
            }
            writer.WriteLine();
            writer.Close();
        }

        // Skip one frame
        yield return null;

        // Copy to tmp file
        File.Copy(bakFilePath, tmpFilePath, true);

        // Skip one frame
        yield return null;

        // Copy to main file
        File.Copy(tmpFilePath, mainFilePath, true);
    }

    // -------------------- Support methods ------------------

    private string ToCSVString(Vector3 v)
    {
        return string.Format("{0:F3},{1:F3},{2:F3}", v.x, v.y, v.z);
    }

    // Get standardized distance from pos1 to pos 2 due to error
    private float PairDistance(Vector3 pos1, Vector3 pos2)
    {
        return (pos2 - pos1).magnitude;
    }

    // Get standardized angles between two Euler Angles
    private float PairRotation(Vector3 euler1, Vector3 euler2)
    {
        float rawAngle = Vector3.Angle(Quaternion.Euler(euler1) * Vector3.forward, Quaternion.Euler(euler2) * Vector3.forward);
        return rawAngle >= 0.1f ? rawAngle : 0f;
    }
}
