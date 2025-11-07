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
    public List<Pair> emitters = new List<Pair>();
    private int emitterIndex = 0;
    //private int bpm = 384;
    private float timePerBeat = 0.5f;
    private float bulletHellStartTime;

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
        bulletHellStartTime = Time.time; //TODO: i am just setting this to the time the bullet hell manager is activated
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //TODO: set bpm to the same value as rhythm game manager, update time per beat accordingly
    }

    // Update is called once per frame
    void Update()
    {
        //emitters will be set active on a certain beat 
        //ex: 240 bpm, 3 min song = 720 beats and each emitter is released on one of those beats
        //will continuously set projectile emitters to active
        Pair p = emitters[emitterIndex];
        if (Time.time >= bulletHellStartTime + p.beatToActivate * timePerBeat)
        {
            p.emitter.SetActive(true);
            emitterIndex++;
            if (emitterIndex == emitters.Count)
            {
                OnBulletHellComplete();
            }
        }

    }
    void OnBulletHellComplete()
    {
        gameObject.SetActive(false);
        //TODO something (placeholder^^)
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
