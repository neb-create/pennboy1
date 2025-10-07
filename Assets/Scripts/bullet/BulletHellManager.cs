using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BulletHellManager : MonoBehaviour
{
    public static BulletHellManager instance;
    public GameObject player;
    public ObjectPool<GameObject> masterProjectilePool;
    public int defaultPoolSize = 50;
    public int maxPoolSize = 100;
    [SerializeField] GameObject bulletPrefab;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        masterProjectilePool = new ObjectPool<GameObject>(
            CreatePooledBullet,
            OnGetBullet,
            OnReleaseBullet,
            OnDestroyBullet,
            collectionCheck: false,
            defaultCapacity: defaultPoolSize,
            maxSize: maxPoolSize
        );
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //will continuously set projectile emitters to active
        //we can create a list of emitters, set to false, and then itll enable / disable them on time
    }

    public GameObject CreatePooledBullet()
    {
        GameObject proj = Instantiate(bulletPrefab);
        proj.SetActive(false);
        return proj;
    }

    public void OnGetBullet(GameObject bullet) {
        bullet.SetActive(true);
    }
    public void OnReleaseBullet(GameObject bullet) {
        bullet.SetActive(false);
    }
    public void OnDestroyBullet(GameObject bullet) {
        Destroy(bullet);
    }
}
