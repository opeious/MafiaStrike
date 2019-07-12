using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameboardCharacterController : MonoBehaviour
{
    public float speed;
    public float unitVelocity;
    public float currentHealth;
    public Rigidbody rb;
    private GameboardUnitData activeData;

    [SerializeField]
    private TextMeshPro debugIdText;

    [SerializeField]
    private Material team1Color;
    
    [SerializeField]
    private Material team2Color;

    [SerializeField]
    private MeshRenderer DebugColorSetupMesh;

    [SerializeField]
    private Transform HealthBarAnchor;

    [SerializeField]
    private GameObject HealthBarPrefab;

    private GameObject ActiveHealthBar;

    [SerializeField]
    public GameObject VisualPrefabContainer;

    private GameObject activeCircle;
    
    [SerializeField]
    private GameObject activeCircleTeam1;
    [SerializeField]
    private GameObject activeCircleTeam2;

    private string NameOfGameObject;

    [SerializeField] public GameObject ArrowSpriteGO;
    [SerializeField] public GameObject ArrowSpriteContainerGO;

    private Animator _animator;

    private List<GameObject> activatedVFX;

    [SerializeField]
    private Transform cameraAnchor;

    [SerializeField] private TransformFollower disableWithArrow;
    
    public void Setup(GameboardUnitData data)
    {
        activeData = data;
        speed = activeData.UnitSpeed;
        unitVelocity = activeData.UnitVelocity;
        currentHealth = activeData.maxHealth;

        if (debugIdText != null)
        {
            debugIdText.text = activeData.debugId + "";
        }

        if (activeData.teamId == 0)
        {
            DebugColorSetupMesh.material = team1Color;
        }
        else
        {
            DebugColorSetupMesh.material = team2Color;
        }

        NameOfGameObject = gameObject.name;
        
        var unitVisual = DataManager.Instance.GetVisualPrefabForClass(activeData.charType);
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
    
    public void DisableActiveCircle()
    {
        if (activeCircle.activeSelf)
        {
            activeCircle.SetActive(false);
            SwitchArrow(false);
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
    
    public void SetParentRotation(Quaternion q)
    {
        parent.transform.rotation = q;
    }


    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;
        RefreshHealthBar();
        //DO TAKING DAMAGE ANIMATION
    }

    public void DoAttackAnimation()
    {
        //DO ATTACK ANIMATION
//        AnimatorSetBool("punch", true);
//        StartCoroutine(DisablePunch());
    }

    IEnumerator DisablePunch()
    {
        yield return new WaitForSeconds(1f);
//        AnimatorSetBool("punch", false);
    }

    public void RefreshHealthBar()
    {
        if (ActiveHealthBar != null)
        {
            var hbComp = ActiveHealthBar.GetComponent<Slider>();
            if (hbComp != null)
            {
                float sliderNormalizedValue = currentHealth / activeData.maxHealth;
                sliderNormalizedValue = Mathf.Clamp(sliderNormalizedValue, 0f, 1f);
                hbComp.value = sliderNormalizedValue;
                if (sliderNormalizedValue <= 0f)
                {
                    TestSingletonManager.Instance.KillUnit(this);
                }   
            }   
        }
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

    private void OnDestroy()
    {
        if (ActiveHealthBar != null)
        {
            Destroy(ActiveHealthBar);
        }
    }

    public void AnimatorSetBool(string name, bool value)
    {
        if (_animator != null)
        {
            _animator.SetBool(name, value);
        }
    }

    private void FixedUpdate()
    {
        if (ActiveHealthBar != null)
        {
            if (rb.velocity.magnitude > 1)
            {
//                Debug.Log("now");
                Vector2 localPoint;
                var screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, HealthBarAnchor.position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(TestSingletonManager.Instance.CanvasTransform, screenPos, null, out localPoint);
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

    public void CreateHealthbar()
    {        
        ActiveHealthBar = Instantiate(HealthBarPrefab);
        ActiveHealthBar.transform.SetParent(TestSingletonManager.Instance.CanvasTransform);

        var hbComp = ActiveHealthBar.GetComponent<HealthBarView>();
        if (hbComp != null)
        {
            hbComp.SetupColor(isPlayerMe());
        }
        
        Vector2 localPoint;
        var screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, HealthBarAnchor.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(TestSingletonManager.Instance.CanvasTransform, screenPos, null, out localPoint);
        ActiveHealthBar.transform.localPosition = localPoint;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (NameOfGameObject == other.gameObject.name)
        {
            var characterHit = other.gameObject.GetComponent<GameboardCharacterController>();
            if (characterHit != null && characterHit.activeData.teamId != activeData.teamId)
            {
                if (TurnManager.Instance.TurnOrder[0].activeData.teamId == activeData.teamId)
                {
                       characterHit.TakeDamage(activeData.damage);
                       DoAttackAnimation();
                }   
            }
        }
    }


    public GameObject parent
    {
        get { return gameObject.transform.parent.gameObject; }
    }
    
    public GameboardUnitData Data
    {
        get { return activeData; }
    }

    public void OnTryMove(Vector3 direction)
    {
//        Debug.Log(direction);
//        Debug.Log(direction.magnitude);
        rb.velocity += (direction * unitVelocity);
    }

    public bool isPlayerMe()
    {
        return TurnManager.Instance.thisPlayerId == activeData.teamId;
    }
}
