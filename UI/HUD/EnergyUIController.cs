using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EnergyUIController : MonoBehaviour
{
    [SerializeField] private Slider energyBarPrefabLong;
    [SerializeField] private Slider energyBarPrefabShort;
    [SerializeField] private Transform energyBarContainer;

    private List<Slider> energyBars = new List<Slider>();
    private Coroutine[] refillCoroutines;
    private float dashCooldown;
    private int currentlyFillingIndex = -1;
    private bool dashInProgress = false;

    public void Init(float cooldown, int dashCount)
    {
        dashCooldown = cooldown;

        // 리스트 먼저 클리어
        energyBars.Clear();

        // 기존 UI 제거 (Destroy 전에 리스트 비워야 참조 문제 안 생김)
        foreach (Transform child in energyBarContainer)
        {
            Destroy(child.gameObject);
        }

        // 새 UI 생성
        if (dashCount == 3)
        {
            for (int i = 0; i < dashCount; i++)
            {
                Slider bar = Instantiate(energyBarPrefabShort, energyBarContainer);
                bar.value = 1f;
                energyBars.Add(bar);
            }
        }
        else
        {
            for (int i = 0; i < dashCount; i++)
            {
                Slider bar = Instantiate(energyBarPrefabLong, energyBarContainer);
                bar.value = 1f;
                energyBars.Add(bar);
            }
        }

        refillCoroutines = new Coroutine[dashCount];
        currentlyFillingIndex = -1;
    }

    public bool RequestDash()
    {
        if (dashInProgress) return false;

        if (UseEnergy())
        {
            dashInProgress = true;
            return true;
        }
        return false;
    }

    public void EndDash()
    {
        dashInProgress = false;
    }

    private bool UseEnergy()
    {
        for (int i = energyBars.Count - 1; i >= 0; i--)
        {
            if (energyBars[i].value >= 0.9999f && refillCoroutines[i] == null)
            {
                float carryOverProgress = -1f;
                int fromIndex = currentlyFillingIndex;

                if (fromIndex >= 0 && refillCoroutines[fromIndex] != null)
                {
                    carryOverProgress = energyBars[fromIndex].value;
                    StopCoroutine(refillCoroutines[fromIndex]);
                    refillCoroutines[fromIndex] = null;
                    energyBars[fromIndex].value = 0f;
                    currentlyFillingIndex = -1;
                }

                energyBars[i].value = carryOverProgress >= 0f ? carryOverProgress : 0f;

                refillCoroutines[i] = StartCoroutine(RefillEnergy(i, energyBars[i].value));
                currentlyFillingIndex = i;

                return true;
            }
        }
        return false;
    }

    private void TryStartRefillNext()
    {
        for (int i = 0; i < energyBars.Count; i++)
        {
            if (energyBars[i].value <= 0f && refillCoroutines[i] == null)
            {
                currentlyFillingIndex = i;
                refillCoroutines[i] = StartCoroutine(RefillEnergy(i, energyBars[i].value));
                break;
            }
        }
    }

    private IEnumerator RefillEnergy(int index, float startValue)
    {
        float time = startValue * dashCooldown;

        while (time < dashCooldown)
        {
            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / dashCooldown);
            energyBars[index].value = Mathf.Lerp(startValue, 1f, progress);
            yield return null;
        }

        energyBars[index].value = 1f;
        refillCoroutines[index] = null;
        currentlyFillingIndex = -1;
        TryStartRefillNext();
    }

    public void ForceUseAndResetEnergy()
    {
        for (int i = 0; i < energyBars.Count; i++)
        {
            if (energyBars[i].value >= 0.9999f && refillCoroutines[i] == null)
            {
                energyBars[i].value = 0f;

                for (int j = 0; j < energyBars.Count; j++)
                {
                    if (refillCoroutines[j] != null)
                    {
                        StopCoroutine(refillCoroutines[j]);
                        refillCoroutines[j] = null;
                    }

                    if (j != i)
                        energyBars[j].value = 0f;
                }

                currentlyFillingIndex = -1;
                TryStartRefillNext();
                break;
            }
        }
    }
}
