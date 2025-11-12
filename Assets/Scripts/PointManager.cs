using System.Collections.Generic;
using UnityEngine;

public class PointManager : MonoBehaviour

{
    public static PointManager Instance;

    private int totalPoints = 0;
    private Dictionary<string, int> pointLog = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddPoints(string action, int points)
    {
        totalPoints += points;

        if (pointLog.ContainsKey(action))
            pointLog[action] += points;
        else
            pointLog.Add(action, points);
    }

    public int GetTotalPoints()
    {
        return totalPoints;
    }

    public Dictionary<string, int> GetPointLog()
    {
        return pointLog;
    }
}

