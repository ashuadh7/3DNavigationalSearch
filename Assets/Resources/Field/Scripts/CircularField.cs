using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CircularField : BaseField
{
    // Number of targets
    public int numberOfTargets = 16;
    // Number of special targets that have cores inside
    public int numberOfSpecialTargets = 8;
    // Radius of the field
    public float radius = 3f;
    // Mininum distance between targets
    public float minimumDistanceBetweenTargets = 2f;
    // Maximum trials to reduce maximum distance
    public int maximumTrialsToReduceMinimumDistance = 200;
    // Ratio of reducing maximumDistance
    public float ratioOfReducingMinimumDistance = 0.8f;
    // Radius of preserved area (space for player in the center of the field)
    public float radiusOfPreservedArea = 0.3f;
    public float areaHeight = 3f;
    
    public float scale; 

    // ------------------ Override abstract fields -------------------

    // Number of targets
    public override int NumberOfTargets
    {
        get { return numberOfTargets; }
        set
        {
            // Setter is only allowed before Setup state
            if (state == ComponentState.Initial)
            {
                numberOfTargets = value;
            }
        }
    }

    // Number of special targets
    public override int NumberOfSpecialTargets
    {
        get { return numberOfSpecialTargets; }
        set
        {
            // Setter is only allowed before Setup state
            if (state == ComponentState.Initial)
            {
                numberOfSpecialTargets = value;
            }
        }
    }

    // ------------------ Internals --------------------

    // Phases of setup state
    private enum SetupPhase
    {
        Initial,
        GeneratePositions,
        CreateTargets,
        SetupTargets
    }
    private SetupPhase setupPhase;

    // Flags for phases
    private bool isGenerated = false;
    private bool isCreated = false;

    // List of positions
    private List<Vector3> positions;

    // Called at the beginning Setup State of Game Manager
    public override void BeginSetupState()
    {
        base.BeginSetupState();
        setupPhase = SetupPhase.Initial;
        GameManager gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        scale = gameManager.scaleSize;
    }

    // Called during Setup State of Game Manager
    protected override void OnSetupState()
    {
        base.OnSetupState();
        // If positions are generated, create targets once
        switch (setupPhase)
        {
            case SetupPhase.Initial:
                isGenerated = false;
                isCreated = false;
                // Try generating list of target positions asynchronously
                setupPhase = SetupPhase.GeneratePositions;
                StartCoroutine(GeneratePositions());
                break;
            case SetupPhase.GeneratePositions:
                if (isGenerated)
                {
                    // Create targets in coroutine
                    setupPhase = SetupPhase.CreateTargets;
                    StartCoroutine(CreateTargets());
                }
                break;
            case SetupPhase.CreateTargets:
                if (isCreated)
                {
                    setupPhase = SetupPhase.SetupTargets;
                    // Setup all targets
                    foreach (BaseTarget target in targetList)
                    {
                        target.BeginSetupState();
                    }
                }
                break;
            case SetupPhase.SetupTargets:
                // Check if all targets are setup
                bool allAreSetup = true;
                foreach (BaseTarget target in targetList)
                {
                    allAreSetup = allAreSetup && (target.State == ComponentState.Pending);
                }
                if (allAreSetup)
                {
                    // Setup is done
                    BeginPendingState();
                }
                break;
            default:
                break;
        }
    }

    // Coroutine to create target
    private IEnumerator CreateTargets()
    {
        targetList = new List<BaseTarget>();
        for (int i = 0; i < numberOfTargets; ++i)
        {
            // Create new object
            GameObject targetObject = Instantiate(targetPrefab) as GameObject;
            targetObject.transform.localScale += new Vector3(scale-1,scale-1,scale-1);
            BaseTarget target = targetObject.GetComponent<BaseTarget>();
            // Set parent for the target
            targetObject.transform.parent = this.transform;
            // Set position
            targetObject.transform.localPosition = positions[i];
            // Randomly rotate the target
            targetObject.transform.Rotate(0, Random.value * 360, 0, Space.World);
            // Rename target
            targetObject.name = string.Format("Target_{0}", i.ToString("00"));
            // Set speciality
            target.IsSpecial = i < numberOfSpecialTargets;
            // Set index
            target.Index = i;
            // Add to list
            targetList.Add(target);
            // Skip to next frame
            yield return null;
        }
        isCreated = true;
    }

    // Generate target posotions
    private IEnumerator GeneratePositions()
    {
        positions = new List<Vector3>();
        while (true)
        {
            // List of current target positions
            positions.Clear();

            // Randomly create targets
            int trialCount = 0;
            while (positions.Count < numberOfTargets && trialCount < maximumTrialsToReduceMinimumDistance)
            {
                ++trialCount;
                Vector2 pos = Random.insideUnitCircle;
				float height = Random.Range(0,areaHeight);
                // Debug.Log("Random Circle points: " + pos.x.ToString()+ " " + pos.y.ToString());
                Vector3 newPos = new Vector3(radius * pos.x * scale, height * scale, radius * pos.y * scale);
                // Check position
                bool chk = false;
                foreach (Vector3 anotherPos in positions)
                {
                    // Check if the new target's position violated the minimum distance with other targets
                    // And if it is in preserved area
                    if (Vector3.Distance(newPos, anotherPos) < minimumDistanceBetweenTargets * scale || GetXZ(newPos).magnitude < radiusOfPreservedArea * scale)
                    {
                        chk = true;
                        break;
                    }
                }
                if (chk)
                {
                    continue;
                }
                // Update
                positions.Add(newPos);
            }

            // If enough positions have been generated, then returns
            if (positions.Count == numberOfTargets)
            {
                break;
            }
            else
            {
                // Reduce minimum distance between two any targets for the next trial
                minimumDistanceBetweenTargets *= ratioOfReducingMinimumDistance;
            }
            yield return null;
        }
        isGenerated = true;
    }

    // Return new vector with X, Z components.
    protected static Vector2 GetXZ(Vector3 vector)
    {
        return new Vector2(vector.x, vector.z);
    }

}
