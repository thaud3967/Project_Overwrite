using UnityEngine;
using UnityEditor;
using System.IO;

public class CSVToSOConverter : EditorWindow
{
    [MenuItem("Overwrite/Convert CSV to SO")]
    public static void BuildSO()
    {
        string csvPath = "Assets/_Project/Data/SkillTable.csv";
        string[] lines = File.ReadAllLines(csvPath);

        for (int i = 1; i < lines.Length; i++)
        {
            string[] data = lines[i].Split(',');

            SkillData asset = ScriptableObject.CreateInstance<SkillData>();

            // 엑셀 컬럼 순서와 정확히 매칭
            asset.ID = int.Parse(data[0]);           // 0번: ID
            asset.Name = data[1];                   // 1번: Name
            asset.Description = data[2];            // 2번: Description
            asset.AP_Cost = int.Parse(data[3]);      // 3번: AP_Cost
            asset.Power = float.Parse(data[4]);     // 4번: Power
            asset.CoolTime = int.Parse(data[5]);     // 5번: CoolTime
            asset.CommandKey = data[6];             // 6번: CommandKey

            string savePath = $"Assets/_Project/Resources/Skills/Skill_{asset.ID}.asset";

            if (!Directory.Exists("Assets/_Project/Resources/Skills"))
                Directory.CreateDirectory("Assets/_Project/Resources/Skills");

            AssetDatabase.CreateAsset(asset, savePath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CSV 데이터가 새로운 규격에 맞춰 ScriptableObject로 변환되었습니다!");
    }
}