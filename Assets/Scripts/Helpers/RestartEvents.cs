/*******************************************************************
* This is not attached to any GameObject
*
* This is a STATIC class, meaning it acts as a "Global Notebook" for 
* for the game. It lives in the computer's memory * to store  
* data (Scores, Time) and broadcast "Radio Signals" (Actions) so that 
* other scripts (like the UI) know when things change.
* 
* Bruce Gustin
* Jan 2, 2026
*******************************************************************/

using System;
using UnityEngine;

public static class RestartEvents
{
    // 1. THE NOTEBOOK (This is where the data is stored)
    // This array stays in memory as long as the game is open.
    public static int SceneIndex = 0;
    public static int maxHealth = 2; 

    // 2. THE RADIO STATION (The Event)
    public static Action<int, int> OnScoreAdded;
    public static Action<int> OnTimeChange;
    public static Action<int> OnSceneChange;

    // 3. THE BROADCASTERS (The helper method)
   public static void SendSceneIndex()
    {
        SceneIndex++;
        OnSceneChange?.Invoke(SceneIndex);
    }
}