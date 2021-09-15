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
    /// ステータスアイコンをパネルに入れ、セットアップ
    /// </summary>
    /// <param name="coolTimePanel"></param>
    /// <param name="statusIconPanel"></param>
    public void SetupIcon(GameObject coolTimePanel, GameObject statusIconPanel)
    {
        //statusIconをセット //GMの方で生成はやってもらう？
        m_statusIcon = Instantiate(m_statusIconPrefab, statusIconPanel.transform).GetComponent<StatusIconController>();
        //SetStatusIcon(statusIcon.GetComponent<StatusIconController>());
        base.SetupIcon(coolTimePanel);
    }

    ///// <summary>
    ///// StatusIconをセット
    ///// </summary>
    ///// <param name="statusIconController"></param>
    //void SetStatusIcon(StatusIconController statusIconController)
    //{
    //    m_statusIcon = statusIconController;
    //}

    /// <summary>
    /// 行動開始(味方)
    /// </summary>
    public override void BeginAction()
    {
        base.BeginAction();
        BattleManager.Instance.BeginCommandSelect(m_havesSkills, this);
        //m_backCamera.gameObject.SetActive(true);
        BattleManager.Instance.BeginBackCamera(this.transform);
    }
    protected override void EndAction()
    {
        base.EndAction();
        //m_backCamera.gameObject.SetActive(false);
        BattleManager.Instance.EndBackCamera();
    }

    /// <summary>
    /// 行動コマンド(Player)
    /// </summary>
    public void PlayerActionCommand(SkillDatabase skill)
    {
        BattleManager.Instance.EndCommandSelect();
        //N
        //m_CurrentSkill = skill;
        skill.Effect(this, FindObjectsOfType<BattleEnemyController>());
        UseSkill(skill);
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
