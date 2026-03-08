using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;





public class TurretFire : MonoBehaviour
{
    public GameObject shell;
    private Vector2 direction;
    public List<Transform> firePoints = new List<Transform>();
    public Transform barrel;
    public float degreespeed;//�ڹܵ���ת�ٶ�
    public float attackCooldown = 1f;//���ù���Ƶ��
    public float range;
    public float firefiled;
    public bool isFullRotation = false; // �Ƿ�����360����ת
    private List<GameObject> Enemys = new List<GameObject> { };
    public List<string> enemyTags = new List<string> {};
    public Transform Aimline;
    private GameObject parentObject;
    private float attackTimer;//���ù���Ƶ��

    // Start is called before the first frame update



    // Update is called once per frame
    private void Start()
    {
        // �ڳ�����Ѱ�����о���ָ��Tag��NPC
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (enemyTags.Contains(obj.tag))
            {
                Enemys.Add(obj);
            }
        }
    }
    private void Awake()
    {
        parentObject = transform.parent.gameObject;


        // �����������������ƣ���ѡ��  

    }

    // �ݹ鷽����ȡָ�� GameObject �µ�����������  
    List<GameObject> GetAllChildrenOf(GameObject parent)
    {
        List<GameObject> children = new List<GameObject>();

        // ��������ֱ��������  
        foreach (Transform childTransform in parent.transform)
        {
            // ��ÿ�����������ӵ��б���  
            GameObject childGameObject = childTransform.gameObject;
            children.Add(childGameObject);
            children.Add(parentObject);
            // �ݹ���û�ȡ��������  
            children.AddRange(GetAllChildrenOf(childGameObject));
        }

        return children;
    }
    void FixedUpdate()
    {
        //  Debug.Log(parentObject);
        float distance = Vector2.Distance(GameObject.Find("Enemy").transform.position, transform.position);
        if (distance <= range)//����͵��˵ľ���������ڣ��Ϳ���
        {
            //Vector3 aaaaa = transform.InverseTransformPoint(GameObject.Find("Enemy").transform.position);
            Vector3 posOffset = GameObject.Find("Enemy").transform.position - transform.position;
            //    float cosValue = Vector2.Dot(transform.up,posOffset.normalized);
            //     float rad = Mathf.Acos(cosValue);
            //     float angle = rad * Mathf.Rad2Deg-180; // ������ת��Ϊ�Ƕ�
            //     float adjustedAngle = Mathf.DeltaAngle(0, angle); // ʹ�� Mathf.DeltaAngle ���Ƕ�ת���� -180 �� +180
            Vector3 directionToEnemy = (GameObject.Find("Enemy").transform.position - transform.position).normalized;
            float signedAngle = Vector3.SignedAngle(Vector3.up, directionToEnemy, Vector3.forward);


            // ����Ƿ�Ϊȫ��λ��ת��ǹ
            // Debug.Log(isFullRotation);
            if (isFullRotation)
            {
                //����360����ת


                RotateBarrel(signedAngle);

            }
            else
            {
                // ������ת��ǰ������90��
                if (signedAngle <= firefiled && signedAngle >= -firefiled)
                {
                    RotateBarrel(signedAngle);
                }

            }
            Attack();
        }

        void RotateBarrel(float targetRotation)
        {
            float curBarrelRotZ = barrel.eulerAngles.z;
            //   Debug.Log(curBarrelRotZ);
            if (curBarrelRotZ > 180f) curBarrelRotZ -= 360f; // ��֤�Ƕ���[-180, 180]��Χ��
                                                             // ����ת��Ŀ��Ƕ�
            curBarrelRotZ = Mathf.MoveTowardsAngle(curBarrelRotZ, targetRotation, degreespeed * Time.fixedDeltaTime);
            barrel.eulerAngles = new Vector3(0f, 0f, curBarrelRotZ); // ֻ��תZ��
        }


    }

    void Fire()
    {
        List<GameObject> allChildren = GetAllChildrenOf(parentObject);
        foreach (Transform firePoint in firePoints)
        {
            RaycastHit2D[] hit = Physics2D.RaycastAll(Aimline.transform.position, Aimline.up);


            foreach (RaycastHit2D hits in hit)
            {


                GameObject target = hits.collider.gameObject;
                foreach (GameObject child in allChildren)
                {

                
                    if (target == child)
                {
                    continue;
                }
                else
                {
                        foreach (GameObject Enemy in Enemys)
                        {
                            if (target == Enemy)
                            {
                                Debug.Log("����");
                                GameObject bullet = Instantiate(shell, firePoint.position, firePoint.rotation);
                                bullet bulletScript = shell.GetComponent<bullet>();
                                if (bulletScript != null)
                                {
                                    bulletScript.SetSpaceship(allChildren);
                                }
                            }
                        }
 
                    Debug.Log(target);
                    break;
                }

                }


            }
        }





    }
    void Attack()
    {
        attackTimer += Time.deltaTime;



        if (attackTimer >= attackCooldown)
        {
            Fire();
            attackTimer = 0f;
        }
    }

}

