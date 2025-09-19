using UnityEngine;

namespace SEEEJam.Player
{
    [CreateAssetMenu(menuName = "Configs/Player Config", fileName = "PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Movement")]
        [Min(0f)]
        [Tooltip("Tốc độ di chuyển cơ bản của người chơi.")]
        public float moveSpeed = 5f;

        [Header("Dash")]
        [Min(0f)]
        [Tooltip("Thời gian giữa các lần lướt.")]
        public float dashCooldown = 1.2f;

        [Min(0f)]
        [Tooltip("Độ dài quãng đường lướt.")]
        public float dashDistance = 3f;

        [Min(0.01f)]
        [Tooltip("Thời gian thực hiện một lần lướt.")]
        public float dashDuration = 0.2f;

        [Header("Combat")]
        [Tooltip("Máu tối đa của người chơi.")]
        public int maxHp = 3;

        [Min(0f)]
        [Tooltip("Thời gian hồi sau mỗi lần bắn.")]
        public float shootCooldown = 0.25f;
    }
}
