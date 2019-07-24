using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class GameboardCharacterController : MonoBehaviour
{
    public GameboardUnitData Data;
    
    public GameObject parent
    {
        get { return gameObject.transform.parent.gameObject; }
    }

    #region properties
    public float speed;
    public float unitVelocity;
    public float currentHealth;
    #endregion

    #region SerializedObjects
    public Rigidbody rb;
    [SerializeField] public GameObject VisualPrefabContainer;
    [SerializeField] private GameObject activeCircleTeam1;
    [SerializeField] private GameObject activeCircleTeam2;
    [SerializeField] public GameObject ArrowSpriteContainerGO;
    [SerializeField] public GameObject ArrowSpriteGO;
    [SerializeField] private TransformFollower disableWithArrow;
    [SerializeField] private GameObject HealthBarPrefab;
    [SerializeField] private Transform HealthBarAnchor;
    [SerializeField] private Transform CameraAnchor;
    #endregion
    
    public string NameOfGameObject;
    
    private Animator _animator;
    private List<GameObject> activatedVFX;
    private GameObject activeCircle;
    private GameObject ActiveHealthBar;
    private HealthBarView ActiveHealthBarView;

    private const float BonusDamageMultiplier = 0.25f;

    public void Setup(GameboardUnitData data)
    {
        Data = data;
        speed = Data.UnitSpeed;
        unitVelocity = Data.UnitVelocity;
        currentHealth = Data.maxHealth;

        NameOfGameObject = gameObject.name;
        
        var unitVisual = DataManager.Instance.GetVisualPrefabForClass(Data.charType);
        if (unitVisual != null)
        {
            var visualIns = Instantiate(unitVisual);
            if (visualIns != null)
            {
                visualIns.transform.SetParent(VisualPrefabContainer.transform);
                _animator = visualIns.GetComponent<Animator>();
                var vfxList = visualIns.GetComponent<VFXList>();
                if (vfxList != null)
                {
                    activatedVFX = vfxList.OnlyActivateOnActiveTurn;
                }
            }
        }
        
        SetActiveCircleAndArrowRoation(isPlayerMe());
    }

    public void AnimatorSetBool(string name, bool value)
    {
        if (_animator != null)
        {
            _animator.SetBool(name, value);
        }
    }

    public void AniamtionSetFloat(string name, float value) {
        if (_animator != null)
        {
            _animator.SetFloat(name, value);
        }
    }
    
    public void RefreshHealthBar()
    {
        if (ActiveHealthBar != null)
        {
            var hbComp = ActiveHealthBarView.sliderMain;
            if (hbComp != null)
            {
                float sliderNormalizedValue = currentHealth / Data.maxHealth;
                sliderNormalizedValue = Mathf.Clamp(sliderNormalizedValue, 0f, 1f);
                hbComp.value = sliderNormalizedValue;
                if (sliderNormalizedValue <= 0f)
                {
                    TurnManager.Instance.KillUnit(this);
                }   
            }   
        }
    }

    bool dying = false;
    
    public void TakeDamage(float dmg, int indexOf)
    {
        // TimeScaleManager.Instance.EnterSloMo();
        bool strong = false;
        bool weak = false;
        if (TurnManager.Instance.TurnOrder[indexOf].Data.strongTo == Data.charType) {
            dmg += (dmg * BonusDamageMultiplier);
            strong = true;
        }
        if (TurnManager.Instance.TurnOrder[indexOf].Data.weakTo == Data.weakTo) {
            dmg -= (dmg * BonusDamageMultiplier);
            weak = true;
        }
        HealthBarSetPeekDamage((int)dmg);
        if(dying) {
            SoundManager.PlayAudio(2);
        }
        if(strong) {
            if(!dying) {
                SoundManager.PlayAudio(0);
            }
            ActiveHealthBarView.takingStrongDamage = true;
            StartCoroutine(ActiveHealthBarView.DoTakeDmgAniamtion(ActiveHealthBarView.strongAnimDmg));
        } else if (weak) {
            if(!dying) {
                SoundManager.PlayAudio(4);
            }
            ActiveHealthBarView.takingWeakDamage = true;
            StartCoroutine(ActiveHealthBarView.DoTakeDmgAniamtion(ActiveHealthBarView.weakAnimDmg));
        } else {
            if(!dying) {
                SoundManager.PlayAudio(3);
            }
            ActiveHealthBarView.takingNeutralDamage = true;
            StartCoroutine(ActiveHealthBarView.DoTakeDmgAniamtion(ActiveHealthBarView.neutralAnimDmg));
        }

        currentHealth -= dmg;
        RefreshHealthBar();
    }


    public int PeekDamageStrengthFrom(GameboardCharacterController gccFrom) {
        int retVal = 0;
        if(gccFrom != null) {
            if(gccFrom.Data.strongTo == Data.charType) {
                retVal = 1;
            }
            if(gccFrom.Data.weakTo == Data.charType) {
                retVal = -1;
            }
        }
        return retVal;
    }

    public float PeekDamageFrom(GameboardCharacterController gccFrom) {
        float retVal = 0f;
        if(gccFrom != null) {
            retVal = gccFrom.Data.damage;
            if(gccFrom.Data.strongTo == Data.charType) {
                retVal += (retVal * BonusDamageMultiplier);
            }
            if(gccFrom.Data.weakTo == Data.charType) {
                retVal -= (retVal * BonusDamageMultiplier);
            }
        }
        return retVal;
    }
    
    public void OnUnitDead()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.gameObject.GetComponent<CapsuleCollider>().enabled = false;
        if (ActiveHealthBar != null)
        {
            Destroy(ActiveHealthBar);
        }
    }

    bool _currentlyPunching = false;
    
    IEnumerator EndPunch(float waitTime) {
        yield return new WaitForSeconds(waitTime);
        TimeScaleManager.Instance.ExitSloMo();
        // SpawningManager.Instance.CanvasTransform.gameObject.SetActive(true);
        AnimatorSetBool("punch", false);
        yield return new WaitForSeconds(waitTime);
        _currentlyPunching = false;
        SpawningManager.Instance.CanvasTransform.gameObject.SetActive(true);
        CameraLerp.Instance.StartReturning();
    }

    public void SlomoMaybe(int indexOf, float distance) {
        var hitChar = TurnManager.Instance.TurnOrder[indexOf];
        if(hitChar != null) {
            if(hitChar.Data.teamId != Data.teamId) {
                if(hitChar.currentHealth < Data.damage) {
                    if(_currentlyPunching == false) {
                        hitChar.dying = true;
                        SpawningManager.Instance.CanvasTransform.gameObject.SetActive(false);
                        TimeScaleManager.Instance.EnterSloMo();
                        AnimatorSetBool("punch", true);
                        float timeToDoAnim = distance / rb.velocity.magnitude;
                        StartCoroutine(EndPunch(_animator.GetCurrentAnimatorStateInfo(0).length * timeToDoAnim));
                        AniamtionSetFloat("punchAnimationSpeed", _animator.GetCurrentAnimatorStateInfo(0).length / timeToDoAnim);
                        _currentlyPunching = true;
                        hitChar.AnimatorSetBool("dieded", true);
                        hitChar.AnimatorSetBool("getHit", true);
                        CameraLerp.Instance.StartLerping(CameraAnchor, CameraAnchor.position, CameraAnchor);
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (ActiveHealthBar != null)
        {
            Vector2 localPoint;
            var screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, HealthBarAnchor.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(SpawningManager.Instance.CanvasTransform, screenPos, null, out localPoint);
            ActiveHealthBar.transform.localPosition = localPoint;
        }

        if(rb.velocity.magnitude > 1f) {
            if(TurnManager.Instance.TurnOrder[0].Data.teamId == Data.teamId) {
                RaycastHit hitinfo;
                bool slowMoRay = Physics.SphereCast(rb.position, 5f, rb.velocity.normalized, out hitinfo, rb.velocity.magnitude);
                if(slowMoRay) {
                    if(hitinfo.collider != null && hitinfo.collider.gameObject != null) {        
                        if(hitinfo.collider.gameObject.name == NameOfGameObject) {
                            if(PhotonNetwork.IsMasterClient) {
                                var hitChar = hitinfo.collider.gameObject.GetComponentInChildren<GameboardCharacterController>();
                                if(hitChar != null) {
                                    var indexOf = TurnManager.Instance.TurnOrder.IndexOf(hitChar);
                                    var indexFrom = TurnManager.Instance.TurnOrder.IndexOf(this);
                                    if(GameSetupController.isGameSinglePlayer) {
                                        GameSetupController.PCInstance.SlomoMaybe(indexOf, hitinfo.distance, indexFrom);
                                    } else {
                                        SpawningManager.Instance.myPhotonView.RPC("SlomoMaybe", RpcTarget.AllBuffered, indexOf, hitinfo.distance, indexFrom);
                                    }
                                }
                            }
                        }
                        
                        else {
                            if (rb.velocity.magnitude > 3)
                            {
                                AnimatorSetBool("dashStart", true);
                                AnimatorSetBool("pounceStart", false);
                            }
                            else
                            { 
                                AnimatorSetBool("dashStart", false);
                                AnimatorSetBool("pounceStart", false);
                            }
                        }
                    }
                }
            }
            
            if (TurnManager.Instance.currentTurn == this)
            {
                var directionVisualPrefab = isPlayerMe() ? -rb.velocity.normalized : rb.velocity.normalized;
                if(PhotonNetwork.IsMasterClient) {
                    VisualPrefabContainer.transform.localRotation = Quaternion.LookRotation(-directionVisualPrefab);
                } else {
                    VisualPrefabContainer.transform.localRotation = Quaternion.LookRotation(directionVisualPrefab);
                }
            }
        }

        if (rb.velocity.magnitude < 2f)
        {
            AnimatorSetBool("dashStart", false);
        }
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (PhotonNetwork.IsMasterClient || GameSetupController.isGameSinglePlayer)
        {
            if (NameOfGameObject == other.gameObject.name)
            {
                var characterHit = other.gameObject.GetComponent<GameboardCharacterController>();
                if (characterHit != null && characterHit.Data.teamId != Data.teamId)
                {
                    if (TurnManager.Instance.TurnOrder[0].Data.teamId == Data.teamId)
                    {
                        for (int i = 0; i < TurnManager.Instance.TurnOrder.Count; i++)
                        {
                            if (characterHit == TurnManager.Instance.TurnOrder[i])
                            {
                                if(GameSetupController.isGameSinglePlayer) {
                                    GameSetupController.PCInstance.TakeDamage(i, Data.damage,TurnManager.Instance.TurnOrder.IndexOf(this));
                                } else {
                                    SpawningManager.Instance.myPhotonView.RPC("TakeDamage", RpcTarget.AllBuffered, i, Data.damage, TurnManager.Instance.TurnOrder.IndexOf(this));
                                }
                                break;   
                            }
                        }
                    }
                }
            }
        }
    }

    public void HealthBarSetPeekDamage(int dmg) {
        ActiveHealthBarView.SetTexts(dmg);
    }

    public void HealthBarSetPeekType(int type) {
        ActiveHealthBarView.SetHealthbarAdvType(type);
    }

    public void HealthBarResetView() {
        ActiveHealthBarView.ResetView();
    }
    
    public void CreateHealthbar()
    {        
        ActiveHealthBar = Instantiate(HealthBarPrefab, SpawningManager.Instance.healthBarContainer);

        ActiveHealthBarView = ActiveHealthBar.GetComponent<HealthBarView>();
        if (ActiveHealthBarView != null)
        {
            ActiveHealthBarView.Setup(this);
            ActiveHealthBarView.SetupColor(isPlayerMe());
        }
        
        Vector2 localPoint;
        var screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, HealthBarAnchor.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(SpawningManager.Instance.CanvasTransform, screenPos, null, out localPoint);
        ActiveHealthBar.transform.localPosition = localPoint;
    }

    public void SetActiveCircleAndArrowRoation(bool isTeam1)
    {
        activeCircle = isTeam1 ? activeCircleTeam1 : activeCircleTeam2;

        if (isTeam1)
        {
            var newEulr = ArrowSpriteContainerGO.transform.localRotation.eulerAngles;
            newEulr.z = 180;
            ArrowSpriteContainerGO.transform.localRotation = Quaternion.Euler(newEulr);
        }
    } 
    
    public void OnTryMove(Vector3 direction)
    {
        rb.velocity += (direction * unitVelocity * 5f);
    }
        
    public void SetParentRotation(Quaternion q)
    {
        parent.transform.rotation = q;
    }
    
    public void SetDefaultRoation()
    {
        VisualPrefabContainer.transform.localRotation = Quaternion.identity;
        if (Data.teamId == 0)
        {
            SetParentRotation(Quaternion.LookRotation(Vector3.left, Vector3.up));
        }
        else
        {
            SetParentRotation(Quaternion.LookRotation(Vector3.right, Vector3.up));
        }
    }
        
    public void DisableActiveCircle()
    {
        if (activeCircle.activeSelf)
        {
            activeCircle.SetActive(false);
            SwitchArrow(false);
        }
    }
    
    public void DisableActiveVFX()
    {
        if (activatedVFX != null)
        {
            foreach (var singleVFX in activatedVFX)
            {
                if (singleVFX.activeSelf)
                {
                    singleVFX.SetActive(false);
                }
            }
        }
    }
    
    public void EnableActiveVFX()
    {
        if (activatedVFX != null)
        {
            foreach (var singleVFX in activatedVFX)
            {
                if (!singleVFX.activeSelf)
                {
                    singleVFX.SetActive(true);
                }
            }
        }
    }
    
    public void EnableActiveCircle()
    {
        if (!activeCircle.activeSelf)
        {
            activeCircle.SetActive(true);
            SwitchArrow(true);
        }
    }
    
    public void SwitchArrow(bool shouldActivate)
    {
        if (shouldActivate)
        {
            if (!ArrowSpriteGO.activeSelf)
            {
                ArrowSpriteGO.SetActive(true);
                disableWithArrow.enabled = false;
                ArrowSpriteGO.transform.localScale = new Vector3(0.01f, 0.01f, 1f);
            }
        }
        else
        {   
            if (ArrowSpriteGO.activeSelf)
            {
                disableWithArrow.enabled = true;
                ArrowSpriteGO.SetActive(false);
            }
        }
    }

    public bool isPlayerMe()
    {
        return TurnManager.Instance.thisPlayerId == Data.teamId;
    }
}
