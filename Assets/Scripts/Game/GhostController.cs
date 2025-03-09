using UnityEngine;
using Random = UnityEngine.Random;

public class GhostController : MonoBehaviour
{
    public enum GhostColour
    {
        Red,
        Teal,
        Pink,
        Orange
    };
    public GhostColour ghostColour;
    [SerializeField] private float moveSpeed =2f;
    [SerializeField] private float randomModeLegnth;
    [SerializeField] private float chaseModeLegnth;
    [SerializeField] private float turnDelay = 0.2f;
    [SerializeField] private float minRayDistance = 0.6f;
    [SerializeField] private float distanceToSeeTeleport = 8f;
    [SerializeField] private PlayerController player;
    [SerializeField] private PlayerController player2;
    [SerializeField] private Material scaredMat;
    [SerializeField] private Material deadMat;
    [SerializeField] private GameObject ghostHouse;
    [SerializeField] private GameObject ghostHouseExitL;
    [SerializeField] private GameObject ghostHouseExitR;
    [SerializeField] private GameObject teleportR;
    [SerializeField] private GameObject teleportL;
    [SerializeField] private GameObject triggerGhost;
    [SerializeField] private Renderer headRenderer;
    [SerializeField] private Renderer bodyRenderer;
    private GameObject[] pellets;
    private Rigidbody rb;
    private Material normalMat;
    private Vector3 origPos;
    private Vector3 startPos;
    private Vector3 directionToTarget;
    private float startSpeed;
    private float origSpeed;
    private float flashMatTimer;
    private float directionTimer;
    private float switchRandomTimer;
    private float switchChaseTimer;
    private int switchMode;
    private int facing;
    private int ghostHouseExitSide;
    private bool ChaseLock;
    private bool RandomLock;
    private bool dead;
    private bool inGhostHouse = true;
    private bool moveTowards;
    private bool twoPlayers;
    private bool startLock;
    private bool Player1Close = true;

    void Start()
    {
        normalMat = headRenderer.GetComponent<Renderer>().material;
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        origPos = transform.position;
        ghostHouseExitSide = Random.Range(0, 2);
        startSpeed = moveSpeed;
        origSpeed = moveSpeed;
        pellets = GameObject.FindGameObjectsWithTag("Pellet");
    }
    private void FixedUpdate()
    {
        if (player.IsDead() || player2.IsDead())
        {
            ResetGhosts();
        }
        else
        {
            CalculateFacing();
            if (inGhostHouse)
            {
                LeaveGhostHouse();
            }
            else
            {
                if (!startLock)
                {
                    if (PlayerPrefs.GetInt("TwoPlayer") == 1)
                    {
                        twoPlayers = true;
                        distanceToSeeTeleport *= 0.5f;
                    }
                    startLock = true;
                }
                if (twoPlayers)
                {
                    FindClosestPlayer();
                }
                if (ghostColour == GhostColour.Red)
                {
                    RedController();
                }
                //Main Movement
                ModeChanger();
                CheckNearTeleport();
                MoveForwardOne(switchMode, directionToTarget, moveTowards);
            }
            SetRenderMat();
            directionTimer += Time.deltaTime;
            flashMatTimer += Time.deltaTime;
        }
    }



    private void FindClosestPlayer()
    {
        float Player1Dis = Vector3.Distance(player.transform.position, player.transform.position);
        float Player2Dis = Vector3.Distance(player2.transform.position, player.transform.position);
        if (Player1Dis > Player2Dis)
        {
            Player1Close = true;
        }
        else
        {
            Player1Close = false;
        }
    }
    

    private void ChooseTargetByColour()
    {
        if (ghostColour == GhostColour.Red)
        {
            if (Player1Close)
            {
                directionToTarget = player.transform.position - transform.position;
            }
            else
            {
                directionToTarget = player2.transform.position - transform.position;
            }
        }else if (ghostColour == GhostColour.Teal)
        {
            if (Player1Close)
            {
                directionToTarget = (player.transform.position + (player.transform.forward*4)) - transform.position;
            }
            else
            {
                directionToTarget = (player2.transform.position + (player2.transform.forward*4))- transform.position;
            }
        }else if (ghostColour == GhostColour.Pink)
        {
            if (Player1Close)
            {
                directionToTarget =  (player.transform.position + (player.transform.forward*8)) - transform.position;
            }
            else
            {
                directionToTarget =(player2.transform.position + (player2.transform.forward*8)) - transform.position;
            }
        }
    }
    
