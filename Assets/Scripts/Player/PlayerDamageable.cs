using System.Linq;
using UnityEngine;

public class PlayerDamageable : MonoBehaviour, IDamageable
{
    public AudioClip audioClip, deathAudioClip;
    [Range(0,1)] public float volumeScale;
    [SerializeField] private float health;
    public GameObject DestroyedBody;
    private Rigidbody[] ragdollRigibodies;
    [SerializeField] private HealthBarRennder healthBarRennder = new HealthBarRennder();
    private bool isDead;
    private SoundManager soundManager;
    private GameManager gameManager;
    private ObjectPooler objectPooler;
    private void Awake() {
        healthBarRennder.CreateHealthBar(transform, health);
        gameManager = GameManager.Instance;
        soundManager = SoundManager.Instance;
        objectPooler = ObjectPooler.Instance;

        gameManager.UpdatePlayerHealth(health);
    }

    private void LateUpdate() {
        healthBarRennder.UpdateHealthBarRotation();
    }

    public  void TakeDamge(Vector3 hitPoint , Vector3 force, float damage)
    {   
        soundManager.PlayOneShot(audioClip, volumeScale);
        Quaternion rot = Quaternion.LookRotation(-force);
        objectPooler.SpawnObject("HitEffect",hitPoint,rot);
        health -= damage;
        healthBarRennder.UpdateHealthBarValue(health);
        if(health <= 0 && !isDead) {
            isDead = true;
            soundManager.PlayOneShot(deathAudioClip,volumeScale);
            health = 0;
            GameObject weapon = gameObject.GetComponent<PlayerAttack>().weapon;

            //phá hủy gameobject hiện tại và thay thế bằng ragdoll
            gameObject.SetActive(false);
            GameObject deadBody = Instantiate(DestroyedBody, transform.position, transform.rotation);

            // thêm súng của nhân vật vào ragdoll
            Transform gunHolder =  deadBody.transform.Find("GunHolder").transform;
            GameObject w = Instantiate(weapon, gunHolder.position, gunHolder.rotation);
            w.transform.SetParent(gunHolder);
            // thêm rigi body vào súng để có hiệu ứng vật lý
            Rigidbody rigidbodyWeapon = w.AddComponent<Rigidbody>();

            //thêm lực vào bộ phận gần vị trí trúng nhất
            ragdollRigibodies = deadBody.GetComponentsInChildren<Rigidbody>();
            Rigidbody hitRigi = ragdollRigibodies.OrderBy(rb => Vector3.Distance(rb.position, hitPoint)).First();
            hitRigi.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);
            //thêm lực văng vào súng
            rigidbodyWeapon.AddForce(force.normalized * 5f, ForceMode.Impulse);
            gameManager.EndGame(false);
        }
        gameManager.UpdatePlayerHealth(health);
    }
}
