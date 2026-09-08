using UnityEngine;
using System;

public class HitParticleEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem hitParticle;
    [SerializeField] private Vector3 offet;
    [SerializeField] private CatMoveController[] cats;

    private IDisposable _subscription;

    private void OnEnable()
    {
        _subscription = this.WhenReady(() => RhythmController.Instance)
            .Subscribe(this, controller =>
            {
                controller.OnNoteHitEvent += OnHit;
            })
            .AddTo(this);
    }

    private void OnDisable()
    {
        _subscription?.Dispose();
    }

    private void OnHit(int laneIndex, ObjectType candyType)
    {
        foreach (var cat in cats)
        {
            if (cat != null && cat.CurrentGlobalLaneIndex == laneIndex)
            {
                // Play particle at cat's position
                hitParticle.transform.position = cat.transform.position + offet;
                hitParticle.Play();
                break;
            }
        }
    }
}