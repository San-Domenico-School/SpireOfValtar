// v1 Gleb created
// Tracks which spells the player has purchased. Starts with no spells unlocked.
// Other systems check this to decide whether a spell can be cast or shown in the HUD.

using UnityEngine;

public class SpellInventory : MonoBehaviour
{
    public static SpellInventory Instance { get; private set; }

    // 0=Lightning, 1=Fireball, 2=Freeze, 3=Dash
    private bool[] unlockedSpells = new bool[4];

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool IsUnlocked(int spellIndex)
    {
        if (spellIndex < 0 || spellIndex >= unlockedSpells.Length) return false;
        return unlockedSpells[spellIndex];
    }

    public void UnlockSpell(int spellIndex)
    {
        if (spellIndex < 0 || spellIndex >= unlockedSpells.Length) return;
        unlockedSpells[spellIndex] = true;
        Debug.Log($"[SpellInventory] Spell {spellIndex} unlocked.");
    }

    public int UnlockedCount()
    {
        int count = 0;
        foreach (bool b in unlockedSpells) if (b) count++;
        return count;
    }

    public void Reset()
    {
        unlockedSpells = new bool[4];
        Debug.Log("[SpellInventory] Reset: all spells locked.");
    }
}
