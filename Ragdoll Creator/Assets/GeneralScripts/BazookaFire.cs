using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BazookaFire : MonoBehaviour
{
    [SerializeField] public Rigidbody cannonBallPrefab;
    [SerializeField] public Transform spawnPoint;
    [SerializeField] private float launchForce = 100, timeInterval = 3;
    [SerializeField] private int maxCannonBalls = 5;
    [SerializeField]
    private int cannonBallSolverIterations = 8,
                                 cannonBallVelSolverIterations = 8;

    private float timer;
    private Queue<Rigidbody> cannonBalls;

    // Start is called before the first frame update
    void Start()
    {
        cannonBalls = new Queue<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        if (timer < timeInterval) timer += Time.deltaTime;
        else
        {
            var cannonBall = SpawnCannonBall();
            cannonBall.velocity = cannonBall.angularVelocity = Vector3.zero;
            cannonBall.MovePosition(spawnPoint.position);
            cannonBall.MoveRotation(spawnPoint.rotation);
            cannonBall.AddForce(spawnPoint.forward * launchForce, ForceMode.Impulse);
            timer = 0;
        }
    }
    private Rigidbody SpawnCannonBall()
    {
        if (cannonBalls.Count < maxCannonBalls)
        {
            var cannonBall = Instantiate(cannonBallPrefab);
            cannonBall.solverIterations = cannonBallSolverIterations;
            cannonBall.solverVelocityIterations = cannonBallVelSolverIterations;
            cannonBalls.Enqueue(cannonBall);
            return cannonBall;
        }
        else
        {
            var cannonBall = cannonBalls.Dequeue();
            cannonBalls.Enqueue(cannonBall);
            return cannonBall;
        }
    }
}

