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

    private float scalingFactor = 1.4f;

    public float stepInterval = 0.3f;   // seconds between moves
    private float stepTimer = 0f;

    public float step_size = 0.5f;

    private void revert_direction()
    {
        this.currentDirection = (this.currentDirection == Direction.Left) ? Direction.Right : Direction.Left;

    }

    private void step_down()
    {
        this.transform.position += Vector3.down * 0.5f;
    }

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

    void Update()
    {
        stepTimer += Time.deltaTime;

        if (stepTimer >= stepInterval)
        {
            stepTimer = 0f; // reset timer

            // do your step movement here
            if (currentDirection == Direction.Right)
            {
                this.transform.position += Vector3.right * this.step_size;
            }
            else
            {
                this.transform.position += Vector3.left * this.step_size;
            }
        }


    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Boundary"))
        {
            print(this.currentDirection);
            revert_direction();
            print(this.currentDirection);
            step_down();
        }
    }

}
