using UnityEngine;
using UnityEditor;
using System.IO;

public class CSVToSOConverter : EditorWindow
{
    // 유니티 상단 메뉴에 버튼을 만듭니다.
    [MenuItem("Overwrite/Convert CSV to SO")]
    public static void BuildSO()
    {
        // CSV 파일 경로
        string csvPath = "Assets/_Project/Data/SkillTable.csv";

        // CSV 파일의 모든 줄을 읽어옵니다.
        string[] lines = File.ReadAllLines(csvPath);

        // 첫 번째 줄은 건너뛰고 데이터를 파싱합니다.
        for (int i = 1; i < lines.Length; i++)
        {
            string[] data = lines[i].Split(',');

            // ScriptableObject 인스턴스 생성
            SkillData asset = ScriptableObject.CreateInstance<SkillData>();
            asset.ID = int.Parse(data[0]);
            asset.Name = data[1];
            asset.AP_Cost = int.Parse(data[2]);
            asset.Power = float.Parse(data[3]);
            asset.CommandKey = data[4];

            // 저장할 경로 설정 및 에셋 생성
            string savePath = $"Assets/_Project/Resources/Skills/Skill_{asset.ID}.asset";

            // 폴더가 없으면 생성 (Resources/Skills)
            if (!Directory.Exists("Assets/_Project/Resources/Skills"))
                Directory.CreateDirectory("Assets/_Project/Resources/Skills");

            AssetDatabase.CreateAsset(asset, savePath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("모든 CSV 데이터가 ScriptableObject로 변환되었습니다!");
    }
}