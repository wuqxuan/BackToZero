using UnityEngine;
using System.Collections;

public class CameraFollowPlayer : MonoBehaviour
{
    //==============================================================================================
    // Fields
    private GroundController m_groundController;
    private Camera m_camera;

    //==============================================================================================
    // Methods
    void Awake()
    {
        m_groundController = this.gameObject.GetComponent<GroundController>();
        m_camera = FindObjectOfType(typeof(Camera)) as Camera;
    }
    void LateUpdate()
    {
        if (m_groundController.IsStartGame)
        {
            // 相机跟随
            m_camera.transform.position = new Vector3(m_groundController.BallTransform.position.x, m_groundController.BallTransform.position.y, -10.0f); 
        }
       
    }
}

