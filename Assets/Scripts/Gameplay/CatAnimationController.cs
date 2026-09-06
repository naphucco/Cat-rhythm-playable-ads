using UnityEngine;
using Spine.Unity;
using Spine;

/// <summary>
/// Manages cat animations via Spine based on game events and states.
/// </summary>
public class CatAnimationController : MonoBehaviour
{
    private SkeletonAnimation skeletonAnimation;
    private CatMoveController moveController;

    [Header("Animation Pools (Multiple Variants)")]
    [SpineAnimation] [SerializeField] private string[] startAnims;
    [SpineAnimation] [SerializeField] private string[] eatAnims;
    [SpineAnimation] [SerializeField] private string[] missAnims;
    [SpineAnimation] [SerializeField] private string[] victoryAnims;
    [SpineAnimation] [SerializeField] private string[] idleAnims;

    [Header("Idle Settings")]
    [SerializeField] private float idleInterval = 4f;
    private float lastActionTime;
    private bool isIdling = false;
    private bool isGameOver = false; // Flag to lock animations when game ends

    private void Awake()
    {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        moveController = GetComponent<CatMoveController>();
    }

    private void Start()
    {
        PlayIdle();

        if (RhythmController.Instance != null)
        {
            RhythmController.Instance.OnNoteHitEvent += HandleNoteHit;
        }

        // Listen to game state changes from GameManager to lock animations on lose
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLoseStateEntered += HandleLoseStateEntered;
        }

        if (skeletonAnimation != null && skeletonAnimation.AnimationState != null)
        {
            skeletonAnimation.AnimationState.Complete += HandleAnimationComplete;
        }
    }

    private void OnDestroy()
    {
        if (RhythmController.Instance != null)
        {
            RhythmController.Instance.OnNoteHitEvent -= HandleNoteHit;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLoseStateEntered -= HandleLoseStateEntered;
        }

        if (skeletonAnimation != null && skeletonAnimation.AnimationState != null)
        {
            skeletonAnimation.AnimationState.Complete -= HandleAnimationComplete;
        }
    }

    private void Update()
    {
        // Stop updating idle if game is over
        if (isGameOver) return;

        // Trigger a random idle animation if the cat has been inactive for too long
        if (!isIdling && Time.time - lastActionTime >= idleInterval)
        {
            PlayIdle();
        }
    }

    private void HandleAnimationComplete(TrackEntry trackEntry)
    {
        if (isGameOver) return;

        // Automatically switch to another random idle variant when the current idle finishes playing
        if (isIdling && idleAnims != null && idleAnims.Length > 0)
        {
            string completedAnimName = trackEntry.Animation.Name;
            foreach (var idle in idleAnims)
            {
                if (completedAnimName == idle)
                {
                    PlayIdle();
                    break;
                }
            }
        }
    }

    private void HandleNoteHit(int laneIndex, ObjectType objectType)
    {
        if (isGameOver) return;

        if (IsMyLane(laneIndex))
        {
            PlayEat();
        }
    }

    private void HandleLoseStateEntered()
    {
        isGameOver = true;
        // Ensure both cats play miss animation and stay locked without returning to idle
        PlayMiss();
    }

    private bool IsMyLane(int laneIndex)
    {
        if (moveController != null)
        {
            return moveController.CurrentGlobalLaneIndex == laneIndex;
        }
        return false;
    }

    public void PlayRandomAnimation(string[] animArray, bool loop = false)
    {
        if (skeletonAnimation == null || animArray == null || animArray.Length == 0) return;

        string randomAnim = animArray[Random.Range(0, animArray.Length)];
        if (!string.IsNullOrEmpty(randomAnim))
        {
            skeletonAnimation.AnimationState.SetAnimation(0, randomAnim, loop);
            lastActionTime = Time.time;
            isIdling = false;
        }
    }

    public void PlayStart() => PlayRandomAnimation(startAnims, false);
    public void PlayEat() => PlayRandomAnimation(eatAnims, false);
    public void PlayMiss() => PlayRandomAnimation(missAnims, false);
    public void PlayVictory() => PlayRandomAnimation(victoryAnims, false);

    public void PlayIdle()
    {
        if (isGameOver || skeletonAnimation == null || idleAnims == null || idleAnims.Length == 0) return;

        string randomIdle = idleAnims[Random.Range(0, idleAnims.Length)];
        if (!string.IsNullOrEmpty(randomIdle))
        {
            skeletonAnimation.AnimationState.SetAnimation(0, randomIdle, false);
            lastActionTime = Time.time;
            isIdling = true;
        }
    }
}