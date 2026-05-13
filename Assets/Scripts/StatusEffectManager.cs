using System;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    private class StatusEffect
    {
        public float timeLeft;
        public Action<Player> onApply;
        public Action<Player> onRemove;
    }

    private readonly List<StatusEffect> effects = new();
    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        for (int i = effects.Count - 1; i >= 0; i--)
        {
            effects[i].timeLeft -= dt;

            if (effects[i].timeLeft <= 0f)
            {
                effects[i].onRemove?.Invoke(player);
                effects.RemoveAt(i);
            }
        }
    }

    public void AddEffect(float duration, Action<Player> onApply, Action<Player> onRemove)
    {
        var effect = new StatusEffect
        {
            timeLeft = duration,
            onApply = onApply,
            onRemove = onRemove
        };

        onApply?.Invoke(player);

        effects.Add(effect);
    }
}