    private void ModeChanger()
    {
        if (dead)
        {   
            moveSpeed = origSpeed;
            switchMode = 1;
            directionToTarget = ghostHouse.transform.position - transform.position;
            moveTowards = true;
            return;
        }
        if (player.PowerUpActive() || player2.PowerUpActive())
        {
            if (Player1Close)
            {
                directionToTarget = player.transform.position - transform.position;
            }
            else
            {
                directionToTarget = player2.transform.position - transform.position;
            }
            moveSpeed = origSpeed - 1;
            switchMode = 1;
            moveTowards = false;
            return;
        }
        ChooseTargetByColour();
        moveTowards = true;
        moveSpeed = startSpeed;
        if (switchChaseTimer > chaseModeLegnth)
        {
            if (!ChaseLock)
            {
                switchRandomTimer = 0;
                ChaseLock = true;
                RandomLock = false;
            }
            switchMode = 0;
        }

        if (switchRandomTimer > randomModeLegnth)
        {
            if (!RandomLock)
            {
                switchChaseTimer = 0;
                RandomLock = true;
                ChaseLock = false;
            }
            switchMode = 1;
        }
        if (ghostColour == GhostColour.Orange)
        {
            OrangeSeePlayer();
        }
        switchRandomTimer += Time.deltaTime;
        switchChaseTimer += Time.deltaTime;
    }
    
    private void RedController()
    {
        int pelletsEaten = player.GetPelletsEaten() + player2.GetPelletsEaten();
        if ((float)pelletsEaten/pellets.Length >= 0.66f)
        { 
            startSpeed = origSpeed + 0.5f;
        }else if ((float)pelletsEaten / pellets.Length >= 0.33f)
        {
            startSpeed = origSpeed + 0.25f;
        }
    }
    private void OrangeSeePlayer()
    {
        switchMode = 0;
        RaycastHit toPlayer1;
        RaycastHit toPlayer2;
        float distanceToPlayer1 = Vector3.Distance(transform.position, player.transform.position);
        float distanceToPlayer2 = Vector3.Distance(transform.position, player2.transform.position);
        if (Physics.Raycast(transform.position, (player.transform.position -transform.position).normalized, out toPlayer1))
        {
            if (toPlayer1.collider.gameObject.CompareTag("Player") || distanceToPlayer1 <=4f)
            {
                directionToTarget = player.transform.position - transform.position;
                switchMode = 1;
            }
        }
        if (Physics.Raycast(transform.position, (player2.transform.position-transform.position).normalized, out toPlayer2))
        {
            if (toPlayer2.collider.gameObject.CompareTag("Player") || distanceToPlayer2 <=4f)
            {
                directionToTarget = player2.transform.position - transform.position;
                switchMode = 1;
            }
        }
    }

    
    private void CheckNearTeleport()
    {
        if (player.PowerUpActive() || player2.PowerUpActive() || dead || switchMode == 0)
        {
            return;
        }
        Vector3 directionToTeleport;
        if (IsPlayerNearTeleport(teleportR) && IsGhostNearTeleport(teleportL))
        {
            directionToTeleport = teleportL.transform.position - transform.position;
        }
        else if (IsPlayerNearTeleport(teleportL) && IsGhostNearTeleport(teleportR))
        {
            directionToTeleport = teleportR.transform.position - transform.position;
        }else if (IsPlayer2NearTeleport(teleportR) && IsGhostNearTeleport(teleportL))
        {
            directionToTeleport = teleportL.transform.position - transform.position;
        } else if (IsPlayer2NearTeleport(teleportL) && IsGhostNearTeleport(teleportR))
        {
            directionToTeleport = teleportR.transform.position - transform.position;
        }
        else
        {
            return;
        }
        directionToTarget = directionToTeleport;
    }

    private bool IsPlayerNearTeleport(GameObject teleport)
    {
        return Vector3.Distance(teleport.transform.position, player.transform.position) < distanceToSeeTeleport;
    }
    private bool IsPlayer2NearTeleport(GameObject teleport)
    {
        return Vector3.Distance(teleport.transform.position, player2.transform.position) < distanceToSeeTeleport;
    }

    private bool IsGhostNearTeleport(GameObject teleport)
    {
        return Vector3.Distance(teleport.transform.position, transform.position) < distanceToSeeTeleport;
    }

