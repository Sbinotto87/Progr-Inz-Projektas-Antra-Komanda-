using UnityEngine;

/// <summary>
/// Top-of-screen boss HP bar (IMGUI). Drop on the same GameObject as BossSpawner,
/// or any persistent object. Auto-finds the spawner.
///
/// Replace with a proper UGUI bar later by reading BossSpawner.CurrentBoss.health.
/// </summary>
public class BossHealthBarUI : MonoBehaviour
{
    [SerializeField] private float barWidthFraction = 0.6f;
    [SerializeField] private float barHeight = 28f;
    [SerializeField] private float barTopOffset = 16f;

    private BossSpawner spawner;
    private Texture2D bgTex;
    private Texture2D fgTex;

    private void Start()
    {
        spawner = UnityEngine.Object.FindFirstObjectByType<BossSpawner>();
        bgTex = MakeTex(new Color(0f, 0f, 0f, 0.7f));
        fgTex = MakeTex(new Color(0.85f, 0.1f, 0.1f, 0.95f));
    }

    private Texture2D MakeTex(Color c)
    {
        var t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }

    private void OnGUI()
    {
        if (spawner == null) { spawner = UnityEngine.Object.FindFirstObjectByType<BossSpawner>(); return; }
        if (!spawner.HasActiveBoss) return;

        var boss = spawner.CurrentBoss;
        if (boss == null) return;

        float width = Screen.width * barWidthFraction;
        float x = (Screen.width - width) * 0.5f;
        float y = barTopOffset;

        // Background
        GUI.DrawTexture(new Rect(x, y, width, barHeight), bgTex);

        // Fill
        float frac = Mathf.Clamp01(boss.health / Mathf.Max(0.01f, boss.maxHealth));
        GUI.DrawTexture(new Rect(x + 2, y + 2, (width - 4) * frac, barHeight - 4), fgTex);

        // Label
        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
        string phase = boss.CurrentPhase.phaseName;
        GUI.Label(new Rect(x, y, width, barHeight),
            $"{boss.bossName}  -  {Mathf.CeilToInt(boss.health)} / {Mathf.CeilToInt(boss.maxHealth)}  ({phase})", style);
    }
}
