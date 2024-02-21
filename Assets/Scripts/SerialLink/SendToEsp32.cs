using System;
using System.IO.Ports;
using UnityEngine;
using System.Collections;

public class SendToEsp32 : MonoBehaviour
{
    private int LF_Hole;
    private int RF_Hole;
    private string Wind_Data;

    private const int CLOSE = 1;
    private const int OPEN = 2;
    private WindManager windManager;

    // WindManagerのup(bool)をa or b　のstringにして格納するようの変数
    private string windBoostedRise;

    private void Start()
    {
        // spManager = SerialPortManager.Instance;

        // if (spManager == null)
        // {
        //     Debug.LogError("SerialPortManager instance not found!");
        //     return;
        // }

        // if (windManager == null)
        // {
        //     Debug.LogError("WindManager reference is not set on SendToEsp32.");
        //     return;
        // }

        // {
        //     StartCoroutine(SendDataCoroutine());
        // }
    }

    public void StartSendData(SerialPortManager spManager, WindManager windManager)
    {
        StartCoroutine(SendDataCoroutine(spManager, windManager));
    }

    // 0.5秒ずつ情報を送信　
    private IEnumerator SendDataCoroutine(SerialPortManager spManager, WindManager windManager)
    {
        Debug.Log(GameManager.instance.gameTimer + "時間：状態" + GameManager.instance.isAblePlayingHard);
        while (true)  // 無限ループで送信処理を繰り返す
        {
            try
            {
                // 送信データの作成
                int Wind_Data = windManager.CurrentWindIndex;
                SetPortIndices(windManager);

                // spManager.WriteToPort(0, Wind_Data);を使用して送信
                spManager.WriteToPort(0, LF_Hole.ToString());
                spManager.WriteToPort(1, RF_Hole.ToString());
                spManager.WriteToPort(2, Wind_Data.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogError("Could not send to ESP32: " + ex.Message);
            }

            yield return new WaitForSeconds(2f);  // 2秒ごとにループ
        }
    }

    //caseの中でpullpowerを考慮してそれぞれ(LF_port~4)の力を決める
    private void SetPortIndices(WindManager windManager)
    {
        switch (windManager.CurrentWindIndex)
        {
            case 0:
                LF_Hole = OPEN; RF_Hole = OPEN;
                break;

            case 1://北(前)
                LF_Hole = CLOSE; RF_Hole = CLOSE;
                break;

            case 2://北東(右前)
                LF_Hole = OPEN; RF_Hole = CLOSE;
                break;

            case 8://北西(左前)
                LF_Hole = CLOSE; RF_Hole = OPEN;
                break;
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Applicationを停止しました");
    }
}