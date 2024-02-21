using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.XR;
using DG.Tweening;

public class WindManager : MonoBehaviour
{
    // デモかどうか
    [SerializeField] private bool isDemo = true;

    // 動画用か trueだったらUIが消える＆風の方向が順番になる
    [SerializeField] private bool isDemoMovie = true;

    [SerializeField] private SendToEsp32 sendToesp32;
    private SerialPortManager spManager;
    [SerializeField] private WindMovement windMovement;
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private GameObject player;
    [SerializeField] private float speed;

    // PlayerのRigidbody
    private Rigidbody playerRigidbody;

    public bool up;
    private bool boost;
    private bool twiceBoost;// 速度二倍

    // 落ちていく速度
    //[SerializeField] private Vector3 gravityDirection;
    [SerializeField] private Vector3 downVeclocity;

    // 上昇する強さ
    [SerializeField] private int upPower = 5;

    // 上昇する速度
    [SerializeField] private float upVelocity = 1;

    public bool sendToHardUpSignal = false;

    [SerializeField] private Transform centerEyeAnchor;
    [SerializeField] private Transform rightControllerTransform;
    [SerializeField] private GameObject[] directionUIs;
    [SerializeField] private float upHeight = 130;
    [SerializeField] private float startUpHeight = 110;
    [SerializeField] private float windCicleTime = 5;
    [SerializeField] private float similarityStandard = 0.8f;
    private float timer;
    private Vector3 currentWind;

    [SerializeField] private GameObject windPivot;

    [SerializeField] private ReceiveFromEsp32 ReceiveFromEsp32;// SerialPortManager の参照を持つ変数
    private int currentWindIndex = 0;

    // ひとつ前の風の方向
    private int previousWindIndex = 1;

    //　上、北、北東、東、南東、南、南西、西、北西
    public Vector3[] windDirection = Define.windDirection;

    public Vector3[] windXZDirection = new Vector3[]
{new Vector3(0,0,0), new Vector3(0, 0, 1),new Vector3(1, 0, 1),new Vector3(1, 0, 0),
     new Vector3(1,0,-1), new Vector3(0, 0, -1),new Vector3(-1, 0, -1),new Vector3(-1, 0, 0),
     new Vector3(-1, 0, 1)
};

    // 仮UI
    [SerializeField] private TextMeshProUGUI heightText;

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI similarityText;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private Text moveTimerText;

    public GameObject kasa_Port;
    private Quaternion kasaVector;
    public bool isMatching = false;
    private bool isMatchingFinal = false;
    private float judgeTimer = 0;
    private float judgeTime = 1;
    private bool changeOnce = false;

    private const int UP_READY_TIME = 4;
    public bool upFinish = false;

    public bool IsMatchingFinal { get => isMatchingFinal; set => isMatchingFinal = value; }
    public int CurrentWindIndex { get => currentWindIndex; set => currentWindIndex = value; }

    private void Start()
    {
        judgeTimer = 0;
        endPanel.SetActive(false);
        moveTimerText.enabled = false;
        /*
        Vector3 oculusForward = InputTracking.GetLocalRotation(XRNode.CenterEye) * Vector3.forward;
        player.transform.rotation = Quaternion.LookRotation(oculusForward, Vector3.up);*/
        player.transform.forward = centerEyeAnchor.forward;
        heightText.text = this.transform.position.y.ToString();
        playerRigidbody = player.GetComponent<Rigidbody>();
        timer = 0;
        timerText.text = timer.ToString();
        currentWind = currentWindDirection();
        SetWindEffectDirection(CurrentWindIndex);
        spManager = SerialPortManager.Instance;

        if (isDemo == false)
        {
            heightText.enabled = false;
            timerText.enabled = false;
            similarityText.enabled = false;
        }

        if (spManager == null)
        {
            Debug.LogError("SerialPortManager instance not found!");
            return;
        }

        {
            sendToesp32.StartSendData(spManager, this);
        }
    }

    private Vector3 rightControllerTilt;
    private int index = 0;

