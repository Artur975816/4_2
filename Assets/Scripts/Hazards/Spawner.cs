using UnityEngine;
namespace Assignment6 {
public class Spawner:MonoBehaviour {
 public Hazard[] prefabs; public float repeatSeconds=1.5f,spawnXMin=-8f,spawnXMax=8f,spawnY=6f;
 void Start(){InvokeRepeating(nameof(Spawn),1f,repeatSeconds);}
 void Spawn(){if(prefabs==null||prefabs.Length==0){Debug.LogWarning("Spawner: назначь Rock, BombHazard и Heart.");return;}Hazard p=prefabs[Random.Range(0,prefabs.Length)];if(p)Instantiate(p,new Vector3(Random.Range(spawnXMin,spawnXMax),spawnY,0),Quaternion.identity);}
 public void StopSpawning(){CancelInvoke(nameof(Spawn));}
}}
