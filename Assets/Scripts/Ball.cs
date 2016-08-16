using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class Ball : MonoBehaviour
{
    //==============================================================================================
    // Fields
    private Rigidbody m_ballRigidbody;
    private GroundController m_groundController;
    private float m_maxSpeed = 2.0f;
    private bool m_isCollideWithObject;
    private string m_collideObjectName;
    public Text m_gameOver;
    //==============================================================================================
    // Property
    public bool IsCollideWithObject
    {
        get { return m_isCollideWithObject; }
    }
    public Rigidbody BallRigidbody
    {
        get { return m_ballRigidbody; }
    }
    public string CollideObjectName
    {
        get { return m_collideObjectName; }
    }
    //==============================================================================================
    // Methods
    void Awake()
    {
        m_ballRigidbody = gameObject.GetComponent<Rigidbody>();
        m_groundController = FindObjectOfType(typeof(GroundController)) as GroundController;
    }

    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");
        if (!m_groundController.IsStartGame)
        {
            moveHorizontal = 0.0f;
            moveVertical = 0.0f;
        }
        if (m_groundController.IsInTopScene)
        {
            m_ballRigidbody.velocity = new Vector3(moveHorizontal * m_maxSpeed, moveVertical * m_maxSpeed, m_ballRigidbody.velocity.z);
        }
        else
        {
            moveVertical = 0.0f;
            m_ballRigidbody.velocity = new Vector3(moveHorizontal * m_maxSpeed, moveVertical * m_maxSpeed, m_ballRigidbody.velocity.z);
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            m_isCollideWithObject = true;
            m_collideObjectName = other.gameObject.name;
        }
        // Debug.Log("Ball: 开始接触 " + m_collideObjectName + "," + "m_isCollideWithObject = " + m_isCollideWithObject);
    }

    void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            m_isCollideWithObject = true;
            m_collideObjectName = other.gameObject.name;
        }
        // Debug.Log("Ball: 持续接触 " + m_collideObjectName + "," + "m_isCollideWithObject = " + m_isCollideWithObject);
    }

    void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            m_isCollideWithObject = false;
            // Debug.Log("Ball: 不再接触 " + m_collideObjectName + "," + "m_isCollideWithObject = " + m_isCollideWithObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Goal"))
        {
            other.gameObject.SetActive(false);
            m_groundController.IsStartGame = false;
            m_gameOver.DOFade(1.0f, 3.0f);
        }
    }
}
