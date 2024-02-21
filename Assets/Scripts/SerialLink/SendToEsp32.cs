using System;
using System.IO.Ports;
using UnityEngine;
using System.Collections;

public class SendToEsp32 : MonoBehaviour
{
    // WindManagerの参照
    //public WindManager windManager;
    private int LF_Hole;

    private int RF_Hole;

    private bool sendAboveCapOpenSignalOnce = false;

    private const int OPEN = 1;

    private const int CLOSE = 2;

    // WindManagerのup(bool)をa or b　のstringにして格納するようの変数
    private string windBoostedRise;

    private string Wind_Data;

    public void StartSendData(SerialPortManager spManager, WindManager windManager)
    {
        //StartCoroutine(SendDataCoroutine(spManager, windManager));
    }

    public void SendDataOnce(SerialPortManager spManager, WindManager windManager)
    {
        Debug.Log("実行");
        SetPortIndices(windManager);
        spManager.WriteToPort(0, LF_Hole.ToString());
        spManager.WriteToPort(1, RF_Hole.ToString());
        spManager.WriteToPort(2, windManager.CurrentWindIndex.ToString());
        Debug.Log("方向" + windManager.CurrentWindIndex + "RF_Hole:" + RF_Hole + "LF_Hole:" + LF_Hole);
    }

    // 0.5秒ずつ情報を送信　
    // private IEnumerator SendDataCoroutine(SerialPortManager spManager, WindManager windManager)
    // {
    //     while (true)  // 無限ループで送信処理を繰り返す
    //     {
    //         try
    //         {
    //             //  0:LF_port, 1:RF_Port, 2:Wind_Port, 3:kasa_Port
    //             SetPortIndices(windManager);
    //             spManager.WriteToPort(0, LF_Hole.ToString());
    //             spManager.WriteToPort(1, RF_Hole.ToString());
    //             spManager.WriteToPort(2, windManager.CurrentWindIndex.ToString());
    //             Debug.Log("RF_Hole:" + RF_Hole.ToString() + "LF_Hole:" + LF_Hole.ToString());
    //         }
    //         catch (Exception ex)
    //         {
    //             Debug.LogError("Could not send to ESP32: " + ex.Message);
    //         }
    //         yield return new WaitForSeconds(1f);  // 2秒ごとにループ
    //     }
    // }
    private IEnumerator SendDataCoroutine(SerialPortManager spManager, WindManager windManager)
    {
        Debug.Log(GameManager.instance.gameTimer + "時間：状態" + GameManager.instance.isAblePlayingHard);
        int lastRandomNumber = 0; // 前回のランダム値を保存
        while (true)  // 無限ループで送信処理を繰り返す
        {
            try
            {
                int randomNumber = UnityEngine.Random.Range(1, 3); // 1, 2, 3の中からランダムに選択
                while (randomNumber == lastRandomNumber) // 前回と同じ数字が出ないようにする
                {
                    randomNumber = UnityEngine.Random.Range(1, 4);
                }
                lastRandomNumber = randomNumber; // 今回のランダム値を保存

                // 送信データの作成
                Wind_Data = randomNumber.ToString();

                // spManager.WriteToPort(0, Wind_Data);を使用して送信
                spManager.WriteToPort(1, Wind_Data);
                spManager.WriteToPort(0, Wind_Data);
                Debug.Log("send");

                // spManager.Read(5)の結果をデバッグログで表示
                // 例: Debug.Log(spManager.Read(5)); // 必要に応じてコメントアウトを解除してください
            }
            catch (Exception ex)
            {
                Debug.LogError("Could not send to ESP32: " + ex.Message);
            }

            yield return new WaitForSeconds(1f);  // 2秒ごとにループ
        }
    }

    //caseの中でpullpowerを考慮してそれぞれ(LF_port~4)の力を決める
    private void SetPortIndices(WindManager windManager)
    {
        /*
        if (windManager.IsMatchingFinal == true)
        {*/
        switch (windManager.CurrentWindIndex)
        {
            case 1://北(前)
                LF_Hole = OPEN; RF_Hole = OPEN;
                break;

            case 2://北東(右前)
                LF_Hole = OPEN; RF_Hole = CLOSE;
                break;

            case 8://北西(左前)
                LF_Hole = CLOSE; RF_Hole = OPEN;
                break;
        }
        /*}
        else
        {
            LF_Hole = CLOSE; RF_Hole = CLOSE;
        }*/
    }
}