    private void MoveForwardOne(int mode,Vector3 targetDirection, bool towards)
    {
        if (facing == 0)
        {
            if (transform.position.z >= startPos.z+1)
            {
                if (mode == 0)
                {
                    TurnInDirection(ChooseRandomDirection());
                }else if (mode == 1)
                {
                    TurnInDirection(ChooseDirectionToTarget(targetDirection, towards));
                }
                startPos.z++;
            }
        }else if (facing == 1)
        {
            if (transform.position.x >= startPos.x + 1)
            {
                if (mode == 0)
                {
                    TurnInDirection(ChooseRandomDirection());
                }else if (mode == 1)
                {
                    TurnInDirection(ChooseDirectionToTarget(targetDirection, towards));
                }
                startPos.x++;
            }
        }else if (facing == 2)
        {
            if (transform.position.z <= startPos.z - 1)
            {
                if (mode == 0)
                {
                    TurnInDirection(ChooseRandomDirection());
                }else if (mode == 1)
                {
                    TurnInDirection(ChooseDirectionToTarget(targetDirection, towards));
                }
                startPos.z--;
            }
        }else if (transform.position.x <= startPos.x - 1)
        {
            if (mode == 0)
            {
                TurnInDirection(ChooseRandomDirection());
            }else if (mode == 1)
            {
                TurnInDirection(ChooseDirectionToTarget(targetDirection, towards));
            }
            startPos.x--;
        }
        rb.velocity = transform.forward * moveSpeed;
    }

    private int ChooseRandomDirection()
    {
        Vector3 rayPos = transform.position;
        int wallLayerMask = 0;
        if (!dead && !inGhostHouse)
        {
            wallLayerMask = (1 << LayerMask.NameToLayer("Walls")) | (1 << LayerMask.NameToLayer("GhostHouse"));
        }
        else
        {
            wallLayerMask = 1 << LayerMask.NameToLayer("Walls");
        }
        RaycastHit frontRay;
        RaycastHit rightRay;
        RaycastHit leftRay;
        Physics.Raycast(rayPos, transform.forward, out frontRay, 100f, wallLayerMask);
        Physics.Raycast(rayPos, transform.right, out rightRay, 100f, wallLayerMask);
        Physics.Raycast(rayPos, -transform.right, out leftRay, 100f, wallLayerMask);
        if(frontRay.distance >= minRayDistance && rightRay.distance>= minRayDistance&& leftRay.distance>= minRayDistance)
        {
            return Random.Range(0, 3);
        }if (leftRay.distance >= minRayDistance && rightRay.distance >= minRayDistance)
        {
            return Random.Range(1, 3);
        }if (rightRay.distance>= minRayDistance && frontRay.distance >= minRayDistance )
        {
            return Random.Range(0, 2);
        }if (leftRay.distance >= minRayDistance && frontRay.distance >= minRayDistance)
        {
            return Random.Range(0, 2) * 2;
        }if (rightRay.distance >= minRayDistance)
        {
            return 1;
        }if (leftRay.distance >= minRayDistance)
        {
            return 2;
        }
        return 0;
    }
    private int ChooseDirectionToTarget(Vector3 targetDirection, bool towards)
    {
        Vector3 rayPos = transform.position;
        int wallLayerMask = 0;
        if (!dead && !inGhostHouse)
        {
            wallLayerMask = (1 << LayerMask.NameToLayer("Walls")) | (1 << LayerMask.NameToLayer("GhostHouse"));
        }
        else
        {
            wallLayerMask = 1 << LayerMask.NameToLayer("Walls");
        }
        RaycastHit frontRay;
        RaycastHit rightRay;
        RaycastHit leftRay;
        Physics.Raycast(rayPos, transform.forward, out frontRay, 100f, wallLayerMask);
        Physics.Raycast(rayPos, transform.right, out rightRay, 100f, wallLayerMask);
        Physics.Raycast(rayPos, -transform.right, out leftRay, 100f, wallLayerMask);
        if(frontRay.distance >= minRayDistance && rightRay.distance>= minRayDistance&& leftRay.distance>= minRayDistance)
        {
            return ChooseDirectionByTargetAngle(1, targetDirection, towards);
        }if (leftRay.distance >= minRayDistance && rightRay.distance >= minRayDistance)
        {
            return ChooseDirectionByTargetAngle(0, targetDirection, towards);
        }if (rightRay.distance>= minRayDistance && frontRay.distance >= minRayDistance )
        {
            return ChooseDirectionByTargetAngle(2, targetDirection,towards);
        }if (leftRay.distance >= minRayDistance && frontRay.distance >= minRayDistance)
        {
            return ChooseDirectionByTargetAngle(3, targetDirection,towards);
        }if (rightRay.distance >= minRayDistance)
        {
            return 1;
        } if (leftRay.distance >= minRayDistance)
        {
            return 2;
        }
        return 0;
    }


    private void TurnInDirection(int turn)
    {
        if (directionTimer > turnDelay)
        {
            Vector3 rot = rb.transform.localEulerAngles;
            if (turn == 1)
            {
                rot.y += 90;
            }
            else if (turn == 2)
            {
                rot.y -= 90;
            }
            rb.transform.localEulerAngles = rot;
            directionTimer = 0;
        }
    }

