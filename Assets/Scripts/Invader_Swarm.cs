using UnityEngine;


// TODO: FIX Stepsize and step interval
// TODO: Add shooting
public class Invaders : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int rows = 5;
    public int columns = 11;

    public enum Direction { Left, Right };
    private Direction currentDirection = Direction.Right;

    public Invader[] prefabs;

    public float scalingFactor = 1.4f; // sprite scaling factor

    public float stepInterval = 1f;   // seconds between moves

    public float step_size_horizontal = 1f;
    public float step_size_vertical = 0.3f;

    private float stepTimer = 0f;
    private bool boundaryHitThisStep = false;
    private void revert_direction()
    {
        currentDirection = (currentDirection == Direction.Left) ? Direction.Right : Direction.Left;

    }
    /// <summary>
    /// Moves the Invaders Grid down one step.
    /// </summary>
    private void step_down()
    {
        this.transform.position += Vector3.down * step_size_vertical;
    }

    /// <summary>
    /// Updates the BoxCollider2D attached to the swarm so that it always
    /// matches the collective bounds of all active child Invaders.
    /// </summary>
    /// <remarks>
    /// - If there are no child objects, the method exits early.
    /// - It calculates world-space bounds that encapsulate all active
    ///   child positions, then converts that to local space.
    /// - Finally, it applies the calculated center and size to the
    ///   BoxCollider2D component on this GameObject.
    /// </remarks>
    private void UpdateColliderBounds()
    {
        if (transform.childCount == 0) return;

        Bounds bounds = new Bounds(transform.GetChild(0).position, Vector3.zero);

        foreach (Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy)
                bounds.Encapsulate(child.position);
        }

        // convert world bounds into local space
        Vector3 localCenter = transform.InverseTransformPoint(bounds.center);
        BoxCollider2D swarmCollider = GetComponent<BoxCollider2D>();
        swarmCollider.offset = localCenter;
        swarmCollider.size = bounds.size;
    }



    private void Awake()
    {
        for (int row = 0; row < this.rows; row++)
        {
            for (int col = 0; col < this.columns; col++)
            {
                Invader invader = Instantiate(this.prefabs[row], this.transform);

                invader.transform.localPosition = new Vector3(
                    scalingFactor * (col - (this.columns - 1) / 2.0f),
                    scalingFactor * (row - (this.rows - 1) / 2.0f),
                    0
                );
                invader.transform.localScale = Vector3.one * this.scalingFactor;

            }
        }
        this.UpdateColliderBounds();
    }

    /// <summary>
    /// Moves the Invaders Grid right or left depending on the current direction.
    /// </summary>
    private void move()
    {
        if (currentDirection == Direction.Right)
        {
            this.transform.position += Vector3.right * this.step_size_horizontal;
        }
        else
        {
            this.transform.position += Vector3.left * this.step_size_horizontal;
        }
    }


    void Update()
    {
        stepTimer += Time.deltaTime;

        if (stepTimer >= stepInterval)
        {
            stepTimer = 0f; // reset timer

            // if boundary hit revert_direction, move down and reset
            if (boundaryHitThisStep)
            {
                revert_direction();
                step_down();
                boundaryHitThisStep = false;
            }



            move();
        }


    }

    private void OnTriggerEnter2D(Collider2D other)
    {


        if (other.CompareTag("Boundary") && !boundaryHitThisStep)
        {
            boundaryHitThisStep = true;
        }

    }

}
