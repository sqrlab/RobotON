using System.Linq;
using System.Net.Sockets;
using System.Diagnostics.SymbolStore;
using System.Xml.Schema;
using UnityEngine.EventSystems; 
using UnityEngine;
using UnityEngine.UI; 
using System;
using System.Collections;

// BOOKMARK -END 5/31/2016
public class hero2Controller : MonoBehaviour
{

    public bool onWall = false;
    public bool dropping = false;
    public bool throwing = false;
    public bool isMoving = false;
    public float maxSpeed = 10f;
    public float climbSpeed = 10f;
    public float dropDelay = 0.0f;
    public float fMoveVelocityVertical = 0.0f;
    public int projectilecode = 0;
    public GameObject codescreen;
    public GameObject selectedTool;
    public Rigidbody2D[] projectiles = new Rigidbody2D[stateLib.NUMBER_OF_TOOLS];
    private GameController controller;
    private bool walkloop = false;
    private bool facingRight = true;
    private bool quitting = false;
    private float fireRate = 0.5f;
    private float nextFire = 0.0f;
    private float animTime = 0.2f;
    private float animDelay = 0.0f;
    private float dropTime = 0.25f;
    private float climbTime;
    private float climbDelay = 0.2f;
    private Animator anim;
    private LevelGenerator lg;
    private int lastLineNumberactive;
    private float verticalMovement = 0.6f;
    private bool isMovingX = false;
    public bool reachedPosition = true;
    EnergyController energyController;
    private FireButton fire;
    bool canTakeDamage = true;

