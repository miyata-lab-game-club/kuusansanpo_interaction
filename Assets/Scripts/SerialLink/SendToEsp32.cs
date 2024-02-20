using System;
using System.IO.Ports;
using UnityEngine;
using System.Collections;

public class SendToEsp32 : MonoBehaviour
{
    //private SerialPortManager spManager; // SerialPortManager の参照を持つ変数

    // WindManagerの参照
    //public WindManager windManager;

    // Port４とネックファンは後で追加
    private int LF_Hole;

    private int RF_Hole;

    private bool sendAboveCapOpenSignalOnce = false;

    // UNDER:閉じる
    private const int CLOSE = 1;

    // UNDER:中くらい
    private const int OPEN = 2;

    // WindManagerのup(bool)をa or b　のstringにして格納するようの変数
    private string windBoostedRise;

    //文字送る用の型
    private string LF_Data;

    private string RF_Data;
    private string Wind_Data;

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
        int lastRandomNumber = 0; // 前回のランダム値を保存
        while (true)  // 無限ループで送信処理を繰り返す
        {
            try
            {
                int randomNumber = UnityEngine.Random.Range(1, 4); // 1, 2, 3の中からランダムに選択
                while (randomNumber == lastRandomNumber) // 前回と同じ数字が出ないようにする
                {
                    randomNumber = UnityEngine.Random.Range(1, 4);
                }
                lastRandomNumber = randomNumber; // 今回のランダム値を保存

                // 送信データの作成
                Wind_Data = randomNumber.ToString();

                // spManager.WriteToPort(0, Wind_Data);を使用して送信
                spManager.WriteToPort(2, Wind_Data);

                // spManager.Read(5)の結果をデバッグログで表示
                // 例: Debug.Log(spManager.Read(5)); // 必要に応じてコメントアウトを解除してください
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
        switch (windManager.currentWindIndex)
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
}