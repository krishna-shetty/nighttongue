using UnityEngine;

public class BossAttackAnimationIntermediary : MonoBehaviour
{
    public void SetSlam()
    {
        Debug.Log("intermediary slam");
        BossAttack.Instance.SetSlam();
    }
}
