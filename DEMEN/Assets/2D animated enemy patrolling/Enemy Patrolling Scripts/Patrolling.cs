using UnityEngine;

namespace EnemyPatrolling
{
    public class Patrolling : MonoBehaviour
    {
        // Array of waypoints the enemy will patrol between
        public Transform[] patrolPoints;

        // Speed at which the enemy moves between patrol points
        public float moveSpeed = 3.0f;

        // Rotation to apply to the enemy when it reaches a patrol point
        public Vector3 rotationAngles = new Vector3(0f, 180f, 0f);

        // Keeps track of the index of the current target patrol point
        private int currentPointIndex = 0;

        // The transform of the current patrol target
        private Transform currentTarget;

        // Indicates whether the enemy is dead; prevents movement and sounds if true
        private bool isDead = false;

        // Reference to the Animator component for controlling animations
        private Animator anim;

        // The sound that plays while patrolling
        [SerializeField] private AudioClip PatrolSound;

        // The audio source component used to play the patrol sound
        [SerializeField] private AudioSource audioSource;

        // Called once when the game starts
        private void Start()
        {
            // Ensure there are patrol points set up
            if (patrolPoints.Length > 0)
            {
                // Set the initial patrol target and position
                currentTarget = patrolPoints[currentPointIndex];
                transform.position = currentTarget.position;

                // Start playing the patrol sound every 4 seconds
                InvokeRepeating("PlayPatrolSound", 0f, 10f);

                // Get the Animator component attached to this GameObject
                anim = GetComponent<Animator>();
            }
        }

        // Plays a patrol sound if the enemy is alive
        private void PlayPatrolSound()
        {
            if (!isDead)
            {
                audioSource.PlayOneShot(PatrolSound);
            }
        }

        // Called at a fixed time interval, suitable for physics-based movement
        private void FixedUpdate()
        {
            // Only patrol if the enemy is alive and has patrol points
            if (!isDead && patrolPoints.Length > 0)
            {
                Patrol();
            }
        }

        // Handles moving the enemy towards the current patrol point
        private void Patrol()
        {
            // Calculate normalized direction vector towards the target
            Vector3 moveDirectionVector = (currentTarget.position - transform.position).normalized;

            // Move the enemy towards the target point at the defined speed
            transform.position += moveDirectionVector * moveSpeed * Time.deltaTime;

            // Check if the enemy is close enough to the patrol point
            if (Vector2.Distance(transform.position, currentTarget.position) < 0.2f)
            {
                // Rotate the enemy when it reaches a patrol point
                RotateAtPoint();

                // Move to the next point in the patrol array
                currentPointIndex++;

                // Loop back to the first patrol point if at the end
                if (currentPointIndex >= patrolPoints.Length)
                {
                    currentPointIndex = 0;
                }

                // Set the new patrol target
                currentTarget = patrolPoints[currentPointIndex];
            }
        }

        // Applies a rotation to the enemy when it reaches a patrol point
        private void RotateAtPoint()
        {
            transform.eulerAngles += rotationAngles;
        }
    }
}
