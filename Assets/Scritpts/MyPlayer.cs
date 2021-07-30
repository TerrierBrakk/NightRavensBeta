using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayer : MonoBehaviour
{

    [Header("Component Variables")]
    private Rigidbody2D _rb;
    private Animator _anim;

    /// <summary>
    /// ////
    /// </summary>
    [Header("Mask")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _wallLayer;
    /// <summary>
    /// ///////////
    /// </summary>


    [Header("Movement Variables")]
    //[SerializeField] just to have them on inspector
    [SerializeField] private float _movementAcc = 70f;
    [SerializeField] private float _maxMoveSpd = 12f;
    [SerializeField] private float _groundDesAcc =7f;
    private float _hDirection;
    private float _vDirection;
    private bool _facingRight = true;
    private bool _changeDir => (_rb.velocity.x > 0f && _hDirection < 0f) || (_rb.velocity.x < 0f && _hDirection > 0f);
    private bool _canMove => !_spiderHands;



    [Header("Dash!! Variables!!")]
    [SerializeField] private float _dashSpeed = 15f;
    [SerializeField] private float _dashlength = .3f;
    [SerializeField] private float _dashBufferlength = .1f;
    private float _dashBufferCounter;
    private bool _isDashing;
    private bool _hasDashed;
    private bool _canDash => _dashBufferCounter >0f && !_hasDashed;

    /// <summary>
    /// ///////////
    /// </summary>
    [Header("Jump Variables")]
    [SerializeField] private float _jumpForce = 12f;
    [SerializeField] private float _airLinearDrag = 2.5f;
    [SerializeField] private float _fallMultiplier = 9.8f;
    [SerializeField] private float _lowJumpFallMultiplier = 12f;
    [SerializeField] private float _downMultiplier = 12f;
    [SerializeField] private float _hangTime = .1f;
    [SerializeField] private float _jumpBufferLength = .1f;
    [SerializeField] private int _extrajump = 1;
    private int _extraJumpValue;
    private float _hangTimeCounter;
    private float _jumpBufferCounter;
    private bool _canJump => _jumpBufferCounter > 0f && (_hangTimeCounter > 0f || _extraJumpValue > 0 || _onWall);
    private bool _isJumping = false;
    /// <summary>
    /// //////////////////
    /// </summary>
    [Header("Wall Mov Var")]
    [SerializeField] private float _wallslideMod = 0.5f;
    [SerializeField] private float _wallRunMod = 0.85f;
    [SerializeField] private float _wallJumpVelHDelay = 0.2f;
    private bool _spiderHands => _onWall && !_onGround && Input.GetButton("Spider") && !_wallRun;
    private bool _Wallslide => _onWall && !_onGround && !Input.GetButton("Spider") && _rb.velocity.y <= 0f && !_wallRun;
    private bool _wallRun => _onWall && _vDirection > 0f;




    [Header("Ground Colision Var")]
    [SerializeField] private float _groundRaycastLength;
    [SerializeField] private Vector3 _groundRaycastOffset;
    private bool _onGround;

    [Header("Wall Colision Var")]
    [SerializeField] private float _wallRaycastLength;
    public bool _onWall;
    public bool _onRightWall;
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
      

    }
    private void Update()
    {


        _hDirection = GetInput().x;
        _vDirection = GetInput().y;
        if (Input.GetButtonDown("Jump"))
        {
            print("Im jumping wiii");
            _jumpBufferCounter = _jumpBufferLength;
        }
        else
        {
            _jumpBufferCounter -= Time.deltaTime; 
        }

        if(Input.GetButtonDown("Dash"))
        {
            print("Im dashing !");
            _dashBufferCounter = _dashBufferlength;
        }
        else
        {
            _dashBufferCounter -= Time.deltaTime;
        }
        //Animations();


    }

    private void FixedUpdate()
    {   
        CheckCollisions();
        if (_canDash) StartCoroutine(Dash(_hDirection, _vDirection));
        if(!_isDashing)
        {
            if (_canMove) MoveCharacter();
            else _rb.velocity = Vector2.Lerp(_rb.velocity, (new Vector2(_hDirection * _maxMoveSpd, _rb.velocity.y)), .5f * Time.deltaTime);
            if (_onGround)
            {

                friccion();
                _extraJumpValue = _extrajump;
                _hangTimeCounter = _hangTime;
                _hasDashed = false;

            }
            else
            {
                ApplyingAirDesAcc();
                FallMultiplier();
                _hangTimeCounter -= Time.fixedDeltaTime;
                if (!_onWall || _rb.velocity.y < 0f || _wallRun) _isJumping = false;
            }
            if (_canJump)
            {
                if (_onWall && !_onGround)
                {
                    if (!_wallRun && (_onRightWall && _hDirection > 0f || !_onRightWall && _hDirection < 0f))
                    {
                        StartCoroutine(NeutralWallJump());
                    }
                    else
                    {
                        WallJump();
                    }
                    JustFlip();
                }

                else
                {
                    Jump(Vector2.up);
                }
            }

            if (!_isJumping)
            {
                if (_spiderHands) Spiderman();
                if (_Wallslide) WallSlide();
                if (_wallRun) WallRun();
                if (_onWall) StickToWallPlease();
            }
        }
    }

    private static Vector2 GetInput()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
    }

    private void MoveCharacter()
    {
        _rb.AddForce(new Vector2(_hDirection, 0f) * _movementAcc);
    //clamping velocity
    if(Mathf.Abs(_rb.velocity.x)> _maxMoveSpd)
        {
            _rb.velocity = new Vector2(Mathf.Sign(_rb.velocity.x) * _maxMoveSpd, _rb.velocity.y);
        }    
    }

    private void friccion()
    {
        if(Mathf.Abs(_hDirection)<0.4f || _changeDir)
        {
            _rb.drag = _groundDesAcc;
        }
        else
        {
            _rb.drag = 0f;
        }
    }

    private void ApplyingAirDesAcc()
    {
        _rb.drag = _airLinearDrag;
    }
    private void Jump(Vector2 direction)
    {
        if (!_onGround && !_onWall)
            _extraJumpValue--;

        ApplyingAirDesAcc();
        _rb.velocity = new Vector2(_rb.velocity.x, 0f);
        _rb.AddForce(direction * _jumpForce, ForceMode2D.Impulse);
        _hangTimeCounter = 0f;
        _jumpBufferCounter = 0f;
        _isJumping = true;
    }

    void WallJump()
    {
        Vector2 jumpDirection = _onRightWall ? Vector2.left : Vector2.right;
        Jump(Vector2.up + jumpDirection);
    }

    IEnumerator NeutralWallJump()
    {
        Vector2 jumpDirection = _onRightWall ? Vector2.left : Vector2.right;
        Jump(Vector2.up + jumpDirection);
        yield return new WaitForSeconds(_wallJumpVelHDelay);
            _rb.velocity = new Vector2(0f, _rb.velocity.y);

    }



 


    private void Spiderman()
    {
        _rb.gravityScale = 0f;
        _rb.velocity = Vector2.zero;
    }

    void WallSlide()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, -_maxMoveSpd * _wallslideMod);
        
    }    
    
    void WallRun()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, _vDirection * _maxMoveSpd * _wallRunMod);
        
    }
    
    void JustFlip()
    {
        _facingRight = !_facingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    private void StickToWallPlease()
    {
        if (_onRightWall && _hDirection >= 0f)
        {
            _rb.velocity = new Vector2(1f, _rb.velocity.y);
        }
        else if (!_onRightWall && _hDirection <= 0f)
        {
            _rb.velocity = new Vector2(-1f, _rb.velocity.y);
        }

        //volteamos ala direccion correcta
        if (_onRightWall && !_facingRight)
        {
            JustFlip();
        }
        else if(!_onRightWall && _facingRight)
        {
            JustFlip();
        }

    }

    IEnumerator Dash(float x, float y)
    {
        float dashStartTime = Time.time;
        _hasDashed = true;
        _isDashing = true;
        _isJumping = false;

        _rb.velocity = Vector2.zero;
        _rb.gravityScale = 0f;
        _rb.drag = 0f;

        //Direccion del dash
        Vector2 dir;
        if (x != 0f || y != 0f) dir = new Vector2(x, y);
        else
        {
            if (_facingRight) dir = new Vector2(1f, 0f);
            else dir = new Vector2(-1f, 0f);
        }

        while (Time.time <dashStartTime +_dashlength)
        {
            _rb.velocity = dir.normalized * _dashSpeed;
            yield return null;
        }

        _isDashing = false;

    }

   
    private void CheckCollisions()
    {
        _onGround = Physics2D.Raycast(transform.position + _groundRaycastOffset, Vector2.down, _groundRaycastLength, _groundLayer) ||
                    Physics2D.Raycast(transform.position - _groundRaycastOffset, Vector2.down, _groundRaycastLength, _groundLayer); 
           


        //Wall Colision
        _onWall = Physics2D.Raycast(transform.position, Vector2.right, _wallRaycastLength, _wallLayer) ||
            Physics2D.Raycast(transform.position, Vector2.left, _wallRaycastLength, _wallLayer);
        _onRightWall = Physics2D.Raycast(transform.position, Vector2.right, _wallRaycastLength, _wallLayer);
    }

    private void FallMultiplier()   
    {
        if (_vDirection < 0f)
        {
            _rb.gravityScale = _downMultiplier;
        }
        else
        {


            if (_rb.velocity.y < 0)
            {
                _rb.gravityScale = _fallMultiplier;

            }
            else if (_rb.velocity.y > 0 && !Input.GetButton("Jump"))
            {
                _rb.gravityScale = _lowJumpFallMultiplier;
            }
            else
            {
                _rb.gravityScale = 1f;
            }
        }
    }

     private void OnDrawGizmos()
    {
        //groundcheck
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + _groundRaycastOffset, transform.position + _groundRaycastOffset + Vector3.down * _groundRaycastLength);
        Gizmos.DrawLine(transform.position - _groundRaycastOffset, transform.position - _groundRaycastOffset + Vector3.down * _groundRaycastLength);


        //wallcheck
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * _wallRaycastLength);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * _wallRaycastLength);


    }


}
