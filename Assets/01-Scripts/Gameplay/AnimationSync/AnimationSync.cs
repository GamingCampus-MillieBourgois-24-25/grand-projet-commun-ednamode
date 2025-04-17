using UnityEngine;

public class AnimationSync : MonoBehaviour
{
    private Animator clothingAnimator; 
    private Animator characterAnimator; 
    private Vector3 initialLocalPosition; 
    private Quaternion initialLocalRotation;

    private bool isInitialized = false;
    private int lastStateHash = 0; 
    private float lastNormalizedTime = 0f; 

    void Start()
    {
        Initialize(transform.parent.gameObject);
    }

    void Update()
    {
        if (!isInitialized || clothingAnimator == null || characterAnimator == null) return;

        SyncAnimatorParameters();

        SyncAnimationState();

        MaintainLocalTransform();
    }

    public void Initialize(GameObject character)
    {
        clothingAnimator = GetComponent<Animator>();
        if (clothingAnimator == null)
        {
            return;
        }

        if (transform.parent == null || transform.parent.gameObject != character)
        {
            transform.SetParent(character.transform, false);
        }

        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;

        characterAnimator = character.GetComponent<Animator>();
        if (characterAnimator == null)
        {
            return;
        }

        if (clothingAnimator.runtimeAnimatorController == null)
        {
            clothingAnimator.runtimeAnimatorController = characterAnimator.runtimeAnimatorController;
        }
    

        SyncAnimatorParameters();
        SyncAnimationState();

        isInitialized = true;
    }

    private void SyncAnimatorParameters()
    {
        foreach (AnimatorControllerParameter param in characterAnimator.parameters)
        {
            switch (param.type)
            {
                case AnimatorControllerParameterType.Float:
                    float floatValue = characterAnimator.GetFloat(param.name);
                    if (clothingAnimator.GetFloat(param.name) != floatValue)
                    {
                        clothingAnimator.SetFloat(param.name, floatValue);
                    }
                    break;
                case AnimatorControllerParameterType.Int:
                    int intValue = characterAnimator.GetInteger(param.name);
                    if (clothingAnimator.GetInteger(param.name) != intValue)
                    {
                        clothingAnimator.SetInteger(param.name, intValue);
                    }
                    break;
                case AnimatorControllerParameterType.Bool:
                    bool boolValue = characterAnimator.GetBool(param.name);
                    if (clothingAnimator.GetBool(param.name) != boolValue)
                    {
                        clothingAnimator.SetBool(param.name, boolValue);
                    }
                    break;
                case AnimatorControllerParameterType.Trigger:
                    if (characterAnimator.GetBool(param.name))
                    {
                        clothingAnimator.SetTrigger(param.name);
                    }
                    break;
            }
        }
    }

    private void SyncAnimationState()
    {
        AnimatorStateInfo characterState = characterAnimator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo clothingState = clothingAnimator.GetCurrentAnimatorStateInfo(0);

        if (characterState.fullPathHash != lastStateHash || Mathf.Abs(characterState.normalizedTime - lastNormalizedTime) > 0.01f)
        {
            clothingAnimator.CrossFade(characterState.fullPathHash, 0.01f, 0, characterState.normalizedTime);

            lastStateHash = characterState.fullPathHash;
            lastNormalizedTime = characterState.normalizedTime;
        }
    }

    private void MaintainLocalTransform()
    {
        if (transform.localPosition != initialLocalPosition)
        {
            transform.localPosition = initialLocalPosition;
        }
        if (transform.localRotation != initialLocalRotation)
        {
            transform.localRotation = initialLocalRotation;
        }
    }
}