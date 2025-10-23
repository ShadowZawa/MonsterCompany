


using UnityEngine;
public enum FarmerTargetType
{
    meat,
    tree
}
public class HouseController : MonoBehaviour
{
    public Team team;
    public GameObject residentPrefab;
    public Transform spawnPos;
    private EntityModel _model;
    public EntityModel getModel => _model;
    public FarmerTargetType farmerTarget = FarmerTargetType.meat;
    public int farmerMaxStorage = 10;
    public void setTag(string teamName)
    {
        team = (teamName == "Blue") ? Team.Blue : Team.Red;
        gameObject.tag = teamName;
    }
    void Start()
    {
        _model = GetComponent<EntityModel>();
        InvokeRepeating("heal", 1f, 1f);
        // 生成1個居民
        GameObject resident = Instantiate(residentPrefab, spawnPos.position, Quaternion.identity);
        resident.SendMessage("init", this, SendMessageOptions.DontRequireReceiver);
    }




    
}