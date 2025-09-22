using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    UnityEvent DieEvent;
    [SerializeField] private float maxHealth = 100;
    private float health;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (DieEvent == null) DieEvent = new UnityEvent();
        DieEvent.AddListener(OnHealthReachZero);
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnHealthReachZero()
    {
        PlayDeathVFX();
    }
    void PlayDeathVFX()
    {
        //TODO placeholder
        StartCoroutine(ChangePlayerColor(Color.black, false, 0f));
    }

    public void TakeDamage(float dmg)
    {
        Debug.Log("took " + dmg + " damage");
        health -= dmg;
        PlayDamageVFX();

        if (health <= 0) DieEvent.Invoke();
    }
    void PlayDamageVFX()
    {
        //TODO this is palceholder! make this rly good in the future!!
        StartCoroutine(ChangePlayerColor(Color.red, true, 1f));
    }
    private IEnumerator ChangePlayerColor(Color c, bool shouldRevert, float lengthOfTime)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color ogColor = sr.color;
        sr.color = c;
        if (shouldRevert)
        {
            yield return new WaitForSeconds(lengthOfTime);
            sr.color = ogColor;
        }
        else
        {
            yield return new WaitForSeconds(0);
        }
    }
    
}
