using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private float disappearTimer;
    private Color textColor;
    private Vector3 moveVector;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    public void Setup(float damageAmount, bool isCritical)
    {
        textMesh.text = damageAmount.ToString("0"); // 소수점 제거

        if (isCritical) // 크리티컬이면 폰트 크기와 색상을 다르게
        {
            textMesh.fontSize = 45;
            textColor = Color.red;
        }
        else
        {
            textMesh.fontSize = 30;
            textColor = Color.yellow;
        }

        textMesh.color = textColor;
        disappearTimer = 1f; // 1초 뒤 사라짐
        moveVector = new Vector3(0, 100f, 0); // 위로 올라가는 속도
    }

    private void Update()
    {
        //  위로 이동
        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * 2f * Time.deltaTime; // 점점 느려지게

        //  서서히 투명해지기
        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            float disappearSpeed = 3f;
            textColor.a -= disappearSpeed * Time.deltaTime;
            textMesh.color = textColor;

            if (textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}