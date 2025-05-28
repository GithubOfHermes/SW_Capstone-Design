using UnityEngine;
using System.Collections;

public class EnemyIntroManager : MonoBehaviour
{
    [SerializeField] private GameObject[] enemies;

    public IEnumerator DisableEnemiesSequentially()
    {
        foreach (GameObject enemy in enemies)
        {
            yield return new WaitForSeconds(0.5f);
            if (enemy != null)
                enemy.SetActive(false);
        }
    }
}