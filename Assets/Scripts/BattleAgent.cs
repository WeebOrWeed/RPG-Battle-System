using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleAgent: MonoBehaviour
{
    [Header("Agent Actions")]
    [SerializeField]
    public List<BattleAction> availableActions;

    [Header("Agent Info")]
    public string Name;
    public int HP;
    public int Attack;
    public int Defense;
    public int Speed;
    public bool IsPlayer;
    [HideInInspector]
    public bool IsActing = false;
    public LineDrawer lineDrawerPrefab;

    // Effects from action
    // Tuple [Bonus amount, Time/Turn to expire]
    public Dictionary<BonusType, Tuple<int, int>> BuffsWithTurn =
        new Dictionary<BonusType, Tuple<int, int>>() {
            { BonusType.ATTACK, new Tuple<int, int>(0, 0) },
            { BonusType.DEFENSE, new Tuple<int, int>(0, 0) },
            { BonusType.SPEED, new Tuple<int, int>(0, 0) }
        };
    public Dictionary<BonusType, Tuple<int, int>> DebuffsWithTurn = 
        new Dictionary<BonusType, Tuple<int, int>>() {
            { BonusType.ATTACK, new Tuple<int, int>(0, 0) },
            { BonusType.DEFENSE, new Tuple<int, int>(0, 0) },
            { BonusType.SPEED, new Tuple<int, int>(0, 0) }
        };
    public Tuple<int, int> HealWithTurn = new Tuple<int, int>(0, 0);
    public Tuple<int, int> DamageWithTurn = new Tuple<int, int>(0, 0);

    [HideInInspector]
    public int realAttack;
    [HideInInspector]
    public int realDefense;
    [HideInInspector]
    public int realSpeed;
    [HideInInspector]
    public int curHealth;

    private AgentStatus statusUI;
    private WeightedRandomSelector selector;
    private BattleManager battleManager;
    private Animator animator;

    private void Start()
    {
        statusUI = transform.GetChild(0).GetComponent<AgentStatus>();
        animator = transform.GetChild(1).GetComponent<Animator>();
        selector = new WeightedRandomSelector(availableActions);
        battleManager = FindAnyObjectByType<BattleManager>();
        CalculateStatus();
        UpdateUI();
    }

    public void ApplyEffects()
    {
        // Deal Damage and Heal
        int healthChange = (HealWithTurn.Item2 > 0? HealWithTurn.Item1 : 0) - (DamageWithTurn.Item2 > 0 ? DamageWithTurn.Item1 : 0);
        HealthChanged(healthChange);
        // Subtract timer by 1
        bool noBuffs = true;
        foreach (var bonus in BuffsWithTurn.Keys.ToList())
        {
            BuffsWithTurn[bonus] = new Tuple<int,int>(BuffsWithTurn[bonus].Item1, Math.Max(0, BuffsWithTurn[bonus].Item2 - 1));
            if (DebuffsWithTurn[bonus].Item2 > 0) noBuffs = false;
        }
        if (noBuffs)
        {
            animator.GetComponent<AnimatorManager>().SetEffects("buffed", false);
        }
        bool noDebuffs = true;
        foreach (var bonus in DebuffsWithTurn.Keys.ToList())
        {
            DebuffsWithTurn[bonus] = new Tuple<int, int>(DebuffsWithTurn[bonus].Item1, Math.Max(0, DebuffsWithTurn[bonus].Item2 - 1));
            if (DebuffsWithTurn[bonus].Item2 > 0) noDebuffs = false;
        }
        if (noDebuffs)
        {
            animator.GetComponent<AnimatorManager>().SetEffects("debuffed", false);
        }

        HealWithTurn = new Tuple<int, int>(HealWithTurn.Item1, Math.Max(0, HealWithTurn.Item2 - 1));
        if (HealWithTurn.Item2 == 0)
        {
            animator.GetComponent<AnimatorManager>().SetEffects("healing", false);
        }
        DamageWithTurn = new Tuple<int, int>(DamageWithTurn.Item1, Math.Max(0, DamageWithTurn.Item2 - 1));
        if (DamageWithTurn.Item2 == 0)
        {
            animator.GetComponent<AnimatorManager>().SetEffects("poisoned", false);
        }
        CalculateStatus();
        UpdateUI();
        CheckDeath();
    }       

    public void ApplyTurn()
    {
        // Select Action
        BattleAction action = selector.GetRandomAction();
        // Apply Action
        ApplyAction(action);
    }

    public void HealthChanged(int amount)
    {
        curHealth += amount;
        curHealth = Math.Min(curHealth, HP); // Do not exceed max health
        CalculateStatus();
        UpdateUI();
        CheckDeath();
    }

    // This would override the previous damage
    public void Poisoned(int damage, int duration)
    {
        DamageWithTurn = new Tuple<int, int>(damage, duration);
    }

    // This would override the healing
    public void HealingOverTime(int healAmount, int duration)
    {
        HealWithTurn = new Tuple<int, int>(healAmount, duration);
    }

    public void Buffed(BonusType type, int amount, int duration)
    {
        BuffsWithTurn[type] = new Tuple<int, int>(amount, duration);
    }

    public void Debuffed(BonusType type, int amount, int duration)
    {
        DebuffsWithTurn[type] = new Tuple<int, int>(amount, duration);
    }

    public void PhysicalDamage(int amount)
    {
        curHealth -= Math.Max(0, amount - realDefense); // You cannot deal negative damage
        CalculateStatus();
        UpdateUI();
        CheckDeath();
    }

    private void CheckDeath()
    {
        if (curHealth <= 0)
        {
            battleManager.AgentDeath(this);
        }
    }

    private void CalculateStatus()
    {
        // Orignal value + buff if timer not expired - debuff if timer not expired
        realAttack = Math.Max(0, Attack +
            (BuffsWithTurn[BonusType.ATTACK].Item2 > 0 ? BuffsWithTurn[BonusType.ATTACK].Item1 : 0 -
            DebuffsWithTurn[BonusType.ATTACK].Item2 > 0 ? DebuffsWithTurn[BonusType.ATTACK].Item1 : 0));
        realDefense = Math.Max(0, Defense +
            (BuffsWithTurn[BonusType.DEFENSE].Item2 > 0 ? BuffsWithTurn[BonusType.DEFENSE].Item1 : 0 -
            DebuffsWithTurn[BonusType.DEFENSE].Item2 > 0 ? DebuffsWithTurn[BonusType.DEFENSE].Item1 : 0));
        realSpeed = Math.Max(0, Speed +
            (BuffsWithTurn[BonusType.SPEED].Item2 > 0 ? BuffsWithTurn[BonusType.SPEED].Item1 : 0 -
            DebuffsWithTurn[BonusType.SPEED].Item2 > 0 ? DebuffsWithTurn[BonusType.SPEED].Item1 : 0));
    }

    private void UpdateUI()
    {
        if (!statusUI) statusUI = transform.GetChild(0).GetComponent<AgentStatus>();
        if (!animator) animator = transform.GetChild(1).GetComponent<Animator>();
        statusUI.SetAgentName(Name);
        statusUI.SetAttack(Attack, realAttack - Attack);
        statusUI.SetDefense(Defense, realDefense - Defense);
        statusUI.SetSpeed(Speed, realSpeed - Speed);
        statusUI.SetHealth(curHealth);
    }


    private void ApplyAction(BattleAction action)
    {
        LineDrawer drawLine = Instantiate(lineDrawerPrefab);
        switch (action.Type)
        {
            case ActionType.Damage:
                // Choose a random opponent
                BattleAgent attackedTarget = battleManager.GetRandomAgentFromTeam(!IsPlayer);
                attackedTarget.PhysicalDamage(realAttack);
                // Attack & Defend Animation
                animator.SetTrigger("attack");
                attackedTarget.animator.SetTrigger("defend");
                // Draw a line to better let user see the attack-defense relations
                drawLine.SetLine(transform.position, attackedTarget.transform.position);
                break;
            case ActionType.DamageOverTime:
                // Choose a random opponent
                BattleAgent poinsonedTarget = battleManager.GetRandomAgentFromTeam(!IsPlayer);
                poinsonedTarget.Poisoned(action.Value, action.Duration);
                // Poison & Poisoned Animation
                animator.SetTrigger("poison");
                poinsonedTarget.animator.GetComponent<AnimatorManager>().SetEffects("poisoned", true);
                drawLine.SetLine(transform.position, poinsonedTarget.transform.position);
                break;
            case ActionType.Heal:
                // Choose a random ally
                BattleAgent healedTarget = battleManager.GetRandomAgentFromTeam(IsPlayer);
                healedTarget.HealthChanged(action.Value);
                // Poison & Poisoned Animation
                animator.SetTrigger("heal");
                healedTarget.animator.SetTrigger("healed");
                break;
            case ActionType.HealOverTime:
                // Choose a random ally
                BattleAgent recoveringTarget = battleManager.GetRandomAgentFromTeam(IsPlayer);
                recoveringTarget.HealingOverTime(action.Value, action.Duration);
                // Poison & Poisoned Animation
                animator.SetTrigger("healMagic");
                recoveringTarget.animator.GetComponent<AnimatorManager>().SetEffects("healing", true);
                break;
            case ActionType.Buff:
                // Choose a random ally
                BattleAgent buffedTarget = battleManager.GetRandomAgentFromTeam(IsPlayer);
                buffedTarget.Buffed(action.Bonus, action.Value, action.Duration);
                // Poison & Poisoned Animation
                animator.SetTrigger("buffing");
                buffedTarget.animator.GetComponent<AnimatorManager>().SetEffects("buffed", true);
                break;
            case ActionType.Debuff:
                // Choose a random opponent
                BattleAgent debuffedTarget = battleManager.GetRandomAgentFromTeam(!IsPlayer);
                debuffedTarget.Debuffed(action.Bonus, action.Value, action.Duration);
                // Poison & Poisoned Animation
                animator.SetTrigger("debuffing");
                debuffedTarget.animator.GetComponent<AnimatorManager>().SetEffects("debuffed", true);
                drawLine.SetLine(transform.position, debuffedTarget.transform.position);
                break;
            default:
                Debug.LogError("Undefined action");
                throw new Exception();
        }
    }
}