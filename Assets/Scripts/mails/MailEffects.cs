using UnityEngine;

public static class MailEffects
{
    public static int Apply(OwnerProfile owner, MailAgency agency)
    {
        if (owner == null) return 0;

        int houses = owner.HouseCount;

        switch (agency)
        {
            // 1) Witch:
            // Removes ALL houses
            // Happiness = -(houses * 10)
            case MailAgency.Witch:
                RemoveAll(owner);
                return -(houses * 10);

            // 2) Department of Experiments:
            // Removes 1 house.
            // For each extra house, 50% chance another is removed.
            // Happiness = -(removed * 8)
            case MailAgency.Experiments:
            {
                int removed = RemoveExperiments(owner);
                return -(removed * 8);
            }

            // 3) Housing Authority:
            // Shrinks ALL houses
            // Happiness = -(houses * 3)
            case MailAgency.HousingAuthority:
                ShrinkAll(owner);
                return -(houses * 3);

            // 4) Bank:
            // If rich: remove 1 house, Happiness = -10
            // If poor: decorate ALL houses, Happiness = +(houses * 5)
            case MailAgency.Bank:
                if (owner.IsRich())
                {
                    RemoveOneActive(owner);
                    return -10;
                }
                else
                {
                    DecorateAll(owner);
                    return +(houses * 5);
                }

            // 5) City Registry:
            // If too many houses for money: shrink ALL, Happiness = -(houses * 5)
            // Else: decorate ALL, Happiness = +(houses * 5)
            case MailAgency.CityRegistry:
                if (owner.Overextended())
                {
                    ShrinkAll(owner);
                    return -(houses * 5);
                }
                else
                {
                    DecorateAll(owner);
                    return +(houses * 5);
                }
        }

        return 0;
    }

    private static void RemoveAll(OwnerProfile owner)
    {
        if (owner.houses == null) return;
        foreach (var h in owner.houses)
            if (h != null) h.gameObject.SetActive(false);
    }

    private static int RemoveExperiments(OwnerProfile owner)
    {
        int removed = 0;
        int houses = owner.HouseCount;
        if (houses <= 0) return 0;

        // always remove 1 if possible
        if (RemoveOneActive(owner)) removed++;

        // for each extra house -> 50% chance remove another
        for (int i = 1; i < houses; i++)
        {
            if (Random.value < 0.5f)
            {
                if (RemoveOneActive(owner)) removed++;
            }
        }

        return removed;
    }

    private static bool RemoveOneActive(OwnerProfile owner)
    {
        if (owner.houses == null) return false;
        foreach (var h in owner.houses)
        {
            if (h != null && h.gameObject.activeSelf)
            {
                h.gameObject.SetActive(false);
                return true;
            }
        }
        return false;
    }

    private static void ShrinkAll(OwnerProfile owner)
    {
        if (owner.houses == null) return;
        foreach (var h in owner.houses)
            if (h != null && h.gameObject.activeSelf)
                h.transform.localScale *= 0.5f;
    }

    private static void DecorateAll(OwnerProfile owner)
    {
        if (owner.houses == null) return;
        foreach (var h in owner.houses)
            if (h != null && h.gameObject.activeSelf)
                h.Decorate();
    }
}
