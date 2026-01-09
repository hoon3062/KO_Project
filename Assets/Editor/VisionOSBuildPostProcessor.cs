#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode; // iOS 및 visionOS용 Xcode API 사용
using System.IO;

public class VisionOSBuildPostProcessor
{
    // 빌드가 끝난 후 자동으로 실행되는 함수
    [PostProcessBuild]
    public static void ChangeXcodePlist(BuildTarget buildTarget, string pathToBuiltProject)
    {
        // 빌드 타겟이 visionOS일 때만 실행
        if (buildTarget == BuildTarget.VisionOS)
        {
            // Info.plist 파일 경로 찾기
            string plistPath = pathToBuiltProject + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            // Root 딕셔너리 가져오기
            PlistElementDict rootDict = plist.root;

            // 1. 파일 공유 활성화 (Application supports iTunes file sharing)
            rootDict.SetBoolean("UIFileSharingEnabled", true);

            // 2. 파일 앱에서 열기 허용 (Supports opening documents in place)
            rootDict.SetBoolean("LSSupportsOpeningDocumentsInPlace", true);

            // 변경 사항 저장
            File.WriteAllText(plistPath, plist.WriteToString());
        }
    }
}
#endif