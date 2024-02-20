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
    private int LF_UnderCap;

    private int RF_UnderCap;
    private int RB_UnderCap;
    private int LB_UnderCap;

    private int LF_AboveCap = 4;
    private int RF_AboveCap = 4;
    private int RB_AboveCap = 4;
    private int LB_AboveCap = 4;

    private bool sendAboveCapOpenSignalOnce = false;

    // ABOVE:全開
    private const int ABOVE_FULL＿OPEN = 4;

    // ABOVE:全閉じ
    private const int ABOVE_HALF＿OPEN = 5;

    // ABOVE:全閉じ
    private const int ABOVE_CLOSE = 6;

    // UNDER:全開
    private const int UNDER_FULL＿OPEN = 3;

    // UNDER:中くらい
    private const int UNDER_HALF＿OPEN = 2;

    // UNDER:閉じる
    private const int UNDER_CLOSE = 1;

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

    private void FixedUpdate()
    {
        //Debug.Log("データ" + LF_Data);
    }

    //caseの中でpullpowerを考慮してそれぞれ(LF_port~4)の力を決める
    private void SetPortIndices(WindManager windManager)
    {
        /*
        if(windManager.isMatching == false){
            LF_power = CLOSE; RF_power = CLOSE; RB_power = CLOSE; LB_power = CLOSE;
            return;
        }*/
        switch (windManager.currentWindIndex)
        {
            case 0:
                LF_UnderCap = UNDER_FULL＿OPEN; RF_UnderCap = UNDER_FULL＿OPEN; RB_UnderCap = UNDER_FULL＿OPEN; LB_UnderCap = UNDER_FULL＿OPEN;
                break;
            /*
            // 1が下のモータ閉める、2が半分、3が下のモータ開いて上がく。
            */
            case 1://北(前)
                LF_UnderCap = UNDER_FULL＿OPEN; RF_UnderCap = UNDER_FULL＿OPEN; RB_UnderCap = UNDER_CLOSE; LB_UnderCap = UNDER_CLOSE;
                break;

            case 2://北東(右前)
                LF_UnderCap = UNDER_CLOSE; RF_UnderCap = UNDER_FULL＿OPEN; RB_UnderCap = UNDER_CLOSE; LB_UnderCap = UNDER_CLOSE;
                break;

            case 3://東(右)
                LF_UnderCap = UNDER_CLOSE; RF_UnderCap = UNDER_FULL＿OPEN; RB_UnderCap = UNDER_FULL＿OPEN; LB_UnderCap = UNDER_CLOSE;
                break;

            case 4://南東(右後)
                LF_UnderCap = UNDER_CLOSE; RF_UnderCap = UNDER_CLOSE; RB_UnderCap = UNDER_FULL＿OPEN; LB_UnderCap = UNDER_CLOSE;
                break;

            case 5://南(後)
                LF_UnderCap = UNDER_CLOSE; RF_UnderCap = UNDER_CLOSE; RB_UnderCap = UNDER_FULL＿OPEN; LB_UnderCap = UNDER_FULL＿OPEN;
                break;

            case 6://南西(左後)
                LF_UnderCap = UNDER_CLOSE; RF_UnderCap = UNDER_CLOSE; RB_UnderCap = UNDER_CLOSE; LB_UnderCap = UNDER_FULL＿OPEN;
                break;

            case 7://西(左)
                LF_UnderCap = UNDER_FULL＿OPEN; RF_UnderCap = UNDER_CLOSE; RB_UnderCap = UNDER_CLOSE; LB_UnderCap = UNDER_FULL＿OPEN;
                break;

            case 8://北西(左前)
                LF_UnderCap = UNDER_FULL＿OPEN; RF_UnderCap = UNDER_CLOSE; RB_UnderCap = UNDER_CLOSE; LB_UnderCap = UNDER_CLOSE;
                break;
        }
    }

    // 上のふたの制御をする　上のふたに命令を送る
    private void SetUnderPortIndices(WindManager windManager)
    {
        if (windManager.isMatchingFinal == true)
        {
            if (LF_UnderCap == UNDER_FULL＿OPEN)
            {
                LF_AboveCap = ABOVE_CLOSE;
            }
            /*else if (LF_UnderCap == UNDER_CLOSE)
            {
                LF_AboveCap = ABOVE_FULL＿OPEN;
            }*/
            if (RF_UnderCap == UNDER_FULL＿OPEN)
            {
                RF_AboveCap = ABOVE_CLOSE;
            }/*
                else if (RF_UnderCap == UNDER_CLOSE)
                {
                    RF_AboveCap = ABOVE_FULL＿OPEN;
                }*/
            if (RB_UnderCap == UNDER_FULL＿OPEN)
            {
                Debug.Log("aaa");
                RB_AboveCap = ABOVE_CLOSE;
            }/*
                else if (RB_UnderCap == UNDER_CLOSE)
                {
                    RB_AboveCap = ABOVE_FULL＿OPEN;
                }*/
            if (LB_UnderCap == UNDER_FULL＿OPEN)
            {
                LB_AboveCap = ABOVE_CLOSE;
            }
            /*else if (LB_UnderCap == UNDER_CLOSE)
            {
                LB_AboveCap = ABOVE_FULL＿OPEN;
            }*/
        }
        else
        {
            if (LF_UnderCap == UNDER_FULL＿OPEN)
            {
                LF_AboveCap = ABOVE_FULL＿OPEN;
            }
            if (RF_UnderCap == UNDER_FULL＿OPEN)
            {
                RF_AboveCap = ABOVE_FULL＿OPEN;
            }
            if (RB_UnderCap == UNDER_FULL＿OPEN)
            {
                // Debug.Log("bbb");
                RB_AboveCap = ABOVE_FULL＿OPEN;
            }
            if (LB_UnderCap == UNDER_FULL＿OPEN)
            {
                LB_AboveCap = ABOVE_FULL＿OPEN;
            }
            /*if (LF_UnderCap == UNDER_FULL＿OPEN)
            {
                LF_AboveCap = ABOVE_CLOSE;
            }
            else if (LF_UnderCap == UNDER_CLOSE)
            {
                LF_AboveCap = ABOVE_FULL＿OPEN;
            }
            if (RF_UnderCap == UNDER_FULL＿OPEN)
            {
                RF_AboveCap = ABOVE_CLOSE;
            }
            else if (RF_UnderCap == UNDER_CLOSE)
            {
                RF_AboveCap = ABOVE_FULL＿OPEN;
            }
            if (RB_UnderCap == UNDER_FULL＿OPEN)
            {
                // Debug.Log("bbb");
                RB_AboveCap = ABOVE_CLOSE;
            }
            else if (RB_UnderCap == UNDER_CLOSE)
            {
                RB_AboveCap = ABOVE_FULL＿OPEN;
            }
            if (LB_UnderCap == UNDER_FULL＿OPEN)
            {
                LB_AboveCap = ABOVE_CLOSE;
            }
            else if (LB_UnderCap == UNDER_CLOSE)
            {
                LB_AboveCap = ABOVE_FULL＿OPEN;
            }*/
        }
        //}
        // 上昇準備 & 上昇中だったら
        /*else
        {
            // 上昇が終わっていなかったら
            if (windManager.upFinish == false)
            {
                // 蓋が閉じる
                LF_AboveCap = ABOVE_CLOSE; RF_AboveCap = ABOVE_CLOSE; RB_AboveCap = ABOVE_CLOSE; LB_AboveCap = ABOVE_CLOSE;
            }
            else
            {
                // 上昇が終わったらふたがあく,多分いらない
                LF_AboveCap = ABOVE_FULL＿OPEN; RF_AboveCap = ABOVE_FULL＿OPEN; RB_AboveCap = ABOVE_FULL＿OPEN; LB_AboveCap = ABOVE_FULL＿OPEN;
            }
        }*/
    }

    /*
    if(windManager.isMatching == false){
        LF_power = CLOSE; RF_power = CLOSE; RB_power = CLOSE; LB_power = CLOSE;
        return;
    }*/

    /*
    if (windManager.isMatchingFinal == true)
    {
        switch (windManager.currentWindIndex)
        {
            // 1が下のモータ閉める、2が半分、3が下のモータ開いて上がく。
            case 1://北(前)
                LF_AboveCap = FULL＿OPEN; RF_AboveCap = FULL＿OPEN; RB_AboveCap = CLOSE; LB_AboveCap = CLOSE;
                break;

            case 2://北東(右前)
                LF_AboveCap = CLOSE; RF_AboveCap = FULL＿OPEN; RB_AboveCap = CLOSE; LB_AboveCap = CLOSE;
                break;

            case 3://東(右)
                LF_AboveCap = CLOSE; RF_AboveCap = FULL＿OPEN; RB_AboveCap = FULL＿OPEN; LB_AboveCap = CLOSE;
                break;

            case 4://南東(右後)
                LF_AboveCap = CLOSE; RF_AboveCap = CLOSE; RB_AboveCap = FULL＿OPEN; LB_AboveCap = CLOSE;
                break;

            case 5://南(後)
                LF_AboveCap = CLOSE; RF_AboveCap = CLOSE; RB_AboveCap = FULL＿OPEN; LB_AboveCap = FULL＿OPEN;
                break;

            case 6://南西(左後)
                LF_AboveCap = CLOSE; RF_AboveCap = CLOSE; RB_AboveCap = CLOSE; LB_AboveCap = FULL＿OPEN;
                break;

            case 7://西(左)
                LF_AboveCap = FULL＿OPEN; RF_AboveCap = CLOSE; RB_AboveCap = CLOSE; LB_AboveCap = FULL＿OPEN;
                break;

            case 8://北西(左前)
                LF_AboveCap = FULL＿OPEN; RF_AboveCap = CLOSE; RB_AboveCap = CLOSE; LB_AboveCap = CLOSE;
                break;
        }
    }
    // 該当の箇所のふたを閉める
    else
    {
        switch (windManager.currentWindIndex)
        {
            // 1が下のモータ閉める、2が半分、3が下のモータ開いて上がく。
            case 1://北(前)
                LF_AboveCap = CLOSE; RF_AboveCap = CLOSE; RB_AboveCap = FULL＿OPEN; LB_AboveCap = FULL＿OPEN;
                break;

            case 2://北東(右前)
                LF_AboveCap = FULL＿OPEN; RF_AboveCap = CLOSE; RB_AboveCap = FULL＿OPEN; LB_AboveCap = FULL＿OPEN;
                break;

            case 3://東(右)
                LF_AboveCap = FULL＿OPEN; RF_AboveCap = CLOSE; RB_AboveCap = FULL＿OPEN; LB_AboveCap = CLOSE;
                break;

            case 4://南東(右後)
                LF_AboveCap = CLOSE; RF_AboveCap = CLOSE; RB_AboveCap = FULL＿OPEN; LB_AboveCap = CLOSE;
                break;

            case 5://南(後)
                LF_AboveCap = CLOSE; RF_AboveCap = CLOSE; RB_AboveCap = FULL＿OPEN; LB_AboveCap = FULL＿OPEN;
                break;

            case 6://南西(左後)
                LF_AboveCap = CLOSE; RF_AboveCap = CLOSE; RB_AboveCap = CLOSE; LB_AboveCap = FULL＿OPEN;
                break;

            case 7://西(左)
                LF_AboveCap = FULL＿OPEN; RF_AboveCap = CLOSE; RB_AboveCap = CLOSE; LB_AboveCap = FULL＿OPEN;
                break;

            case 8://北西(左前)
                LF_AboveCap = FULL＿OPEN; RF_AboveCap = CLOSE; RB_AboveCap = CLOSE; LB_AboveCap = CLOSE;
                break;
        }
    }
}*/
}