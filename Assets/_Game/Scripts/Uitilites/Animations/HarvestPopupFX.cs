using DG.Tweening;
using UnityEngine;

public class HarvestPopupFX : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float jumpHeight = 0.8f;
    [SerializeField] private float duration = 0.7f;
    [SerializeField] private float startScale = 0.3f;
    [SerializeField] private float endScale = 1.0f;

    public void Play(Sprite icon)
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Destroy(gameObject);
            return;
        }

        spriteRenderer.sprite = icon;
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * jumpHeight;

        transform.localScale = Vector3.one * startScale;

        Sequence seq = DOTween.Sequence();

        seq.Join(transform.DOScale(endScale, 0.18f).SetEase(Ease.OutBack));
        seq.Join(transform.DOMoveY(endPos.y, duration).SetEase(Ease.OutQuad));
        seq.Join(spriteRenderer.DOFade(0f, duration).SetEase(Ease.InQuad));

        seq.OnComplete(() => Destroy(gameObject));
    }
}