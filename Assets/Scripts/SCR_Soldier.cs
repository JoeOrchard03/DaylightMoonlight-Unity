using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_Soldier : MonoBehaviour
{
    public GameObject basicAttackHB;
    public float hitBoxPersistenceDuration;

    public void Attack(string attackDirection)
    {
        float attackPosOffset = attackDirection switch
        {
            "right" => 1.0f,
            "left" => -1.0f,
            _ => 0.0f
        };

        Vector2 HBSpawnPosition = new Vector2(transform.position.x, transform.position.y) + new Vector2(attackPosOffset, 0);
        GameObject spawnedHitBox = Instantiate(basicAttackHB, HBSpawnPosition, Quaternion.identity);
        StartCoroutine(HitBoxDeleteTimer(spawnedHitBox));
    }
    
    private IEnumerator HitBoxDeleteTimer(GameObject hitboxToDelete)
    {
        yield return new WaitForSeconds(hitBoxPersistenceDuration);
        Destroy(hitboxToDelete);
    }
}
