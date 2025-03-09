using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private float powerUpSpeed = 4.5f;
    [SerializeField] private int pointsPerPellet = 100;
    [SerializeField] private int pointsPerGhost = 500;
    [SerializeField] private float powerupDuration = 11f;
    [SerializeField] private float minRayDistance = 0.6f;
    [SerializeField] private PlayerController otherPlayer;
    [SerializeField] private bool player1;
    [SerializeField] private Material speedMat;
    [SerializeField] private Renderer renderObj;
    private YellowFellowGame gameController;
    private Rigidbody rb;
    private Material origMat;
    private float powerUpTimer;
    private float speedPowerUpTimer;
    private float deadTimer;
    private int score;
    private int pelletsEaten;
    private int facing;
    private int turnDirSave;
    private Vector3 moveVector;
    private Vector3 origPos;
    private Vector3 startPos;
    private Vector3 origEuler;
    private Quaternion origRot;
    private bool stop;
    private InGameUI InGameUI;
    private float speedFlashTimer;
    
    
    void Awake()
    {
        origMat = renderObj.material;
        origEuler = transform.localEulerAngles;
        origRot = transform.rotation;
        rb = GetComponent<Rigidbody>();
        origPos = transform.position;
        startPos = transform.position;
        score = PlayerPrefs.GetInt("Score");
        gameController = GetComponentInParent<YellowFellowGame>();
        InGameUI = GetComponentInParent<InGameUI>();
        powerupDuration -= PlayerPrefs.GetInt("Level");
        if (powerupDuration <= 3f)
        {
            powerupDuration = 3f;
        }
    }
    
    void FixedUpdate()
    {
        rb.angularVelocity = Vector3.zero;
        CalculateFacing();
        InputManager();
        MoveForwardOne(turnDirSave);
        CheckFrontWall();
        FixRotate();
        SetMaterial();
        Timers();
    }

    private void Timers()
    {
        powerUpTimer = Mathf.Max(0f, powerUpTimer - Time.deltaTime);
        speedPowerUpTimer = Mathf.Max(0f, speedPowerUpTimer - Time.deltaTime);
        deadTimer = Mathf.Max(0f, deadTimer - Time.deltaTime);
        speedFlashTimer += Time.deltaTime;
    }
    private void InputManager()
    {
        if (player1)
        {
            if (Input.GetKey("up"))
            {
                turnDirSave = 0;
            }
            else if (Input.GetKey("right"))
            {
                turnDirSave = 1;
            }
            else if (Input.GetKey("down"))
            {
                turnDirSave = 2;
            }
            else if (Input.GetKey("left"))
            {
                turnDirSave = 3;
            }
            else
            {
                turnDirSave = 4;
            }
        }
        else
        {
            if (Input.GetKey("w"))
            {
                turnDirSave = 0;
            }
            else if (Input.GetKey("d"))
            {
                turnDirSave = 1;
            }
            else if (Input.GetKey("s"))
            {
                turnDirSave = 2;
            }
            else if (Input.GetKey("a"))
            {
                turnDirSave = 3;
            }
            else
            {
                turnDirSave = 4;
            }
        }
    }

    private void FixRotate()
    {
        float rotation = transform.rotation.eulerAngles.y;
        int tempFacing = Mathf.RoundToInt(rotation % 360 / 90);
        if (tempFacing == 0)
        {
            transform.localEulerAngles = new Vector3(0, 0, 0);
        }else if (tempFacing == 1)
        {
            transform.localEulerAngles = new Vector3(0, 90, 0);
        }else if (tempFacing == 2)
        {
            transform.localEulerAngles = new Vector3(0, 180, 0);
        }else if (tempFacing == 3)
        {
            transform.localEulerAngles = new Vector3(0, -90, 0);
        }

        Vector3 rot = transform.localEulerAngles;
        rot.x = 0f;
        rot.z = 0f;
        transform.localEulerAngles = rot;
    }

    private void CheckFrontWall()
    {
        int wallLayerMask = 1 << LayerMask.NameToLayer("Walls");
        RaycastHit frontRay;
        Physics.Raycast(transform.position, transform.forward, out frontRay, 100f, wallLayerMask);
        if (frontRay.distance <= 0.5)
        {
            Vector3 velocity = rb.velocity;
            if (facing == 0 || facing== 2)
            {
                velocity.z = 0;
            }else if (facing == 1|| facing ==3)
            {
                velocity.x = 0;
            }
            rb.velocity = velocity;
            stop = true;
        }
        else
        {
            stop = false;
        }
    }

    private void FindAvailableTurn(int turnDir)
    {
        Vector3 rayPos = transform.position;
        int wallAndGhostHouseLayerMask = (1 << LayerMask.NameToLayer("Walls")) | (1 << LayerMask.NameToLayer("GhostHouse"));
        RaycastHit rightRay;
        RaycastHit leftRay;
        Physics.Raycast(rayPos, transform.right, out rightRay, 100f, wallAndGhostHouseLayerMask);
        Physics.Raycast(rayPos, -transform.right, out leftRay, 100f, wallAndGhostHouseLayerMask);
        if (leftRay.distance >= minRayDistance && rightRay.distance >= minRayDistance)
        {
            MakeTurn(turnDir,0);
        }else if (rightRay.distance >= minRayDistance)
        {
            MakeTurn(turnDir,1);
        }else if (leftRay.distance >= minRayDistance)
        {
            MakeTurn(turnDir,2);
        }
    }

  private void MakeTurn(int turnDir, int availableTurns)
{
    //availableTurns: 0 = R and L, 1 = R, 2 = L
    // turnDir: 0 = Up, 1 = Right, 2 = Down, 3 = Left
    if (turnDir == 4) return;

    Vector3 rot = rb.transform.localEulerAngles;
    int degreesToRotate = 0;

    switch (facing)
    {
        case 0:
            if (availableTurns == 0)
            {
                degreesToRotate = turnDir == 1 ? 90 : turnDir == 3 ? -90 : 0;
            }
            else if (availableTurns == 1)
            {
                degreesToRotate = turnDir == 1 ? 90 : 0;
            }
            else if (availableTurns == 2)
            {
                degreesToRotate = turnDir == 3 ? -90 : 0;
            }
            break;

        case 1:
            if (availableTurns == 0)
            {
                degreesToRotate = turnDir == 2 ? 90 : turnDir == 0 ? -90 : 0;
            }
            else if (availableTurns == 1)
            {
                degreesToRotate = turnDir == 2 ? 90 : 0;
            }
            else if (availableTurns == 2)
            {
                degreesToRotate = turnDir == 0 ? -90 : 0;
            }
            break;

        case 2:
            if (availableTurns == 0)
            {
                degreesToRotate = turnDir == 1 ? -90 : turnDir == 3 ? 90 : 0;
            }
            else if (availableTurns == 1)
            {
                degreesToRotate = turnDir == 3 ? 90 : 0;
            }
            else if (availableTurns == 2)
            {
                degreesToRotate = turnDir == 1 ? -90 : 0;
            }
            break;

        case 3:
            if (availableTurns == 0)
            {
                degreesToRotate = turnDir == 2 ? -90 : turnDir == 0 ? 90 : 0;
            }
            else if (availableTurns == 1)
            {
                degreesToRotate = turnDir == 0 ? 90 : 0;
            }
            else if (availableTurns == 2)
            {
                degreesToRotate = turnDir == 2 ? -90 : 0;
            }
            break;
    }

    rot.y += degreesToRotate;
    rb.transform.localEulerAngles = rot;
}
    
    private void MoveForwardOne(int turnDir)
    {
        if (facing == 0)
        {
            if (transform.position.z >= startPos.z+1)
            {
               FindAvailableTurn(turnDir);
                startPos.z++;
            }else if (stop)
            {
                FindAvailableTurn(turnDir);
            }
        }else if (facing == 1)
        {
            if (transform.position.x >= startPos.x + 1)
            {
                FindAvailableTurn(turnDir);
                startPos.x++;
            }else if (stop)
            {
                FindAvailableTurn(turnDir);
            }
        }else if (facing == 2)
        {
            if (transform.position.z <= startPos.z - 1)
            {
                FindAvailableTurn(turnDir);
                startPos.z--;
            }else if (stop)
            {
                FindAvailableTurn(turnDir);
            }
        }else if (transform.position.x <= startPos.x - 1)
        {
            FindAvailableTurn(turnDir);
            startPos.x--;
        }else if (stop)
        {
            FindAvailableTurn(turnDir);
        }
        if (!stop)
        {
            if (speedPowerUpTimer > 0f)
            {
                rb.velocity = transform.forward * powerUpSpeed;
            }
            else
            {
                rb.velocity = transform.forward * speed;
            }
          
        }
    }
    private void CalculateFacing()
    {
        // facing: 0 = Up, 1 = Right, 2 = Down, 3 = Left
        float rotation = transform.rotation.eulerAngles.y;
        facing = Mathf.RoundToInt(rotation % 360 / 90);
    }
    private void SetMaterial()
    {
        if (speedPowerUpTimer <= 3f && speedPowerUpTimer > 0f)
        {
            FlashSpeedMat();
        }else if (speedPowerUpTimer > 0f)
        {
            renderObj.material = speedMat;
        }
        else
        {
            renderObj.material = origMat;
        }
    }
    private void FlashSpeedMat()
    {
        if (speedFlashTimer > 0.4f)
        {
            speedFlashTimer = 0;
        }else if (speedFlashTimer > 0.2f)
        {
            renderObj.material = speedMat;
        }
        else
        {
            renderObj.material = origMat;
        }
    }
    private void ResetPosition()
    {
        speedPowerUpTimer = 0f;
        powerUpTimer = 0f;
        deadTimer = 1f;
        transform.position = origPos;
        transform.localEulerAngles = origEuler;
        transform.rotation = origRot;
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;
        turnDirSave = 4;
        startPos = origPos;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pellet"))
        {
            pelletsEaten++;
            score += pointsPerPellet;
            if (player1)
            {
                InGameUI.scoreP1 = score;
            }
            else
            {
                InGameUI.scoreP2 = score;
            }
            InGameUI.UpdateScore();
        }
        else if (other.gameObject.CompareTag("Powerup"))
        {
            powerUpTimer = powerupDuration;
        }  else if (other.gameObject.CompareTag("SpeedPowerUp"))
        {
            speedPowerUpTimer = powerupDuration;
        }
        else if (other.gameObject.CompareTag("LTeleport"))
        {
            Vector3 LTele = new Vector3(22f, 0.4f, 7f);
            rb.transform.position = LTele;
            startPos = LTele;
        }
        else if (other.gameObject.CompareTag("RTeleport"))
        {
            Vector3 RTele = new Vector3(-7f, 0.4f, 7f);
            rb.transform.position = RTele;
            startPos = RTele;
        }
        else if (other.gameObject.CompareTag("TriggerGhost"))
        {
            if (PowerUpActive() || otherPlayer.PowerUpActive())
            {
                score += pointsPerGhost;
                if (player1)
                {
                    InGameUI.scoreP1 = score;
                }
                else
                {
                    InGameUI.scoreP2 = score;
                }
                InGameUI.UpdateScore();
            }
            else
            {
                if (PlayerPrefs.GetInt("lives") == 0)
                {
                    Death();
                }
                else
                {
                  ResetPosition();
                  PlayerPrefs.SetInt("lives", PlayerPrefs.GetInt("lives") - 1);
                  InGameUI.UpdateLives();
                  if (PlayerPrefs.GetInt("TwoPlayer") == 1)
                  {
                       otherPlayer.ResetPosition();
                  }
                }
            }
        }
    }

    public int GetPelletsEaten()
    {
        return pelletsEaten;
    }

    public int GetScore()
    {
        return score;
    }

    public bool PowerUpActive()
    {
        return powerUpTimer > 0f;
    }
    public bool PowerUpEnding()
    {
        return powerUpTimer <= 3f && powerUpTimer > 0f;
    }

    public bool IsDead()
    {
        return deadTimer > 0f;
    }

    private void Death()
    {
        InGameUI.SetScoreDeath();
        gameController.StartDeathInput();
        gameObject.SetActive(false);
    }
}