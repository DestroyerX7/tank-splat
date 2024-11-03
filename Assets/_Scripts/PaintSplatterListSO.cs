using UnityEngine;

[CreateAssetMenu(menuName = "Paint Splatter List", fileName = "New Paint Splatter List")]
public class PaintSplatterListSO : ScriptableObject
{
    [field: SerializeField] public Sprite[] PaintSplatterSprites { get; private set; }
    [field: SerializeField] public AudioClip[] PaintSplatterAudioClips { get; private set; }


    public int GetRandSpriteIndex()
    {
        return Random.Range(0, PaintSplatterSprites.Length);
    }

    public AudioClip GetRandAudioClip()
    {
        return PaintSplatterAudioClips[Random.Range(0, PaintSplatterAudioClips.Length)];
    }
}
