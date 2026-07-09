using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SwingTargetResolver : MonoBehaviour
{
    public Collider CurrentTarget { get; private set; }
    public Vector3 CurrentAttachPoint { get; private set; }

    private readonly List<Collider> _candidates = new List<Collider>(); 

    [SerializeField] private LayerMask _swingableLayer;
    [SerializeField] private SwingAbilitySO _rules;
    [SerializeField] private PlayerController _controller;
    [SerializeField] private SwingHandler _handler;
    [SerializeField] private AbilityUser _abilityUser;

    private void Awake()
    {
        if (_controller == null)
        {
            _controller = GetComponent<PlayerController>();
            if (_controller == null)
            {
                Debug.LogError("SwingTargetResolver: No PlayerController found on the GameObject.");
            }
        }

        if (_handler == null)
        {
            _handler = GetComponent<SwingHandler>();
            if (_handler == null)
            {
                Debug.LogError("SwingTargetResolver: No SwingHandler found on the GameObject.");
            }
        }

        if (_abilityUser == null)
        {
            _abilityUser = GetComponent<AbilityUser>();
            if (_abilityUser == null)
            {
                Debug.LogError("SwingTargetResolver: No AbilityUser found on the GameObject.");
            }
        }

    }

    private void Start()
    {
       _swingableLayer = _handler.WhatIsSwingable; 
        _rules = _abilityUser.GetAbility<UniversalTongueAbilitySO>().SwingAbility;
    }

    public void AddCandidate(Collider col)
    {
        if (!_candidates.Contains(col))
            _candidates.Add(col);
    }

    public void RemoveCandidate(Collider col)
    {
        _candidates.Remove(col);
        if (CurrentTarget == col)
        {
            CurrentTarget = null;
        }
    }

    private void Update()
    {
        ResolveTarget();
    }

    private void ResolveTarget()
    {
        CurrentTarget = null;

        float bestScore = float.PositiveInfinity;
        Vector3 playerPos = _controller.transform.position;

        foreach (var c in _candidates)
        {
            if (c == null) continue;

            Vector3 point = c.ClosestPoint(playerPos);
            Vector3 toPoint = point - playerPos;
            float dist = toPoint.magnitude;
            if (dist <= 0.001f) continue;
            if (dist > _rules.MaxRappleDistance) continue;

            Vector3 dir = toPoint / dist;
            if (!Physics.Raycast(playerPos, dir, out var hit, dist + 0.1f, _swingableLayer, QueryTriggerInteraction.Ignore))
                continue;

            if (hit.collider != c) continue;

            float score = dist; // later expand with angle/facing/etc
            if (score < bestScore)
            {
                bestScore = score;
                CurrentTarget = c;
                CurrentAttachPoint = point;
            }
        }
    }
}
