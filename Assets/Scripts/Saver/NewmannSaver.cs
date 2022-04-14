using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NewmannSaver : BaseSaver
{
    // Public variables
    public int SavedRecordsInOneFrame = 50;

    // Game Manager
    private GameManager gameManager;

    // For Newmann
    protected List<NewmannData> recordedData;
    protected DateTime startTime;
    private Vector3 previousHeadOrientation;
    private Vector3 previousVirtualCamOrientation;
    private DateTime lastSavingTime;
    public double DELTA_TIME = 0.01; //second
    private Vector3 lastPosition;

    // Use this for initialization
    void Start ()
    {
        // Add to game manager
        gameManager = GameObject.FindObjectOfType<GameManager>();

        // Initialize at the beginning of play state
        gameManager.OnPlayStateBeginning += OnPlayStateBeginning;

        // Listen to event during play state
        gameManager.OnPlaying += OnPlaying;
	}

    // Initialize parameters
    private void OnPlayStateBeginning(GameManager.GamePlayEventArgs e)
    {
        // Get player (usuall main camera)
        GameObject player = e.player;

        // Initialize
        startTime = DateTime.Now;
        lastSavingTime = startTime;
        previousHeadOrientation = ClosestAngularVector(Vector3.zero, player.transform.localRotation.eulerAngles);
        previousVirtualCamOrientation = ClosestAngularVector(Vector3.zero, player.transform.rotation.eulerAngles);
        recordedData = new List<NewmannData>();
        lastPosition = new Vector3(Mathf.Infinity, 0f, 0f);
    }

    // Collect data while playing
    private void OnPlaying(GameManager.GamePlayEventArgs e)
    {
        // Get player (usuall main camera)
        GameObject player = e.player;

        // Calculate velocity
        Vector3 velocity = (lastPosition.x == Mathf.Infinity) ? Vector3.zero : ((player.transform.position - lastPosition) / Time.deltaTime);
        lastPosition = player.transform.position;

        double currentDeltaT = (DateTime.Now - lastSavingTime).TotalSeconds;
        if (currentDeltaT < DELTA_TIME)
            return;

        Vector3 currentHeadOrientation = ClosestAngularVector(previousHeadOrientation, player.transform.localRotation.eulerAngles);
        Vector3 currentVirtualCamOrientation = ClosestAngularVector(previousVirtualCamOrientation, player.transform.rotation.eulerAngles);
        Vector3 omegaVest = (currentHeadOrientation - previousHeadOrientation) / (float)currentDeltaT;
        Vector3 omegaVis = (currentVirtualCamOrientation - previousVirtualCamOrientation) / (float)currentDeltaT;
        lastSavingTime = DateTime.Now;
        double now = (lastSavingTime - this.startTime).TotalSeconds;
        // TODO: IMPORTANT: Temporarily set sickness level to 0
        //double sicknessLevel = GameObject.FindObjectOfType<SicknessIndicator> ().sicknessLevel;
        double sicknessLevel = 0;
        // Record transform information
        recordedData.Add(new NewmannData(now, player, velocity, omegaVest, omegaVis, sicknessLevel, currentHeadOrientation, currentVirtualCamOrientation)); //CHANGE THIS SICKNESS VALUE
        previousHeadOrientation = currentHeadOrientation;
        previousVirtualCamOrientation = currentVirtualCamOrientation;
    }

    // Save data in coroutine
    public override IEnumerator Save()
    {
        string filePath = Path.Combine(gameManager.SaveFolderPath, 
            string.Format("{0}_NewmannData.csv", gameManager.FileNamePrefix));

        // Write to file
        using (StreamWriter file = new StreamWriter(filePath))
        {
            file.WriteLine("t_sec,xVest_m,yVest_m,zVest_m,omegaXVest_degPSec,omegaYVest_degPSec,omegaZVest_degPSec," +
                "xVis_m,yVis_m,zVis_m,xVisDot_mPSec,yVisDot_mPSec,zVisDot_mPSec,omegaXVis_degPSec,omegaYVis_degPSec,omegaZVis_degPSec," +
                "g_xvis,g_yvis,g_zvis,g,Pos ON,Vel ON,AngVel ON,G ON," + // constant
                "Sickness," +
                "thetaXVest_deg,thetaYVest_deg,thetaZVest_deg,thetaXVis_deg,thetaYVis_deg,thetaZVis_deg");


            for (int i = 0; i < recordedData.Count; ++i)
            {
                file.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23}",
                    recordedData[i].time,
                    recordedData[i].x_in.x, recordedData[i].x_in.y, recordedData[i].x_in.z,
                    recordedData[i].omega_in.x, recordedData[i].omega_in.y, recordedData[i].omega_in.z,
                    recordedData[i].xv_in.x, recordedData[i].xv_in.y, recordedData[i].xv_in.z,
                    recordedData[i].xvdot_in.x, recordedData[i].xvdot_in.y, recordedData[i].xvdot_in.z,
                    recordedData[i].omegav_in.x, recordedData[i].omegav_in.y, recordedData[i].omegav_in.z,
                    "0, 0, -1, 1, 1, 1, 1, 1", // constant
                    recordedData[i].sickness, // sickness
                    recordedData[i].theta.x, recordedData[i].theta.y, recordedData[i].theta.z,
                    recordedData[i].thetav.x, recordedData[i].thetav.y, recordedData[i].thetav.z
                );

                // Skip one frame
                if ((i + 1) % SavedRecordsInOneFrame == 0)
                {
                    yield return null;
                }
            }
            file.Close();
        }
    }

    // -------------------------- Support methods ------------------------

    Vector3 ClosestAngularVector(Vector3 prevAngle, Vector3 currentAngle)
    {
        Vector3 result;
        result.x = FindClosestAngle(prevAngle.x, currentAngle.x);
        result.y = FindClosestAngle(prevAngle.y, currentAngle.y);
        result.z = FindClosestAngle(prevAngle.z, currentAngle.z);
        return result;
    }

    float FindClosestAngle(float prevAngle, float angle)
    {
        if (prevAngle != float.PositiveInfinity)
        {
            float smaller = angle;
            while (smaller > prevAngle)
            {
                smaller -= 360;
            }
            while (smaller + 360 <= prevAngle)
            {
                smaller += 360;
            }
            float larger = angle;
            while (larger < prevAngle)
            {
                larger += 360;
            }
            while (larger - 360 >= prevAngle)
            {
                larger -= 360;
            }
            return Mathf.Abs(smaller - prevAngle) < Mathf.Abs(larger - prevAngle) ? smaller : larger;
        }
        else
        {
            return angle;
        }
    }
}