    private void Update()
    {
        //　テストコード
        timer += Time.deltaTime;
        // 3秒ごとにサーボモータに信号を送る
        int[] windDirections = { 1, 2, 8 };
        if (timer > 5)
        {
            CurrentWindIndex = windDirections[index];
            sendToesp32.SendDataOnce(spManager, this);// 信号を送る
            if (index < 2)
            {
                index++;
            }
            else
            {
                index = 0;
            }
            timer = 0;
        }

        /*
        if (GameManager.instance.isPlaying == false)
        {
            return;
        }
        kasaVector = kasa_Port.transform.rotation;

        Quaternion rightControllerRotation = kasaVector;

        rightControllerTilt = kasa_Port.transform.up;
        // xz平面に戻す
        rightControllerTilt.y = 0;

        heightText.text = this.transform.position.y.ToString();

        if (OVRInput.GetDown(OVRInput.RawButton.A) && !boost)
        {
            boost = true;
            timer = 0;
            twiceBoost = currentWindFromController();
            SetActiveWindDirection(CurrentWindIndex);
        }

        //Debug.Log(ReceiveFromEsp32.buttonState);
        //スイッチ押しているなら加速
        if (ReceiveFromEsp32.buttonState == 'b' && !boost)
        {
            Debug.Log("加速");
            boost = true;
            timer = 0;
            twiceBoost = currentWindFromController();
            SetActiveWindDirection(CurrentWindIndex);
        }

        Debug.DrawLine(new Vector3(-28, 9, -60), new Vector3(-28, 9, -60) + rightControllerTilt * 5, Color.red);

        timerText.text = timer.ToString();

        // 一致している間だけ時間を進める
        if (isMatching == true)
        {
            // 判定時間
            judgeTimer += Time.deltaTime;
        }
        else
        {
            judgeTimer = 0;
        }

        // 判定時間が判定基準時間より大きくなった時 かつ変更するのが一回目だったら
        if (judgeTimer > judgeTime && changeOnce == false)
        {
            // もし判定時間がきて一致フラグがtrueだったら
            if (isMatching == true)
            {
                // 一致している
                IsMatchingFinal = true;
                // 変更できないようにする
                // データを送る
                sendToesp32.SendDataOnce(spManager, this);// 追加
            }
            else
            {
                // 一致していない
                IsMatchingFinal = false;
            }

            Debug.Log("time" + timer + "3秒たって一致しているか" + IsMatchingFinal);
            changeOnce = true;
            judgeTimer = 0;
        }

        // １秒前に一致していない状態にする
        */
    }

    private void FixedUpdate()
    {
        /*
        if (GameManager.instance.isPlaying == false)
        {
            return;
        }
        if (boost != true)
        {
            // if (player.transform.position.y > startUpHeight && !up)
            // {
            // Debug.Log(isMatchingFinal);

            timer += Time.deltaTime;

            //Debug.Log("timer:"+timer+"windCicleTime:"+windCicleTime);
            if (timer > windCicleTime)
            {
                // 風を変更
                currentWind = currentWindDirection();
                sendToesp32.SendDataOnce(spManager, this);// 追加
                timer = 0;
            }
            // 上昇準備中と上昇中は操作を受け付けないようにする
            //if (sendToHardUpSignal == false)
            //{
            //Debug.Log("操作を受け付ける");
            float similarity;
            similarity = Vector3.Dot(rightControllerTilt.normalized, windXZDirection[CurrentWindIndex].normalized);
            // 類似度が0.7よりおおきいとき
            //Debug.Log(similarity);
            similarityText.text = similarity.ToString();
            if (similarity >= similarityStandard)
            {
                isMatching = true;
                moveTimerText.enabled = true;
                moveTimerText.text = judgeTimer.ToString("f1");

                if (timer > windCicleTime - 1)
                {
                    //isMatching = false;　// 念のため
                    IsMatchingFinal = false;
                    moveTimerText.enabled = false;
                    changeOnce = false;
                    judgeTimer = 0;
                }

                if (IsMatchingFinal == true)
                {
                    moveTimerText.enabled = false;
                    Vector3 directionVector = new Vector3(currentWind.x * speed, upVelocity, currentWind.z * speed);
                    playerRigidbody.velocity = directionVector;
                }
            }
            // 類似していなければ
            else
            {
                moveTimerText.enabled = false;
                isMatching = false;
                // おちていく
                playerRigidbody.velocity = downVeclocity;
            }
            //}
            // else
            //{
            //  Debug.Log(timer + "操作を受け付けない");
            //}
        }
        else
        {
            timer += Time.deltaTime;
            if (timer < windCicleTime && twiceBoost)
            {
                currentWind = windDirection[CurrentWindIndex];
                // 傾きに一番近い方向に風が吹く
                playerRigidbody.velocity = new Vector3(currentWind.x * speed, currentWind.y, currentWind.z * speed);
            }
            else if (timer < windCicleTime && !twiceBoost)
            {
                currentWind = windDirection[CurrentWindIndex];
                // 傾きに一番近い方向に風が吹く
                playerRigidbody.velocity = currentWind * speed;
            }
            else
            {
                timer = 0;
                boost = false;
            }
        }*/
    }

