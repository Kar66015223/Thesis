using System.Linq;
using UnityEngine;

public class Tackle : Skill
{
    private TackleData data;

    private bool isDashing = false;
    private float dashElapsed = 0f;

    private Vector3 dashStart;
    private Vector3 dashTargetPoint;
    private Vector3 dashVelocity;
    private float dashDurationLocal;
    private float dashRangeLocal;
    private float clearanceOffset = 0.12f; // tweak in inspector if needed

    private EnemyController target;

    public Tackle(TackleData data, SkillContext context)
    {
        this.data = data;
        skillName = data.skillName;
        skillDesc = data.skillDesc;
        type = data.type;
        usage = data.usage;
        coolDown = data.coolDown;
        cost = data.cost;
        icon = data.icon;

        this.context = context;
    }

    public override void Use()
    {
        if (isDashing /* || target == null */)
            return;
            
        isDashing = true;
        dashElapsed = 0f;
    }

    public override void Tick(float deltaTime)
    {
        // Collider[] cols = Physics.OverlapSphere(
        //     context.User.transform.position, data.dashRange, data.enemyLayer);

        // if (cols.Length == 0) 
        //     return;

        // Collider best = cols.OrderBy(
        //     c => Vector3.Distance(context.User.transform.position, c.transform.position)).First();

        // target = best.GetComponent<EnemyController>();
        
        if (!isDashing /* || target == null */)
            return;

        dashElapsed += Time.deltaTime;
        float speed = data.dashRange / data.dashDuration;
        PlayerController playerCtrl = context.User.GetComponent<PlayerController>();

        if (context.User.TryGetComponent(out CharacterController charCtrl) && playerCtrl != null)
        {
            playerCtrl.SetCanMove(false);
            charCtrl.Move(deltaTime * speed * playerCtrl.GetForward());
        }

        if (dashElapsed >= data.dashDuration)
        {
            isDashing = false;
            playerCtrl.SetCanMove(true);
        }
    }

    public override void HoldUse(bool isHolding) {}
    public override void HandleUse() {}
}