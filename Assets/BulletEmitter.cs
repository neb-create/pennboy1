using UnityEngine;

public class BulletEmitter : MonoBehaviour
{
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] bool emitMultiple;
    [SerializeField] float timeBetweenEmits = 2f;
    [SerializeField] int projectilesToEmit = 5;
    private int numEmitted;
    private float elapsedTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Emit();

        if (!emitMultiple) Destroy(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= timeBetweenEmits)
        {
            Emit();
        }

        if (numEmitted == projectilesToEmit) Destroy(this.gameObject);
    }
    void Emit()
    {
        numEmitted++;
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        proj.GetComponent<Projectile>().SetVelocity(new Vector2(-5, 0));
    }

    //TODO: write helper functions that generate a list of velocity vectors, give each vector to each particle

}
