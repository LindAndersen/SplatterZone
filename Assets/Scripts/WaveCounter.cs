using UnityEngine;

public class WaveCounter : MonoBehaviour
{
    public void SetWave()
    {
        UIManager.Instance.AnimationKeyFrameSetWaveCounter();
    }
}
