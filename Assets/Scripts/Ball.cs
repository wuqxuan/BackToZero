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
        if (m_groundController.IsStartGame)
        {
            float moveHorizontal = Input.GetAxisRaw("Horizontal");
            if (m_groundController.IsInTopScene)
            {
                float moveVertical = Input.GetAxisRaw("Vertical");
                m_ballRigidbody.velocity = new Vector3(moveHorizontal * m_maxSpeed, moveVertical * m_maxSpeed, m_ballRigidbody.velocity.z);
            }
            else
            {
                float moveVertical = 0.0f;
                m_ballRigidbody.velocity = new Vector3(moveHorizontal * m_maxSpeed, moveVertical * m_maxSpeed, m_ballRigidbody.velocity.z);
            }
            m_groundController.BallTransform.position = new Vector3(m_groundController.BallTransform.position.x, m_groundController.BallTransform.position.y, m_groundController.BallTransform.position.z);
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
