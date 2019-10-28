using UnityEngine;

namespace zs
{
    public static class Extensions
    {
        /// <summary>
        /// An alternative to Rigidbody2D.MovePosition() which works with
        /// manual physic updates.
        /// </summary>
        public static void CustomMove(this Rigidbody2D rigidbody, Vector2 position)
        {
            rigidbody.velocity = (position - rigidbody.position) / Time.deltaTime;
        }
    }
}
