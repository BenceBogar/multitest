using Cinemachine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;

namespace MultiTest
{
    public class GamePlayer : NetworkBehaviour
    {
        public NetworkVariableVector3 Position = new NetworkVariableVector3(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.ServerOnly,
            ReadPermission = NetworkVariablePermission.Everyone
        });
        private CharacterController controller;
        private Vector3 playerVelocity;
        private bool groundedPlayer;
        private float playerSpeed = 15.0f;
        private float jumpHeight = 10.0f;
        private float gravityValue = -9.81f;
        public float turnSmoothTime = 0.01f;
        public float turnSmoothVelocity = 0.01f;
        public Transform cam;
        private float distToGround;
        public override void NetworkStart()
        {
            controller = gameObject.AddComponent<CharacterController>();
            Move();
            if (IsOwner)
            {
                cam = GameObject.Find("MainCamera").gameObject.transform;
                cam.gameObject.transform.parent = this.transform;
                Vector3 vector3 = this.transform.position;
                vector3.y += 4;
                vector3.z -= 7;
                cam.gameObject.transform.position = vector3;
            }
            distToGround = GetComponent<Collider>().bounds.extents.y;
        }
        void Update()
        {
            if (!IsOwner) {return; }
            if(playerVelocity.y < -10) { playerVelocity.y = -9; }
            //CinemachineFreeLook camera = gameObject.transform.GetChild(1).gameObject.GetComponent<CinemachineFreeLook>();
            //camera.Follow = this.gameObject.transform;
            //camera.LookAt = this.gameObject.transform;
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

            if (direction.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                controller.Move(moveDir.normalized * playerSpeed * Time.deltaTime);
            }

            // Changes the height position of the player..
            if (Input.GetButtonDown("Jump") && IsGrounded())
            {
                playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            }

            playerVelocity.y += gravityValue * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);
        }
        public void Move()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                var randomPosition = GetRandomPositionOnPlane();
                transform.position = randomPosition;
                Position.Value = randomPosition;
            }
            else
            {
                SubmitPositionRequestServerRpc();
            }
        }

        [ServerRpc]
        void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
        {
            Position.Value = GetRandomPositionOnPlane();
        }

        static Vector3 GetRandomPositionOnPlane()
        {
            return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        }
        private bool IsGrounded()
        {
            return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
        }

    }
}