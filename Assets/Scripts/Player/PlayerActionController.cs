using UnityEngine;

[RequireComponent(typeof(SpikeTargetingController))]
public class PlayerActionController : MonoBehaviour
{
    [SerializeField] private Transform ball;
    [SerializeField] private float spikeRange = 2f;
    [SerializeField] private bool showSpikeRange = true;
    [SerializeField] private Color spikeRangeColor = new Color(0f, 0.75f, 1f, 0.7f);
    [SerializeField] private Color spikeReadyColor = new Color(0f, 1f, 0.25f, 0.9f);
    [SerializeField] private SpikeTargetingController spikeTargetingController;

    private void Awake()
    {
        EnsureReferences();
    }

    private void EnsureReferences()
    {
        if (spikeTargetingController == null)
        {
            spikeTargetingController = GetComponent<SpikeTargetingController>();
        }

        if (ball == null)
        {
            GameObject ballObject = GameObject.Find("volleyball");
            ball = ballObject != null ? ballObject.transform : null;
        }
    }

    public void HandleActionQ()
    {
        if (TryServe())
        {
            return;
        }

        if (TryBlock())
        {
            return;
        }

        if (TryReceive())
        {
            return;
        }

        if (TrySet())
        {
            return;
        }

        TrySpike();
    }

    private bool TryReceive()
    {
        // TODO: 공이 낮으며, 수비 상황일 때 Receive를 실행한다.
        return false;
    }

    private bool TrySet()
    {
        // TODO: 공이 높으며, 플레이어가 세터일 경우 Set을 실행한다.
        return false;
    }

    private bool TrySpike()
    {
        // TODO: 공격 턴인지 확인한다.
        // TODO: 공이 높은 상태인지 확인한다.
        // TODO: 실제 스파이크 동작은 타격 지점 선택이 끝난 뒤 실행한다.
        EnsureReferences();

        if (ball == null || spikeTargetingController == null)
        {
            return false;
        }

        if (!IsBallInSpikeRange())
        {
            return false;
        }

        spikeTargetingController.EnterTargetingMode();
        return true;
    }

    private bool TryBlock()
    {
        // TODO: 네트 근처이며, 수비 상황이고, 상대가 공격할 때 Block을 실행한다.
        return false;
    }

    private bool TryServe()
    {
        // TODO: 서브 위치일 때 Serve를 실행한다.
        return false;
    }

    private void OnDrawGizmos()
    {
        if (!showSpikeRange)
        {
            return;
        }

        Gizmos.color = IsBallInSpikeRange() ? spikeReadyColor : spikeRangeColor;
        Gizmos.DrawWireSphere(transform.position, spikeRange);
    }

    private bool IsBallInSpikeRange()
    {
        Transform targetBall = ball;
        if (targetBall == null)
        {
            GameObject ballObject = GameObject.Find("volleyball");
            targetBall = ballObject != null ? ballObject.transform : null;
        }

        return targetBall != null && Vector3.Distance(transform.position, targetBall.position) <= spikeRange;
    }
}
