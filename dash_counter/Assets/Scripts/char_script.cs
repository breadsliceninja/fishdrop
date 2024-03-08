using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class char_script : MonoBehaviour
{
    public Rigidbody2D rb;
    private bool grounded = false;
    public float wall_slide_speed;
    private bool is_wall_sliding = false;

    public int jumps_total;
    private int jumps_used = 0;
    public float jump_strength;
    public float double_jump_strength;
    public float wall_jump_strength_x;
    public float wall_jump_strength_y;
    public float balloon_strength;

    public float dash_speed;
    public float dash_length;
    public int dashes_total;
    public float dash_delay;
    private int dashes_used = 0;
    private float dash_timer = 0;
    private int dash_dir; // -1, 1
    private bool is_dashing = false;

    public float vel_x_max;
    public float vel_y_max;
    public float acc_x;
    public float decc_x;
    public float friction_coeff;
    public float friction_min;
    private float stored_vel_y;


    // Start is called before the first frame update
    void Start()
    {
        // Don't Rotate
        rb.freezeRotation = true;

        // Bug where player is given an extra jump
        jumps_total--;
    }

    // Update is called once per frame
    void Update()
    {
        // Get velocity
        Vector3 vel = rb.velocity;

        // Update/Check Dash Counter
        if (is_dashing)
        {
            dash_timer -= Time.deltaTime * 100;
            if (dash_timer <= 0)
            {
                // Finish Dash - Restore Y Velocity
                is_dashing = false;
                vel.y = stored_vel_y;
            }
        }

        // Slide slowly
        if (is_wall_sliding && vel.y < 0)
        {
            vel.y = -wall_slide_speed;
        }

        if (!is_dashing)
        {
            // Not Dashing
            // Reduce Max Speed
            if (Mathf.Abs(rb.velocity.x) > vel_x_max)
            {
                vel.x -= decc_x * Mathf.Sign(vel.x);
            }
            if (Mathf.Abs(rb.velocity.y) > vel_y_max)
            {
                vel.y = vel_y_max * Mathf.Sign(vel.y);
            }

            // Get Input

            // Jump
            if (Input.GetKeyDown(KeyCode.J))
            {
                if (is_wall_sliding)
                {
                    // Wall Jump
                    vel.x = wall_jump_strength_x * dash_dir;
                    vel.y = wall_jump_strength_y;
                    jumps_used = 0;
                }
                
                else if (jumps_used < jumps_total)
                {
                    if (grounded)
                    {
                        // Normal Jump
                        vel.y = jump_strength;
                    }
                    else
                    {
                        // Double Jump
                        vel.y = double_jump_strength;
                    }                    
                    jumps_used++;

                }
                grounded = false;
            }
            // Dash
            if (Input.GetKeyDown(KeyCode.K) && dashes_used < dashes_total)
            {
                is_dashing = true;
                dash_timer = dash_length;
                dashes_used++;

                // Store Y Velocity (if positive)
                if (rb.velocity.y > 0)
                {
                    stored_vel_y = rb.velocity.y;
                }
                else
                {
                    stored_vel_y = 0;
                }
            }
            // Left/Right
            else
            {
                if (Input.GetKey(KeyCode.A))
                {
                    vel.x -= acc_x;
                    dash_dir = -1;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    vel.x += acc_x;
                    dash_dir = 1;
                }
            }
            // Friction
            if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)
                && grounded && Mathf.Abs(rb.velocity.x) > friction_min)
            {
                vel.x -= friction_coeff * dash_dir;
            }
        }
        // Continue Dash
        else
        {
            vel = Vector3.right * dash_speed * dash_dir;
        }
        // Set velocity
        //if (dash_delay)
        rb.velocity = vel;
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.layer == 6) // terrain
        {
            // Check collision with floor
            if (col.GetContact(0).normal == Vector2.up)
            {
                jumps_used = 0;
                dashes_used = 0;
                grounded = true;
            }
            // Check collision with wall
            if (col.GetContact(0).normal == Vector2.left && Input.GetKey(KeyCode.D))
            {
                is_wall_sliding = true;
                dash_dir = -1;
            }
            else if (col.GetContact(0).normal == Vector2.right && Input.GetKey(KeyCode.A))
            {
                is_wall_sliding = true;
                dash_dir = 1;
            }
            else
            {
                is_wall_sliding = false;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        Vector3 vel = rb.velocity;

        if (col.GetContact(0).normal == Vector2.left || col.GetContact(0).normal == Vector2.right)
        {
            // Wall hit
            if (is_dashing)
            {                
                vel.x = wall_jump_strength_x * -dash_dir;
                vel.y = wall_jump_strength_y;
            }
            if (dashes_used > 0)
            {
                dashes_used--;
            }
            is_dashing = false;
        }

        // Balloon
        if (col.gameObject.layer == 7)
        {
            vel.y = balloon_strength;
            Debug.Log("Balloon Hit");
        }

        rb.velocity = vel;
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.layer == 6) // terrain
        {
            is_wall_sliding = false;
            grounded = false;
        }
    }
}