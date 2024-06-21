using UnityEngine;

namespace DeathMatch
{
    public class Block : MonoBehaviour
    {
        private Rigidbody rb;
        public Rigidbody Rigidbody { get { return rb ?? GetComponent<Rigidbody>(); } }

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }
    }
}