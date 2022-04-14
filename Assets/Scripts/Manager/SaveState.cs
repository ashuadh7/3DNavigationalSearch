using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveState : ManagerState {

    // All savers
    private BaseSaver[] savers;

    // Flag to check if saving is completed
    private bool isSaved = false;

    // Constructor with context of central manager
    public SaveState(GameManager.Context context) : base(context) { }

    // Initializer for the state
    protected override void Init()
    {
        base.Init();
        Debug.Log("Entering state 3 - Save ...");

        // Check existence of save folder
        if (!Directory.Exists(context.SaveFolderPath))
        {
            Directory.CreateDirectory(context.SaveFolderPath);
        }

        // Find all savers
        savers = GameObject.FindObjectsOfType<BaseSaver>();

        // Saving large amount of data may take a long period of time
        // Using coroutine to avoid blocking UI update
        isSaved = false;
        context.StartCoroutine(SaveData());
    }

    // Internal execution called once per frame
    protected override void Execute()
    {
        base.Execute();
    }

    // Deinitializer for the state
    protected override void Deinit()
    {
        base.Deinit();
        BaseNotifier.ShowInAnyNotifier("Finished!");
    }

    // Condition to stop the state
    protected override bool ShouldBeEnded()
    {
       return isSaved;
    }

    // Save data
    private IEnumerator SaveData()
    {
        foreach (BaseSaver saver in savers)
        {
            yield return saver.Save();
        }
        isSaved = true;
    }

}
