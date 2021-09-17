using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 味方戦闘時の行動
/// </summary>
public class BattlePlayerController : BattleStatusControllerBase
{
    [SerializeField] GameObject m_statusIconPrefab = default;

    //[SerializeField] CinemachineVirtualCamera m_backCamera = default;
    //[SerializeField] ParticleSystem m_fireSwordParticle;

    /// <summary>
    /// ステータスアイコンを生成
    /// </summary>
    /// <param name="statusIconPanel"></param>
    public void InstantiateStatusIcon(GameObject statusIconPanel)
    {
        m_statusIcon = Instantiate(m_statusIconPrefab, statusIconPanel.transform).GetComponent<StatusIconController>();
    }

    /// <summary>
    /// ステータスアイコンをセットアップ(player)
    /// </summary>
    /// <param name="coolTimePanel"></param>
    public override void SetupIcon(GameObject coolTimePanel)
    {
        base.SetupIcon(coolTimePanel);
        m_statusIcon.GetComponent<PlayerStatusIconController>().ActInterruptMethod = ActInterrupt;
    }

    /// <summary>
    /// 行動開始(味方)
    /// </summary>
    public override void BeginAction()
    {
        base.BeginAction();
        BattleManager.Instance.BeginPlayerCommandSelect();
        //m_backCamera.gameObject.SetActive(true);
        BattleManager.Instance.BeginPlayerBackCamera(this.transform);
    }
    /// <summary>
    /// 行動終了（player）
    /// </summary>
    public override void EndAction()
    {
        base.EndAction();
        //m_backCamera.gameObject.SetActive(false);
        BattleManager.Instance.EndPlayerBackCamera();
    }

    /// <summary>
    /// 行動(Player)
    /// </summary>
    public void PlayerAction(SkillDatabase skill, BattleStatusControllerBase[] targets)
    {
        //BattleManager.Instance.EndCommandSelect();//EndTargerSelect
        //N
        //m_CurrentSkill = skill;
       // skill.Effect(this, FindObjectsOfType<BattleEnemyController>());
        UseSkill(skill, targets);
        //UseSP(m_CurrentSkill.m_CostSP);
        //if (m_CurrentSkill.m_FireEffect)
        //{
        //    m_fireSwordParticle.gameObject.SetActive(true);
        //}
        //PlayStateAnimator(m_CurrentSkill);
    }

    //N
    // アニメイベント    
    //public override void Hit(BattleStatusControllerBase target = null)
    //{
    //    BattleStatusControllerBase thisTarget = FindObjectOfType<BattleEnemyController>();
    //    base.Hit(thisTarget);
    //    if (m_CurrentSkill.m_FireEffect)
    //    {
    //        m_fireSwordParticle.gameObject.SetActive(false);
    //    }
    //}

    //public override void End()
    //{
    //    if (m_CurrentSkill.m_FireEffect)
    //    {
    //        m_fireSwordParticle.gameObject.SetActive(false);
    //    }
    //    base.End();
    //}
}
