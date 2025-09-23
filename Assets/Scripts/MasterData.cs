using UnityEngine;



public class MasterData : MonoBehaviour
{
    public static MasterData instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public StageModel currentStage;
    
}