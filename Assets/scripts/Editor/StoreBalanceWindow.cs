#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class StoreBalanceWindow : EditorWindow
{
    private const string ResourcesPath = "/Resources/Store items/";

    private StoreItemData[] _items = {};
    private Vector2 _scroll;
    private float _goldPerGame = 100f;

    [MenuItem("Tools/Match-3/Store Balance")]
    private static void Open() => GetWindow<StoreBalanceWindow>("Store Balance");

    private void OnEnable() => Reload();

    private void OnProjectChange()
    {
        Reload();
        Repaint();
    }

    private void OnSelectionChange() => Repaint();

    private void Reload() => _items = AssetDatabase.FindAssets("t:StoreItemData")
        .Select(g => AssetDatabase.LoadAssetAtPath<StoreItemData>(AssetDatabase.GUIDToAssetPath(g)))
        .OrderBy(i => i.category).ThenBy(i => i.cost).ToArray();

    private IEnumerable<string> Issues() => _items
        .SelectMany(i => StoreBalance.Validate(i).Concat(PathIssue(i)).Select(s => $"{i.name}: {s}"))
        .Concat(StoreBalance.ValidateAll(_items))
        .Concat(StoreBalance.ValidateAll(_items))
        .Concat(StoreBalance.ValidatePricing(_items));

    private static IEnumerable<string> PathIssue(StoreItemData i)
    {
        if (!AssetDatabase.GetAssetPath(i).Contains(ResourcesPath)) yield return "вне Resources/Store items, игра его не загрузит";
    }

    private void OnGUI()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            if (GUILayout.Button("Обновить", EditorStyles.toolbarButton, GUILayout.Width(70))) Reload();
            GUILayout.FlexibleSpace();
        }

        _goldPerGame = Mathf.Max(1f, EditorGUILayout.FloatField("Среднее золото за игру", _goldPerGame));

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        DrawIssues();
        DrawHeader();
        foreach (var i in _items) DrawRow(i);
        EditorGUILayout.EndScrollView();
    }

    private void DrawIssues()
    {
        var list = Issues().ToList();
        if (list.Count == 0) EditorGUILayout.HelpBox("Проблем не найдено", MessageType.Info);
        foreach (var s in list) EditorGUILayout.HelpBox(s, MessageType.Warning);
    }

    private static void Cell(string text, float width, GUIStyle style = null) =>
        GUILayout.Label(text, style ?? EditorStyles.label, GUILayout.Width(width));

    private static void DrawHeader()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            Cell("Предмет", 150, EditorStyles.boldLabel);
            Cell("Тип", 60, EditorStyles.boldLabel);
            Cell("Цена", 60, EditorStyles.boldLabel);
            Cell("Прокачка", 80, EditorStyles.boldLabel);
            Cell("Итого", 70, EditorStyles.boldLabel);
            Cell("Игр", 70, EditorStyles.boldLabel);
            Cell("Окупаемость", 90, EditorStyles.boldLabel);
        }
    }

    private void DrawRow(StoreItemData i)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button(i.name, EditorStyles.linkLabel, GUILayout.Width(150))) Selection.activeObject = i;
            Cell(i.category.ToString(), 60);
            Cell(i.cost.ToString(), 60);
            Cell(StoreBalance.UpgradeTotal(i).ToString(), 80);
            Cell(StoreBalance.Total(i).ToString(), 70);
            Cell(StoreBalance.GamesToAfford(i, _goldPerGame).ToString("0.#"), 70);
            Cell(i.effect == BoosterEffect.GoldBonus ? StoreBalance.PurchasePayback(i, _goldPerGame).ToString("0.#") : "-", 90);
        }
        if (Selection.activeObject == i && i.Upgradable) DrawCurve(i);
    }

    private void DrawCurve(StoreItemData i)
    {
        EditorGUI.indentLevel++;
        var steps = Mathf.Min(i.statValues.Length - 1, i.upgradeCosts.Length);
        for (var s = 0; s < steps; s++)
        {
            var payback = i.effect == BoosterEffect.GoldBonus
                ? $"  |  окупится за {StoreBalance.PaybackGames(i, s, _goldPerGame):0.#} игр"
                : "";
            EditorGUILayout.LabelField($"LVL {s + 1} → {s + 2}",
                $"{i.statValues[s]:0.##} → {i.statValues[s + 1]:0.##}{i.statSuffix}  |  {i.upgradeCosts[s]} зол.  |  {StoreBalance.CostPerPoint(i, s):0.#} зол/ед.{payback}");
        }
        DrawGraph(Enumerable.Range(0, steps).Select(s => StoreBalance.CostPerPoint(i, s))
            .Select(v => float.IsInfinity(v) ? 0f : v).ToArray());
        EditorGUI.indentLevel--;
    }

    private static void DrawGraph(float[] values)
    {
        var rect = GUILayoutUtility.GetRect(0f, 80f, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.2f));
        if (values.Length < 2) return;

        var max = Mathf.Max(values.Max(), 0.0001f);
        var pts = values.Select((v, k) => new Vector3(
            rect.x + rect.width * k / (values.Length - 1),
            rect.yMax - rect.height * v / max)).ToArray();

        Handles.color = Color.yellow;
        Handles.DrawAAPolyLine(2f, pts);
    }
}
#endif
