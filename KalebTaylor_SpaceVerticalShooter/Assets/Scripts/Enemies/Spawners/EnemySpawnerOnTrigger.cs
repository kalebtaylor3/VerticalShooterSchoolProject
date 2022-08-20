/// <summary>
/// This component is used for spawning via a trigger collider
/// The"obstaclePrefab" is spawned at a spawn point when the spawn point enters a collider (attached to the same object using this script).  
/// The spawn point must be tagged with the "spawnPointTag" in order for spawning to occur.
/// The spawned object can be attached to the parent of the collider if attachToParent is true
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerOnTrigger : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Obstacle or Enemy to be spawned.")]
    protected GameObject obstaclePrefab;

    [SerializeField]
    [Tooltip("Attaches spawned object to the parent of collider that triggers it if true")]
    private bool attachToParent = true;

    [SerializeField]
    [Tooltip("Defines the tag name for spawn points for this region.")]
    private string spawnPointTag;

    [SerializeField]
    [Tooltip("Which states of the game is this spawner active during.")]
    protected GameStages[] spawnableStates;

    /// <summary>
    /// Returns true if the Obstacle can spawn during the current stage. 
    /// </summary>
    private bool CanSpawnDuringStage()
    {
        for (int i = 0; i < spawnableStates.Length; i++)
        {
            if (spawnableStates[i] == GameManager.Instance.GameStage)
            {
                return true;
            }
        }

        return false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Only spawn if you are active for this stage
        if (!CanSpawnDuringStage() ) 
        {
            return;
        }

        if(!string.IsNullOrEmpty(spawnPointTag) && collision.tag == spawnPointTag)
        {
            Transform spawnTransform = collision.gameObject.transform;
            GameObject spawnedObject = Instantiate(obstaclePrefab, spawnTransform.position, Quaternion.identity);
            if (attachToParent)
            {
                spawnedObject.transform.parent = spawnTransform.parent;
            }
        }
    }

}
