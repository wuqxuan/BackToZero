using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class GroundController : MonoBehaviour
{
    //==============================================================================================
    // Fields
    private Ball m_ballScript;
    private Transform m_ballTransform;
    private Vector3 m_ballPosition;
    private bool m_isInTopScene = false;
    private bool m_isStartGame = true;
    private bool m_isLargerThanBallPosZ;
    private float m_currentOffsetPosY;
    private string m_groundName;
    private int m_scaleRatio = 1;
    [SerializeField]
    private List<Transform> m_groundTransforms = new List<Transform>();
    [SerializeField]
    private List<Ground> m_groundScripts = new List<Ground>();
    /// <summary> 初始Front视图中Floor的Position. </summary>
    private Dictionary<string, float> m_initialFrontPosZ = new Dictionary<string, float>();
    /// <summary> Front视图中Ground的Position.z </summary>
    private List<float> m_frontPositionZ = new List<float>();
    /// <summary> （旋转后）Top视图中Ground的Position.z </summary>
    private Dictionary<string, float> m_topPositionZ = new Dictionary<string, float>();
    /// <summary> Ground初始的localScale </summary>
    private List<Vector3> m_initialScale = new List<Vector3>();
    /// <summary> Ball在各个Ground上旋转到Top视图，Ground旋转后的Position.z </summary>
    private Dictionary<string, Dictionary<string, float>> m_rotatedPositionZ = new Dictionary<string, Dictionary<string, float>>();
    /// <summary> Ball中心到Ground中心的距离 </summary>
    private const float mc_ballRadius = 0.35f;
    //==============================================================================================
    // Property
    public bool IsInTopScene
    {
        get { return m_isInTopScene; }
    }
    public bool IsStartGame
    {
        get { return m_isStartGame; }
    }
    public Transform BallTransform
    {
        get { return m_ballTransform; }
    }

    //==============================================================================================
    // Methods
    void Awake()
    {
        GetGroundObject();
        GetGroundObjectData();
        m_ballScript = FindObjectOfType(typeof(Ball)) as Ball;
        m_ballTransform = GameObject.FindGameObjectWithTag("Ball").GetComponent<Transform>();
    }
    /// <summary> 获取Ground对象的localScale，Position，携带的脚本等数据 </summary>
    void GetGroundObjectData()
    {
        List<Vector3> ballPosition = new List<Vector3>();
        List<Vector3> initialFrontPosition = new List<Vector3>();
        for (int i = 0; i < m_groundTransforms.Count; i++)
        {
            m_groundScripts.Add(m_groundTransforms[i].GetComponent<Ground>());
            initialFrontPosition.Add(m_groundTransforms[i].position);
            ballPosition.Add(new Vector3(m_groundTransforms[i].position.x, m_groundTransforms[i].position.y + mc_ballRadius, m_groundTransforms[i].position.z));
            m_topPositionZ.Add(m_groundTransforms[i].gameObject.name, m_groundTransforms[i].position.z + mc_ballRadius);
            m_initialScale.Add(m_groundTransforms[i].localScale);
        }
        // 获得Ball在各个Floor上时，旋转后各个Floor的Position.z
        for (int i = 0; i < ballPosition.Count; i++)
        {
            m_frontPositionZ.Add(initialFrontPosition[i].z);
            m_initialFrontPosZ.Add(m_groundTransforms[i].gameObject.name, initialFrontPosition[i].z);

            Dictionary<string, float> rotatePositionZ = new Dictionary<string, float>();
            for (int j = 0; j < initialFrontPosition.Count; j++)
            {
                float rotatedGroundPosZ = GetRotatedPositionZ(initialFrontPosition[j], ballPosition[i], Vector3.right, -90.0f);
                rotatePositionZ.Add(m_groundTransforms[j].gameObject.name, rotatedGroundPosZ);
            }
            m_rotatedPositionZ.Add(m_groundTransforms[i].gameObject.name, rotatePositionZ);
        }
    }
    /// <summary> 获取Ground对象 </summary>
    void GetGroundObject()
    {
        // 使用GameObject.FindGameObjectsWithTag拿到的Ground可能顺序也可能逆序排列, 
        // 使用if－else确保m_groundTransforms中的元素排列顺序与Hierarchy中排列顺序一致（从上到下0->n）.
        GameObject[] groundObjects = GameObject.FindGameObjectsWithTag("Ground");
        if (groundObjects[0].name == "Ground01")
        {
            for (int i = 0; i < groundObjects.Length; i++)
            {
                m_groundTransforms.Add(groundObjects[i].transform);
            }
        }
        else if (groundObjects[groundObjects.Length - 1].name == "Ground01")
        {
            for (int i = 0; i < groundObjects.Length; i++)
            {
                m_groundTransforms.Add(groundObjects[groundObjects.Length - 1 - i].transform);
            }
        }
        else
        {
            Debug.LogError(" FindGameObjectsWithTag()得到的Ground存储顺序不符合要求！");
        }
    }

    // Update is called once per frame
    void Update()
    {
        m_ballPosition = m_ballTransform.position;
        m_groundName = GetNameTouchingBall();
        IsLargerThanBallPosZ();
        float currentScaleX = m_groundTransforms[0].localScale.x;
        float ratio = currentScaleX / m_initialScale[0].x;
        // Debug.Log(ratio + " : ratio");
        // Debug.Log(m_scaleRatio + " : m_scaleRatio");
        //==============================================================================================
        // Q, Z key
        if (Input.GetKeyDown(KeyCode.Q) && m_scaleRatio <= 2)
        {
            m_scaleRatio++;
            if (m_isInTopScene)
            {
                ScaleInTop(2f);
            }
            else
            {
                ScaleInFront(2f);
            }

        }

        if (Input.GetKeyDown(KeyCode.Z) && m_scaleRatio >= 1)
        {
            m_scaleRatio--;
            if (m_isInTopScene)
            {
                ScaleInTop(0.5f);
                // ScaleForFront(0.5f);
            }
            else
            {
                ScaleInFront(0.5f);
                // ScaleForFront(0.5f);
            }

        }
        //==============================================================================================
        // Space key
        // ToDo: 连续按Space键导致旋转bug
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 从Top视图旋转到Front视图
            if (m_isInTopScene)
            {
                // 若不是原始尺寸，先恢复
                // ToDo: top y 移动，放大 旋转front 旋转top位置不对
                BackToTopInitialScale(ratio);
                RotateToFrontScene(m_groundName, 0.1f);
                SetScaleForFront(1.2f);
            }
            // 从Front视图旋转到Top视图
            else
            {
                // 若不是原始尺寸，先恢复
                BackToFrontInitialScale(ratio);
                SetScaleForTop(0.1f);
                RotateToTopScene(m_groundName);
            }
        }
    }
    /// <summary> 开始运行游戏，自动旋转到Top视图 </summary>
    void LateUpdate()
    {
        if (m_isStartGame && m_ballScript.IsCollideWithObject)
        {
            m_isStartGame = false;
            RotateToTopScene(m_groundName);
        }
    }
    /// <summary> 旋转到Front视图 </summary>
    private void RotateToFrontScene(string objectName, float duration)
    {
        StartCoroutine(WaitForRotateToFrontScene(objectName, duration));
        // m_isInTopScene = false;
        // // Begin =======================================================================================
        // // 执行时间顺序: 1->2->3->4
        // // 1.更新Ball的Position.z，等于其后方的Ground旋转到Top视图的Position.z - mc_ballRadius.
        // Debug.Log(objectName + " :objectName");
        // m_ballTransform.position = new Vector3(m_ballTransform.position.x, m_ballTransform.position.y, m_rotatedPositionZ[objectName][objectName] - mc_ballRadius);
        // // 2.恢复Top视图Position.z
        // ResetTopPositionZ(m_rotatedPositionZ[objectName]);
        // // 3.转到Front视图
        // RotateAroundBall(90.0f, 1.0f, 0.1f);
        // // 4.设置Front视图Position.z相等
        // SetPositionZ(m_initialFrontPosZ[objectName], 1.1f);
        // // End =========================================================================================
        // SetGravity(true, 1.2f);
    }
    IEnumerator WaitForRotateToFrontScene(string objectName, float duration)
    {
        yield return new WaitForSeconds(duration);
        m_isInTopScene = false;
        // Begin =======================================================================================
        // 执行时间顺序: 1->2->3->4
        // 1.更新Ball的Position.z，等于其后方的Ground旋转到Top视图的Position.z - mc_ballRadius.
        Debug.Log(objectName + " :objectName");
        m_ballTransform.position = new Vector3(m_ballTransform.position.x, m_ballTransform.position.y, m_rotatedPositionZ[objectName][objectName] - mc_ballRadius);
        // 2.恢复Top视图Position.z
        ResetTopPositionZ(m_rotatedPositionZ[objectName]);
        // 3.转到Front视图
        RotateAroundBall(90.0f, 0.6f, 0.2f);
        // 4.设置Front视图Position.z相等
        SetPositionZ(m_initialFrontPosZ[objectName], 0.9f);
        // End =========================================================================================
        SetGravity(true, 1.2f);
    }
    /// <summary> 旋转到Top视图 </summary>
    private void RotateToTopScene(string objectName)
    {
        SetGravity(false, 0.0f);
        m_isInTopScene = true;
        // Begin =======================================================================================
        // 执行时间顺序: 1->2->3->4
        // 1.更新Ball的Position.z和其下方的Ground初始的Position.z一样
        m_ballTransform.position = new Vector3(m_ballTransform.position.x, m_ballTransform.position.y, m_initialFrontPosZ[objectName]);
        // 2.恢复Front视图Position.z
        ResetFrontPositionZ(m_frontPositionZ);
        // 3.旋转到Top视图: 延时0.1s是为了等待Ball和Ground的Position.z设置好（必须要有延时，否则旋转不正确）
        RotateAroundBall(-90.0f, 0.9f, 0.2f);
        // 4.设置Top视图Position.z相等
        SetPositionZ(m_topPositionZ[objectName], 1.1f);
        // End =========================================================================================
    }

    /// <summary> 若有Ground接触与Ball接触，得到该Ground的名字 </summary>
    string GetNameTouchingBall()
    {
        string groundName = null;
        if (m_ballScript.IsCollideWithObject)
        {
            groundName = m_ballScript.CollideObjectName;
        }
        return groundName;
    }

    //==============================================================================================
    /// <summary> Front视图缩放 </summary>
    public void ScaleInFront(float scaleRatio)
    {
        for (int i = 0; i < m_groundTransforms.Count; i++)
        {
            Vector3 m_currentCubePos = m_groundTransforms[i].position;
            Vector3 m_distanceToBall = m_currentCubePos - m_ballPosition;
            if (m_groundTransforms[i].gameObject.name == m_ballScript.CollideObjectName)
            {
                // 和Ball接触，改变Position.x，不改变Position.y
                m_groundTransforms[i].position = new Vector3(m_ballPosition.x + m_distanceToBall.x * scaleRatio, m_groundTransforms[i].position.y, m_groundTransforms[i].position.z);
            }
            else
            {
                // 不和Ball接触，改变Position.x和Position.y
                m_groundTransforms[i].position = new Vector3(m_ballPosition.x + m_distanceToBall.x * scaleRatio, m_ballPosition.y + m_distanceToBall.y * scaleRatio, m_groundTransforms[i].position.z);
            }
            // 全部缩放Scale.x
            m_groundTransforms[i].localScale = new Vector3(m_groundTransforms[i].localScale.x * scaleRatio, m_groundTransforms[i].localScale.y, m_groundTransforms[i].localScale.z);
            // m_groundTransforms[i].localScale = new Vector3(m_groundTransforms[i].localScale.x * scaleRatio, m_groundTransforms[i].localScale.y, m_groundTransforms[i].localScale.z);
        }
    }
    /// <summary> Top视图缩放 </summary>
    void ScaleInTop(float scaleRatio)
    {
        for (int i = 0; i < m_groundTransforms.Count; i++)
        {
            Vector3 m_currentCubePos = m_groundTransforms[i].position;
            Vector3 m_distanceToBall = m_currentCubePos - m_ballPosition;
            // 改变Position.x, Position.y
            m_groundTransforms[i].position = new Vector3(m_ballPosition.x + m_distanceToBall.x * scaleRatio, m_ballPosition.y + m_distanceToBall.y * scaleRatio, m_groundTransforms[i].position.z);
            // 缩放Scale.x，Scale.y
            m_groundTransforms[i].localScale = new Vector3(m_groundTransforms[i].localScale.x * scaleRatio, m_groundTransforms[i].localScale.y * scaleRatio, m_groundTransforms[i].localScale.z);
        }
    }
    /// <summary> Front视图恢复到初始Scale </summary>
    void BackToFrontInitialScale(float scaleRatio)
    {
        int ratio = (int)scaleRatio;
        switch (ratio)
        {
            case 0:
                ScaleInFront(2.0f);
                break;
            case 2:
                ScaleInFront(0.5f);
                break;
            case 4:
                ScaleInFront(0.25f);
                break;
            default:
                // 缩放1x，不做处理.
                // Debug.Log("缩放1x，不做处理");
                break;
        }
        m_scaleRatio = 1;
    }
    /// <summary> Top视图恢复到初始Scale </summary>
    void BackToTopInitialScale(float scaleRatio)
    {
        int ratio = (int)scaleRatio;
        switch (ratio)
        {
            case 0:
                ScaleInTop(2.0f);
                break;
            case 2:
                ScaleInTop(0.5f);
                break;
            case 4:
                ScaleInTop(0.25f);
                break;
            default:
                // 缩放1x，不做处理.
                // Debug.Log("缩放1x，不做处理");
                break;
        }
        m_scaleRatio = 1;
    }
    /// <summary> 为旋转到Top视图准备, 恢复Scale.y </summary>
    void SetScaleForTop(float duration)
    {
        StartCoroutine(WaitForSetScaleForTop(duration));
    }
    IEnumerator WaitForSetScaleForTop(float duration)
    {
        yield return new WaitForSeconds(duration);
        for (int i = 0; i < m_groundTransforms.Count; i++)
        {
            float ratioY = m_initialScale[i].y;
            SetGroundScale(m_groundTransforms[i], ratioY);
        }
    }
    /// <summary> 为旋转到Front视图准备, 缩放Scale.y </summary>
    void SetScaleForFront(float duration)
    {
        StartCoroutine(WaitForSetScaleForFront(duration));
    }
    IEnumerator WaitForSetScaleForFront(float duration)
    {
        yield return new WaitForSeconds(duration);
        for (int i = 0; i < m_groundTransforms.Count; i++)
        {
            float ratioY = 1 / m_initialScale[i].y;
            SetGroundScale(m_groundTransforms[i], ratioY);
        }
    }
    void SetGroundScale(Transform ground, float ratio)
    {
        float ratioY = ratio;
        Vector3 m_currentCubePos = ground.position;
        Vector3 m_distanceToBall = m_currentCubePos - m_ballPosition;
        // 改变位置Position.y
        ground.position = new Vector3(ground.position.x, m_ballPosition.y + m_distanceToBall.y, ground.position.z);
        // 缩放Scale.y到单位宽度
        ground.localScale = new Vector3(ground.localScale.x, ground.localScale.y * ratioY, ground.localScale.z);
    }
    //==============================================================================================
    /// <summary> 旋转Ground </summary>
    public void RotateAroundBall(float newAngle, float duration, float delay)
    {
        StartCoroutine(WaitForRotateAroundBall(newAngle, duration, delay));
    }
    IEnumerator WaitForRotateAroundBall(float newAngle, float duration, float delay)
    {
        yield return new WaitForSeconds(delay);
        for (int i = 0; i < m_groundTransforms.Count; i++)
        {
            m_groundScripts[i].RotateAroundWithDuration(m_ballPosition, Vector3.right, newAngle, duration);
        }
    }
    /// <summary> 设置Ground的Position.z </summary>
    void SetPositionZ(float posZ, float duration)
    {
        StartCoroutine(WaitForSetPositionZ(posZ, duration));
    }
    IEnumerator WaitForSetPositionZ(float posZ, float duration)
    {
        yield return new WaitForSeconds(duration);
        for (int i = 0; i < m_groundTransforms.Count; i++)
        {
            m_groundTransforms[i].position = new Vector3(m_groundTransforms[i].position.x, m_groundTransforms[i].position.y, posZ);
        }
    }
    /// <summary> 恢复Top视图Position.z </summary>
    void ResetTopPositionZ(Dictionary<string, float> initialTopPosZ)
    {
        for (int i = 0; i < m_groundTransforms.Count; i++)
        {
            m_groundTransforms[i].position = new Vector3(m_groundTransforms[i].position.x, m_groundTransforms[i].position.y, initialTopPosZ[m_groundTransforms[i].name]);
        }
    }
    /// <summary> 恢复Front视图Position.z </summary>
    void ResetFrontPositionZ(List<float> initialFrontPosZ)
    {
        for (int i = 0; i < m_groundTransforms.Count; i++)
        {
            if (m_isLargerThanBallPosZ)
            {
                m_groundTransforms[i].position = new Vector3(m_groundTransforms[i].position.x, m_groundTransforms[i].position.y, initialFrontPosZ[i] + m_currentOffsetPosY);
            }
            else
            {
                m_groundTransforms[i].position = new Vector3(m_groundTransforms[i].position.x, m_groundTransforms[i].position.y, initialFrontPosZ[i] - m_currentOffsetPosY);
            }
        }
    }
    /// <summary> 比较Top视图中Ground与Ball Position.y的差值 </summary>
    private void IsLargerThanBallPosZ()
    {
        if (m_isInTopScene)
        {
            for (int i = 0; i < m_groundTransforms.Count; i++)
            {
                if (m_groundTransforms[i].gameObject.name == m_ballScript.CollideObjectName)
                {
                    m_isLargerThanBallPosZ = (m_ballPosition.y < m_groundTransforms[i].position.y);
                    // Top视图cube与Ball Position.y的差值
                    m_currentOffsetPosY = Mathf.Abs(m_groundTransforms[i].position.y - m_ballPosition.y);
                }
            }
        }
    }
    /// <summary> 激活/关闭重力 </summary>
    void SetGravity(bool isUseGravity, float duration)
    {
        StartCoroutine(WaitForSetGravity(isUseGravity, duration));
    }

    IEnumerator WaitForSetGravity(bool isUseGravity, float duration)
    {
        yield return new WaitForSeconds(duration);
        m_ballScript.BallRigidbody.useGravity = isUseGravity;
    }
    /// <summary> 获得Ground旋转到Top视图的Position.z </summary>
    private float GetRotatedPositionZ(Vector3 startPosition, Vector3 rotateAroundPoint, Vector3 rotateAroundAxis, float rotateDegree)
    {
        // 绕rotateAroundAxis轴旋转rotateDegree
        Quaternion q = Quaternion.AngleAxis(rotateDegree, rotateAroundAxis);
        Vector3 endPosition = q * (startPosition - rotateAroundPoint) + rotateAroundPoint;
        // Debug.Log(endPosition + " :endPosition");
        return endPosition.z;
    }
}
