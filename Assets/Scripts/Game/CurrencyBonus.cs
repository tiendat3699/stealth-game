using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CurrencyBonus : MonoBehaviour
{
    public LayerMask layer;
    [SerializeField] private int point;
    public AudioClip audioClip;
    public float volumeScale = 1;
    private Rigidbody rb;
    private GameManager gameManager;
    private SoundManager soundManager;

    private void Awake() {
        gameManager = GameManager.Instance;
        soundManager = SoundManager.Instance;
        rb = GetComponent<Rigidbody>();
    }
    // Start is called before the first frame update
    void Start()
    {
        Vector3 dir = Random.insideUnitSphere.normalized;
        rb.AddForce(dir * 8f, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other) {
        if((layer & (1 << other.gameObject.layer)) != 0) {
            Destroy(gameObject);
        } 
    }

    private void OnDestroy() {
        soundManager.PlayOneShot(audioClip, volumeScale);
        gameManager.UpdateCurrency(point);
    }
}
