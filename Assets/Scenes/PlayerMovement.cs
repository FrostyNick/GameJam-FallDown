using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI; 
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    public TextMeshProUGUI livesLabel;
    private float horizontal;

    EnemyMovement enemyAttribute;

    [SerializeField] public GameObject lifeCheck;
    [SerializeField] private float speed = 8f;
    [SerializeField] private float jumpingPower = 18f;
    private bool isFacingRight = true;
    public bool restrictMovement = false;
    [SerializeField] public int Lives;
    [SerializeField] public int Slimes = 0;
    Animator myAnimator;
    SpriteRenderer m_SpriteRenderer;
    BoxCollider2D  myBoxCollider;
    [SerializeField] Tilemap tilemap;
    [SerializeField] GameObject player;
    [SerializeField] bool inGoo;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float attackRange = 1f; // The range of the melee attack
    [SerializeField] private int attackDamage = 10; // The damage dealt by the attack
    [SerializeField] private Transform attackPoint; // The point from where the attack originates
    [SerializeField] private LayerMask enemyLayers; // The layers considered as enemies
    [SerializeField] AudioClip runningSound;
    [SerializeField] AudioClip hitSound;
    [SerializeField] float footSteps;
    [SerializeField] bool inTutorial = false;

    public GameObject setCheckPoint = null;
    public bool triggerForceSpawn = false;
    private bool isGrounded;
    private bool isRunning;
    private AudioSource audioSource;
    private AudioSource hitSource;

    void Start()
     {
        enemyAttribute = GetComponent<EnemyMovement>();
        myAnimator = GetComponent<Animator>();
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
        myBoxCollider = GetComponent<BoxCollider2D>();
        if (!livesLabel)
        {
            GameObject labelObject = GameObject.Find("StaticCanvas/LivesTextTMP");
            if (labelObject)
            {
                livesLabel = labelObject.GetComponent<TextMeshProUGUI>();
                livesLabel.text = "Lives: " + Lives.ToString();
            }
            else
            {
                Debug.LogError("LivesLabel GameObject not found!");
            }
        }
        // Tilemap tilemap = GetComponent<Tilemap>();
        audioSource = GetComponent<AudioSource>();
        hitSource = GetComponent<AudioSource>();


        if (restrictMovement == false)
        {
            lifeCheck.SetActive(true);
        }
    }

    // int aaa = 0;
    void Update()
    {
        // Debug.Log(myAnimator.GetBool("isDying")+ " aaa "+aaa++);
        horizontal = Input.GetAxisRaw("Horizontal");

        if (!restrictMovement && Input.GetButtonDown("Jump") && IsGrounded() && !inGoo)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
        }

        if (!restrictMovement && Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        if (rb.velocity.y != 0 && !IsGrounded())
        {
            myAnimator.SetBool("isJumping", true);
        }
        else
        {
            myAnimator.SetBool("isJumping", false);
        }

        if (Input.GetKeyDown("s"))
        {
            Debug.Log("Destroying goo");
            DestroyGooTile(groundCheck.position);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            hitSource.clip = hitSound;
            audioSource.loop = true;
            hitSource.Play();

            //PlayHitSound(hitSound);
            myAnimator.SetTrigger("isHitting");
            PerformMeleeAttack();

        }
        Flip();

        
    }

    private void FixedUpdate()
    {
        if(!restrictMovement){
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
        }
        // Check if the player is grounded
        isGrounded = IsGrounded();

        bool playerHasMovementSpeed = Mathf.Abs(rb.velocity.x) > Mathf.Epsilon;
        if (isGrounded && playerHasMovementSpeed)
        {
            if (!isRunning) 
            {
                RunSound(runningSound);
                audioSource.loop = true;
                isRunning = true;
            }
        }
        else
        {
            isRunning = false; 
            //audioSource.loop = false;
            audioSource.Stop();
        }

        myAnimator.SetBool("isWalking", playerHasMovementSpeed);
    }

    public void PlayHitSound(AudioClip hitSound)
    {
        if (hitSound != null && audioSource != null)
        {
            Debug.Log("yursound");
            audioSource.PlayOneShot(hitSound); // Play hit sound without interrupting the running sound
        }
        else
        {
            Debug.LogWarning("Hit sound or AudioSource is null! Please assign the AudioClip.");
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private void RunSound(AudioClip clip)
    {
        if (!restrictMovement && clip != null && audioSource != null) 
        {
           audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioClip or AudioSource is null! Please assign the AudioClip.");
        }
    }

    private void DestroyGooTile(Vector3 worldPosition)
    {
        Vector3Int gooTilePosition = tilemap.WorldToCell(worldPosition);
        tilemap.SetTile(gooTilePosition, null);
        Debug.Log("Tile destroyed at: " + gooTilePosition);

        if (inGoo)
        {
            Debug.Log(Slimes);
            Slimes++;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("MinecartBoss"))
        {
            StartCoroutine(Damaged());
            //Destroy(collision.gameObject);
            HealthCheck();
        }

        if (collision.gameObject.CompareTag("Hazards"))
        {
            Lives = 0;
            HealthCheck();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Goo"))
        {
            inGoo = true;
            speed = 2f;
        }
        else
        {
            speed = 8f;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        inGoo = false;
        speed = 8f;
    }

    void HealthCheck()
    {
        if (Lives <= 0)
        {
            // if(!myAnimator.GetBool("isDying")){
            //     myAnimator.SetBool("isDying", true);
            // }
            //livesLabel.text = "You've died!";
            StartCoroutine(DeathAnimation());
        } 
        else
        {
            //livesLabel.text = "Lives: " + Lives.ToString();
        }
    }

    
    IEnumerator  DeathAnimation()
    {
        Debug.Log("dead");
        rb.bodyType = RigidbodyType2D.Static;
        myBoxCollider.enabled = false;
        myAnimator.SetBool("isDying", true);
        yield return new WaitForSeconds(1f);
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        if(setCheckPoint)
        {
            this.transform.position = setCheckPoint.transform.position;
            myAnimator.SetBool("isDying", false);
            rb.bodyType = RigidbodyType2D.Dynamic;
            myBoxCollider.enabled = true;
            Lives = 5;
            //livesLabel.text = "Lives: " + Lives.ToString();
        }
        else if(inTutorial)
        {
            myAnimator.SetBool("isDying", false);
            triggerForceSpawn = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
            myBoxCollider.enabled = true;
            Lives = 5;
            //livesLabel.text = "Lives: " + Lives.ToString();
        }
        if(!inTutorial && !setCheckPoint)
        {
            Debug.Log("HELLO?");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    IEnumerator Damaged()
    {
        m_SpriteRenderer.color = Color.red;
        Lives--;
        yield return new WaitForSeconds(.1f);
        m_SpriteRenderer.color = Color.white;
    }

    private void PerformMeleeAttack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("Hit " + enemy.name);
            EnemyMovement enemyAttribute = enemy.GetComponent<EnemyMovement>();

            StartCoroutine(enemyAttribute.Stunned());
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
