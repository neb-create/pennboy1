using System.Collections.Generic;
using UnityEngine;

public class VelocityListGenerator : MonoBehaviour
{
    private int numVectorsToGen;
    private enum EmitPattern {
        radial, //projectiles emitted in an arc, equally spaced 
        linear, //projectile emitted straight in a direction
    }
    [SerializeField] EmitPattern pattern;

    [Header("Radial Attributes (using ANGLES not radians)")]
    [SerializeField] float startAngle; //angle at which the first projectile in the spread is shot
    [SerializeField] float totalArcAngle; //goes ccw from start angle 
    [Header("Linear Attributes (using ANGLES not radians)")]
    [SerializeField] float speed;
    [SerializeField] float angle;
    private List<Vector2> vs;
    private void Start() {
        vs = new List<Vector2>(numVectorsToGen);
        numVectorsToGen = GetComponent<BulletEmitter>().NumToEmitPerRound;

        if (pattern == EmitPattern.radial)
        {
            float dTheta = totalArcAngle / numVectorsToGen;
            for (int i = 0; i < numVectorsToGen; i++)
            {
                Vector2 v = speed * new Vector2(
                    Mathf.Cos(Mathf.Deg2Rad * (startAngle + i * dTheta)),
                    Mathf.Sin(Mathf.Deg2Rad * (startAngle + i * dTheta)));
                vs.Add(v);
            }
        }
        else if (pattern == EmitPattern.linear)
        {
            for (int i = 0; i < numVectorsToGen; i++)
            {
                Vector2 v = speed * new Vector2(
                    Mathf.Cos(Mathf.Deg2Rad * (angle)),
                    Mathf.Sin(Mathf.Deg2Rad * (angle)));
                vs.Add(v);
            }
        }
    }

    public List<Vector2> GetVelocityList(){
        return vs;
    }

}
