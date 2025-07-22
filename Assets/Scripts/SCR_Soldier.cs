using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_Soldier : MonoBehaviour
{
    public GameObject basicAttackHb;
    public float hitBoxPersistenceDuration;

    public void Attack(string attackDirection)
    {
        //Chooses direction to send out attack based on the given attack direction
        float attackPosOffset = attackDirection switch
        {
            "right" => 1.0f,
            "left" => -1.0f,
            _ => 0.0f
        };

        //Spawns in attack hitbox
        Vector2 HBSpawnPosition = new Vector2(transform.position.x, transform.position.y) + new Vector2(attackPosOffset, 0);
        GameObject spawnedHitBox = Instantiate(basicAttackHb, HBSpawnPosition, Quaternion.identity);
        //Delete attack after a delay
        StartCoroutine(HitBoxDeleteTimer(spawnedHitBox));
    }
    
    private IEnumerator HitBoxDeleteTimer(GameObject hitboxToDelete)
    {
        yield return new WaitForSeconds(hitBoxPersistenceDuration);
        Destroy(hitboxToDelete);
    }
}
