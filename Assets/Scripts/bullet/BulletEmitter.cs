using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class BulletEmitter : MonoBehaviour
{
    public enum EmitType
    {
        aoe, projectile, stopFlag
    }
    [Header("Emit Type")]
    [SerializeField] EmitType type;

    [Header("General (all of this is for bullet ngl except nunrounds)")]
    [SerializeField] BulletAttributePreset preset;
    //[SerializeField] GameObject aoePrefab; //you don't need a prefab for aoe bc it's hard coded
    private GameManager manager;
    [Header("if linear, only emit one per round! WILL BE SET IN SCRIPT")]
    [SerializeField] int numToEmitPerRound = 1;
    public int NumToEmitPerRound => numToEmitPerRound;
    [SerializeField] int numRounds = 5;

    [Header("Time Between Rounds")]
    [SerializeField] float timeBetweenRounds = 2f; //in seconds
    [SerializeField] bool emitPerBeat;
    [SerializeField] float beatsBetweenRounds = 2f; //in beats, only use if emit per beat checked
    private int bpm;
    
    //private vars
    private int roundsEmitted;
    private float elapsedTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        roundsEmitted = 0;
        elapsedTime = timeBetweenRounds; //ensure that guaranteed to emit when update runs for first time
    }

    // Update is called once per frame
    void Update()
    {
        if (type != EmitType.stopFlag)
        {
            if (roundsEmitted == numRounds) return;

            if (manager == null)
            {
                manager = GameManager.instance;
                bpm = manager.bpm;
                if (emitPerBeat) timeBetweenRounds = beatsBetweenRounds*(60.0f / bpm);
            }

            elapsedTime += Time.deltaTime;
            if (elapsedTime >= timeBetweenRounds)
            {
                Emit();
                elapsedTime = 0;
            }
        }
    }
    void Emit()
    {
        roundsEmitted++;
        if (type == EmitType.aoe)
        {
            //everything should already be set up in the gameobject with 
            // area projectiles being in the correct positions childed to their emitter
            AreaDamage[] areas = GetComponentsInChildren<AreaDamage>(true);
            foreach (AreaDamage a in areas)
            {
                a.gameObject.SetActive(true);
            }
        }
        else if (type == EmitType.projectile)
        {
            List<Vector2> vs = GetComponent<VelocityListGenerator>().GetVelocityList();
            //this list remains the same for the same bullet emitter

            foreach (Vector2 v in vs)
            {
                GameObject tmp = manager.masterProjectilePool.Get();
                tmp.GetComponent<Projectile>().InitAttributes(preset);
                tmp.transform.position = transform.position;
                tmp.GetComponent<Projectile>().SetVelocity(v);
            }
        }
    }
    public EmitType GetEmitType()
    {
        return type;
    }
}
