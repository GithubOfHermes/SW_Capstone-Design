using UnityEngine;

public class PlayerStateService : MonoBehaviour, IPlayerStateService
{
    private GameObject player;
    private PlayerController pc;
    public static int savedHp = -1;
    private void Awake()
    {
        player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            pc = player.GetComponent<PlayerController>();
        }
    }

    public void DisablePlayer()
    {
        if (pc != null)
        {
            pc.enabled = false;
        }
    }

    public void EnablePlayer()
    {
        if (pc != null)
        {
            pc.enabled = true;
        }
    }
}
