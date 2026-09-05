using UnityEngine;
using Spine.Unity;

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
    }

    private void OnDestroy()
    {
        if (RhythmController.Instance != null)
        {
            RhythmController.Instance.OnNoteHitEvent -= HandleNoteHit;
            RhythmController.Instance.OnNoteMissEvent -= HandleNoteMiss;
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
        }
    }

    public void PlayStart() => PlayRandomAnimation(startAnims, false);
    public void PlayEat() => PlayRandomAnimation(eatAnims, false);
    public void PlayMiss() => PlayRandomAnimation(missAnims, false);
    public void PlayVictory() => PlayRandomAnimation(victoryAnims, false);
    public void PlayIdle() => PlayRandomAnimation(idleAnims, true);
}