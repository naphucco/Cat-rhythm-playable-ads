using UnityEngine;
using Spine.Unity;
using Spine;

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
            RhythmController.Instance.OnNoteMissEvent += HandleNoteMiss;
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
            RhythmController.Instance.OnNoteMissEvent -= HandleNoteMiss;
        }

        if (skeletonAnimation != null && skeletonAnimation.AnimationState != null)
        {
            skeletonAnimation.AnimationState.Complete -= HandleAnimationComplete;
        }
    }

    private void Update()
    {
        // Trigger a random idle animation if the cat has been inactive for too long
        if (!isIdling && Time.time - lastActionTime >= idleInterval)
        {
            PlayIdle();
        }
    }

    private void HandleAnimationComplete(TrackEntry trackEntry)
    {
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

    private void HandleNoteHit(int laneIndex)
    {
        if (IsMyLane(laneIndex))
        {
            PlayEat();
        }
    }

    private void HandleNoteMiss(int laneIndex)
    {
        if (IsMyLane(laneIndex))
        {
            PlayMiss();
        }
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
        if (skeletonAnimation == null || idleAnims == null || idleAnims.Length == 0) return;

        string randomIdle = idleAnims[Random.Range(0, idleAnims.Length)];
        if (!string.IsNullOrEmpty(randomIdle))
        {
            skeletonAnimation.AnimationState.SetAnimation(0, randomIdle, false);
            lastActionTime = Time.time;
            isIdling = true;
        }
    }
}