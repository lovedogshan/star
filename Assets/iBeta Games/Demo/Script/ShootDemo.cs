/* The script was written by iBeta Games
 * 
 */
using UnityEngine;

namespace iBetaGames
{
    public class ShootDemo : MonoBehaviour
    {
        public Transform point;
        public GameObject[] prefabs;
        public float speed;


        public void Shoot()
        {
            BulletController bulletController = Instantiate(prefabs[Random.Range(0, 4)], point.position, Quaternion.identity).GetComponent<BulletController>();
            bulletController.speed = speed;
        }


    }
}