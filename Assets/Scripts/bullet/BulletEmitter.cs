using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BulletEmitter : MonoBehaviour
{
    private enum EmitType
    {
        aoe, projectile
    }
    [SerializeField] GameObject projectilePrefab; 
    [SerializeField] GameObject aoePrefab; //only projectileprefab or aoeprefab will be used
    [SerializeField] int numToEmitPerRound = 1;
    public int NumToEmitPerRound => numToEmitPerRound;
    [SerializeField] int numRounds = 5;

    [Header("Emit Type")]
    [SerializeField] EmitType type;

    [Header("Time Between Rounds")]
    [SerializeField] float timeBetweenRounds = 2f; //in seconds
    [SerializeField] bool useBPM;
    [SerializeField] int bpm;
    
    //private vars
    private int roundsEmitted;
    private float elapsedTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        elapsedTime = timeBetweenRounds; //ensure that guaranteed to emit when update runs for first time

        if (useBPM) timeBetweenRounds = 60.0f / bpm;
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= timeBetweenRounds)
        {
            Emit();
            elapsedTime = 0;
        }

        if (roundsEmitted == numRounds) Destroy(this.gameObject);
    }
    void Emit()
    {
        roundsEmitted++;
        if (type == EmitType.aoe)
        {

        }
        else if(type == EmitType.projectile)
        {
            List<Vector2> vs = GetComponent<VelocityListGenerator>().GetVelocityList();
            //this list remains the same for the same bullet emitter

            foreach (Vector2 v in vs)
            {
                GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                proj.GetComponent<Projectile>().SetVelocity(v);
            }
        }
    }

    //TODO: write helper functions that generate a list of velocity vectors, give each vector to each particle

}
