using UnityEngine;
using System.Collections;
using UnityEngine.Networking; // UnityのネットワーキングAPIを使用

public class FetchAverageAngle : MonoBehaviour
{
    [System.Serializable] // この行を追加
    public class AngleData // クラスを public に変更
    {
        public string average_angle;
    }

    public float LatestAngle { get; private set; } // 外部からアクセス可能なプロパティ

    private IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f); // 0.1秒ごとにリクエスト
            StartCoroutine(GetAngle());
        }
    }

    private IEnumerator GetAngle()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("http://127.0.0.1:5000/get_average_angle"))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                string response = webRequest.downloadHandler.text;
                AngleData angleData = JsonUtility.FromJson<AngleData>(response);
                if (float.TryParse(angleData.average_angle, out float parsedAngle))
                {
                    LatestAngle = parsedAngle; // パースされた数値をプロパティに格納
                    // Debug.Log(LatestAngle);
                    Debug.Log("角度:" + LatestAngle);
                }
                else
                {
                    Debug.LogError("Failed to parse angle: " + angleData.average_angle);
                }
            }
        }
    }
}