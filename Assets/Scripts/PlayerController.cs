using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
    [SerializeField] Image healthbarImage;

    [SerializeField] Image backHealthbar;
    private float lerpTimer;
    public float chipSpeed = 2f;

    [SerializeField] GameObject ui;

    [SerializeField] GameObject cameraHoler;
    [SerializeField] float mouseSensitivity, jumpForce, smoothTime, sprintSpeed, walkSpeed;

    [SerializeField] Item[] items;

    int itemIndex;
    int previousItemIndex = -1;

    float verticalLookRotaton;
    bool canMove = true;
    bool grounded;

    Vector3 smoothMoveVelocity;
    Vector3 moveAmount;

    Rigidbody rb;

    PhotonView PV;

    const float maxHealth = 100f;
    float currentHealth = maxHealth;
    public TextMeshProUGUI healthText;

    PlayerManager playerManager;

    // Start is called before the first frame update
    void Awake()
    {

        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();

        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
    }
     
    void Start()
    {

        if (PV.IsMine)
        {
            EquidItem(0);
        }
        else
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
            Destroy(ui);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!PV.IsMine)
            return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            canMove = !canMove;
            Cursor.lockState = canMove ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !canMove;
        }

        if (canMove)
        {
            Look();
        }

        if (Input.GetMouseButton(0))
        {
            canMove = true;
        }

        Move();
        Jump();

        for(int i = 0; i < items.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                EquidItem(i); 
                break;
            }
        }

        /*if(Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            if (itemIndex >= items.Length - 1)
            {
                EquidItem(0);
            }
            else
            {
                EquidItem(itemIndex + 1);
            }
            
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            if (itemIndex <= 0)
            {
                EquidItem(items.Length - 1);
            }
            else
            {
                EquidItem(itemIndex - 1);
            }
        }*/

        if (Input.GetKeyDown(KeyCode.Q))
        {
            itemIndex++;
            if (itemIndex >= items.Length)
            {
                EquidItem(0);
            }
            else
            {
                EquidItem(itemIndex);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            items[itemIndex].Use();
        }

        if(transform.position.y < -10f)
        {
            Die();
        }

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
        healthText.text = currentHealth.ToString();
        if (Input.GetKey(KeyCode.F))
        {
            TakeDamage(Random.Range(5, 10));
        }

    }

    void Move()
    {
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);

    }

    void Jump()
    {
        if (Input.GetKey(KeyCode.Space) && grounded)
        {
            rb.AddForce(transform.up * jumpForce);
        }
    }

    void Look()
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);

        verticalLookRotaton += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        verticalLookRotaton = Mathf.Clamp(verticalLookRotaton, -90, 90);

        cameraHoler.transform.localEulerAngles = Vector3.left * verticalLookRotaton;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    void EquidItem(int _index)
    {
        if (_index == previousItemIndex)
            return;

        itemIndex = _index;

        items[itemIndex].itemGameObject.SetActive(true);

        if (previousItemIndex != -1)
        {
            items[previousItemIndex].itemGameObject.SetActive(false);
        }

        previousItemIndex = itemIndex;

        if (PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if(!PV.IsMine && targetPlayer == PV.Owner)
        {
            EquidItem((int)changedProps["itemIndex"]);
        }
    }

    public void SetGroundedState(bool _grounded)
    {
        grounded = _grounded;
    }

    public void UpdateHealthUI()
    {
        float fillF = healthbarImage.fillAmount;
        float fillB = backHealthbar.fillAmount;
        float hFraction = currentHealth / maxHealth;
        if(fillB > hFraction)
        {
            healthbarImage.fillAmount = hFraction;
            backHealthbar.color = Color.red;
            lerpTimer += Time.deltaTime;
            float persentComplete = lerpTimer / chipSpeed;
            backHealthbar.fillAmount = Mathf.Lerp(fillB, hFraction, persentComplete);
        }
    }


    void FixedUpdate()
    {
        if (!PV.IsMine)
            return;

        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);    
    }

    
    public void TakeDamage(float damage)
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage)
    {
        if (!PV.IsMine)
            return;

        currentHealth -= damage;
        lerpTimer = 0f;

        //healthbarImage.fillAmount = currentHealth / maxHealth;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        playerManager.Die();
    }

}
