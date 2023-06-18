using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    private void OnSpawnFinished() => FindObjectOfType<BossAI>().StartBossScene();

    private void OnDeathFinished() => FindObjectOfType<BossAI>().Despawn();
}
