using UnityEngine;

[RequireComponent(typeof(CharacterRole))]
public class PlayerStatsDebugUI : MonoBehaviour
{
    [SerializeField] private bool showDebugGui = true;
    [SerializeField] private Vector2 panelSize = new Vector2(220f, 260f);
    [SerializeField] private Vector2 panelMargin = new Vector2(10f, 10f);

    private CharacterRole characterRole;

    private void Awake()
    {
        characterRole = GetComponent<CharacterRole>();
    }

    private void OnGUI()
    {
        if (!showDebugGui)
        {
            return;
        }

        Rect panelRect = new Rect(
            Mathf.Max(panelMargin.x, Screen.width - panelSize.x - panelMargin.x),
            panelMargin.y,
            panelSize.x,
            panelSize.y);

        GUILayout.BeginArea(panelRect, GUI.skin.box);
        GUILayout.Label("Player Stats");

        if (characterRole == null || characterRole.Stats == null)
        {
            GUILayout.Label("Player stats not found");
            GUILayout.EndArea();
            return;
        }

        PlayerStats stats = characterRole.Stats;
        GUILayout.Label($"Power: {stats.Power}");
        GUILayout.Label($"Jump: {stats.Jump}");
        GUILayout.Label($"Receive: {stats.Receive}");
        GUILayout.Label($"Set: {stats.Set}");
        GUILayout.Label($"Block: {stats.Block}");
        GUILayout.Label($"Serve: {stats.Serve}");
        GUILayout.Label($"Speed: {stats.Speed}");
        GUILayout.Label($"Stamina: {stats.Stamina}");
        GUILayout.Label($"Mental: {stats.Mental}");
        GUILayout.EndArea();
    }
}
