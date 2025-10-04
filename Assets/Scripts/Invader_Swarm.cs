using UnityEngine;


// TODO: FIX Stepsize and step interval
// TODO: Add shooting
public class Invaders : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private int rows = 5;
    [SerializeField] private int columns = 11;

    public enum Direction { Left, Right };
    private Direction currentDirection = Direction.Right;

    [SerializeField] private Invader[] prefabs;

    [SerializeField] private float scalingFactor = 1.4f; // sprite scaling factor


    [SerializeField] private float stepSizeHorizontal = 1f;
    [SerializeField] private float stepSizeVertical = 0.3f;


    [SerializeField] private float maxStepInterval = 1.0f; // max seconds between moves.
    [SerializeField] private float minStepInterval = 0.1f; // min seconds between moves.

    private float stepInterval;   // seconds between moves.

    private bool[,] aliveGrid; // true if alive, false if dead

    private float stepTimer = 0f;
    private bool boundaryHitThisStep = false;

    private int invadersAlive;
    private int invadersTotal => rows * columns;
    private void RevertDirection()
    {
        currentDirection = (currentDirection == Direction.Left) ? Direction.Right : Direction.Left;

    }
    /// <summary>
    /// Moves the Invaders Grid down one step.
    /// </summary>
    private void StepDown()
    {
        this.transform.position += Vector3.down * stepSizeVertical;
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
        // Initialize stepInterval min Speed.
        stepInterval = Mathf.Lerp(maxStepInterval, minStepInterval, 0);

        aliveGrid = new bool[columns, rows];

        // Create the grid of invaders
        for (int row = 0; row < rows; row++)
        {

            for (int col = 0; col < columns; col++)
            {

                // Instantiate an invader from the appropriate prefab for this row
                Invader invader = Instantiate(prefabs[row], this.transform);
                invader.Initialize(col, row, rows);
                invadersAlive++;


                // Position the invader in a grid formation
                invader.transform.localPosition = new Vector3(
                    scalingFactor * (col - (this.columns - 1) / 2.0f),
                    scalingFactor * (row - (this.rows - 1) / 2.0f),
                    0
                );



                // Scale the invader sprite
                invader.transform.localScale = Vector3.one * this.scalingFactor;
                // Subscribe to the OnInvaderKilled event
                invader.OnInvaderKilled += HandleInvaderKilled;

                // Set grid coordinates and mark alive
                aliveGrid[col, row] = true;

            }
        }

        this.UpdateColliderBounds();
    }

    /// <summary>
    /// Moves the Invaders Grid right or left depending on the current direction.
    /// </summary>
    private void Move()
    {
        if (currentDirection == Direction.Right)
        {
            this.transform.position += Vector3.right * this.stepSizeHorizontal;
        }
        else
        {
            this.transform.position += Vector3.left * this.stepSizeHorizontal;
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
                RevertDirection();
                StepDown();
                boundaryHitThisStep = false;
            }



            Move();
        }


    }

    private void OnTriggerEnter2D(Collider2D other)
    {


        if (other.CompareTag("Boundary") && !boundaryHitThisStep)
        {
            boundaryHitThisStep = true;
        }

    }


    private void UpdateStepSpeed()
    {
        float t = 1f - (invadersAlive / (float)invadersTotal); // 0..1 based on progress
        stepInterval = Mathf.Lerp(maxStepInterval, minStepInterval, t);


    }
    private void HandleInvaderKilled(Invader killed)
    {
        if (killed != null)
            killed.OnInvaderKilled -= HandleInvaderKilled;

        invadersAlive--;
        UpdateColliderBounds();
        UpdateStepSpeed();
        aliveGrid[killed.getCoordinateX(), killed.getCoordinateY()] = false;

    }

}
