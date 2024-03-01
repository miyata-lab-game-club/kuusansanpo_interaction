using UnityEngine;
using System.Collections;
using UnityEngine.Networking; // UnityのネットワーキングAPIを使用

public class FetchAverageAngle : MonoBehaviour
{
    // FlaskサーバのURL
    private string url = "http://127.0.0.1:5000/get_average_angle";

    private float row_angle = 0;

    public float Row_angle { get => row_angle; set => row_angle = value; }

    private void Start()
    {
        // サーバからデータを定期的に取得するコルーチンを開始
        StartCoroutine(GetAverageAngleRepeatedly());
    }

    private IEnumerator GetAverageAngleRepeatedly()
    {
        // 無限ループでAPIを叩き続ける
        while (true)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                // リクエストを送信し、レスポンスを待機
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError || webRequest.isHttpError)
                {
                    // ネットワークエラーまたはHTTPエラーが発生した場合
                    Debug.Log("Error: " + webRequest.error);
                }
                else
                {
                    AngleData data = JsonUtility.FromJson<AngleData>(webRequest.downloadHandler.text);
                    Debug.Log("Average Angle: " + data.average_angle);
                    row_angle = data.average_angle;
                    //float row_angle = float.Parse(webRequest.downloadHandler.text);
                    //Debug.Log("Received: " + row_angle);
                }
            }
            // 0.2秒待機
            yield return new WaitForSeconds(0.1f);
        }
    }
}

public class AngleData
{
    public float average_angle;
}