using System.Collections.Generic;
using UnityEngine;

public class VelocityListGenerator : MonoBehaviour
{
    private int numVectorsToGen;

    [Header("using ANGLES not radians, set total arc angle to 0 for linear")]
    [SerializeField] float startAngle; //angle at which the first projectile in the spread is shot
    [SerializeField] float totalArcAngle; //goes ccw from start angle, set to 0 for linear
    [SerializeField] float speed;
    
    private List<Vector2> vs;
    private void Start() {
        vs = new List<Vector2>(numVectorsToGen);
        numVectorsToGen = GetComponent<BulletEmitter>().NumToEmitPerRound;

        float dTheta = totalArcAngle / numVectorsToGen;
        for (int i = 0; i < numVectorsToGen; i++)
        {
            Vector2 v = speed * new Vector2(
                Mathf.Cos(Mathf.Deg2Rad * (startAngle + i * dTheta)),
                Mathf.Sin(Mathf.Deg2Rad * (startAngle + i * dTheta)));
            vs.Add(v);
        }
    }

    public List<Vector2> GetVelocityList(){
        return vs;
    }

}
