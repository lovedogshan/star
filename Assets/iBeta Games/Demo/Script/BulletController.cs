/* The script was written by iBeta Games
 * 
 */
using UnityEngine;
namespace iBetaGames
{
    public class BulletController : MonoBehaviour
    {
        public float speed;

        private void Start()
        {
            Invoke("Destroy", 3f);
        }


        void Update()
        {
            transform.Translate(Vector2.up * speed * Time.deltaTime);
        }

        void Destroy()
        {
            Destroy(gameObject);
        }
    }
}