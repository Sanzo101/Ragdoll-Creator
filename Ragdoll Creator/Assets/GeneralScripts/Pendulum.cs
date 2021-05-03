using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pendulum : MonoBehaviour
{
    public float angle = 90.0f;
    public float speed = 2.0f;
    Quaternion start, end;
    public float starttime = 0.0f;
    float SwingTime;


    // Start is called before the first frame update
    void Start()
    {
        start = PendulumRotation(angle);
        end = PendulumRotation(-angle);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        SFX();
        SwingTime = Mathf.Sin(starttime * speed + Mathf.PI / 2) + 1.0f;
        starttime += Time.deltaTime;
        transform.rotation = Quaternion.Lerp(start, end, SwingTime /2.0f);
    }
    void resetTimer()
    {
        starttime = 0.0f;
    }
    Quaternion PendulumRotation(float angle)
    {
        var pendulumRot = transform.rotation;
        var angleZ = pendulumRot.eulerAngles.z + angle;

        if(angleZ > 180)
        {
            angleZ -= 360;          
        }
        else if (angleZ < -180)
        {
            angleZ += 360;
        }
        pendulumRot.eulerAngles = new Vector3(pendulumRot.eulerAngles.x, pendulumRot.eulerAngles.y, angleZ);
        return pendulumRot;

    }
    void SFX()
    {
        if(SwingTime <=.6f)
        {
            GetComponentInChildren<SFX>().SwingingPendulum();
        }
        if(SwingTime >=1.5f)
        {
            GetComponentInChildren<SFX>().SwingingPendulum();
        }
    }
}
