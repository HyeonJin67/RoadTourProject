using UnityEngine;

namespace ML.PlaywallKids.DragonStage
{
    public class ScrollUV : MonoBehaviour
    {
        public float scrollSpeed_X = 0.5f;
        public float scrollSpeed_Y = 0.5f;
        void Update()
        {
            var offsetX = Time.time * scrollSpeed_X;
            var offsetY = Time.time * scrollSpeed_Y;
            GetComponent<Renderer>().material.mainTextureOffset = new Vector2(offsetX, offsetY);
        }
    }
}