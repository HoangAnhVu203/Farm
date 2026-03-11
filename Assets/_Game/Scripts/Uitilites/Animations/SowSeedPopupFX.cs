using DG.Tweening;
using UnityEngine;

public class SowSeedPopupFX : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Anim")]
    [SerializeField] private float startHeight = 0.7f;
    [SerializeField] private float fallDuration = 0.35f;
    [SerializeField] private float bounceHeight = 0.12f;
    [SerializeField] private float endDuration = 0.15f;
    [SerializeField] private float startScale = 0.65f;
    [SerializeField] private float endScale = 1f;

    public void Play(Sprite seedSprite)
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null || seedSprite == null)
        {
            Destroy(gameObject);
            return;
        }

        spriteRenderer.sprite = seedSprite;
        spriteRenderer.color = Color.white;

        Vector3 groundPos = transform.position;
        Vector3 startPos = groundPos + Vector3.up * startHeight;

        transform.position = startPos;
        transform.localScale = Vector3.one * startScale;

        Sequence seq = DOTween.Sequence();

        seq.Join(transform.DOMoveY(groundPos.y, fallDuration).SetEase(Ease.InQuad));
        seq.Join(transform.DOScale(endScale, fallDuration).SetEase(Ease.OutBack));

        seq.Append(transform.DOMoveY(groundPos.y + bounceHeight, endDuration).SetEase(Ease.OutQuad));
        seq.Append(transform.DOMoveY(groundPos.y, endDuration).SetEase(Ease.InQuad));

        seq.AppendInterval(0.05f);
        seq.Join(spriteRenderer.DOFade(0f, 0.15f));

        seq.OnComplete(() => Destroy(gameObject));
    }
}