    // コントローラーの向いている方向に風を吹かせる
    private bool currentWindFromController()
    {
        float similarity;
        float maxSimilarity = -1;
        int tmpCurrentWindIndex = CurrentWindIndex;
        // 傾きと一番近い方向を割り出す
        for (int i = 1; i < windDirection.Length; i++)
        {
            rightControllerTilt.y = 0;
            similarity = Vector3.Dot(rightControllerTilt.normalized, windXZDirection[i].normalized);
            /*Debug.Log("右コントローラーの傾き" + rightControllerTilt.normalized + "×" + windXZDirection[i].normalized);
            Debug.Log("類似度" + similarity);*/
            if (similarity > maxSimilarity)
            {
                maxSimilarity = similarity;
                CurrentWindIndex = i;
            }
        }

        // 風向きを音とエフェクトで提示
        windMovement.WindMove(CurrentWindIndex);
        similarityText.text = maxSimilarity.ToString();
        if (tmpCurrentWindIndex == CurrentWindIndex)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private int tmpIndex = 0;

    // 現在吹いている風を決める
    public Vector3 currentWindDirection()
    {
        // currentWindIndex = Random.Range(1, 9);

        //ここに風の方向を入れる
        int[] choices = { 1, 2, 8 };
        int rand;
        if (isDemoMovie == false)
        {
            rand = choices[Random.Range(0, choices.Length)];
            // 前吹いた風の向きと同じなら
            while (rand == previousWindIndex)
            {
                // もう一度振りなおす
                rand = choices[Random.Range(0, choices.Length)];
            }
            //Debug.Log(rand);
        }
        else
        {
            if (tmpIndex < choices.Length - 1)
            {
                tmpIndex++;
            }
            else
            {
                tmpIndex = 0;
            }
            rand = choices[tmpIndex];
        }
        // 現在の風を更新
        CurrentWindIndex = rand;
        // ひとつ前の風を更新
        previousWindIndex = CurrentWindIndex;
        // UIの表示
        SetActiveWindDirection(CurrentWindIndex);
        // 風向きを音とエフェクトで提示
        windMovement.WindMove(CurrentWindIndex);
        //風のエフェクトの向き設定
        SetWindEffectDirection(CurrentWindIndex);
        return windDirection[CurrentWindIndex].normalized;
    }

    private void SetActiveWindDirection(int currentWindIndex)
    {
        if (isDemo == true)
        {
            for (int i = 0; i < windDirection.Length; i++)
            {
                directionUIs[i].SetActive(false);
            }
            directionUIs[currentWindIndex].SetActive(true);
        }
        else
        {
            for (int i = 0; i < windDirection.Length; i++)
            {
                directionUIs[i].SetActive(false);
            }
        }
    }

    // UI
    public void SetActiveGameEndPanel()
    {
        endPanel.SetActive(true);
        endPanel.transform.DOScale(new Vector3(1, 1, 1), 1);
    }

    private void SetWindEffectDirection(int windindex)
    {
        Quaternion pivotRot;
        switch (CurrentWindIndex)
        {
            case 0:
                pivotRot = Quaternion.Euler(-90, 0, 0);
                windPivot.transform.rotation = pivotRot;
                break;

            case 1:
                pivotRot = Quaternion.Euler(0, 0, 0);
                windPivot.transform.rotation = pivotRot;
                break;

            case 2:
                pivotRot = Quaternion.Euler(0, 45, 0);
                windPivot.transform.rotation = pivotRot;
                break;

            case 3:
                pivotRot = Quaternion.Euler(0, 90, 0);
                windPivot.transform.rotation = pivotRot;
                break;

            case 4:
                pivotRot = Quaternion.Euler(0, 135, 0);
                windPivot.transform.rotation = pivotRot;
                break;

            case 5:
                pivotRot = Quaternion.Euler(0, 180, 0);
                windPivot.transform.rotation = pivotRot;
                break;

            case 6:
                pivotRot = Quaternion.Euler(0, 225, 0);
                windPivot.transform.rotation = pivotRot;
                break;

            case 7:
                pivotRot = Quaternion.Euler(0, 270, 0);
                windPivot.transform.rotation = pivotRot;
                break;

            case 8:
                pivotRot = Quaternion.Euler(0, 315, 0);
                windPivot.transform.rotation = pivotRot;
                break;
        }
    }
}