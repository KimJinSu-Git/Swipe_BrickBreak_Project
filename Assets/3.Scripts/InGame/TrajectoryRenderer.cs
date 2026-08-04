using UnityEngine;

namespace Bird.InGame
{
    [RequireComponent(typeof(LineRenderer))]
    public class TrajectoryRenderer : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Transform arrowTransform;

        [Header("Settings")] 
        [SerializeField] private LayerMask wallLayer;
        [SerializeField] private float maxLineLength = 3f;

        private void Awake()
        {
            if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
            
            HideLine();
        }

        public void ShowLine()
        {
            lineRenderer.enabled = true;
            if (arrowTransform != null) arrowTransform.gameObject.SetActive(true);
        }

        public void HideLine()
        {
            lineRenderer.enabled = false;
            if (arrowTransform != null) arrowTransform.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 방향을 받아 벽에 부딪히면 V자로 꺾이는 궤적과 화살표를 그립니다.
        /// </summary>
        public void DrawTrajectory(Vector2 startPos, Vector2 direction)
        {
            // Raycast로 벽 충돌 검사
            RaycastHit2D hit = Physics2D.Raycast(startPos, direction, maxLineLength, wallLayer);

            if (hit.collider != null)
            {
                // 벽에 부딪힌 경우 (V자 반사)
                lineRenderer.positionCount = 3;
                lineRenderer.SetPosition(0, startPos);
                lineRenderer.SetPosition(1, hit.point); // 꺾이는 지점

                // 입사 방향과 벽의 수직선을 이용해 반사 방향 계산
                Vector2 reflectDir = Vector2.Reflect(direction, hit.normal);
                float remainingLength = maxLineLength - hit.distance; // 남은 길이
                Vector2 endPos = hit.point + (reflectDir * remainingLength);
                
                lineRenderer.SetPosition(2, endPos);
                
                UpdateArrow(endPos, reflectDir);
            }
            else
            {
                // 부딪히지 않은 경우 (직선)
                lineRenderer.positionCount = 2;
                Vector2 endPos = startPos + (direction * maxLineLength);
                
                lineRenderer.SetPosition(0, startPos);
                lineRenderer.SetPosition(1, endPos);

                UpdateArrow(endPos, direction);
            }
        }
        
        private void UpdateArrow(Vector2 position, Vector2 direction)
        {
            if (arrowTransform == null) return;
            
            arrowTransform.position = position;
            
            // 방향(Vector)을 각도(Degree)로 변환하여 화살표 회전
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrowTransform.rotation = Quaternion.Euler(0, 0, angle+40);
        }
        
        public void DrawLine(Vector2 startPos, Vector2 endPos)
        {
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
        }
    }
}