    int timeStart, timeEnd, totalTime, timeCurrent;
    DateTime time;
    private AudioClip throwTool;
    AudioSource audioSource;
    //.................................>8.......................................
    // Use this for initialization
    void Start()
    {
        audioSource = this.GetComponent<AudioSource>();
        codescreen = GameObject.Find("CodeScreen");
        energyController = GameObject.Find("Energy").GetComponent<EnergyController>();
        fire = GameObject.Find("FireTool").transform.GetChild(0).GetComponent<FireButton>();
        selectedTool = GameObject.Find("Sidebar").transform.GetChild(2).transform.Find("Sidebar Tool").gameObject;
        projectiles[0] = Resources.Load<GameObject>("Prefabs/projectileBug").GetComponent<Rigidbody2D>();
        projectiles[1] = Resources.Load<GameObject>("Prefabs/projectileActivator").GetComponent<Rigidbody2D>();
        projectiles[2] = Resources.Load<GameObject>("Prefabs/projectileWarp").GetComponent<Rigidbody2D>();
        projectiles[3] = Resources.Load<GameObject>("Prefabs/projectileComment").GetComponent<Rigidbody2D>();
        projectiles[4] = Resources.Load<GameObject>("Prefabs/projectileDebug").GetComponent<Rigidbody2D>();
        projectiles[5] = Resources.Load<GameObject>("Prefabs/projectileHelp").GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        throwTool = Resources.Load<AudioClip>("Sound/Triggers/throw");
        climbTime = 0f;
        lg = codescreen.GetComponent<LevelGenerator>();
        controller = Camera.main.GetComponent<GameController>();
        timeStart = DateTime.Now.Second;
        if (GlobalState.level.IsDemo)
            maxSpeed = 10f; 
        else
            maxSpeed = GlobalState.Stats.Speed; 
    }
    /// <summary>
    /// Hero actions related to messing up an action: 
    /// a)Output box telling the player they are damaged
    /// b) lowering the energy bar
    /// c) Flash the character
    /// </summary>
    public void onFail()
    {
        GameObject.Find("OutputCanvas").transform.GetChild(0).GetComponent<Output>().PlayCharacterOutput("Ow, that didn't work!");
        energyController.onFail(projectilecode);
        if (!(GlobalState.GameMode == stringLib.GAME_MODE_BUG && projectilecode == stateLib.TOOL_CATCHER_OR_CONTROL_FLOW))
            StartCoroutine(DamageDelay()); 
    }
    /// <summary>
    /// Flash the character with a red hue
    /// --currently not in use --
    /// </summary>
    /// <returns></returns>
    IEnumerator RedFlinch(){
        SpriteRenderer color = this.GetComponent<SpriteRenderer>(); 
        float speed = 0.04f; 
        while(color.color.g > 0.5f){
            color.color = new Color(color.color.r, color.color.g - speed, color.color.b - speed); 
            yield return null; 
        }
        while(color.color.g < 1){
            color.color = new Color(color.color.r, color.color.g + speed, color.color.b + speed); 
            yield return null; 
        }
    }
    /// <summary>
    /// Delay any further damage taken by obstacles. During
    /// this time the player will blink.
    /// </summary>
    /// <returns></returns>
    IEnumerator DamageDelay()
    {
        canTakeDamage = false;
        float seconds = 2f;
        int blinks = 3;

        for (int i = 0; i < blinks; i++)
        {
            GetComponent<SpriteRenderer>().color = new Color(0.3f, 0.3f, 0.3f);
            yield return new WaitForSecondsRealtime(seconds / (blinks * 2));
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
            yield return new WaitForSecondsRealtime(seconds / (blinks * 2));
        }
        canTakeDamage = true;
    }
    /// <summary>
    /// Handle how the player gets damaged by obstacles specifically.
    /// </summary>
    /// <param name="damage">Amount of damage that obstacle dealt</param>
    /// <param name="obstacleCode">Which obstacle dealt the damage</param>
    /// <returns>Whether the player was able to take damage</returns>
    public bool onTakeDamage(float damage, int obstacleCode)
    {
        if (canTakeDamage)
        {
			if (GlobalState.AdaptiveMode == 1 && GlobalState.HintMode == 0){
				damage=damage*0.75f;				
			}
			else if (GlobalState.AdaptiveMode == 2 && GlobalState.HintMode == 0){
				damage=damage*0.5f;				
			}
            float preEnergy = energyController.currentEnergy;
            energyController.onDamage(damage);
            float finEnergy = energyController.currentEnergy;
            RoundPosition(this.transform.position);
            controller.logger.onDamageStateJson(obstacleCode, lastLineNumberactive, RoundPosition(this.transform.position), preEnergy, finEnergy);
            GameObject.Find("OutputCanvas").transform.GetChild(0).GetComponent<Output>().PlayCharacterOutput("Ow, be careful!");
            StartCoroutine(DamageDelay());
            return true;
        }
        return false;
    }
    //.................................>8.......................................
    void FixedUpdate()
    {
        if (GlobalState.GameState == stateLib.GAMESTATE_IN_GAME && (!Output.IsAnswering || GlobalState.level.IsDemo))
        {
            //movement
            float fMoveVelocityHorizontal = 0f;
			fMoveVelocityVertical = 0f; 
            if (!GlobalState.level.IsDemo && !anim.GetCurrentAnimatorStateInfo(0).IsName(GlobalState.Character.ToLower() + "Die")
            && !anim.GetCurrentAnimatorStateInfo(0).IsName(GlobalState.Character.ToLower() + "Dead"))
            {
                fMoveVelocityHorizontal = Input.GetAxis("Horizontal");
                fMoveVelocityVertical = Input.GetAxis("Vertical");
                
                if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
                    fMoveVelocityVertical = -0.6f; 
                }
                else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)){
                    fMoveVelocityVertical = 0.6f; 
                }
            }
            if (((Input.GetMouseButton(0) && isMoving)|| GlobalState.level.IsDemo) && !reachedPosition)
            {
                if (isMovingX)
                    fMoveVelocityHorizontal = (facingRight) ? 1f : -1f;
                else fMoveVelocityVertical = verticalMovement;
            }
            if (fMoveVelocityVertical > 0)
            {
                GetComponent<Rigidbody2D>().gravityScale = 0;
                if (!onWall)
                {
                    onWall = true;
                    climbTime = Time.time + climbDelay;
                }
            }
            if (climbTime > Time.time)
            {
                fMoveVelocityVertical = fMoveVelocityVertical > 0.5f ? fMoveVelocityVertical : 0.5f;
            }
            if (fMoveVelocityVertical < 0)
            {
                fMoveVelocityVertical = -0.1f;
                onWall = false;
                GetComponent<Rigidbody2D>().gravityScale = 1;
            }

            //set animation for movement
            anim.SetBool("onWall", onWall);
            anim.SetFloat("speed", Mathf.Abs(fMoveVelocityHorizontal));
            anim.SetFloat("climbSpeed", fMoveVelocityVertical);
            if (fMoveVelocityHorizontal > 0)
            {
                facingRight = true;

            }
            else if (fMoveVelocityHorizontal < 0)
            {
                facingRight = false;
            }
            anim.SetBool("facingRight", facingRight);

            if (this.GetComponent<SpriteRenderer>().flipX == facingRight && GlobalState.Character == "Boy")
            {
                float offset = 0.8f;
                Transform pos = this.GetComponent<Transform>();
                Transform newTool = this.transform.Find("NewTool").GetComponent<Transform>();
                if (!facingRight) offset *= -1;
                newTool.position = new Vector3(newTool.position.x - offset / 2, newTool.position.y, newTool.position.z);
                pos.position = new Vector3(pos.position.x + offset, pos.position.y, pos.position.z);
            }
            this.GetComponent<SpriteRenderer>().flipX = !facingRight;

            if (Time.time > dropDelay && !GlobalState.level.IsDemo && dropping){
                fMoveVelocityVertical = 0; 
                reachedPosition = true; 
            }   
            //code for falling down through platforms
            if (fMoveVelocityVertical < 0 && !onWall && !dropping)
            {
                dropDelay = Time.time + dropTime;
                dropping = true;
            }
            else if (Time.time > dropDelay && fMoveVelocityVertical == 0)
            {
                dropping = false;
            }

            //new dropcode
            if (fMoveVelocityVertical == 0 && fMoveVelocityHorizontal == 0 && onWall && (Time.time > climbDelay))
            {
                onWall = false;
                GetComponent<Rigidbody2D>().gravityScale = 1;
            }

            //Physics2D.IgnoreLayerCollision(0, 8, onWall || dropping || Input.GetKey("fMoveVelocityVertical"));
            Physics2D.IgnoreLayerCollision(0, 8, (onWall || dropping));
            //move up if on the wall, otherwise let gravity do the work
            if (dropping && !GlobalState.level.IsDemo)
            {
                if (GetComponent<Rigidbody2D>().velocity.y == 0)
                {
                    GetComponent<Rigidbody2D>().isKinematic = true;
                    GetComponent<Rigidbody2D>().AddForce(Vector2.up * -50);
                    GetComponent<Rigidbody2D>().isKinematic = false;
                }
                GetComponent<Rigidbody2D>().velocity = new Vector2(fMoveVelocityHorizontal * maxSpeed, GetComponent<Rigidbody2D>().velocity.y);
            }
            else
            {
                if (!onWall)
                {
                    GetComponent<Rigidbody2D>().velocity = new Vector2(fMoveVelocityHorizontal * maxSpeed, Mathf.Min(0f, GetComponent<Rigidbody2D>().velocity.y));
                }
                else
                {
                    GetComponent<Rigidbody2D>().isKinematic = true;
                    GetComponent<Rigidbody2D>().velocity = new Vector2(fMoveVelocityHorizontal * maxSpeed, fMoveVelocityVertical * climbSpeed);
                    GetComponent<Rigidbody2D>().isKinematic = false;
                }
            }
        }
        else if (Output.IsAnswering && !GlobalState.level.IsDemo)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
        }
    }
    public void ThrowTool()
    {
        if (Time.time > nextFire &&
               !onWall &&
               (!Output.IsAnswering || GlobalState.level.IsDemo) &&
               energyController.currentEnergy > 0 &&
               GameObject.FindGameObjectsWithTag("Projectile").Length == 0 &&
               GetComponent<Rigidbody2D>().velocity == Vector2.zero &&
               projectilecode >= 0 &&
               selectedTool.GetComponent<SelectedTool>().toolCounts[projectilecode] + selectedTool.GetComponent<SelectedTool>().bonusTools[projectilecode] > 0 &&
               !GlobalState.foundBug)
        {
            throwing = true;
            audioSource.PlayOneShot(throwTool, 2f);
            anim.SetBool("throw", true);
            float currentEnergy = energyController.currentEnergy;
            energyController.onThrow(projectilecode);
            GameObject.Find("FireTool").transform.GetChild(0).GetComponent<FireButton>().Fire();
            nextFire = Time.time + fireRate;
            animDelay = Time.time + animTime;
            Rigidbody2D newstar = (Rigidbody2D)Instantiate(projectiles[projectilecode], RoundPosition(transform.position), transform.rotation);
            controller.logger.onToolUse(projectilecode, lastLineNumberactive);
            timeCurrent = DateTime.Now.Second - timeStart;
            controller.logger.onStateChangeJson(projectilecode, lastLineNumberactive, RoundPosition(transform.position), currentEnergy, energyController.currentEnergy, true, timeCurrent);
            if (facingRight)
            {
                newstar.GetComponent<Rigidbody2D>().AddForce(Vector2.right * 300);
            }
            else
            {
                newstar.GetComponent<Rigidbody2D>().AddForce(Vector2.right * -300);
            }
        }
    }

    /// <summary>
    /// Move the character to the specified position.
    /// </summary>
    /// <param name="position">Position to move the character in Game/World space</param>
    /// <returns></returns>
    public IEnumerator MoveToPosition(Vector3 position)
    {
        isMoving = true;
        if (position.x > transform.position.x)
            facingRight = true;
        else facingRight = false;
        anim.SetBool("facingRight", facingRight);
        //yield return new WaitForSeconds(0.1f); 
        if (Input.GetMouseButton(0) || GlobalState.level.IsDemo)
        {
            reachedPosition = false;
            isMovingX = true;
            while (Math.Abs(GetComponent<Transform>().localPosition.x - position.x) > 0.6f)
            {
                if (this.transform.position.x - position.x < 0) facingRight = true;
                else facingRight = false;
                yield return null;
            }
            isMovingX = false;
            while (Math.Abs(GetComponent<Transform>().localPosition.y - position.y) > 0.6f && !reachedPosition)
            {
                if (GetComponent<Transform>().localPosition.y - position.y < 0)
                    verticalMovement = 0.5f;
                else verticalMovement = -1f;
                yield return null;
            }
            if (GlobalState.level.IsDemo) facingRight = false; 
            anim.SetBool("facingRight", facingRight);
            reachedPosition = true;
        }
        isMoving = false;
        //if (!GlobalState.level.IsDemo)
            //HandleMouseMovement(); 
    }

    /// <summary>
    /// Compares the inputed position with line objects and rounds the position to be 
    /// set directly in between those two lines.
    /// </summary>
    /// <param name="position">The position to be rounded</param>
    /// <returns></returns>
    public Vector3 RoundPosition(Vector3 position)
    {
        Transform lineAbove = null, lineBelow = null;
        int lineNumber = 0;
        //find the lines above and below
        foreach (GameObject line in lg.manager.lines)
        {
            lineNumber++;
            if (line.GetComponent<Transform>().position.y < position.y)
            {
                lineBelow = line.GetComponent<Transform>();
                break;
            }
            else lineAbove = line.GetComponent<Transform>();
        }
        lastLineNumberactive = lineNumber;
        if (lineAbove == null || lineBelow == null)
        {
            return position;
        }
        return new Vector3(position.x, (lineAbove.position.y - lineBelow.position.y) / 2 + lineBelow.position.y, position.z);
    }
    //.................................>8.......................................
    void Update()
    {
        
        if (GlobalState.GameState == stateLib.GAMESTATE_IN_GAME &&
            !anim.GetCurrentAnimatorStateInfo(0).IsName(GlobalState.Character.ToLower() + "Die")
            && !anim.GetCurrentAnimatorStateInfo(0).IsName(GlobalState.Character.ToLower() + "Dead"))
        {
            AudioSource ad = GetComponent<AudioSource>();
            //Handle the player climbing
            if (!walkloop && (Input.GetAxis("Horizontal") != 0f && Input.GetAxis("Mouse X") != 0f) &&
            Input.GetMouseButton(0) &&
            GetComponent<Rigidbody2D>().velocity.y == 0 &&
            !onWall)
            {
                ad.Play();
                walkloop = true;
                ad.loop = true;
            }
            //handle the player falling
            if (Input.GetAxis("Horizontal") == 0f && (Input.GetAxis("Mouse X") == 0f && Input.GetMouseButton(0)) ||
            GetComponent<Rigidbody2D>().velocity.y != 0 ||
            onWall)
            {
                ad.loop = false;
                walkloop = false;
            }

            //firing
            if ((Input.GetKeyDown("left ctrl") || Input.GetKeyDown("right ctrl")) && !GlobalState.level.IsDemo)
            {
                ThrowTool();
            }
            //handle mouse input to move
            if (Input.GetMouseButtonDown(0) && !GlobalState.level.IsDemo)
            {
                Debug.Log(Input.mousePosition); 
                Vector2 finalScale = new Vector2(fire.GetComponent<RectTransform>().rect.width*((float)Screen.width/1920f), fire.GetComponent<RectTransform>().rect.height*((float)Screen.height/1080f)); 
                Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Bounds collider = GameObject.Find("CodeScreen").GetComponent<EdgeCollider2D>().bounds;
                //check if the mouse click was in the bounds of the code screen
                if ( pos.x < collider.center.x + collider.size.x / 2 && pos.y > collider.center.y - collider.size.y / 2&& !fire.IsFiring && !(Input.mousePosition.x < finalScale.x && Input.mousePosition.y < finalScale.x))
                {
                    GameObject obj = EventSystem.current.currentSelectedGameObject;
                    Debug.Log(Input.mousePosition); 
                    if (obj == null){
                        
                        StartCoroutine(MoveToPosition(RoundPosition(pos)));
                    }else Debug.Log(obj.ToString()); 
                }else Debug.Log("Failed"); 
            }
            //Cancel the movement after the player stops pressing/holding
            else if (Input.GetMouseButtonUp(0) && !GlobalState.level.IsDemo)
            {
                StopAllCoroutines();
                isMoving = false; 
                GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
                reachedPosition = true; 
            }
            //if the player moves the cursor while they are already in motion, 
            //continue the motion upon reaching the position
            else if (Input.GetMouseButton(0) && !GlobalState.level.IsDemo && reachedPosition){
                
                Vector2 finalScale = new Vector2(fire.GetComponent<RectTransform>().rect.width*((float)Screen.width/1920f), fire.GetComponent<RectTransform>().rect.height*((float)Screen.height/1080f)); 
                Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Bounds collider = GameObject.Find("CodeScreen").GetComponent<EdgeCollider2D>().bounds;
                if ( pos.x < collider.center.x + collider.size.x / 2 && pos.y > collider.center.y - collider.size.y / 2 && !fire.IsFiring && !(Input.mousePosition.x < finalScale.x && Input.mousePosition.y < finalScale.x))
                {
                    GameObject obj = EventSystem.current.currentSelectedGameObject; 
                    if (obj == null){
                        
                        StartCoroutine(MoveToPosition(RoundPosition(pos)));
                    }
                }
                
            }
            if (Time.time > animDelay)
            {
                anim.SetBool("throw", false);
            }
            //quit
            if (Input.GetKeyDown(KeyCode.Escape) == true)
            {
                quitting = true;
            }
            if (quitting)
            {
                if (Input.GetKeyDown("y"))
                {
                    Application.Quit();
                }
                else if (Input.GetKeyDown("n"))
                {
                    quitting = false;
                }
            }
        }
        else
        {
            GetComponent<AudioSource>().loop = false;
        }
    }


    //.................................>8.......................................

}
