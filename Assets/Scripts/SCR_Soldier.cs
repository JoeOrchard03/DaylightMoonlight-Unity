using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
        spawnedHitBox.transform.parent = gameObject.transform;

        StumbleForward(attackPosOffset, spawnedHitBox);
        
        //Delete attack after a delay
        StartCoroutine(HitBoxDeleteTimer(spawnedHitBox));
    }

    private void StumbleForward(float attackPosOffset, GameObject spawnedHitBox)
    { 
        gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(attackPosOffset * 10,0), ForceMode2D.Impulse);  
    }
    
    private IEnumerator HitBoxDeleteTimer(GameObject hitboxToDelete)
    {
        yield return new WaitForSeconds(hitBoxPersistenceDuration);
        Destroy(hitboxToDelete);
    }
}
