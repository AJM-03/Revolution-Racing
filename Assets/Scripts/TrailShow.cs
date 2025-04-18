using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailShow : MonoBehaviour
{
    public CarController carScript;
    public bool showOnGround;
    public bool showOffroad;
    public TrailRenderer renderer1;
    public TrailRenderer renderer2;


    void Update()
    {
        if (carScript != null)
        {
            bool grounded = carScript.grounded;
            bool respawning = carScript.respawning;
            float offroad = carScript.currentOffroadEffect;

            if (showOnGround && !showOffroad)
            {
                if (grounded && !respawning && offroad == 1)
                    EnableRenderers();

                else
                    DisableRenderers();
            }

            else if (showOnGround && showOffroad)
            {
                if (grounded && !respawning && offroad > 1)
                    EnableRenderers();

                else
                    DisableRenderers();
            }

            else if (!showOnGround)
            {
                if (grounded || respawning)
                    DisableRenderers();

                else
                    EnableRenderers();
            }
        }
    }

    private void EnableRenderers()
    {
        renderer1.emitting = true;
        renderer2.emitting = true;
    }

    private void DisableRenderers()
    {
        renderer1.emitting = false;
        renderer2.emitting = false;
    }
}
