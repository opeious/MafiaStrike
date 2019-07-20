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
    #endregion
    
    private string NameOfGameObject;
    
    private Animator _animator;
    private List<GameObject> activatedVFX;
    private GameObject activeCircle;
    private GameObject ActiveHealthBar;

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

    private void FixedUpdate()
    {
        if (ActiveHealthBar != null)
        {
            if (rb.velocity.magnitude > 1)
            {
                Vector2 localPoint;
                var screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, HealthBarAnchor.position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(SpawningManager.Instance.CanvasTransform, screenPos, null, out localPoint);
                ActiveHealthBar.transform.localPosition = localPoint;

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

        if (rb.velocity.magnitude > 2f && TurnManager.Instance.currentTurn == this)
        {
            var directionVisualPrefab = isPlayerMe() ? -rb.velocity.normalized : rb.velocity.normalized;
            VisualPrefabContainer.transform.localRotation = Quaternion.LookRotation(-directionVisualPrefab);
        }

        if (rb.velocity.magnitude < 2f)
        {
            AnimatorSetBool("dashStart", false);
        }
    }

    public void AnimatorSetBool(string name, bool value)
    {
        if (_animator != null)
        {
            _animator.SetBool(name, value);
        }
    }
    
    public void RefreshHealthBar()
    {
        if (ActiveHealthBar != null)
        {
            var hbComp = ActiveHealthBar.GetComponent<Slider>();
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
    
    public void TakeDamage(float dmg)
    {
        TimeScaleManager.Instance.EnterSloMo();
        currentHealth -= dmg;
        RefreshHealthBar();
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
    
    private void OnCollisionEnter(Collision other)
    {
        if (PhotonNetwork.IsMasterClient)
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
                                SpawningManager.Instance.myPhotonView.RPC("TakeDamage", RpcTarget.AllBuffered, i, Data.damage);
                                break;   
                            }
                        }
                    }
                }
            }
        }
    }

    
    public void CreateHealthbar()
    {        
        ActiveHealthBar = Instantiate(HealthBarPrefab);
        ActiveHealthBar.transform.SetParent(SpawningManager.Instance.CanvasTransform);

        var hbComp = ActiveHealthBar.GetComponent<HealthBarView>();
        if (hbComp != null)
        {
            hbComp.SetupColor(isPlayerMe());
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