    private void CalculateFacing()
    {
        float rotation = transform.rotation.eulerAngles.y;
        facing = Mathf.RoundToInt(rotation % 360 / 90);
    }

    private int ChooseDirectionByTargetAngle(int choices, Vector3 targetDirection, bool towards)
    {
        // Choices: 0=L and R: 1 = F, L and R: 2 = F and R: 3 = F and L
        //Return 0 for forwards, 1 for right and 2 for left
        float angle = Vector3.SignedAngle(transform.forward, targetDirection, Vector3.up);
        if (choices == 0)
        {
            if (angle > 0)
            {
                return towards ? 1 : 2;
            }
            return towards ? 2 : 1;
            
        }if (choices == 1)
        {
            if (towards)
            {
                if (angle > 20)
                {
                    return 1;
                }

                if (angle < -20)
                {
                    return 2;
                }
                return 0;
            }
            if (angle > 135 || angle < -135)
            {
                return 0;
            } if (angle < 135 && angle > 0)
            {
                return 2;
            }
            return 1;
            
        }if (choices == 2)
        {
            if (angle > 10)
            {
                return towards ? 1 : 0;
            }
            return towards ? 0 : 1;
            
        }if (choices == 3)
        {
            if (angle < -10)
            {
                return towards ? 2 : 0;
            }
            return towards ? 0 : 2;
        }
        return -1;
    }
    private void LeaveGhostHouse()
    {
        if (ghostHouseExitSide == 0)
        {
            if (transform.position.x <= ghostHouseExitL.transform.position.x)
            {
                inGhostHouse = false;
            }
            else
            {
                MoveForwardOne(1, ghostHouseExitL.transform.position - transform.position, true);
            }
        }
        else
        {
            if (transform.position.x >= ghostHouseExitR.transform.position.x)
            {
                inGhostHouse = false;
            }
            else
            {
                MoveForwardOne(1, ghostHouseExitR.transform.position - transform.position, true);
            }
        }
    }
    
    
    private void ResetGhosts()
    {
        inGhostHouse = true;
        switchMode = 0;
        switchRandomTimer = 0;
        switchChaseTimer = 0;
        dead = false;
        transform.position = origPos;
        startPos = origPos;
        transform.rotation = Quaternion.Euler(0,0,0);
        rb.velocity = new Vector3(0, 0, 0);
        gameObject.layer = LayerMask.NameToLayer("Ghost");
        triggerGhost.layer = LayerMask.NameToLayer("TriggerGhost");
        SetRenderMat();
    }
    private void SetRenderMat()
    {
        if (dead)
        {
            headRenderer.material = deadMat;
            bodyRenderer.material = deadMat;
        }else if (player.PowerUpEnding() || player2.PowerUpEnding()) {
            FlashScaredColour();
        }else if (player.PowerUpActive() || player2.PowerUpActive())
        {
            headRenderer.material = scaredMat;
            bodyRenderer.material = scaredMat;
        }
        else
        {
            headRenderer.material = normalMat;
            bodyRenderer.material = normalMat;
        }
    }

    private void FlashScaredColour()
    {
        if (flashMatTimer > 0.4f)
        {
            flashMatTimer = 0;
        }else if (flashMatTimer > 0.2f)
        {
            headRenderer.material = scaredMat;
            bodyRenderer.material = scaredMat;
        }
        else
        {
            headRenderer.material = normalMat;
            bodyRenderer.material = normalMat;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("LTeleport"))
        {
            Vector3 telePointR = new Vector3(14.75f, -0.7307688f, 1.25f);
            transform.localPosition = telePointR;
            startPos = transform.position;
        }else if (other.gameObject.CompareTag("RTeleport"))
        {
            Vector3 telePointL = new Vector3(-14.25f, -0.7307688f, 1.25f);
            transform.localPosition = telePointL;
            startPos = transform.position;
        }else if (other.gameObject.CompareTag("GhostHouse"))
        {
            if (dead)
            {
                dead = false;
                gameObject.layer = LayerMask.NameToLayer("Ghost");
                triggerGhost.layer = LayerMask.NameToLayer("TriggerGhost");
            }
            inGhostHouse = true;
        }else if (other.gameObject.CompareTag("Player"))
        {
            if (player.PowerUpActive() || player2.PowerUpActive())
            {
                dead = true;
                gameObject.layer = LayerMask.NameToLayer("DeadGhost");
                triggerGhost.layer = LayerMask.NameToLayer("DeadGhost");
            }
        }
    }
}
