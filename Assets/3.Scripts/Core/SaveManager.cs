using System;
using System.IO;
using System.Threading.Tasks;
using Bird.Data;
using UnityEngine;
using Newtonsoft.Json;

namespace Bird.Core
{
    public class SaveManager : MonoBehaviour
    {
        private const string SAVE_FILE_NAME = "SwipeBrickSave.json";
        
        private string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        
        public async Task SaveGameAsync(SaveData data)
        {
            try
            {
                // Newtonsoft.Json을 사용하여 데이터를 포맷팅된 문자열로 직렬화합니다.
                string json = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
                
                // 백그라운드 스레드에서 파일 쓰기 작업을 비동기로 수행하여 메인 스레드 렉을 방지합니다.
                using StreamWriter writer = new StreamWriter(SaveFilePath);
                await writer.WriteAsync(json);
                
                Debug.Log($"[SaveManager] 데이터 저장 완료! 경로: {SaveFilePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] 데이터 저장 실패: {e.Message}");
            }
        }
        
        public async Task<SaveData> LoadGameAsync()
        {
            if (!File.Exists(SaveFilePath))
            {
                Debug.Log("[SaveManager] 저장된 데이터가 없습니다. 새로운 SaveData를 생성합니다.");
                return new SaveData(); // 파일이 없으면 새 데이터를 반환합니다.
            }

            try
            {
                // 백그라운드 스레드에서 비동기로 텍스트를 읽어옵니다.
                using StreamReader reader = new StreamReader(SaveFilePath);
                string json = await reader.ReadToEndAsync();
                
                // 문자열을 다시 SaveData 객체로 조립(역직렬화)합니다.
                SaveData loadedData = JsonConvert.DeserializeObject<SaveData>(json);
                
                Debug.Log("[SaveManager] 데이터 불러오기 완료!");
                return loadedData;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] 데이터 불러오기 실패: {e.Message}");
                return new SaveData(); // 에러 발생 시 진행이 막히지 않도록 빈 데이터를 반환합니다.
            }
        }
        
        public void DeleteSaveData()
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
                Debug.Log("[SaveManager] 세이브 파일 삭제 완료.");
            }
        }
    }
}